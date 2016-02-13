using System;
using System.Collections.Generic;


namespace Haumea_Core
{
    public class Realms
    {
        // Maps province => realm
        private IDictionary<int, string> _ownerships;
        private BiDictionary<int, string> _realmTagIdMapping;

        /// <summary>
        /// Bidirectional dictionary that maps tag => id and id => tag for realms.
        /// </summary>
        public BiDictionary<int, string> RealmTagIdMapping
        {
            get { return RealmTagIdMapping; }
        }
            
        public Realms()
        {
            _ownerships = new Dictionary<int, string>();
            _realmTagIdMapping = new BiDictionary<int, string>();
            _realmTagIdMapping.Add(0, "TEU");
            _realmTagIdMapping.Add(1, "DAN");
            _realmTagIdMapping.Add(2, "NOR");
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