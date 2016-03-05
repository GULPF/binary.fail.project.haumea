using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Geometric;
using Haumea_Core.Collections;

namespace Haumea_Core.Game
{
    // This will be a central class in the game - it should handle all province functionality at a high level,
    // either directly or by delegating it to another class. 
    // It is implemented using Data Oriented Design (DOD, see http://www.dataorienteddesign.com/dodmain/).
    // Any delegators should also prefer DOD.

    public partial class Provinces
    {
        private readonly Poly[] _polys;
        private readonly AABB[] _labelBoxes;

        private readonly Haumea _game;

        #region properties

        /// <summary>
        /// Bidirectional dictionary mapping tag => id and id => tag.
        /// </summary>
        public BiDictionary<int, string> ProvinceTagIdMapping { get; }

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
        //  Graph over how the provinces are connected,
        //  with a distance value assigned to every connection.
        /// </summary>
        public NodeGraph<int> MapGraph { get; }

        /// <summary>
        /// Handles the realms: keeps track of which realm each province belongs to.
        /// </summary>
        public Realms Realms { get; }

        /// <summary>
        /// Indicate which realm ID belongs to the player.
        /// </summary>
        public int PlayerID { get; }

        /// <summary>
        /// Handles the armies: keeps track of how many units are stationed in each province,
        /// and should handle movement etcetera.
        /// </summary>
        public Units Units { get; }

        #endregion

        public Provinces(RawProvince[] rawProvinces, NodeGraph<int> mapGraph, Haumea game)
        {
            _polys = new Poly[rawProvinces.Length];
            _game  = game;

            MouseOver     = -1;
            LastMouseOver = -1;
            Selected      = -1;
            LastSelected  = -1;

            ProvinceTagIdMapping = new BiDictionary<int, string>();
            MapGraph = mapGraph;
            Realms = new Realms();
            Units = new Units(this, _game);

            for (int id = 0; id < _polys.Length; id++) {
                ProvinceTagIdMapping.Add(id, rawProvinces[id].Tag);
                Realms.AssignOwnership(id, rawProvinces[id].RealmTag);
                _polys[id] = rawProvinces[id].Poly;
                Units.AddUnits(rawProvinces[id].Units, id);
            }
        }
            
        public void Update(InputState input)
        {
            UpdateSelection(input);
            Units.Update();
        }

        public void UpdateSelection(InputState input)
        {
            Vector2 position = input.Mouse;

            bool doDeselect  = input.WentActive(Keys.Escape);
            bool doSelect    = input.WentActive(Buttons.LeftButton) && !doDeselect;

            if (doDeselect)
            {
                ClearSelection();
            }

            for (int id = 0; id < _polys.Length; id++)
            {
                if (_polys[id].IsPointInside(position)) {
                    // Only handle new selections.
                    if (id != Selected && doSelect)
                    {
                        LastSelected = Selected;
                        Selected = id;   
                    }

                    // If this is not a new mouse over, don't bother.
                    if (id != MouseOver)
                    {
                        LastMouseOver = MouseOver;
                        MouseOver = id;    
                    }

                    // Provinces can't overlap so we exit immediately when we find a hit.
                    return;
                }
            }

            // Not hit - clear mouse over.
            if (MouseOver > -1) {
                LastMouseOver = MouseOver;
                MouseOver = -1;
            }
        }

        public void ClearSelection()
        {
            Selected = -1;
            LastSelected = -1;
        }

        // Not intended to be used for anyting else other than temporarily holding parsed data. 
        public struct RawProvince
        {
            public Poly Poly { get; }
            public string Tag { get; }
            public string RealmTag { get; }
            public Color Color { get; }
            public int Units { get; }

            public RawProvince(Poly poly, String tag, String realmTag, Color color, int units)
            {
                Poly = poly;
                Tag = tag;
                RealmTag = realmTag;
                Color = color;
                Units = units;
            }
        }
    }
}
