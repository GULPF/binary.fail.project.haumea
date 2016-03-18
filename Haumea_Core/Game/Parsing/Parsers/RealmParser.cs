using System;
using System.Collections.Generic;

namespace Haumea_Core.Game.Parsing
{
    internal class RealmParser : IParser<RawRealm>
    {
        RawRealm IParser<RawRealm>.Parse(IList<string> lines)
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

        public int NLinesPerEntry { get; } = 2;
        public Modes Mode { get; } = Modes.Realm;
        public string Marker { get; } = "[realms]";
    }
}

