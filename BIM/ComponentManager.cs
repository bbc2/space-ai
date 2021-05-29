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
    internal partial class Program
    {
        public class ComponentManager
        {
            private readonly MyGridProgram _grid;

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

            private class ComponentInfo
            {
                public string Name { get; }
                public double Ratio { get; }
                public MyItemType Type { get; }

                public ComponentInfo(string name, double ratio)
                {
                    Name = name;
                    Ratio = ratio;
                    Type = MyItemType.MakeComponent("TODO");
                }
            }

            readonly Dictionary<Component, ComponentInfo> Info = new Dictionary<Component, ComponentInfo>()
            {
                {Component.BulletproofGlass, new ComponentInfo("Bulletproof Glass", 2)},
                {Component.Computer, new ComponentInfo("Computer", 5)},
                {Component.Construction, new ComponentInfo("Construction", 20)},
                {Component.Detector, new ComponentInfo("Detector", .1)},
                {Component.Display, new ComponentInfo("Display", .5)},
                {Component.Explosives, new ComponentInfo("Explosives", .1)},
                {Component.Girder, new ComponentInfo("Girder", .5)},
                {Component.GravityGenerator, new ComponentInfo("Gravity Generator", .1)},
                {Component.InteriorPlate, new ComponentInfo("Interior Plate", 10)},
                {Component.LargeTube, new ComponentInfo("Large Tube", 2)},
                {Component.Medical, new ComponentInfo("Medical", .1)},
                {Component.MetalGrid, new ComponentInfo("Metal Grid", 2)},
                {Component.Motor, new ComponentInfo("Motor", 4)},
                {Component.PowerCell, new ComponentInfo("Power Cell", 1)},
                {Component.RadioCommunication, new ComponentInfo("Radio", .5)},
                {Component.Reactor, new ComponentInfo("Reactor", .5)},
                {Component.SmallTube, new ComponentInfo("Small Tube", 3)},
                {Component.SolarCell, new ComponentInfo("Solar Cell", .1)},
                {Component.SteelPlate, new ComponentInfo("Steel Plate", 40)},
                {Component.Superconductor, new ComponentInfo("Superconductor", 1)},
                {Component.Thrust, new ComponentInfo("Thrust", 5)},
                {Component.Canvas, new ComponentInfo("Canvas", .01)},
            };

            public ComponentManager(MyGridProgram grid)
            {
                _grid = grid;
            }

            internal void Init()
            {
            }

            internal void Update()
            {
                // TODO
            }
        }
    }
}