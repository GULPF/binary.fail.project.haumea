using System;
using System.Collections.Generic;

using Haumea.Collections;

namespace Haumea.Components
{
    public class Units : IModel
    {
        // A lot of things in this class might seem weird and clunky, but it's not that bad.

        private readonly Provinces _provinces;
        private readonly IDGenerator _guid;
        private readonly EventController _events;

        private readonly IDictionary<int, ISet<int>> _loadableNavies;

        /// <summary>
        /// Keeps track of which armies are located in which province.
        /// </summary>
        public IDictionary<int, ISet<int>> ProvinceArmies { get; }

        /// <summary>
        /// (ID, ARMY) pairs for all armies.
        /// </summary>
        public IDictionary<int, Army> Armies { get; }

        public IDictionary<int, ISet<Battle>> Battles { get; }
    
        /// <summary>
        /// Contains the selected armies.
        /// </summary>
        public ISet<int> SelectedArmies { get; }

        public Units(Provinces provinces, EventController events)
        {
            _provinces = provinces;
            _guid = new IDGenerator(0);
            _events = events;

            ProvinceArmies = new Dictionary<int, ISet<int>>();
            SelectedArmies = new HashSet<int>();
            Armies =  new Dictionary<int, Army>();
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

        public void DeleteSelected()
        {
            foreach (int armyID in SelectedArmies)
            {
                Army army = Armies[armyID];
                Armies.Remove(armyID);
                RemoveArmyFromProvince(army.Location, armyID);    
            }

            ClearSelection();
        }

        public void Delete(IEnumerable<int> armyIDs)
        {
            foreach (int armyID in armyIDs)
            {
                Army army = Armies[armyID];
                Armies.Remove(armyID);
                RemoveArmyFromProvince(army.Location, armyID);    
                SelectedArmies.Remove(armyID);
            }            
        }
           
        /// <summary>
        /// Merge the selected armies into a single army.
        /// </summary>
        /// <returns><c>true</c>, if merge was succesfull, <c>false</c> otherwise.</returns>
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

        public void AddOrder(int armyID, int destination)
        {
            Army army = Armies[armyID];
            if (army.Location == destination) return;

            GraphPath<int> path = _provinces.Graph.Dijkstra(army.Location, destination);
            if (path == null) return;

            int daysUntilFirstMove = _provinces.Graph.NeighborDistance(army.Location, path.Nodes[1]);
            ArmyOrder order = new ArmyOrder(armyID, path);

            Action moveUnit = null; // need to initialize it twice due to recursion below
            moveUnit = () => {
                RemoveArmyFromProvince(order.CurrentNode, order.ArmyID);
                AddArmyToProvince(order.NextNode, order.ArmyID);
                Armies[order.ArmyID].Location = order.NextNode;

                if (order.MoveForward())
                {
                    int daysUntilNextMove = _provinces.Graph.NeighborDistance(order.CurrentNode, order.NextNode);
                    _events.AddEvent(daysUntilNextMove, moveUnit);    
                }
            };

            _events.AddEvent(daysUntilFirstMove, moveUnit);
        }

        public void AddOrder(IEnumerable<int> armyIDs, int destination)
        {
            foreach (int armyID in armyIDs)
            {
                AddOrder(armyID, destination);
            }
        }
           
        public void AddArmy(Army army)
        {
            int armyID = _guid.Generate();
            Armies.Add(Armies.Count, army);
            AddArmyToProvince(army.Location, armyID);
        }
            
        private bool IsValidMerge()
        {
            using (var itr = SelectedArmies.GetEnumerator())
            {
                itr.MoveNext();
                int location = Armies[itr.Current].Location;

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
    
        public class Battle
        {
            public int Army1 { get; }
            public int Army2 { get; }
        }

        // TODO: It's really bad that this class is exposed & mutable.
        // ..... It is possible to restrict it so we only use it in the Units class,
        //...... but then we won't be able to move logic out to other class (e.g Battles.cs).
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
                return PathIndex < Path.NJumps;
            }

            public ArmyOrder(int armyID, GraphPath<int> path)
            {
                ArmyID = armyID;
                Path = path;
                PathIndex = 0;
            }
        }

        private class IDGenerator
        {
            private int _nextID;

            public IDGenerator(int nextID)
            {
                _nextID = nextID;
            }

            public int Generate()
            {
                return _nextID++;
            }
        }
    }
}

