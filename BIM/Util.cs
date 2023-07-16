using System;
using VRage;

namespace IngameScript
{
    internal partial class Program
    {
        private static string Float(MyFixedPoint value)
        {
            return $"{(float) value,13:F3}";
        }

        private static string Int(MyFixedPoint value)
        {
            return $"{(int) value,7}";
        }

        private static string ProgressBar(double ratio, int width)
        {
            var filled = (int) Math.Floor(width * ratio);
            var empty = width - filled;

            if (empty < 0)
            {
                return $"[{new string('=', width - 1)}+]";
            }
            else
            {
                return $"[{new string('=', filled)}{new string('_', empty)}]";
            }
        }
    }
}