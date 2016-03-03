using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

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

        // Delegators
        public Realms Realms { get; }

        public Provinces(RawProvince[] provinces)
        {
            _polys                 = new Poly[provinces.Length];
            MouseOver             = -1;
            LastMouseOver         = -1;

            ProvinceTagIdMapping = new BiDictionary<int, string>();
            Realms = new Realms();

            for (int id = 0; id < _polys.Length; id++) {
                
                ProvinceTagIdMapping.Add(id, provinces[id].Tag);
                Realms.AssignOwnership(id, provinces[id].RealmTag);
                _polys[id] = provinces[id].Poly;
            }
        }
            
        public void Update(Vector2 mousePos)
        {
            for (int id = 0; id < _polys.Length; id++)
            {
                if (_polys[id].IsPointInside(mousePos)) {
                    // If this is not a new mouse over, don't bother.
                    if (id == MouseOver) return;

                    LastMouseOver = MouseOver;
                    MouseOver = id;

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

        // Not intended to be used for anyting else other than temporarily holding parsed data. 
        public struct RawProvince
        {
            public Poly Poly { get; }
            public string Tag { get; }
            public string RealmTag { get; }
            public Color Color { get; }
            public int Units { get; }

            public RawProvince(Poly poly, String tag, String realmTag, Color color)
            {
                Poly = poly;
                Tag = tag;
                RealmTag = realmTag;
                Color = color;
                Units = 0;
            }
        }
    }
}
