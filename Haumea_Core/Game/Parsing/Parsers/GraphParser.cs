namespace Haumea_Core.Game.Parsing
{
    internal class GraphParser : IParser<RawConnector>
    {
        public RawConnector Parse(System.Collections.Generic.IList<string> lines)
        {
            string[] tokens = lines[0].Split(' ');
            string tag1 = tokens[0];
            string tag2 = tokens[1];
            int cost = int.Parse(tokens[2]);

            return new RawConnector(tag1, tag2, cost);
        }

        public int NLinesPerEntry { get; } = 1;
        public Modes Mode { get; } = Modes.Graph;
        public string Marker { get; } = "[graph]";
    }
}

