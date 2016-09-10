using System;
using System.Collections.Generic;

using Haumea.Collections;

namespace Haumea.Components
{
    public class Realms : IModel
    {
        // Maps province => realm
        private readonly IDictionary<int, string> _ownerships;

        /// <summary>
        /// Indicate which realm ID belongs to the player.
        /// </summary>
        public int PlayerID { get; }

        public Realms()
        {
            _ownerships = new Dictionary<int, string>();
            PlayerID = 0;
        }

        public void Update(WorldDate date) {}

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
