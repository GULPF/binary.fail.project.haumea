using System;
using System.Collections.Generic;

namespace Haumea.Components
{
    public class Belligerents
    {
        public ISet<int> Attackers { get; }
        public ISet<int> Defenders { get; }
        public int AttackingLeader { get; }
        public int DefendingLeader { get; }

        public Belligerents(int attacker, int defender)
        {
            AttackingLeader = attacker;
            DefendingLeader = defender;
            Attackers = new HashSet<int> { attacker };
            Defenders = new HashSet<int> { defender };
        }

        public ISet<int> Enemies(int realmID)
        {
            Debug.Assert(Attackers.Contains(realmID) || Defenders.Contains(realmID));
            return Attackers.Contains(realmID) ? Defenders : Attackers;
        }
    }
}

