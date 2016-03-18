using System;
using System.Collections.Generic;

namespace Haumea_Core.Game.Parsing
{
    internal class ArmyParser : IParser<RawArmy>
    {
        public RawArmy Parse(IList<string> lines)
        {
            string[] tokens = lines[0].Split(' ');
            string rtag = tokens[0];
            string ptag = tokens[1];
            int nunits = int.Parse(tokens[2]);

            return new RawArmy(rtag, ptag, nunits);
        }

        public int NLinesPerEntry { get; } = 1;
        public Modes Mode { get; } = Modes.Army;
        public string Marker { get; } = "[armies]";
    }
}

