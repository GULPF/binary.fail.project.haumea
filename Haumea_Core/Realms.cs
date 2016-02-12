using System;
using System.Collections.Generic;

public class Realms
{
    private IDictionary<int, string> _ownerships;

    public void AssignOwnership(int province, string realmTag)
    {
        
    }

    public struct Realm
    {
        private IList<int> _provinces;
        private string _tag;

        public static void TransferOwnership(Realm source, Realm dest, int province)
        {
            source._provinces.Remove(province);
            dest._provinces.Add(province);
        }
    }
}