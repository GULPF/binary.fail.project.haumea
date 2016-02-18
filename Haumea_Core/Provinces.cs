using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Haumea_Core.Rendering;
using Haumea_Core.Geometric;
using Haumea_Core.Collections;

namespace Haumea_Core
{
    // This will be a central class in the game - it should handle all province functionality at a high level,
    // either directly or by delegating it to another class. 
    // It is implemented using Data Oriented Design (DOD, see http://www.dataorienteddesign.com/dodmain/).
    // Any delegators should also prefer DOD.

    public class Provinces
    {
        private enum RenderState { Hover, Idle };
        private readonly Dictionary<int, Dictionary<RenderState, RenderInstruction>> _allRenderInstructions;
        private readonly Poly[] _polys;
        private int _mouseOver;

        /// <summary>
        /// Bidirectional dictionary mapping tag => id and id => tag.
        /// </summary>
        public BiDictionary<int, string> ProvinceTagIdMapping { get; }

        /// <summary>
        /// Indicate which province the mouse is over.
        /// If -1, no province exists under the mouse.
        /// </summary>
        public int MouseOver {
            get { return _mouseOver; }
        }

        /// <summary>
        /// A list of all active render instructions.
        /// </summary>
        public RenderInstruction[] RenderInstructions { get; }

        // Delegators
        public Realms Realms { get; }

        public Provinces(RawProvince[] provinces)
        {
            _polys                 = new Poly[provinces.Length];
            RenderInstructions     = new RenderInstruction[provinces.Length];
            _allRenderInstructions = new Dictionary<int, Dictionary<RenderState, RenderInstruction>>();
            _mouseOver             = -1;

            ProvinceTagIdMapping = new BiDictionary<int, string>();
            Realms = new Realms();


            for (int id = 0; id < _polys.Length; id++) {
                
                ProvinceTagIdMapping.Add(id, provinces[id].Tag);
                Realms.AssignOwnership(id, provinces[id].RealmTag);

                //
                // Initialize the render states - all provinces start in `Idle`.
                //

                _polys[id] = provinces[id].Poly;
                var renderInstructions = new Dictionary<RenderState, RenderInstruction>();

                Color color = provinces[id].Color;

                // This means that the polygons are triangulated at run time, which is not optimal.
                // It shouldn't *really* matter though.
                renderInstructions[RenderState.Idle] = RenderInstruction.
                    Polygon(_polys[id].Points, color);
                renderInstructions[RenderState.Hover] = RenderInstruction.
                    Polygon(_polys[id].Points, color.Darken());

                _allRenderInstructions[id] = renderInstructions; 
                RenderInstructions[id]    = _allRenderInstructions[id][RenderState.Idle];
            }
        }
            
        public void Update(Vector2 mousePos)
        {
            for (int id = 0; id < _polys.Length; id++)
            {
                if (_polys[id].IsPointInside(mousePos)) {
                    // If this is not a new mouse over, don't bother.
                    if (id == _mouseOver) return;

                    RenderInstructions[id] = _allRenderInstructions[id][RenderState.Hover];
                    if (_mouseOver > -1) {
                        RenderInstructions[_mouseOver] = _allRenderInstructions[_mouseOver][RenderState.Idle];    
                    }
                    _mouseOver = id;

                    // Provinces can't overlap so we exit immediately when we find a hit.
                    return;
                }
            }

            // Not hit - clear mouse over.
            if (_mouseOver > -1) {
                RenderInstructions[_mouseOver] = _allRenderInstructions[_mouseOver][RenderState.Idle];    
                _mouseOver = -1;
            }
        }

        public void Draw(Renderer renderer, GameTime gameTime)
        {
            renderer.Render(RenderInstructions);
        }

        // Not intended to be used for anyting else other than temporarily holding parsed data. 
        public struct RawProvince
        {
            public Poly Poly;
            public string Tag, RealmTag;
            public Color Color;

            public RawProvince(Poly poly, String tag, String realmTag, Color color)
            {
                Poly = poly;
                Tag = tag;
                RealmTag = realmTag;
                Color = color;
            }
        }
    }
}
