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
    
        private static Regex vectorRgx = new Regex(@" *\( *(-?\d+) *, *(-?\d+) *\) *");

        // It would be nice to make this work without needing a mutable field.
        // The problem is that the mode changes inside a iterator, so it's not trivial to
        // communicate the change to the caller.
        private static Modes _currentMode;

        private static IList<string> ReadLines(StreamReader stream, int count)
        {
            IList<string> lines = new List<string>();
            Modes startMode = _currentMode;

            while (lines.Count < count)
            {
                string line = stream.ReadLine().Trim();
                _currentMode = GetMode(line);
                if (_currentMode != startMode) return null;
                if (line != "") lines.Add(line);
            }
            return lines;
        }

        private static Modes GetMode(string line)
        {
            switch (line)
            {
            case "[provinces]": return Modes.Province;
            case "[realms]":    return Modes.Realm;
            case "[mapgraph]":  return Modes.MapGraph;
            default:            return _currentMode;
            }
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

        private static IEnumerable<RawProvince> ParseProvinces(StreamReader stream)
        {
            while (!stream.EndOfStream)
            {
                IList<string> lines = ReadLines(stream, 2);
                if (lines == null) yield break;

                string[] tokens = lines[0].Split(' ');
                string tag = tokens[0];
                Color color = ColorFromHex(tokens[1]);
                int units = int.Parse(tokens[2]);

                List<Vector2> vectors = new List<Vector2>();

                foreach (string vectortoken in lines[1].Split('%'))
                {
                    Match match = vectorRgx.Match(vectortoken);
                    vectors.Add(new Vector2(
                        20 * int.Parse(match.Groups[1].Value),
                        20 * int.Parse(match.Groups[2].Value)));
                    Console.WriteLine(vectors[vectors.Count - 1]);
                }

                yield return new RawProvince(new Poly(vectors.ToArray()), tag, color, units);     
            }
        }

        private static IEnumerable<RawRealm> ParseRealms(StreamReader stream)
        {
            while (!stream.EndOfStream)
            {
                IList<string> lines = ReadLines(stream, 2);
                if (lines == null) yield break;

                string[] tokens = lines[0].Split(' ');
                string tag = tokens[0];

                IList<string> provinceTags = new List<string>();

                foreach (string provincetoken in lines[1].Split(','))
                {
                    provinceTags.Add(provincetoken.Trim());
                }

                yield return new RawRealm(provinceTags, tag);   
            }
        }

        private static IEnumerable<RawConnector> ParseConnectors(StreamReader stream)
        {
            while (!stream.EndOfStream)
            {
                IList<string> lines = ReadLines(stream, 1);
                if (lines == null) yield break;

                string[] tokens = lines[0].Split(' ');
                string tag1 = tokens[0];
                string tag2 = tokens[1];
                int cost = int.Parse(tokens[2]);

                yield return new RawConnector(tag1, tag2, cost);
            }
        }

        public static RawGameData Parse(StreamReader stream)
        {
            _currentMode = Modes.Invalid;

            IList<RawProvince>  provinces  = new List<RawProvince>();
            IList<RawRealm>     realms     = new List<RawRealm>(); 
            IList<RawConnector> connectors = new List<RawConnector>();

            while (!stream.EndOfStream && _currentMode == Modes.Invalid)
            {
                string line = stream.ReadLine().Trim();
                if (line == "") continue;

                _currentMode = GetMode(line);    
            }

            while (!stream.EndOfStream)
            {
                switch (_currentMode)
                {
                case Modes.Province:
                    foreach (RawProvince prov in ParseProvinces(stream))
                    {
                        provinces.Add(prov);
                    }
                    break;
                case Modes.Realm:
                    foreach (RawRealm realm in ParseRealms(stream))
                    {
                        realms.Add((realm));
                    }
                    break;
                case Modes.MapGraph:
                    foreach (RawConnector conn in ParseConnectors(stream))
                    {
                        connectors.Add(conn);
                    }
                    break;
                case Modes.Invalid:
                    throw new ParseException();
                }
            }

            return new RawGameData(provinces, realms, connectors);
        }
            
        private enum Modes { Province, Realm, MapGraph, Invalid }
    }

    public class UnexpectedCallExeption : Exception {}
    public class ParseException : Exception {}
}

