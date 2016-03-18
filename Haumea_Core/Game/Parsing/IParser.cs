using System;
using System.Collections.Generic;

namespace Haumea_Core.Game.Parsing
{
    internal interface IParser<O> {
        int NLinesPerEntry { get; }
        Modes Mode { get; }
        String Marker { get; }

        O Parse(IList<string> lines);
    }
}

