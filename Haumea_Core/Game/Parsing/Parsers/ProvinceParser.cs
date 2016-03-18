using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Haumea_Core.Geometric;

namespace Haumea_Core.Game.Parsing
{
    internal class ProvinceParser : IParser<RawProvince>
    {
        public RawProvince Parse(IList<string> lines)
        {
            string[] tokens = lines[0].Split(' ');
            string tag = tokens[0];
            Color color = ColorFromHex(tokens[1]);

            List<Vector2> vectors = new List<Vector2>();

            foreach (string vectortoken in lines[1].Split('%'))
            {
                Match match = GameFile.VectorRgx.Match(vectortoken);
                vectors.Add(new Vector2(
                    20 * int.Parse(match.Groups[1].Value),
                    20 * int.Parse(match.Groups[2].Value)));
            }

            return new RawProvince(new Poly(vectors.ToArray()), tag, color, false); 
        }

        private static Color ColorFromHex(string hexString)
        {
            if (hexString.StartsWith("#")) hexString = hexString.Substring(1);
            uint hex = Convert.ToUInt32(hexString, 16);
            Color color = Color.White;

            if (hexString.Length != 6) 
            {
                throw new InvalidOperationException("Invalid hex representation of an RGB color value.");
            }

            color.R = (byte)(hex >> 16);
            color.G = (byte)(hex >> 8);
            color.B = (byte)(hex);

            return color;
        }

        public int NLinesPerEntry { get; } = 2;
        public Modes Mode { get; } = Modes.Province;
        public string Marker { get; } = "[provinces]";
    }
}

