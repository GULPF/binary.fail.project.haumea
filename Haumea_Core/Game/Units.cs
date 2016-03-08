using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Haumea_Core.Collections;

namespace Haumea_Core.Game
{
    public class Units
    {
        // This is the new system (should comply more with DoD).
        // When arriving to province <p> (or after battle), an army has to take the following steps:
        //      1. O(1) If OccupiedProvinces[<p>].Count == 0, Done
        //      2. O(n) Search Battles[<p>] for battles that can be joined. If found, join then Done.
        //      3. O(n) Search OccupiedProvinces[<p>] for enemy. If found, attack then Done.
        //      4. O(1) Update OccupiedProvinces[<p>] then Done.

        // _orders is checked and updated every tick.

        // This __should__ be good enough. There is a couple of O(n) in there, but the n is small.

        public IDictionary<int, IList<int>> ProvinceArmies { get; }
        public IList<Army> Armies { get; }
        public IDictionary<int, IList<Battle>> Battles { get; }
    
        public IList<int> SelectedArmies { get; }

        // These are splitted into two because DoD.
        // In the common case, a moving army will be between two provinces.
        // To check if an army is between two provinces, only the information
        // in _orders is needed.
        private IList<ArmyOrder> _orders;
        private IList<ArmyPath> _paths;

        public class Battle
        {
            public int Army1 { get; }
            public int Army2 { get; }
        }

        public class Army
        {
            public int Owner { get; }
            public int Location { get; set; }
            public int NUnits { get; }

            public Army(int owner, int location, int nUnits)
            {
                Owner = owner;
                Location = location;
                NUnits = nUnits;
            }
        }

        private class ArmyOrder
        {
            public int ArmyID { get; }
            public int DaysUntilNext { get; set; }

            public ArmyOrder(int armyID, int daysUntilNext)
            {
                ArmyID = armyID;
                DaysUntilNext = daysUntilNext;
            }
        }

        private class ArmyPath
        {
            public GraphPath<int> Path { get; }
            public int PathIndex { get; set; }

            public ArmyPath(GraphPath<int> path)
            {
                Path = path;
                PathIndex = 0;
            }
        }

        private Provinces _provinces;
        private Haumea _game;

        public Units(Provinces provinces, Haumea game)
        {
            _provinces = provinces;
            _game = game;
            _orders = new List<ArmyOrder>();
            _paths  = new List<ArmyPath>();

            ProvinceArmies = new Dictionary<int, IList<int>>();
            SelectedArmies = new List<int>();
            Armies =  new List<Army>();
        }

        public void Update()
        {
            if (_game.IsNewDay)
            {
                for (int orderID = 0; orderID < _orders.Count; orderID++)
                {
                    ArmyOrder order = _orders[orderID];
                    order.DaysUntilNext--;
                    //Console.WriteLine(order.DaysUntilNext);
                    if (order.DaysUntilNext == 0)
                    {
                        ArmyPath path = _paths[orderID];
                        Army army = Armies[order.ArmyID];

                        int from = path.Path.Nodes[path.PathIndex];
                        int to   = path.Path.Nodes[path.PathIndex + 1];

                        army.Location = to;

                        if (path.Path.Nodes.Count > path.PathIndex + 2)
                        {
                            path.PathIndex++;

                            order.DaysUntilNext = _provinces.MapGraph.NeighborDistance(
                                path.Path.Nodes[path.PathIndex], path.Path.Nodes[path.PathIndex + 1]);    
                        }
                        else
                        {
                            _orders.RemoveAt(orderID);
                            _paths.RemoveAt(orderID);
                        }


                        if (ProvinceArmies[from].Count == 1)
                        {
                            ProvinceArmies.Remove(from);
                        }
                        else
                        {
                            ProvinceArmies[from].Remove(order.ArmyID);    
                        }

                        if (ProvinceArmies.ContainsKey(to))
                        {
                            ProvinceArmies[to].Add(order.ArmyID);
                        }
                        else
                        {
                            ProvinceArmies[to] = new List<int> { order.ArmyID };
                        }
                    }
                } 
            }
        }

        public void SelectArmy(int provinceID, bool keepOldSelection)
        {
            // We asume that the province actually contains an army.
            IList<int> armies = ProvinceArmies[provinceID];

            if (keepOldSelection)
            {
                foreach (int armyID in armies)
                {
                    if (SelectedArmies.IndexOf(armyID) > -1)
                    {
                        SelectedArmies.Add(armyID);
                    }
                }    
            }
            else
            {
                SelectedArmies.Clear();
                SelectedArmies.Add(armies[0]);
            }
        }

        public void ClearSelection()
        {
            SelectedArmies.Clear();
        }

        public bool MoveUnits(int armyID, int destination)
        {
            Army army = Armies[armyID];

            GraphPath<int> path = _provinces.MapGraph.Dijkstra(army.Location, destination);
            if (path == null) return false;


            int daysUntilFirstMove = _provinces.MapGraph.NeighborDistance(army.Location, path.Nodes[1]);
            ArmyOrder order = new ArmyOrder(armyID, daysUntilFirstMove);
            ArmyPath armyPath = new ArmyPath(path);
            Console.WriteLine(daysUntilFirstMove);
            _orders.Add(order);
            _paths.Add(armyPath);
                
            return true;
        }

        // RESEARCH: So... is this bad? It's not DoD, but maybe it's fine anyway?
        /*private void ExecOrder(ArmyOrder order)
        {
            var path = order.Path;
            int from = path.Nodes[order.PathIndex];
            int to   = path.Nodes[order.PathIndex + 1];

            int cost = _provinces.MapGraph.NeighborDistance(from, to);

            _game.AddEvent(cost, delegate {
//                StationedUnits[from] -= order.NUnits;
//                StationedUnits[to]   += order.NUnits;
                order.PathIndex++;

                if (order.PathIndex < path.NJumps - 1)
                {
                    ExecOrder(order);
                }
            });
        }
*/
        public void AddArmy(Army army)
        {
            int id = Armies.Count;
            Armies.Add(army);

            if (ProvinceArmies.ContainsKey(army.Location))
            {
                ProvinceArmies[army.Location].Add(id);
            }
            else 
            {
                ProvinceArmies[army.Location] = new List<int> { id };
            }
        }
    }
}

