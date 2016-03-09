using System;
using System.Collections.Generic;

using Haumea_Core.Collections;

namespace Haumea_Core.Game
{
    public class Units : IEntity
    {
        // A lot of things in this class might seem weird and clunky, but it's not that bad.
        // 

        private Provinces _provinces;
        private IntGUID _guid;

        public IDictionary<int, ISet<int>> ProvinceArmies { get; }
        public IDictionary<int, Army> Armies { get; }
        public IDictionary<int, ISet<Battle>> Battles { get; }
    
        public ISet<int> SelectedArmies { get; }

        // These are splitted into two because DoD.
        // In the common case, a moving army will be between two provinces.
        // To check if an army is between two provinces, only the information
        // in _orders is needed.
        private IList<ArmyOrder> _orders;
        private IList<ArmyPath> _paths;

        //private 

        public class Battle
        {
            public int Army1 { get; }
            public int Army2 { get; }
        }

        public class Army
        {
            public int Owner { get; }
            public int Location { get; set; }
            public int NUnits { get; set; }

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

        public Units(Provinces provinces, IList<RawArmy> rawArmies)
        {
            _provinces = provinces;
            _orders = new List<ArmyOrder>();
            _paths  = new List<ArmyPath>();
            _guid = new IntGUID(0);

            ProvinceArmies = new Dictionary<int, ISet<int>>();
            SelectedArmies = new HashSet<int>();
            Armies =  new Dictionary<int, Army>();

            foreach (RawArmy rawArmy in rawArmies)
            {
                int ownerID = _provinces.Realms.RealmTagIdMapping[rawArmy.Owner];
                int locationID = _provinces.TagIdMapping[rawArmy.Location];
                Units.Army army = new Units.Army(ownerID, locationID, rawArmy.NUnits);
                AddArmy(army);
            }
        }

        public void Update(WorldDate date)
        {
            if (date.IsNewDay)
            {
                for (int orderID = 0; orderID < _orders.Count; orderID++)
                {
                    ArmyOrder order = _orders[orderID];
                    order.DaysUntilNext--;

                    if (order.DaysUntilNext == 0)
                    {
                        // If the army order is completely done it will be removed from the list
                        // so we need to compensate the loop variable.
                        // TODO: This seems like a bad idea. It means that the orderID is not constant.
                        if (MoveArmy(orderID)) 
                        {
                            orderID--;
                        }
                    }
                }
            }
        }

        public void SelectArmy(int armyID, bool keepOldSelection)
        {
            if (keepOldSelection)
            {
                SelectedArmies.Add(armyID);
            }
            else
            {
                SelectedArmies.Clear();
                SelectedArmies.Add(armyID);
            }
        }

        public void ClearSelection()
        {
            SelectedArmies.Clear();
        }

        public bool AddOrder(int armyID, int destination)
        {
            Army army = Armies[armyID];

            GraphPath<int> path = _provinces.MapGraph.Dijkstra(army.Location, destination);
            if (path == null) return false;

            int daysUntilFirstMove = _provinces.MapGraph.NeighborDistance(army.Location, path.Nodes[1]);
            ArmyOrder order = new ArmyOrder(armyID, daysUntilFirstMove);
            ArmyPath armyPath = new ArmyPath(path);

            _orders.Add(order);
            _paths.Add(armyPath);
                
            return true;
        }

        public bool MergeSelected()
        {
            if (SelectedArmies.Count < 2) return false;

            using (var enumer = SelectedArmies.GetEnumerator())
            {
                enumer.MoveNext();
                int location = Armies[enumer.Current].Location;

                // We are only allowed to merge if all armies are in the same province.
                foreach (int armyID in SelectedArmies)
                {
                    if (location != Armies[armyID].Location)
                    {
                        return false;
                    }
                }
            }

            using (var enumer = SelectedArmies.GetEnumerator())
            {
                enumer.MoveNext();
                int mergedArmyID = enumer.Current;
                Army army = Armies[mergedArmyID];

                while (enumer.MoveNext())
                {
                    army.NUnits += Armies[enumer.Current].NUnits;
                    RemoveArmyFromProvince(Armies[enumer.Current].Location, enumer.Current);
                    Armies.Remove(enumer.Current);
                }

                SelectedArmies.Clear();
                SelectedArmies.Add(mergedArmyID);
            }

            return true;
        }

        private bool MoveArmy(int orderID)
        {
            ArmyPath path   = _paths[orderID];
            ArmyOrder order = _orders[orderID];
         
            // Because orders belonging to deleted armies are not removed immedietly
            // (would be expensive), we need to watch out for orders without armies.
            if (!Armies.ContainsKey(order.ArmyID)) return true;

            Army army = Armies[order.ArmyID];
            bool orderDone = false;

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
                // RemoveAt is O(n), maybe this is bad?
                _orders.RemoveAt(orderID);
                _paths.RemoveAt(orderID);
                orderDone = true;
            }

            RemoveArmyFromProvince(from, order.ArmyID);
            AddArmyToProvince(to, order.ArmyID);

            return orderDone;
        }

        public void AddArmy(Army army)
        {
            int armyID = _guid.Generate();
            Armies.Add(Armies.Count, army);
            AddArmyToProvince(army.Location, armyID);
        }

        private void RemoveArmyFromProvince(int province, int armyID)
        {
            if (ProvinceArmies[province].Count == 1)
            {
                ProvinceArmies.Remove(province);
            }
            else
            {
                ProvinceArmies[province].Remove(armyID);    
            }
        }

        private void AddArmyToProvince(int province, int armyID)
        {
            ISet<int> armies;
            if (ProvinceArmies.TryGetValue(province, out armies))
            {
                armies.Add(armyID);
            }
            else
            {
                ProvinceArmies[province] = new HashSet<int> { armyID };
            }
        }
    
        private class IntGUID
        {
            private int _nextID;

            public IntGUID(int nextID)
            {
                _nextID = nextID;
            }

            public int Generate()
            {
                return _nextID++;
            }
        }
    }

    public struct RawArmy
    {
        public string Owner { get; }
        public string Location { get; }
        public int NUnits { get; }

        public RawArmy(string owner, string location, int nUnits)
        {
            Owner = owner;
            Location = location;
            NUnits = nUnits;
        }
    }
}

