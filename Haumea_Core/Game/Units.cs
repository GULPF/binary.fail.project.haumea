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

        private Provinces _provinces;
        private WorldDate _worldDate;
        private Haumea _game;

        public Units(Provinces provinces, Haumea game)
        {
            StationedUnits = new Dictionary<int, int>();
            _provinces = provinces;
            _game = game;
            _worldDate = _worldDate;
        }

        public bool MoveUnits(int from, int to, int amount)
        {
            if (StationedUnits[from] < amount) return false;

            GraphPath<int> path = _provinces.MapGraph.Dijkstra(from, to);

            // If no path exists.
            if (path == null) return false;

            _game.AddEvent(path.Cost, delegate {
                Console.WriteLine("Units have reached destination");
            });
                
            return true;
        }

        public void AddUnits(int units, int province)
        {
            StationedUnits[province] = units;
        }
    }
}

