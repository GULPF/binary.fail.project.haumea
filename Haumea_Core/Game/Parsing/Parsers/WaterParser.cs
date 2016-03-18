using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.Xna.Framework;

using Haumea_Core.Geometric;

namespace Haumea_Core.Game.Parsing
{
    internal class WaterParser : IParser<RawProvince>
    {
        private static readonly Color WaterColor = Color.Blue.Lighten();

        public RawProvince Parse(IList<string> lines)
        {
            string tag = lines[0];

            List<Vector2> vectors = new List<Vector2>();

            foreach (string vectortoken in lines[1].Split('%'))
            {
                Match match = GameFile.VectorRgx.Match(vectortoken);
                vectors.Add(new Vector2(
                    20 * int.Parse(match.Groups[1].Value),
                    20 * int.Parse(match.Groups[2].Value)));
            }

            return new RawProvince(new Poly(vectors.ToArray()), tag, WaterColor, true);   
        }

        public int NLinesPerEntry { get; } = 2;
        public Modes Mode { get; } = Modes.Water;
        public string Marker { get; } = "[water]";
    }
}

