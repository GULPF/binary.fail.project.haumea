using System;
using System.Collections.Generic;

namespace Haumea.Parsing
{
    static internal partial class Parser
    {
        public static IList<RawConnector> Graph(string text)
        {
            IList<RawConnector> connectors = new List<RawConnector>();

            foreach (string line in Lines(text))
            {
                string[] tokens = line.Split(' ');
                string tag1 = tokens[0];
                string tag2 = tokens[1];
                int cost = int.Parse(tokens[2]);

                connectors.Add(new RawConnector(tag1, tag2, cost)); 
            }

            return connectors;
        }
    }
}

