using System;
using System.Linq;
using System.Collections.Generic;
using Haumea.Components;

namespace Haumea.Components
{

    public class War : IRelation
    {
        public ISet<int> Participants { get; }
        public ISet<int> Attackers { get; }
        public ISet<int> Defenders { get; }
        public int AttackingWarleader { get; set; }
        public int DefendingWarleader { get; set; }
        public CasusBellis CasusBelli { get; }
        public DateTime Start { get; }
        public float Warscore { get; }

        public War(ISet<int> attackers, ISet<int> defenders,
            int attackingWarleader, int defendingWarleader, CasusBellis casusBeli)
        {
            Participants = new HashSet<int>(attackers);
            Participants.UnionWith(defenders);
            Attackers = attackers;
            Defenders = defenders;
            AttackingWarleader = attackingWarleader;
            DefendingWarleader = defendingWarleader;
            CasusBelli = casusBeli;

            Start = DateTime.Now; // obv temporary
            Warscore = 0f;
        }

        public void Update(WorldDate date)
        {
            throw new NotImplementedException();
        }

        public ISet<int> Enemies(int realmID)
        {
            Debug.Assert(Attackers.Contains(realmID) || Defenders.Contains(realmID));
            return Attackers.Contains(realmID) ? Defenders : Attackers;
        }
    }

    public static class WarDiplomaticExtensions
    {
        public static ISet<int> AllEnemies(this Diplomacy diplo, int realmID)
        {
            return diplo.GetRelations<War>(realmID)
                .SelectMany(war => war.Defenders.Contains(realmID) ? war.Attackers : war.Defenders)
                .ToHashSet();
        }
    }

    public enum CasusBellis
    {
        Conquest
    }

}

