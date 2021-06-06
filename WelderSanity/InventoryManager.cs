using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using Sandbox.Game;
using Sandbox.Game.Replication.History;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Network;
using VRage.Utils;
using VRageMath;
using IMyLightingBlock = Sandbox.ModAPI.Ingame.IMyLightingBlock;
using IMyShipConnector = Sandbox.ModAPI.Ingame.IMyShipConnector;

namespace IngameScript
{
    internal partial class Program
    {
        public class InventoryManager
        {
            private Dictionary<MyItemType, MyFixedPoint> _reference;
            private readonly MyGridProgram _program;
            private bool _paused;

            private class TransferResult
            {
                private readonly List<MyItemType> _missingItems;

                public bool MissingItem => _missingItems.Count > 0;
                public bool DiffDetected { get; }
                public bool TransferredSomething { get; }
                public IEnumerable<MyItemType> MissingItems => _missingItems;

                internal TransferResult(List<MyItemType> missingItems, bool diffDetected, bool transferredSomething)
                {
                    _missingItems = missingItems;
                    DiffDetected = diffDetected;
                    TransferredSomething = transferredSomething;
                }
            }

            private class Payload
            {
                public MyFixedPoint MaxVolume { get; }
                public MyFixedPoint Volume { get; }
                public MyFixedPoint Mass { get; }

                internal Payload(MyFixedPoint maxVolume, MyFixedPoint volume, MyFixedPoint mass)
                {
                    MaxVolume = maxVolume;
                    Volume = volume;
                    Mass = mass;
                }
            }

            /// <summary>
            /// Return inventories sorted by decreasing capacity.
            /// </summary>
            private static List<IMyInventory> GetInventories(IEnumerable<IMyTerminalBlock> blocks)
            {
                var inventories =
                    from block in blocks
                    where block.InventoryCount == 1
                    select block.GetInventory();
                return inventories.OrderByDescending(inv => inv.MaxVolume).ToList();
            }

            private static Dictionary<MyItemType, MyFixedPoint> CountComponents(IEnumerable<IMyInventory> inventories)
            {
                var itemCounts = new Dictionary<MyItemType, MyFixedPoint>();

                foreach (var inventory in inventories)
                {
                    var items = new List<MyInventoryItem>();
                    inventory.GetItems(items, item => item.Type.TypeId == "MyObjectBuilder_Component");

                    foreach (var item in items)
                    {
                        MyFixedPoint value;
                        itemCounts.TryGetValue(item.Type, out value);
                        itemCounts[item.Type] = value + item.Amount;
                    }
                }

                return itemCounts;
            }

            private TransferResult TryOneTransfer(IReadOnlyDictionary<MyItemType, MyFixedPoint> components,
                IReadOnlyCollection<IMyInventory> connectedInventories,
                IReadOnlyCollection<IMyInventory> shipInventories)
            {
                var missingItems = new List<MyItemType>();
                var diffDetected = false;
                var transferredSomething = false;

                foreach (var entry in _reference)
                {
                    var type = entry.Key;
                    var targetCount = entry.Value;

                    MyFixedPoint currentCount;
                    components.TryGetValue(type, out currentCount);

                    if (currentCount == 0)
                    {
                        missingItems.Add(type);
                    }

                    var diff = targetCount - currentCount;
                    if (diff <= 0)
                    {
                        continue;
                    }

                    diffDetected = true;

                    foreach (var source in connectedInventories)
                    {
                        var sourceName = ((IMyCubeBlock) source.Owner).DefinitionDisplayNameText;

                        var itemOpt = source.FindItem(type);
                        if (!itemOpt.HasValue)
                        {
                            continue;
                        }

                        var item = itemOpt.Value;

                        foreach (var destination in shipInventories.Where(destination =>
                            source.CanTransferItemTo(destination, item.Type)))
                        {
                            var destinationName = ((IMyCubeBlock) destination.Owner).DefinitionDisplayNameText;

                            var before = destination.GetItemAmount(item.Type);
                            _program.Echo(
                                $"Transferring ({diff} {item.Type.SubtypeId}): {sourceName} -> {destinationName}");
                            source.TransferItemTo(destination, item, diff);
                            var after = destination.GetItemAmount(item.Type);

                            if (after <= before) continue;

                            transferredSomething = true;
                            goto finished;
                        }
                    }
                }

                finished:

                return new TransferResult(missingItems, diffDetected, transferredSomething);
            }

            private static void UpdateShipStatus(Payload payload, IMyTextSurface screen)
            {
                screen.ContentType = ContentType.TEXT_AND_IMAGE;
                screen.Alignment = TextAlignment.CENTER;
                screen.FontColor = Color.White;
                screen.BackgroundColor = Color.Black;
                screen.Font = "Monospace";
                screen.FontSize = 1.2f;

                screen.WriteText("PAYLOAD\n\n");
                screen.WriteText(
                    $"Mass\n{(float) payload.Mass,10:F2} kg\n\n", append: true);
                screen.WriteText($"Volume ({100f * (float) payload.Volume / (float) payload.MaxVolume:F2}%)\n",
                    append: true);
                screen.WriteText($"  {(float) payload.Volume,10:F2} L\n/ {(float) payload.MaxVolume,10:F2} L",
                    append: true);
            }


