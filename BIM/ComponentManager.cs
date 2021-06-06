using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using Sandbox.Game.Entities.Cube;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;
using VRageRender;

namespace IngameScript
{
    internal partial class Program
    {
        public class ComponentManager
        {
            private readonly MyGridProgram _grid;
            private readonly ComponentIterator _componentIterator;
            private DateTime _start;

            private enum Component
            {
                BulletproofGlass,
                Computer,
                Construction,
                Detector,
                Display,
                Explosives,
                Girder,
                GravityGenerator,
                InteriorPlate,
                LargeTube,
                Medical,
                MetalGrid,
                Motor,
                PowerCell,
                RadioCommunication,
                Reactor,
                SmallTube,
                SolarCell,
                SteelPlate,
                Superconductor,
                Thrust,
                Canvas,
            };

            private class ComponentIterator
            {
                private int _index;
                private static readonly Array Values = Enum.GetValues(typeof(Component));

                public void Increment()
                {
                    _index = (_index + 1) % Values.Length;
                }

                public Component GetPrevious()
                {
                    return (Component) Values.GetValue((_index + Values.Length - 1) % Values.Length);
                }

                public Component GetCurrent()
                {
                    return (Component) Values.GetValue(_index);
                }
            }

            private class ComponentInfo
            {
                public string Id { get; }
                public MyDefinitionId Blueprint { get; }
                public string Name { get; }
                public double Ratio { get; }
                public MyItemType Type { get; }

                public ComponentInfo(string id, string blueprint, string name, double ratio)
                {
                    Id = id;
                    Blueprint = MyDefinitionId.Parse($"MyObjectBuilder_BlueprintDefinition/{blueprint}");
                    Name = name;
                    Ratio = ratio;
                    Type = MyItemType.MakeComponent(id);
                }
            }


            static readonly Dictionary<Component, ComponentInfo> Info = new Dictionary<Component, ComponentInfo>()
            {
                {
                    Component.BulletproofGlass,
                    new ComponentInfo("BulletproofGlass", "BulletproofGlass", "Bulletproof Glass", 2)
                },
                {Component.Computer, new ComponentInfo("Computer", "ComputerComponent", "Computer", 5)},
                {
                    Component.Construction,
                    new ComponentInfo("Construction", "ConstructionComponent", "Construction", 20)
                },
                {Component.Detector, new ComponentInfo("Detector", "DetectorComponent", "Detector", .1)},
                {Component.Display, new ComponentInfo("Display", "Display", "Display", .5)},
                {Component.Explosives, new ComponentInfo("Explosives", "ExplosivesComponent", "Explosives", .1)},
                {Component.Girder, new ComponentInfo("Girder", "GirderComponent", "Girder", .5)},
                {
                    Component.GravityGenerator,
                    new ComponentInfo("GravityGenerator", "GravityGeneratorComponent", "Gravity Generator", .1)
                },
                {Component.InteriorPlate, new ComponentInfo("InteriorPlate", "InteriorPlate", "Interior Plate", 10)},
                {Component.LargeTube, new ComponentInfo("LargeTube", "LargeTube", "Large Tube", 2)},
                {Component.Medical, new ComponentInfo("Medical", "MedicalComponent", "Medical", .1)},
                {Component.MetalGrid, new ComponentInfo("MetalGrid", "MetalGrid", "Metal Grid", 2)},
                {Component.Motor, new ComponentInfo("Motor", "MotorComponent", "Motor", 4)},
                {Component.PowerCell, new ComponentInfo("PowerCell", "PowerCell", "Power Cell", 1)},
                {
                    Component.RadioCommunication,
                    new ComponentInfo("RadioCommunication", "RadioCommunicationComponent", "Radio", .5)
                },
                {Component.Reactor, new ComponentInfo("Reactor", "ReactorComponent", "Reactor", .5)},
                {Component.SmallTube, new ComponentInfo("SmallTube", "SmallTube", "Small Tube", 6)},
                {Component.SolarCell, new ComponentInfo("SolarCell", "SolarCell", "Solar Cell", .1)},
                {Component.SteelPlate, new ComponentInfo("SteelPlate", "SteelPlate", "Steel Plate", 40)},
                {Component.Superconductor, new ComponentInfo("Superconductor", "Superconductor", "Superconductor", 1)},
                {Component.Thrust, new ComponentInfo("Thrust", "ThrustComponent", "Thrust", 5)},
                {Component.Canvas, new ComponentInfo("Canvas", "Canvas", "Canvas", .01)},
            };

            public ComponentManager(MyGridProgram grid)
            {
                _grid = grid;
                _componentIterator = new ComponentIterator();
            }

