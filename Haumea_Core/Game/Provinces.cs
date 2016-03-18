using System.Collections.Generic;

using Haumea_Core.Geometric;
using Haumea_Core.Collections;

namespace Haumea_Core.Game
{ 
    // This is implemented using Data Oriented Design (DOD, see http://www.dataorienteddesign.com/dodmain/).

    public class Provinces : IModel
    {
        #region properties

        public NodeGraph<int> Graph { get; }

        /// <summary>
        /// The polygon boundaries for every province.
        /// </summary>
        public Poly[] Boundaries { get; }

        /// <summary>
        /// Indicate which province the mouse is over.
        /// If -1, no province exists under the mouse.
        /// </summary>
        public int MouseOver { get; private set; }
    
        /// <summary>
        /// Holds the value which <code>Mouseover</code> changed from.
        /// </summary>
        public int LastMouseOver { get; private set; }

        /// <summary>
        /// Indicate which province is selected. If -1, not province is selected.
        /// </summary>
        public int Selected { get; private set; }

        /// <summary>
        /// Holds the value which <code>MouseOver</code> changed from.
        /// </summary>
        public int LastSelected { get; private set; }

        /// <summary>
        /// Set of all provinces that are water.
        /// </summary>
        public ISet<int> WaterProvinces { get; }

        #endregion

        public Provinces(Poly[] boundaries, ISet<int> waterProvinces, NodeGraph<int> graph)
        {
            Boundaries = boundaries;
            WaterProvinces = waterProvinces;
            Graph = graph;

            MouseOver     = -1;
            LastMouseOver = -1;
            Selected      = -1;
            LastSelected  = -1;
        }
            
        public void Update(WorldDate date) {}

        public void Select(int provinceID)
        {
            LastSelected = Selected;
            Selected = provinceID;   
        }

        public void Hover(int provinceID)
        {
            LastMouseOver = MouseOver;
            MouseOver = provinceID;
        }

        public void ClearSelection()
        {
            Selected = -1;
            LastSelected = -1;
        }
    }
}
