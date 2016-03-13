using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

using Microsoft.Xna.Framework;

using Haumea_Core.Geometric;

namespace Haumea_Core.Game.Parsing
{
    // TODO: error handling
    public static class GameFile
    {
        // Parses this vector notation: (x, y)
        private static Regex vectorRgx = new Regex(@" *\( *(-?\d+) *, *(-?\d+) *\) *");

        public static RawGameData Parse(StreamReader stream)
        {
            Modes currentMode = Modes.Invalid;

            var provinceParser  = new SubParser<RawProvince> (2, ParseProvince);
            var realmParser     = new SubParser<RawRealm>    (2, ParseRealm);
            var connectorParser = new SubParser<RawConnector>(1, ParseConnector);
            var armyParser      = new SubParser<RawArmy>     (1, ParseArmy);

            IDictionary<Modes, ISubParser> parsers = new Dictionary<Modes, ISubParser> {
                { Modes.Province, provinceParser  },
                { Modes.Realm,    realmParser     },
                { Modes.MapGraph, connectorParser },
                { Modes.Army,     armyParser      }
            };

            while (!stream.EndOfStream && currentMode == Modes.Invalid)
            {
                string line = stream.ReadLine().Trim();
                if (line == "") continue;

                currentMode = GetMode(line, currentMode);    
            }

            while (!stream.EndOfStream)
            {
                currentMode = parsers[currentMode].Parse(stream, currentMode);
            }

            return new RawGameData(provinceParser.Output, realmParser.Output,
                connectorParser.Output, armyParser.Output);
        }
            
        internal static Modes GetMode(string line, Modes currentMode)
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
    }

    internal enum Modes { Province, Realm, MapGraph, Army, Invalid }

    internal interface ISubParser {
        Modes Parse(StreamReader Stream, Modes mode);
    }

    internal class SubParser<O> : ISubParser
    {
        public int NLines { get; }
        private readonly Func<IList<string>, O> _doParse;
        public IList<O> Output { get; }

        public SubParser(int nLines, Func<IList<string>, O> doParse)
        {
            _doParse = doParse;

            NLines = nLines;
            Output = new List<O>();
        }

        public Modes Parse(StreamReader stream, Modes currentMode) {
            while (!stream.EndOfStream)
            {
                IList<string> lines = new List<string>();
                Modes nextMode;

                while (lines.Count < NLines)
                {
                    string line = stream.ReadLine().Trim();
                    nextMode = GameFile.GetMode(line, currentMode);
                    if (nextMode != currentMode) return nextMode;
                    if (line != "" && !IsComment(line)) lines.Add(line);
                }

                Output.Add(_doParse(lines));
            }

            return Modes.Invalid;
        }

        private static bool IsComment(string line)
        {
            return line[0] == '/' && line[1] == '/';
        }
    }

    public class ParseException : Exception {}
}

