using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;

namespace Haumea_Core.Game.Parsing
{
    internal enum Modes { Province, Realm, Graph, Army, Water, Invalid }

    // TODO: error handling
    // This isn't the most well designed class (it uses a lot of mutable arguments f.ex),
    // but it is fairly easy to modify which is more important.
    public static partial class GameFile
    {
        // Parses this vector notation: (x, y)
        internal static Regex VectorRgx { get; } = new Regex(@" *\( *(-?\d+) *, *(-?\d+) *\) *");

        // When adding new sub parsers, just add them here.
        public static RawGameData Parse(StreamReader stream)
        {
            var parsers = new Dictionary<Modes, ISubParser>();

            // AddSubParsers does a bit of magic, but the important thing is that the
            // list it returns will be filled with values by calling ApplyParsers().
            var rProvinces  = AddSubParser<RawProvince, ProvinceParser>(parsers);
            var rArmies     = AddSubParser<RawArmy, ArmyParser>(parsers);
            var rConnectors = AddSubParser<RawConnector, GraphParser>(parsers);
            var rWater      = AddSubParser<RawProvince, WaterParser>(parsers);
            var rRealms     = AddSubParser<RawRealm, RealmParser>(parsers);

            ApplyParsers(stream, parsers);

            // This is kinda silly since they could use the same list all the time instead,
            // but since this is only done once at startup (can even be done pre-start) I simply don't care.
            rProvinces = new List<RawProvince>(rProvinces.Union(rWater));

            return new RawGameData(rProvinces, rRealms, rConnectors, rArmies);
        }

        private static void ApplyParsers(StreamReader stream, IDictionary<Modes, ISubParser> parsers)
        {
            Modes currentMode = Modes.Invalid;

            Func<string, Modes, Modes> getMode = (line, mode) => GetMode(line, mode, parsers);

            while (!stream.EndOfStream && currentMode == Modes.Invalid)
            {
                string line = stream.ReadLine().Trim();
                if (line == "") continue;

                currentMode = GetMode(line, currentMode, parsers);    
            }

            while (!stream.EndOfStream)
            {
                currentMode = parsers[currentMode].Parse(stream, getMode);
            }            
        }

        private static IList<R> AddSubParser<R, P>(IDictionary<Modes, ISubParser> output)
            where P : IParser<R>, new()
        {
            var parser = new P();
            var list = new List<R>();
            var parseApplier = new SubParser<R>(parser, list.ToCollector());
            output.Add(parser.Mode, parseApplier);
            return list;
        }

        internal static Modes GetMode(string line, Modes currentMode, IDictionary<Modes, ISubParser> parsers)
        {
            foreach (ISubParser parser in parsers.Values)
            {
                if (line == parser.Marker) return parser.Mode;
            }
            return currentMode;
        }
    }

    internal interface ISubParser {
        Modes Parse(StreamReader stream, Func<string, Modes, Modes> getMode);
        Modes Mode { get; }
        string Marker { get; }
    }

    internal class SubParser<O> : ISubParser
    {
        private readonly IParser<O> _parser;
        private readonly ICollector<O> _collector;

        public Modes Mode
        {
            get { return _parser.Mode; }
        }

        public string Marker
        {
            get { return _parser.Marker; }
        }

        public SubParser(IParser<O> parser, ICollector<O> collector)
        {
            _parser = parser;
            _collector = collector;
        }

        public Modes Parse(StreamReader stream, Func<string, Modes, Modes> getMode) {
            while (!stream.EndOfStream)
            {
                IList<string> lines = new List<string>();
                Modes nextMode;

                while (lines.Count < _parser.NLinesPerEntry)
                {
                    string line = stream.ReadLine().Trim();
                    nextMode = getMode(line, Mode);
                    if (nextMode != Mode) return nextMode;
                    if (line != "" && !IsComment(line)) lines.Add(line);
                }

                _collector.Collect(_parser.Parse(lines));
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

