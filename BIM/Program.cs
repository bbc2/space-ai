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
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    internal partial class Program : MyGridProgram
    {
        private readonly StatusDisplay _statusDisplay;
        private readonly OreManager _oreManager;
        private readonly ComponentManager _componentManager;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            _statusDisplay = new StatusDisplay(screen: Me.GetSurface(0), name: "BIM");
            _statusDisplay.Init();

            _oreManager = new OreManager(grid: this);
            _oreManager.Init();

            _componentManager = new ComponentManager(this);
            _componentManager.Init();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _statusDisplay.Update();
            _oreManager.Update();
            _componentManager.Update();
        }
    }
}