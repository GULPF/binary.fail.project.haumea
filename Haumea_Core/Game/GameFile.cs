using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

using Microsoft.Xna.Framework;

using Haumea_Core.Geometric;

namespace Haumea_Core.Game
{
    // TODO: error handling
    public static class GameFile
    {
        // Parses this vector notation: (x, y)
        private static Regex vectorRgx = new Regex(@" *\( *(-?\d+) *, *(-?\d+) *\) *");

        private static Modes GetMode(string line, Modes currentMode)
        {
            switch (line)
            {
            case "[provinces]": return Modes.Province;
            case "[realms]":    return Modes.Realm;
            case "[mapgraph]":  return Modes.MapGraph;
            case "[armies]":    return Modes.Army;
            default:            return currentMode;
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

        private static RawProvince ParseProvince (IList<string> lines)
        {
            string[] tokens = lines[0].Split(' ');
            string tag = tokens[0];
            Color color = ColorFromHex(tokens[1]);

            List<Vector2> vectors = new List<Vector2>();

            foreach (string vectortoken in lines[1].Split('%'))
            {
                Match match = vectorRgx.Match(vectortoken);
                vectors.Add(new Vector2(
                    20 * int.Parse(match.Groups[1].Value),
                    20 * int.Parse(match.Groups[2].Value)));
            }

            return new RawProvince(new Poly(vectors.ToArray()), tag, color);  
        }

        private static RawRealm ParseRealm(IList<string> lines)
        {
            string[] tokens = lines[0].Split(' ');
            string tag = tokens[0];

            IList<string> provinceTags = new List<string>();

            foreach (string provincetoken in lines[1].Split(','))
            {
                provinceTags.Add(provincetoken.Trim());
            }

            return new RawRealm(provinceTags, tag);   
        }

        private static RawConnector ParseConnector(IList<string> lines)
        {
            string[] tokens = lines[0].Split(' ');
            string tag1 = tokens[0];
            string tag2 = tokens[1];
            int cost = int.Parse(tokens[2]);

            return new RawConnector(tag1, tag2, cost);
        }

        private static RawArmy ParseArmy(IList<string> lines)
        {
            string[] tokens = lines[0].Split(' ');
            string rtag = tokens[0];
            string ptag = tokens[1];
            int nunits = int.Parse(tokens[2]);

            return new RawArmy(rtag, ptag, nunits);
        }

        public static RawGameData Parse(StreamReader stream)
        {
            Modes currentMode = Modes.Invalid;

            var provinceParser  = new SubParser<RawProvince> (2, ParseProvince,  Modes.Province);
            var realmParser     = new SubParser<RawRealm>    (2, ParseRealm,     Modes.Realm);
            var connectorParser = new SubParser<RawConnector>(1, ParseConnector, Modes.MapGraph);
            var armyParser      = new SubParser<RawArmy>     (1, ParseArmy,      Modes.Army);

            IList<ISubParser> parsers = new List<ISubParser> {
                provinceParser, realmParser, connectorParser, armyParser
            };

            while (!stream.EndOfStream && currentMode == Modes.Invalid)
            {
                string line = stream.ReadLine().Trim();
                if (line == "") continue;

                currentMode = GetMode(line, currentMode);    
            }

            while (!stream.EndOfStream)
            {
                foreach (ISubParser parser in parsers)
                {
                    if (parser.Mode == currentMode)
                    {
                        currentMode = parser.Parse(stream);
                        break;
                    }
                }
            }

            return new RawGameData(provinceParser.Output, realmParser.Output,
                connectorParser.Output, armyParser.Output);
        }

        private enum Modes { Province, Realm, MapGraph, Army, Invalid }

        private interface ISubParser {
            Modes Mode { get; }
            Modes Parse(StreamReader Stream);
        }

        private class SubParser<O> : ISubParser
        {
            private readonly int _nLines;
            private readonly Func<IList<string>, O> _parseLines;
            public IList<O> Output { get; }
            public Modes Mode { get; }

            public SubParser(int nLines, Func<IList<string>, O> parseLines, Modes mode)
            {
                _nLines = nLines;
                _parseLines = parseLines;
                Output = new List<O>();
                Mode = mode;
            }

            public Modes Parse(StreamReader stream) {
                while (!stream.EndOfStream)
                {
                    IList<string> lines = new List<string>();
                    Modes nextMode;

                    while (lines.Count < _nLines)
                    {
                        string line = stream.ReadLine().Trim();
                        nextMode = GetMode(line, Mode);
                        if (nextMode != Mode) return nextMode;
                        if (line != "") lines.Add(line);
                    }

                    Output.Add(_parseLines(lines));
                }

                return Modes.Invalid;
            }
        }
    }

    public class ParseException : Exception {}

    public struct RawConnector
    {
        public string Tag1 { get; }
        public string Tag2 { get; }
        public int Cost { get; }

        public RawConnector(string tag1, string tag2, int cost)
        {
            Tag1 = tag1;
            Tag2 = tag2;
            Cost = cost;
        }
    }

    public struct RawGameData
    {
        public IList<RawProvince> RawProvinces { get; }
        public IList<RawRealm> RawRealms { get; }
        public IList<RawConnector> RawConnectors { get; }
        public IList<RawArmy> RawArmies { get; }

        public RawGameData(IList<RawProvince> rawProvinces,
            IList<RawRealm> rawRealms, IList<RawConnector> rawConnectors, IList<RawArmy> rawArmies)
        {
            RawProvinces = rawProvinces;
            RawRealms = rawRealms;
            RawConnectors = rawConnectors;
            RawArmies = rawArmies;
        }
    }
}

