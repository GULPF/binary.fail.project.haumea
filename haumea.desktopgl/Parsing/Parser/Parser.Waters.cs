using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Haumea.Geometric;

namespace Haumea.Parsing
{
    internal static partial class Parser
    {
        private static readonly Color WaterColor = Color.Blue.Lighten();

        public static IList<RawProvince> Waters(string text)
        {
            IList<RawProvince> waterProvinces = new List<RawProvince>();
            bool firstToken = true;
            string tag = "";

            foreach (string line in Lines(text))
            {
                if (firstToken)
                {
                    tag = line;    
                    firstToken = false;
                }
                else
                {
                    string[] vectorTokens = line.Split('%');
                    Vector2[] vectors = new Vector2[vectorTokens.Length];
                    int index = 0;

                    foreach (string vectorToken in vectorTokens)
                    {
                        Match match = VectorRgx.Match(vectorToken);
                        int x = 0, y = 0;

                        if (!int.TryParse(match.Groups[1].Value, out x) ||
                            !int.TryParse(match.Groups[2].Value, out y))
                        {
                            Console.Error.WriteLine("Failed to parse coordinate '" + vectorToken +
                                "' belonging to '" + tag + "'");
                        }

                        vectors[index++] = new Vector2(20 * x, 20 *y);
                    }

                    var list = new List<IPoly> { new Poly(vectors) };
                    waterProvinces.Add(new RawProvince(list, tag, WaterColor, true));
                    firstToken = true;
                }
            }

            return waterProvinces;
        }
    }
}

