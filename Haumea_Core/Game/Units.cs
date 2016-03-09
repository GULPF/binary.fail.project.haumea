using System;
using System.Collections.Generic;

using Haumea_Core.Collections;

namespace Haumea_Core.Game
{
    public class Units : IEntity
    {
        // A lot of things in this class might seem weird and clunky, but it's not that bad.

        private Provinces _provinces;
        private IntGUID _guid;

        public IDictionary<int, ISet<int>> ProvinceArmies { get; }
        public IDictionary<int, Army> Armies { get; }
        public IDictionary<int, ISet<Battle>> Battles { get; }
    
        public ISet<int> SelectedArmies { get; }

        public class Battle
        {
            public int Army1 { get; }
            public int Army2 { get; }
        }

        // TODO: It's really bad that this class is exposed & mutable.
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
            public GraphPath<int> Path { get; }
            public int PathIndex { get; private set; }

            public int CurrentNode
            {
                get { return Path.Nodes[PathIndex]; }
            }

            public int NextNode
            {
                get { return Path.Nodes[PathIndex + 1]; }
            }

            public bool MoveForward()
            {
                PathIndex++;
                return PathIndex + 1 < Path.NJumps;
            }

            public ArmyOrder(int armyID, GraphPath<int> path)
            {
                ArmyID = armyID;
                Path = path;
                PathIndex = 0;
            }
        }

        public Units(Provinces provinces, IList<RawArmy> rawArmies)
        {
            _provinces = provinces;
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
            ArmyOrder order = new ArmyOrder(armyID, path);

            Action moveUnit = null; // need to initialize it twice due to recursion below
            moveUnit = delegate {
                RemoveArmyFromProvince(order.CurrentNode, order.ArmyID);
                AddArmyToProvince(order.NextNode, order.ArmyID);
                Armies[order.ArmyID].Location = order.NextNode;

                if (order.MoveForward())
                {
                    int daysUntilNextMove = _provinces.MapGraph.NeighborDistance(order.CurrentNode, order.NextNode);
                    EventController.Instance.AddEvent(daysUntilNextMove, moveUnit);    
                }
            };

            EventController.Instance.AddEvent(daysUntilFirstMove, moveUnit);

            return true;
        }

        public bool MergeSelected()
        {
            if (SelectedArmies.Count < 2 || !IsValidMerge()) return false;

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

        public void AddArmy(Army army)
        {
            int armyID = _guid.Generate();
            Armies.Add(Armies.Count, army);
            AddArmyToProvince(army.Location, armyID);
        }

        private bool IsValidMerge()
        {
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

            return true;
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

