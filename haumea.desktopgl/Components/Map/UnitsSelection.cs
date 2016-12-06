using System;
using System.Linq;
using System.Collections.Generic;

namespace Haumea.Components
{
    public class UnitsSelection
    {
        public ISet<int> Set { get; }

        public int Count { get { return Set.Count; } }

        public UnitsSelection()
        {
            Set = new HashSet<int>();
        }

        public bool IsSelected(int armyID)
        {
            return Set.Contains(armyID);
        }

        public void Select(int armyID, bool keepOldSelection = false)
        {
            if (keepOldSelection)
            {
                Set.Add(armyID);
            }
            else
            {
                Set.Clear();
                Set.Add(armyID);
            }
        }

        public void Deselect(int armyID)
        {
            Set.Remove(armyID);
        }

        public void DeselectAll()
        {
            Set.Clear();
        }
    }
}

