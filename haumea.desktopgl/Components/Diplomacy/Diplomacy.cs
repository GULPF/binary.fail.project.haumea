using System;
using System.Collections.Generic;
using Haumea.Components;

namespace Haumea.Components
{
    // Use cases: 
    // - List all relations containing country X
    // - List all relations between country X, Y
    // - Remove all relations containing country x (annexed country)
    // - Remove a specific relation (broken alliance etc)
    // - Different relations have different data (wars have casus beli f.ex)
    public class Diplomacy : IModel
    {
        private IDictionary<int, IRelation> Relations { get; }
        public IDictionary<int, ISet<int>> RealmRelations { get; }

        public Diplomacy(Realms realms)
        {
            Relations = new Dictionary<int, IRelation>();
            RealmRelations = new Dictionary<int, ISet<int>>();

            foreach (var mapping in realms.TagIdMapping)
            {
                RealmRelations.Add(mapping.Key, new HashSet<int>());
            }
        }

        public void Update(WorldDate date)
        {
            foreach (var rel in Relations)
            {
                rel.Value.Update(date);
            }
        }

        public T GetRelation<T>(int r1, int r2) where T : class, IRelation
        {
            foreach (int relID in SharedRelations(r1, r2))
            {
                var rel = Relations[relID];
                var t = rel as T;
                if (t != null)
                {
                    return t;
                }
            }

            return null;
        }

        public IList<T> GetRelations<T>(int r1) where T : class, IRelation
        {
            IList<T> rels = new List<T>();

            foreach (int relID in RealmRelations[r1])
            {
                var rel = Relations[relID];
                var t  = rel as T;
                if (t != null)
                {
                    rels.Add(t);
                }
            }

            return rels;
        }

        public bool HaveRelation<T>(int r1, int r2) where T : class, IRelation
        {
            foreach (int relID in SharedRelations(r1, r2))
            {
                if (Relations[relID] is T)
                {
                    return true;
                }
            }

            return false;
        }

        public void EndRelation<T>(int r1, int r2) where T : class, IRelation
        {
            foreach (int relID in SharedRelations(r1, r2))
            {
                if (Relations[relID] is T)
                {
                    RealmRelations[r1].Remove(relID);
                    RealmRelations[r2].Remove(relID);
                    Relations.Remove(relID);
                }
            }
        }
            
        public void EndAllRelations(int r1) 
        {
            foreach (int relID in RealmRelations[r1])
            {
                IRelation rel = Relations[relID];

                if (rel.Participants.Count == 2)
                {
                    Relations.Remove(relID);
                }
                else
                {
                    rel.Participants.Remove(r1);
                }
            }
        }

        public void StartRelation<T>(int r1, int r2, T rel) where T : class, IRelation
        {
            Relations.Add(_nextID, rel);
            RealmRelations[r1].Add(_nextID);
            RealmRelations[r2].Add(_nextID);
            _nextID++;
        }

        /// <summary>
        /// Slightly better than Set.IntersectWith(), since we only need one iteration instead of two.
        /// </summary>
        private IEnumerable<int> SharedRelations(int r1, int r2)
        {
            var set1 = RealmRelations[r1];
            var set2 = RealmRelations[r2];

            foreach (int relID in set1)
            {
                if (set2.Contains(relID))
                {
                    yield return relID;
                }
            }
        }

        private static int _nextID = 0;
    }

    public interface IRelation : IModel
    {
        ISet<int> Participants { get; }
    }
}

