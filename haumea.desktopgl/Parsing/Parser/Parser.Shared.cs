using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace Haumea.Parsing
{
    internal static partial class Parser
    {
        // Parses this vector notation: (x, y)
        private static Regex VectorRgx { get; } = new Regex(@" *\( *(-?\d+) *, *(-?\d+) *\) *");

        private static IEnumerable<string> Lines(string text)
        {
            if (text.Length == 0) yield break;

            foreach (string line in text.Split(Environment.NewLine))
            {
                if (line.Length > 0) yield return line;
            }
        }

        private static bool TryParseHexColor(string hexString, out Color color)
        {
            if (hexString.StartsWith("#")) hexString = hexString.Substring(1);
            uint hex = Convert.ToUInt32(hexString, 16);

            if (hexString.Length != 6) 
            {
                color = default(Color);
                return false;
            }

            byte r = (byte)(hex >> 16);
            byte g = (byte)(hex >> 8);
            byte b = (byte)(hex);
            color = new Color(r, g, b);

            return true;
        }
    }
}

