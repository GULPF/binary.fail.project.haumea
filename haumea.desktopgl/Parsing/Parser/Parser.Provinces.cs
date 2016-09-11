using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Haumea.Geometric;


namespace Haumea.Parsing
{
    internal static partial class Parser
    {
        public static IList<RawProvince> Provinces(string text)
        {
            IList<RawProvince> provinces = new List<RawProvince>();

            Color color = Color.White;
            string tag = "";
            List<IPoly> polygons = new List<IPoly>();
            IPoly outline = null;
            List<IPoly> holes = new List<IPoly>();

            foreach (string line in Lines(text))
            {
                if (line.StartsWith("§"))
                {
                    if (tag != "")
                    {
                        provinces.Add(new RawProvince(polygons, tag, color, false));
                    }

                    string[] tokens = line.Split(' ');
                    tag = tokens[0].Substring(1);

                    if (!TryParseHexColor(tokens[1], out color))
                    {
                        color = Color.Black;
                        Console.Error.WriteLine("Failed to parse color: " + tokens[1]);
                    }
                }
                else
                {
                    bool isHole = line.StartsWith("\\");

                    string[] vectorTokens = isHole
                        ? line.Substring(1).Split('%')
                        : line.Split('%');
                    
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

                    if (isHole)
                    {
                        holes.Add(new Poly(vectors));
                    }
                    else
                    {
                        if (outline != null)
                        {
                            polygons.Add(CreatePoly(outline, holes));
                            holes.Clear();
                        }

                        outline = new Poly(vectors);
                    }
                }
            }

            polygons.Add(CreatePoly(outline, holes));
            provinces.Add(new RawProvince(polygons, tag, color, false));
            return provinces;
        }

        private static IPoly CreatePoly(IPoly poly, List<IPoly> holes)
        {
            if (holes.Count > 0)
            {
                return new ComplexPoly(poly,  holes.ToArray());
            }
            else
            {
                return poly;   
            }
        }
    }
}

