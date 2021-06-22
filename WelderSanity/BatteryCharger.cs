using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Common.ObjectBuilders;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    internal partial class Program
    {
        public class BatteryCharger
        {
            private readonly MyGridProgram _program;

            public BatteryCharger(MyGridProgram program)
            {
                _program = program;
            }

            public void Init()
            {
            }

            internal void Update()
            {
                var connectors = new List<IMyShipConnector>();
                _program.GridTerminalSystem.GetBlocksOfType(connectors, block => block.IsSameConstructAs(_program.Me));

                var connected = connectors.Any(connector => connector.Status == MyShipConnectorStatus.Connected);

                var batteries = new List<IMyBatteryBlock>();
                _program.GridTerminalSystem.GetBlocksOfType(batteries, block => block.IsSameConstructAs(_program.Me));

                var orderedBatteries =
                    batteries.OrderByDescending(battery => battery.CurrentStoredPower);
                var highestBattery = orderedBatteries.First();
                var lowBatteries = orderedBatteries.Skip(1);

                var chargeMode = connected ? ChargeMode.Recharge : ChargeMode.Auto;
                _program.Echo($"Connected: {connected}");

                highestBattery.ChargeMode = ChargeMode.Auto;
                foreach (var battery in lowBatteries)
                {
                    battery.ChargeMode = chargeMode;
                }
            }
        }
    }
}