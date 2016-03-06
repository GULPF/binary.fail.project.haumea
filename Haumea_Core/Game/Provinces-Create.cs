using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

using Microsoft.Xna.Framework;

using Haumea_Core.Geometric;
using Haumea_Core.Collections;

namespace Haumea_Core.Game
{
    public partial class Provinces
    {
        // Create the world.
        public static RawProvince[] CreateRawProvinces()
        {
            // Just to make the polygon initialization a bit prettier
            Func<double, double, Vector2> V = (x, y) => new Vector2(20 * (float)x,  20 * (float)y);

            Poly[] polys = {
                new Poly(new Vector2[] { 
                    V(0, 0), V(1, 2), V(2, 2), V(3, 1), V(4, 1), V(5, 3),
                    V(7, 3), V(9, 4), V(12, 3), V(12, 1), V(9, 0), V(8, -1), V(8, -2),
                    V(6, -3), V(5, -2), V(3, -2), V(1, -1)
                }),
                new Poly(new Vector2[] {
                    V(0, 0), V(1, -1), V(3, -2), V(5, -2), V(6, -3), V(5, -4), V(5, -5), V(4, -6),
                    V(2, -6), V(0, -5), V(-2, -3), V(-2, -2), V(-3, -1), V(-2, 0)
                }),
                new Poly(new Vector2[] {
                    V(0, 0), V(1, 2), V(2, 2), V(3, 1), V(4, 1), V(5, 3),
                    V(3, 4), V(2, 4), V(0, 5), V(0, 6), V(-2, 6), V(-3, 5),
                    V(-5, 5), V(-7, 3), V(-7, 1), V(-8, 0), V(-8, -1), V(-7, -2), V(-4, -3),
                    V(0, -5), V(-2, -3), V(-2, -2), V(-3, -1), V(-2, 0)
                }),
                new Poly(new Vector2[] {
                    V(-15, -15), V(-10, -12), V(-16, -5)
                })
            };

            Provinces.RawProvince[] rawProvinces = {
                new Provinces.RawProvince(polys[0], "P1", Color.Red, 1),
                new Provinces.RawProvince(polys[1], "P2", Color.DarkGoldenrod, 2),
                new Provinces.RawProvince(polys[2], "P3", Color.Brown, 0),
                new Provinces.RawProvince(polys[3], "P4", Color.BurlyWood, 1)}; 

            return rawProvinces;
        }

        public static RawRealm[] CreateRawRealms()
        {
            return new RawRealm[] {
                new Provinces.RawRealm(new List<int> { 0, 1 }, "DAN"),
                new Provinces.RawRealm(new List<int> { 2 }   , "TEU"),
                new Provinces.RawRealm(new List<int> { 3 }   , "GOT")
            };
        }

        public static NodeGraph<int> CreateMapGraph()
        {
            IList<Connector<int>> data = new List<Connector<int>>{
                new Connector<int>(0, 1, 2),
                new Connector<int>(0, 2, 1),
                new Connector<int>(2, 1, 3),
                new Connector<int>(2, 3, 5)
            };

            NodeGraph<int> graph = new NodeGraph<int>(data, true);

            return graph;
        }
    
        private static Regex vectorRgx = new Regex(@" *\( *(\d) *, *(\d) *\) *");

        private static string[] ReadLines(StreamReader stream, int count)
        {
            string[] lines = new string[2];
            for (int i = 0; i < count; i++) {
                lines[i] = stream.ReadLine();
                if (lines[i] == null) throw new ParseException();
            }
            return lines;
        }

        private static Modes GetMode(string line, Modes current)
        {
            switch (line)
            {
            case "[provinces]": return Modes.Province;
            case "[realms]":    return Modes.Realm;
            default:            return current;
            }
        }

        private static RawProvince ParseProvince(StreamReader stream, int expectedId)
        {
            string[] lines = ReadLines(stream, 2);

            string[] tokens = lines[0].Substring(1).Split(' ');
            int id = int.Parse(tokens[0]);
            string tag = tokens[1];

            List<Vector2> vectors = new List<Vector2>();

            foreach (string vectortoken in lines[1].Split('-'))
            {
                Match match = vectorRgx.Match(vectortoken);
                vectors.Add(new Vector2(
                    int.Parse(match.Captures[0].Value),
                    int.Parse(match.Captures[1].Value)));
            }

            if (id != expectedId) throw new ParseException();

            return new RawProvince(new Poly(vectors.ToArray()), tag, Color.Black, 0);
        }

        private static RawRealm ParseRealm(StreamReader stream, int expectedId)
        {
            string[] lines = ReadLines(stream, 2);
            string[] tokens = lines[0].Substring(1).Split(' ');

            int id = int.Parse(tokens[0]);
            string tag = tokens[1];

            IList<int> provinceIds = new List<int>();

            foreach (string provincetoken in lines[1].Split(','))
            {
                provinceIds.Add(int.Parse(provincetoken.Trim()));
            }

            if (id != expectedId) throw new ParseException();

            return new RawRealm(provinceIds, tag);
        }

        public static RawGameData Parse(StreamReader stream)
        {
            Modes mode = Modes.Invalid;

            IList<RawProvince> provinces = new List<RawProvince>();
            IList<RawRealm>    realms    = new List<RawRealm>(); 

            while (!stream.EndOfStream)
            {
                string line = stream.ReadLine().Trim();
                if (line == "") continue;

                mode = GetMode(line, mode);

                switch (mode)
                {
                case Modes.Province:
                    provinces.Add(ParseProvince(stream, provinces.Count));
                    break;
                case Modes.Realm:
                    realms.Add(ParseRealm(stream, realms.Count));
                    break;
                case Modes.Invalid:
                    throw new ParseException();
                }
            }

            return new RawGameData(provinces, realms, null);
        }
            
        private enum Modes { Province, Realm, Invalid }
    }

    public class UnexpectedCallExeption : Exception {}
    public class ParseException : Exception {}
}

