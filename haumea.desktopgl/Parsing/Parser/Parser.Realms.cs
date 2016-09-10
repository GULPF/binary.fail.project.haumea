using System;
using System.Collections.Generic;

namespace Haumea.Parsing
{
    static internal partial class Parser
    {
        public static IList<RawRealm> Realms(string text)
        {
            IList<RawRealm> realms = new List<RawRealm>();
            bool firstToken = true;
            string tag = "";

            foreach (string line in Lines(text))
            {
                if (firstToken)
                {
                    string[] tokens = line.Split(' ');
                    tag = tokens[0];    
                    firstToken = false;
                }
                else
                {
                    IList<string> provinceTags = new List<string>();
                    
                    foreach (string provincetoken in line.Split(','))
                    {
                        provinceTags.Add(provincetoken.Trim());
                    }

                    realms.Add(new RawRealm(provinceTags, tag));
                    firstToken = true;
                }
            }

            return realms;
        }
    }
}

