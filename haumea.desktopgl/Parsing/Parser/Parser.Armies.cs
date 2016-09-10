using System;
using System.Collections.Generic;

namespace Haumea.Parsing
{
    internal static partial class Parser
    {
        public static IList<RawArmy> Armies(string text)
        {
            IList<RawArmy> armies = new List<RawArmy>();

            foreach (string line in Lines(text))
            {
                string[] tokens = line.Split(' ');
                string rtag = tokens[0];
                string ptag = tokens[1];
                int nunits = int.Parse(tokens[2]);

                armies.Add(new RawArmy(rtag, ptag, nunits));
            }

            return armies;
        }
    }
}

