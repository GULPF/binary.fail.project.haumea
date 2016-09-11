using System;
using System.Collections.Generic;

namespace Haumea.Components
{
    public class SelectionManager<T> {

        public bool IsEmpty
        {
            get
            {
                return Selected.Count == 0;
            }
        }

        public ISet<T> Selected { get; }
        public ISet<T> Hovering { get; }

        public SelectionManager()
        {
            Selected = new HashSet<T>();
            Hovering = new HashSet<T>();
        }

        public void DeselectAll()
        {
            Selected.Clear();
        }

        public void StopHoveringAll()
        {
            Hovering.Clear();
        }

        public bool Select(T id, bool keepSelected = false)
        {
            if (Selected.Contains(id)) return false;
            if (!keepSelected) DeselectAll();
            Selected.Add(id);
            return true;
        }

        public bool Hover(T id, bool keepHovered = false)
        {
            if (Hovering.Contains(id)) return false;
            if (!keepHovered) StopHoveringAll();
            Hovering.Add(id);
            return true;
        }

        public bool IsSelected(T id)
        {
            return Selected.Contains(id);
        }

        public bool IsHovered(T id)
        {
            return Hovering.Contains(id);
        }
    }
}

