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

        // Currently active orders.
        // public IList<ArmyOrder> Orders { get; }

        // Maps province => position to render unit sprite
        public IDictionary<int, Vector2> RenderPosition { get; }

        private Provinces _provinces;
        private Haumea _game;

        public Units(Provinces provinces, Haumea game)
        {
            _provinces = provinces;
            _game = game;

            StationedUnits = new Dictionary<int, int>();
            //Orders = new List<ArmyOrder>();
        }

        public void Update()
        {
            if (_provinces.Selected > -1 && _provinces.LastSelected > -1)
            {
                MoveUnits(_provinces.LastSelected, _provinces.Selected, StationedUnits[_provinces.LastSelected]);
                _provinces.ClearSelection();
            }
        }

        public bool MoveUnits(int from, int to, int amount)
        {
            if (StationedUnits[from] < amount) return false;

            GraphPath<int> path = _provinces.MapGraph.Dijkstra(from, to);
            if (path == null) return false;

            ArmyOrder order = new ArmyOrder(amount, path);

            ExecOrder(order);
                
            return true;
        }

        // RESEARCH: So... is this bad? It's not DoD, but maybe it's fine anyway?
        private void ExecOrder(ArmyOrder order)
        {
            var path = order.Path;
            int from = path.Nodes[order.PathIndex];
            int to   = path.Nodes[order.PathIndex + 1];

            int cost = _provinces.MapGraph.NeighborDistance(from, to);

            _game.AddEvent(cost, delegate {
                StationedUnits[from] -= order.NUnits;
                StationedUnits[to]   += order.NUnits;
                order.PathIndex++;

                if (order.PathIndex < path.NJumps - 1)
                {
                    ExecOrder(order);
                }
            });
        }

        public void AddUnits(int units, int province)
        {
            StationedUnits[province] = units;
        }
    
        private struct ArmyOrder
        {
            public int NUnits { get; }
            public GraphPath<int> Path { get; }
            public int PathIndex { get; set; }

            public ArmyOrder(int nUnits, GraphPath<int> path)
            {
                NUnits = nUnits;
                Path = path;
                PathIndex = 0;
            }
        }
    }
}

