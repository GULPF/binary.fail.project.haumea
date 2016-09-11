using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

namespace Haumea.Parsing
{
    // TODO: error handling
    public static partial class GameFile
    {
        private static Regex _groupNameRgx = new Regex(@"^\[(.*?)\]\s*?$"); // ignores trailing spaces

        public static RawGameData Parse(TextReader stream)
        {
            IDictionary<string, string> groups = SplitToGroups(stream);
            var rProvinces  = ApplyParser(groups, "provinces", Parser.Provinces);
            var rArmies     = ApplyParser(groups, "armies",    Parser.Armies);
            var rConnectors = ApplyParser(groups, "graph",     Parser.Graph);
            var rWater      = ApplyParser(groups, "water",     Parser.Waters);
            var rRealms     = ApplyParser(groups, "realms",    Parser.Realms);

            return new RawGameData(rProvinces.Concat(rWater).ToList(), rRealms, rConnectors, rArmies);
        }

        private static IList<O> ApplyParser<O>(IDictionary<string, string> groups,
                string groupName, Func<string, IList<O>> parser)
        {
            string groupText;
            if (groups.TryGetValue(groupName, out groupText))
            {
                return parser(groupText);    
            }
            else
            {
                // Just ignore it if a group is missing. Makes testing a lot cleaner.
                return new List<O>();
            }
        }
            
        private static IDictionary<string, string> SplitToGroups(TextReader stream)
        {
            string groupName = "";

            string line;
            while ((line = stream.ReadLine()) != null)
            {
                if (!line.StartsWith("//") && line.Length > 0)
                {
                    Match match = _groupNameRgx.Match(line);
                    if (match.Success)
                    {
                        groupName = match.Groups[1].ToString();
                        break;
                    }
                    else{
                        // Fail
                    }
                }
            }

            StringBuilder groupText = new StringBuilder();
            IDictionary<string, string> groups = new Dictionary<string, string>();

            while ((line = stream.ReadLine()) != null)
            {
                line = line.Trim();
                Match match = _groupNameRgx.Match(line);

                if (match.Success)
                {
                    groups.Add(groupName, groupText.ToString());
                    groupText.Clear();

                    groupName = match.Groups[1].ToString();
                    Debug.Assert(!groups.ContainsKey(groupName), "Corrupt world file: " +
                            "group '" + groupName + "' occured more than once");
                }
                else if (!line.StartsWith("//") && line.Length > 0)
                {
                    groupText.AppendLine(line);
                }
            }

            if (groupName != "") groups.Add(groupName, groupText.ToString());

            return groups;
        }
    }
}
