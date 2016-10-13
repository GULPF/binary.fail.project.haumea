using System;
using System.Collections.Generic;

namespace Haumea.Components
{
    public class ProvinceSelection {

        public bool IsEmpty
        {
            get
            {
                return Selected.Count == 0;
            }
        }

        public ISet<int> Selected { get; }
        public int Hovering { get; private set; }

        public ProvinceSelection()
        {
            Selected = new HashSet<int>();
            Hovering = -1;
        }

        public void DeselectAll()
        {
            Selected.Clear();
        }

        public void StopHovering()
        {
            Hovering = -1;
        }

        public bool Select(int id, bool keepSelected = false)
        {
            if (Selected.Contains(id)) return false;
            if (!keepSelected) DeselectAll();
            Selected.Add(id);
            return true;
        }

        public void Hover(int id)
        {
            Hovering = id;
        }

        public bool IsSelected(int id)
        {
            return Selected.Contains(id);
        }

        public bool IsHovered(int id)
        {
            return Hovering == id;
        }
    }
}

