using System;
using System.Linq;
using System.Collections.Generic;

using Haumea.Collections;

namespace Haumea.Components
{
    public class Units : IModel
    {
        private int _nextID;

        private readonly Provinces _provinces;
        private readonly Wars _wars;
        private readonly EventController _events;

        // Called when a unit is deleted.
        public event Action<int> OnDelete;
        // Called when a battle have ended.
        public event Action OnBattle;

        /// <summary>
        /// Keeps track of which armies are located in which province.
        /// </summary>
        public IDictionary<int, ISet<int>> ProvinceArmies { get; }

        /// <summary>
        /// (ID, ARMY) pairs for all armies.
        /// </summary>
        public IDictionary<int, Army> Armies { get; }

        public Units(Provinces provinces, Wars wars, EventController events)
        {
            _provinces = provinces;
            _wars = wars;
            _events = events;

            ProvinceArmies = new Dictionary<int, ISet<int>>();
            Armies =  new Dictionary<int, Army>();
        }

        public void Update(WorldDate date)
        {

        }
            
        public void Delete(IEnumerable<int> armyIDs)
        {
            foreach (int armyID in armyIDs)
            {
                Delete(armyID);
            }
        }

        public void Delete(int armyID)
        {
            Army army;
            if (Armies.TryGetValue(armyID, out army))
            {
                Armies.Remove(armyID);
                RemoveArmyFromProvince(army.Location, armyID);    
                if (OnDelete != null) OnDelete(armyID);
            }
        }
            
        /// <summary>
        /// Merge the selected armies into a single army.
        /// </summary>
        /// <returns><c>true</c>, if merge was succesfull, <c>false</c> otherwise.</returns>
        public bool Merge(ICollection<int> ids)
        {
            if (ids.Count < 2 || !IsValidMerge(ids)) return false;

            using (var enumer = ids.GetEnumerator())
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
            int armyID = _nextID++;
            Armies.Add(Armies.Count, army);
            AddArmyToProvince(army.Location, armyID);
        }
            
        public bool IsPlayerArmy(int armyID)
        {
            return Armies[armyID].Owner == Realms.PlayerID;
        }

        private bool IsValidMerge(ICollection<int> ids)
        {
            int location = Armies[ids.First()].Location;

            // We are only allowed to merge if all armies are in the same province.
            foreach (int armyID in ids)
            {
                if (location != Armies[armyID].Location)
                {
                    return false;
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
                ISet<int> warIDs = _wars.RealmWars[attacker];

                foreach (var warID in warIDs)
                {
                    ISet<int> enemies = _wars.WarBelligerents[warID].Enemies(attacker);
                    var enemyArmiesInProvince = provinceArmies
                        .Where(aID => enemies.Contains(Armies[aID].Owner))
                        .ToHashSet();
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

            foreach (var defendingArmyID in defendingArmyIDs)
            {
                var defendingArmy = Armies[defendingArmyID];

                if (defendingArmy.NUnits > army.NUnits)
                {
                    defendingArmy.NUnits -= army.NUnits;
                    Delete(attackingArmyID);
                    break;
                }
                else if (defendingArmy.NUnits < army.NUnits)
                {
                    army.NUnits -= defendingArmy.NUnits;
                    defendingArmy.NUnits = 0;
                    Delete(defendingArmyID);
                }
                else
                {
                    Delete(attackingArmyID);
                    Delete(defendingArmyID);
                }

                _wars.HandleBattleResult();
            }  
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

        public class BattleResult
        {
            public int Attacker { get; }
            public int Defender { get; }
            public int Winner { get; }
            public int AttackerLoses { get; }
            public int DefenderLoses { get; }

            public BattleResult(int attacker, int defender, int winner, int attackerLoses, int defenderLoses)
            {
                Attacker = attacker;
                Defender = defender;
                AttackerLoses = attackerLoses;
                DefenderLoses = defenderLoses;
            }
        }
    }
}

