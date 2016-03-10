using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Geometric;
using Haumea_Core.Collections;

namespace Haumea_Core.Game
{ 
    // This is implemented using Data Oriented Design (DOD, see http://www.dataorienteddesign.com/dodmain/).

    public class Provinces : IEntity
    {
        #region properties

        /// <summary>
        /// The polygon boundaries for every province.
        /// </summary>
        public Poly[] Boundaries { get; }

        /// <summary>
        /// Bidirectional dictionary mapping tag => id and id => tag.
        /// </summary>
        public BiDictionary<int, string> TagIdMapping { get; }

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
        /// Handles the realms: keeps track of which realm each province belongs to.
        /// </summary>
        public Realms Realms { get; }

        /// <summary>
        /// Indicate which realm ID belongs to the player.
        /// </summary>
        public int PlayerID { get; }

        #endregion

        public Provinces(Poly[] boundaries, BiDictionary<int, string> tagIdMapping)
        {
            Boundaries = boundaries;
            TagIdMapping = tagIdMapping;

            MouseOver     = -1;
            LastMouseOver = -1;
            Selected      = -1;
            LastSelected  = -1;

            PlayerID = 0;
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
