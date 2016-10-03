using System;
using System.Linq;
using System.Collections.Generic;
using Haumea.Collections;

namespace Haumea.Components
{
    public class Wars : IModel
    {
        // ticking warscore updates every fourteen days
        private static readonly int _tickingWarscoreDelta = 14;
        // after 40 updates, ticking warscore stops
        private static readonly int _maxTickingWarscore = 40;
        // ticking warscore gives one warscore every update
        private static readonly int _tickingWarscoreValue = 1;

        // to avoid checking every war each frame, we manage a list
        // of wars sorted by when ticking warscore will update (soonest first).
        private readonly LinkedList<int> _tickingWarscoreQueue;
        private DateTime _lastTickingWarscoreUpdate;

        public IDictionary<int, int> Warscores                 { get; }
        public IDictionary<int, ISet<int>> RealmWars          { get; }
        public IDictionary<int, Belligerents> WarBelligerents { get; }
        public IDictionary<int, CasusBellis> CasusBellis      { get; }
        public IDictionary<int, DateTime> StartDates          { get; }

        private int _nextID = 0;

        public Wars(BiDictionary<int, string> realmsTagId)
        {
            Warscores        = new Dictionary<int, int>();
            RealmWars       = new Dictionary<int, ISet<int>>();
            WarBelligerents = new Dictionary<int, Belligerents>();
            CasusBellis     = new Dictionary<int, CasusBellis>();
            StartDates      = new Dictionary<int, DateTime>();

            _tickingWarscoreQueue = new LinkedList<int>();
            _lastTickingWarscoreUpdate = DateTime.MinValue;

            foreach (var mapping in realmsTagId)
            {
                RealmWars.Add(mapping.Key, new HashSet<int>());
            }
        }
            
        public void Update(WorldDate date)
        {
            if (date.Date > _lastTickingWarscoreUpdate && _tickingWarscoreQueue.Count > 0)
            {
                UpdateTickingWarscores(date.Date);
                _lastTickingWarscoreUpdate = date.Date;
            }

            if (Warscores.ContainsKey(0))
            {
                Debug.WriteToScreen("Warscore", Warscores[0] + "%");
            }
        }

        public void DeclareWar(int attacker, int defender, CasusBellis casusBelli, DateTime now)
        {
            int id = _nextID++;
            var belligs = new Belligerents(attacker, defender);
            RealmWars[attacker].Add(id);
            RealmWars[defender].Add(id);
            WarBelligerents.Add(id, belligs);
            CasusBellis.Add(id, casusBelli);
            StartDates.Add(id, now);
            Warscores.Add(id, 0);
            _tickingWarscoreQueue.AddLast(id);
        }

        public ISet<int> GetAllEnemies(int realmID)
        {
            return RealmWars[realmID]
                .SelectMany(warID => WarBelligerents[warID].Enemies(realmID))
                .ToHashSet();
        }

        public void HandleBattleResult()
        {
            
        }

        private void UpdateTickingWarscores(DateTime now)
        {
            int warID = _tickingWarscoreQueue.First.Value;
            TimeSpan span = now - StartDates[warID];
            int nUpdated = 0;

            // Safe floating point comparision since only full days are handled in the game.
            while (span.TotalDays % _tickingWarscoreDelta == 0)
            {
                Warscores[warID] += _tickingWarscoreValue;

                _tickingWarscoreQueue.RemoveFirst();

                if (span.TotalDays / _tickingWarscoreDelta < _maxTickingWarscore - 1)
                {
                    _tickingWarscoreQueue.AddLast(warID);                    
                }

                // Avoid getting stuck when all wars are updated
                if (++nUpdated >= _tickingWarscoreQueue.Count)
                {
                    break;
                }
            }
        }
    }

    public enum CasusBellis
    {
        Conquest
    }
}

