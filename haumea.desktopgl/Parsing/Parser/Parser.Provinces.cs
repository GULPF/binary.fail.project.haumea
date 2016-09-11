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

            ProvinceBuilder provBuilder = new ProvinceBuilder();
            PolygonBuilder  polyBuilder = new PolygonBuilder();

            foreach (string line in Lines(text))
            {
                Vector2[] vectors;

                switch (line[0])
                {
                case '§':  // New province
                    if (provBuilder.Tag != "")
                    {
                        provBuilder.Polys.Add(polyBuilder.Build());
                        provinces.Add(provBuilder.Build());
                    }

                    string[] tokens = line.Split(' ');
                    provBuilder.Tag = tokens[0].Substring(1);

                    if (!TryParseHexColor(tokens[1], out provBuilder.Color))
                    {
                        Console.Error.WriteLine("Failed to parse color: " + tokens[1]);
                    }
                    break;
                case '\\': // New hole to existing polygon
                    vectors = ParseVectors(line.Substring(1));
                    polyBuilder.Holes.Add(new Poly(vectors));
                    break;
                default:   // New polygon
                    vectors = ParseVectors(line);

                    if (polyBuilder.Outline != null)
                    {
                        provBuilder.Polys.Add(polyBuilder.Build());
                    }

                    polyBuilder.Outline = new Poly(vectors);
                    break;
                }
            }

            provBuilder.Polys.Add(polyBuilder.Build());
            provinces.Add(provBuilder.Build());
            return provinces;
        }

        private static Vector2[] ParseVectors(string text)
        {
            string[] vectorTokens = text.Split('%');
            Vector2[] vectors = new Vector2[vectorTokens.Length];
            int index = 0;

            foreach (string vectorToken in vectorTokens)
            {
                Match match = VectorRgx.Match(vectorToken);
                int x = 0, y = 0;

                if (!int.TryParse(match.Groups[1].Value, out x) ||
                    !int.TryParse(match.Groups[2].Value, out y))
                {
                    Console.Error.WriteLine("Failed to parse coordinate '" + vectorToken + "'");
                }

                vectors[index++] = new Vector2(20 * x, 20 *y);
            }

            return vectors;
        }

        private class ProvinceBuilder
        {
            public String Tag;
            public List<IPoly> Polys;
            public Color Color;

            public ProvinceBuilder()
            {
                Reset();
            }

            public RawProvince Build()
            {
                var res = new RawProvince(Polys, Tag, Color, false);
                Reset();
                return res;
            }

            private void Reset()
            {
                Tag = "";
                Polys = new List<IPoly>();
                Color = Color.CornflowerBlue;
            }
        }

        private class PolygonBuilder
        {
            public IPoly Outline;
            public List<IPoly> Holes;

            public PolygonBuilder()
            {
                Reset();
            }

            public IPoly Build()
            {
                var res = CreatePoly(Outline, Holes);
                Reset();
                return res;
            }

            private void Reset()
            {
                Holes = new List<IPoly>();
                Outline = null;
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
}