            private static void AddInventoryCounts(
                IMyInventory inventory,
                IDictionary<Component, MyFixedPoint> counts)
            {
                foreach (var entry in Info)
                {
                    var resource = entry.Key;
                    var info = entry.Value;

                    MyFixedPoint value;
                    counts.TryGetValue(resource, out value);
                    counts[resource] = value + inventory.GetItemAmount(info.Type);
                }
            }

            private Dictionary<Component, MyFixedPoint> GetCounts()
            {
                var counts = new Dictionary<Component, MyFixedPoint>();
                {
                    var blocks = new List<IMyTerminalBlock>();
                    _grid.GridTerminalSystem.GetBlocks(blocks);

                    foreach (var block in blocks.Where(block => block.IsSameConstructAs(_grid.Me)))
                    {
                        if (block.InventoryCount == 1)
                        {
                            AddInventoryCounts(block.GetInventory(), counts);
                        }
                        else if (block is IMyProductionBlock)
                        {
                            var prod = (IMyProductionBlock) block;
                            AddInventoryCounts(prod.InputInventory, counts);
                            AddInventoryCounts(prod.OutputInventory, counts);
                        }
                    }
                }
                return counts;
            }

            private static Dictionary<Component, MyFixedPoint> GetTargetCounts(int multiplier)
            {
                var counts = new Dictionary<Component, MyFixedPoint>();
                {
                    foreach (var entry in Info)
                    {
                        var component = entry.Key;
                        var info = entry.Value;
                        counts[component] = (MyFixedPoint) Math.Ceiling(multiplier * info.Ratio);
                    }
                }
                return counts;
            }

            private static void ShowSummary(IMyTextSurface screen, IReadOnlyDictionary<Component, MyFixedPoint> counts,
                IReadOnlyDictionary<Component, MyFixedPoint> targetCounts, Component currentComponent)
            {
                screen.FontSize = .7f;
                screen.WriteText("COMPONENTS\n");

                var projected = new Dictionary<Res, MyFixedPoint>();

                foreach (var entry in Info)
                {
                    var resource = entry.Key;
                    var info = entry.Value;

                    var cursor = resource == currentComponent ? ">" : " ";
                    MyFixedPoint count;
                    counts.TryGetValue(resource, out count);
                    MyFixedPoint targetCount;
                    targetCounts.TryGetValue(resource, out targetCount);
                    var ratio = (double) count / (double) targetCount;
                    screen.WriteText($"{cursor} {info.Name,-18} {Int(count)} / {Int(targetCount)} {ProgressBar(ratio, 26)}\n", true);
                }
            }

            internal void Init()
            {
            }

            internal void Update(Config config)
            {
                var blocks = new List<IMyTerminalBlock>();
                _grid.GridTerminalSystem.SearchBlocksOfName("(bim#components)", blocks,
                    block => block.IsSameConstructAs(_grid.Me) && block is IMyTextSurface);
                var screens = blocks.Cast<IMyTextSurface>().ToList();

                var counts = GetCounts();
                var targetCounts = GetTargetCounts(config.Multiplier);
                foreach (var screen in screens)
                {
                    InitScreen(screen);
                    ShowSummary(screen, counts, targetCounts, _componentIterator.GetPrevious());
                }

                var assemblers = new List<IMyAssembler>();
                _grid.GridTerminalSystem.GetBlocksOfType(assemblers, block => block.IsSameConstructAs(_grid.Me));
                _grid.Echo($"Assemblers: {assemblers.Count}");

                if (DateTime.UtcNow < _start + TimeSpan.FromSeconds(3))
                {
                    _grid.Echo("Waiting");
                    return;
                }

                if (assemblers.Any(assembler => assembler.IsProducing))
                {
                    _grid.Echo("Producing");
                    return;
                }

                var currentComponent = _componentIterator.GetCurrent();
                var diff = targetCounts[currentComponent] - counts[currentComponent];

                if (diff <= 0)
                {
                    _grid.Echo("No diff");
                    _componentIterator.Increment();
                    return;
                }

                foreach (var assembler in assemblers)
                {
                    assembler.ClearQueue();
                }

                var info = Info[currentComponent];
                var increment = (int) diff / assemblers.Count;
                var remainder = (int) diff % assemblers.Count;
                foreach (var assembler in assemblers)
                {
                    assembler.AddQueueItem(info.Blueprint, (MyFixedPoint) increment + remainder);
                    remainder = 0;
                }

                _start = DateTime.UtcNow;
                _componentIterator.Increment();
            }
        }
    }
}
