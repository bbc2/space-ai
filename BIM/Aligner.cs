using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    internal partial class Program
    {
        public class Aligner
        {
            private static readonly Dictionary<char, int> Widths = new Dictionary<char, int>
            {
                {' ', 15},
                {':', 25},
                {'A', 37},
                {'B', 37},
                {'C', 35},
                {'D', 37},
                {'E', 34},
                {'F', 32},
                {'G', 36},
                {'H', 35},
                {'I', 24},
                {'J', 31},
                {'K', 33},
                {'L', 30},
                {'M', 42},
                {'N', 37},
                {'O', 37},
                {'P', 35},
                {'Q', 37},
                {'R', 37},
                {'S', 37},
                {'T', 32},
                {'U', 36},
                {'V', 35},
                {'W', 47},
                {'X', 35},
                {'Y', 36},
                {'Z', 35},
                {'[', 25},
                {'\\', 28},
                {']', 25},
                {'^', 34},
                {'_', 31},
                {'`', 23},
                {'a', 33},
                {'b', 33},
                {'c', 32},
                {'d', 33},
                {'e', 33},
                {'f', 24},
                {'g', 33},
                {'h', 33},
                {'i', 23},
                {'j', 23},
                {'k', 32},
                {'l', 23},
                {'m', 42},
                {'n', 33},
                {'o', 33},
                {'p', 33},
                {'q', 33},
                {'r', 25},
                {'s', 33},
                {'t', 25},
                {'u', 33},
                {'v', 30},
                {'w', 42},
                {'x', 31},
                {'y', 33},
                {'z', 31},
            };

            public static int StringLength(string str)
            {
                return str.Sum(chr => Widths[chr]);
            }

            public static string Pad(string str, int length)
            {
                var spaceWidth = Widths[' '];
                var diff = length * spaceWidth - StringLength(str);
                if (diff < 0)
                {
                    return str;
                }

                var spaceCount = (int) Math.Round((float) diff / Widths[' ']);
                return str + new string(' ', spaceCount);
            }

            public static string PadMono(string str, int length)
            {
                if (str.Length > length)
                {
                    return str;
                }

                return str + new string(' ', length - str.Length);
            }
        }
    }
}