            private static void UpdateCargoStatus(TransferResult result, bool reset, IMyTextSurface screen,
                IEnumerable<IMyTerminalBlock> lights)
            {
                screen.ContentType = ContentType.TEXT_AND_IMAGE;
                screen.Alignment = TextAlignment.CENTER;
                screen.FontColor = Color.White;
                screen.Font = "Debug";
                screen.FontSize = 2f;

                Color color;

                if (reset)
                {
                    color = Color.White;
                    screen.BackgroundColor = new Color(255, 255, 255);
                    screen.FontColor = Color.Black;
                    screen.WriteText("\nReset");
                }
                else if (result == null)
                {
                    color = new Color(0, 0, 0); // black;
                    screen.BackgroundColor = new Color(130, 130, 130);
                    screen.WriteText("\nPaused");
                }
                else if (!result.DiffDetected)
                {
                    color = new Color(30, 130, 30); // green;
                    screen.BackgroundColor = new Color(0, 150, 0);
                    screen.WriteText("\nFull");
                }
                else if (result.TransferredSomething)
                {
                    color = new Color(30, 90, 130); // yellow;
                    screen.BackgroundColor = new Color(150, 150, 0);
                    screen.WriteText("\nLoading");
                }
                else if (result.MissingItem)
                {
                    color = new Color(130, 30, 30); // red
                    screen.BackgroundColor = new Color(150, 0, 0);
                    screen.WriteText("\nMissing items:\n\n");
                    screen.FontSize = 1.7f;

                    foreach (var type in result.MissingItems)
                    {
                        screen.WriteText($"{type.SubtypeId}\n", append: true);
                    }
                }
                else
                {
                    color = new Color(30, 30, 130); // blue
                    screen.BackgroundColor = new Color(0, 0, 150);
                    screen.WriteText("\nPartial");
                }

                foreach (var light in lights.OfType<IMyLightingBlock>())
                {
                    light.Color = color;
                }
            }

            private static Payload GetPayload(IEnumerable<IMyInventory> inventories)
            {
                MyFixedPoint maxVolume = 0;
                MyFixedPoint volume = 0;
                MyFixedPoint mass = 0;

                foreach (var inventory in inventories)
                {
                    maxVolume += inventory.MaxVolume;
                    volume += inventory.CurrentVolume;
                    mass += inventory.CurrentMass;
                }

                return new Payload(maxVolume * 1000, volume * 1000, mass);
            }

            private void UpdateStatus(TransferResult result, bool reset, Payload payload)
            {
                var lights = new List<IMyTerminalBlock>();
                _program.GridTerminalSystem.SearchBlocksOfName("(ws#light)", lights,
                    block => block.IsSameConstructAs(_program.Me) && block is IMyLightingBlock);

                var cockpits = new List<IMyCockpit>();
                _program.GridTerminalSystem.GetBlocksOfType(cockpits, block => block.IsSameConstructAs(_program.Me));

                if (cockpits.Count != 1)
                {
                    _program.Echo("ERROR: Not exactly one cockpit");
                    return;
                }

                var cockpit = cockpits[0];

                var leftScreen = cockpit.GetSurface(0);
                var centerScreen = cockpit.GetSurface(1);
                UpdateShipStatus(payload, leftScreen);
                UpdateCargoStatus(result, reset, centerScreen, lights);
            }


            public InventoryManager(MyGridProgram program)
            {
                _program = program;
            }

            public void Init()
            {
                _reference = null;
            }

            public void Update(string argument)
            {
                var reset = false;

                switch (argument)
                {
                    case "pause":
                        _paused = !_paused;
                        break;
                    case "reset":
                        reset = true;
                        break;
                }

                var blocks = new List<IMyTerminalBlock>();
                _program.GridTerminalSystem.GetBlocks(blocks);

                var shipBlocks = blocks.Where(block => block.IsSameConstructAs(_program.Me)).ToList();
                var shipInventories = GetInventories(shipBlocks);
                var components = CountComponents(shipInventories);

                if (_reference == null || reset)
                {
                    _reference = components;
                }

                var shipConnectors = shipBlocks.OfType<IMyShipConnector>().ToList();
                var connectedConnectors = (
                    from connector in shipConnectors
                    where connector.Status == MyShipConnectorStatus.Connected && connector.OtherConnector != null
                    select connector.OtherConnector
                );
                var connectedBlocks = blocks.Where(block => connectedConnectors.Any(block.IsSameConstructAs));
                var connectedInventories = GetInventories(connectedBlocks);

                TransferResult result = null;
                if (!_paused)
                {
                    result = TryOneTransfer(components, connectedInventories, shipInventories);
                }

                _program.Echo(
                    $"Result: (missing: {result?.MissingItem}, diff: {result?.DiffDetected}, tx: {result?.TransferredSomething})");

                var payload = GetPayload(shipInventories);

                UpdateStatus(result, reset, payload);
            }
        }
    }
}