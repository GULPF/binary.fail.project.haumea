using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Haumea_Core.Collections;

namespace Haumea_Core.Game
{
    public class Units
    {
        // Maps province => units
        public IDictionary<int, int> StationedUnits { get; }

        // Maps province => position to render unit sprite
        public IDictionary<int, Vector2> RenderPosition { get; }

        // Graph over how the provinces are connected,
        // with a distance value assigned to every connection.
        public NodeGraph<int> MapGraph { get; }

        //public event UnitsArrived

        public Units(NodeGraph<int> mapGraph)
        {
            StationedUnits = new Dictionary<int, int>();
            MapGraph = mapGraph;
        }

        public bool MoveUnits(int from, int to, int amount)
        {
            GraphPath<int> path = MapGraph.Dijkstra(from, to);

            // If no path exists.
            if (path == null) return false;

            // TODO: some kind of event?

            return true;
        }
    }
}

