﻿using System;
using System.Linq;
using System.Collections.Generic;

using Haumea.Collections;

namespace Haumea.Components
{
    public class Units : IModel
    {
        // A lot of things in this class might seem weird and clunky, but it's not that bad.

        private readonly Provinces _provinces;
        private readonly Diplomacy _diplomacy;
        private readonly IDGenerator _guid;
        private readonly EventController _events;

        // Called when a unit is deleted.
        public event EventHandler<int> OnDelete;

        /// <summary>
        /// Keeps track of which armies are located in which province.
        /// </summary>
        public IDictionary<int, ISet<int>> ProvinceArmies { get; }

        /// <summary>
        /// (ID, ARMY) pairs for all armies.
        /// </summary>
        public IDictionary<int, Army> Armies { get; }

        /// <summary>
        /// Indicates the owner (realm id) of every army.
        /// </summary>
        public IDictionary<int, int> Ownership { get; }

        /// <summary>
        /// Contains the selected armies.
        /// </summary>
        public ISet<int> SelectedArmies { get; }

        public Units(Provinces provinces, Diplomacy diplomacy, EventController events)
        {
            _provinces = provinces;
            _diplomacy = diplomacy;
            _guid = new IDGenerator(0);
            _events = events;

            ProvinceArmies = new Dictionary<int, ISet<int>>();
            SelectedArmies = new HashSet<int>();
            Ownership = new Dictionary<int, int>();
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
            
        public void Delete(IEnumerable<int> armyIDs)
        {
            foreach (int armyID in armyIDs)
            {
                Army army;
                if (Armies.TryGetValue(armyID, out army))
                {
                    Armies.Remove(armyID);
                    RemoveArmyFromProvince(army.Location, armyID);    
                    SelectedArmies.Remove(armyID);
                    if (OnDelete != null) OnDelete(this, armyID);
                }
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

                // Since battles might start as a result of moving into a province, we need to check if the army has been defeated.
                if (!Armies.ContainsKey(order.ArmyID)) return;

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
            
        public bool IsPlayerArmy(int armyID)
        {
            return Ownership[armyID] == Realms.PlayerID;
        }
            
        public void Recruit(int army, int realm)
        {
            Ownership.Add(army, realm);
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

        private void RemoveArmyFromProvince(int provinceID, int armyID)
        {
            if (!ProvinceArmies.ContainsKey(provinceID))
            {
                return;
            }
            else if (ProvinceArmies[provinceID].Count == 1)
            {
                ProvinceArmies.Remove(provinceID);
            }
            else
            {
                ProvinceArmies[provinceID].Remove(armyID);    
            }
        }

        private void AddArmyToProvince(int province, int armyID)
        {
            ISet<int> provinceArmies;
            if (ProvinceArmies.TryGetValue(province, out provinceArmies))
            {
                var army = Armies[armyID];
                provinceArmies.Add(armyID);
                army.Location = province;

                // Check if a battle should start.
                int attacker = army.Owner;
                IList<War> wars = _diplomacy.GetRelations<War>(attacker);

                foreach (var war in wars)
                {
                    ISet<int> enemies = war.Enemies(attacker);
                    var enemyArmiesInProvince = provinceArmies.Where(aID => enemies.Contains(Armies[aID].Owner)).ToHashSet();
                    if (enemyArmiesInProvince.Count > 0)
                    {
                        Battle(armyID, enemyArmiesInProvince);
                        break;
                    }
                }
            }
            else
            {
                ProvinceArmies[province] = new HashSet<int> { armyID };
                Armies[armyID].Location = province;
            }
        }
    
        /// <summary>
        /// Start a battle.
        /// </summary>
        /// <param name="province">Battleground</param>
        /// <param name="attackingArmyID">Attacking army</param>
        /// <param name="defendingArmyIDs">Defending armys</param>
        /// <returns>true if attacking army won, false otherwise</returns>
        private void Battle(int attackingArmyID, ISet<int> defendingArmyIDs)
        {
            var army = Armies[attackingArmyID];

            foreach (var enemyArmyID in defendingArmyIDs)
            {
                var enemyArmy = Armies[enemyArmyID];

                if (enemyArmy.NUnits > army.NUnits)
                {
                    enemyArmy.NUnits -= army.NUnits;
                    DeleteArmy(army.NUnits);
                    break;
                }
                else
                {
                    army.NUnits -= enemyArmy.NUnits;
                    enemyArmy.NUnits = 0;
                    DeleteArmy(enemyArmyID);
                }
            }  
        }

        private void DeleteArmy(int armyID)
        {
            int province = Armies[armyID].Location;
            RemoveArmyFromProvince(province, armyID);
            Armies.Remove(armyID);
            SelectedArmies.Remove(armyID);
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

