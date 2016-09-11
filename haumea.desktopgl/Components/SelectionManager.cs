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

        public void Select(T id, bool keepSelected = false)
        {
            if (!keepSelected) DeselectAll();
            Selected.Add(id);
        }

        public void Hover(T id, bool keepHovered = false)
        {
            if (!keepHovered) StopHoveringAll();
            Hovering.Add(id);
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

