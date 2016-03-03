using System;
using System.Collections.Generic;

using Haumea_Core.Collections;

namespace Haumea_Core.Game
{
    public class Realms
    {
        // Maps province => realm
        private readonly IDictionary<int, string> _ownerships;

        /// <summary>
        /// Bidirectional dictionary that maps tag => id and id => tag for realms.
        /// </summary>
        public BiDictionary<int, string> RealmTagIdMapping { get; }
            
        public Realms()
        {
            _ownerships = new Dictionary<int, string>();
            RealmTagIdMapping = new BiDictionary<int, string>();
            RealmTagIdMapping.Add(0, "TEU");
            RealmTagIdMapping.Add(1, "DAN");
            RealmTagIdMapping.Add(2, "NOR");
        }

        public void AssignOwnership(int province, string realmTag)
        {
            _ownerships.Add(province, realmTag);
        }

        public string GetOwnerTag(int province)
        {
            return _ownerships[province];
        }

        public struct Realm
        {
            private IList<int> _provinces;
            private string _tag;
            private int _id;

            public static void TransferOwnership(Realm source, Realm dest, int province)
            {
                source._provinces.Remove(province);
                dest._provinces.Add(province);
            }
        }
    }    
}
