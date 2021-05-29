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
    internal partial class Program : MyGridProgram
    {
        private readonly StatusDisplay _statusDisplay;
        private readonly InventoryManager _inventoryManager;
        private readonly BatteryCharger _batteryCharger;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            _statusDisplay = new StatusDisplay(Me.GetSurface(0), name: "WelderSanity");
            _statusDisplay.Init();
            _inventoryManager = new InventoryManager(this);
            _inventoryManager.Init();
            _batteryCharger = new BatteryCharger(this);
            _batteryCharger.Init();
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _statusDisplay.Update();
            _inventoryManager.Update(argument);
            _batteryCharger.Update();
        }
    }
}