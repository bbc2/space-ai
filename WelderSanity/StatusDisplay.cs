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
using Sandbox.Common.ObjectBuilders.Definitions;
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
        public class StatusDisplay
        {
            private readonly IMyTextSurface _screen;
            private long _count;
            private readonly string _name;

            public StatusDisplay(IMyTextSurface screen, string name)
            {
                _screen = screen;
                _name = name;
            }

            public void Init()
            {
                _screen.ContentType = ContentType.TEXT_AND_IMAGE;
                _screen.Font = "Monospace";
                _screen.Alignment = TextAlignment.CENTER;
                _screen.FontSize = 1.2f;
                _count = 0;
            }

            private int Triphased(int phase)
            {
                var sin = Math.Sin(((float) _count / 16 + (float) phase / 3) * 2 * Math.PI);
                return (int) Math.Floor((sin + 1) / 2 * 256);
            }

            public void Update()
            {
                _screen.WriteText($"\n\n{_name}\n\n{_count:X}\n");
                _screen.FontColor = new Color(Triphased(0), Triphased(1), Triphased(2));
                _count++;
            }
        }
    }
}