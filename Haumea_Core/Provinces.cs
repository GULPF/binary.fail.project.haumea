using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Haumea_Core.Rendering;

namespace Haumea_Core
{
    // This will be a central class in the game - it should handle all province functionality at a high level,
    // either directly or by delegating it to another class. 
    // It is implemented using Data Oriented Design (DOD, see http://www.dataorienteddesign.com/dodmain/).
    // Any delegators should also prefer DOD.

    public class Provinces
    {
        private readonly BiDictionary<int, string> _provinceTagIdMapping;

        // Rendering related - maybe move to a delegate if it gets hairy.
        private enum RenderState { Hover, Idle };
        private readonly RenderInstruction[] _currentRenderBatch;
        private readonly Dictionary<int, Dictionary<RenderState, RenderInstruction>> _allRenderInstructions;

        // Hit detection.
        private readonly Poly[] _polys;
        private int _mouseOver;

        // Delegators
        private Realms _realms;

        public Realms Realms {
            get { return _realms; }
        }

        /// <summary>
        /// Bidirectional dictionary mapping tag => id and id => tag.
        /// </summary>
        public BiDictionary<int, string> ProvinceTagIdMapping
        {
            get { return _provinceTagIdMapping; }
        }

        /// <summary>
        /// Indicate which province the mouse is over.
        /// If -1, no province exists under the mouse.
        /// </summary>
        public int MouseOver
        {
            get { return _mouseOver; }
        }

        /// <summary>
        /// A list of all active render instructions.
        /// </summary>
        public RenderInstruction[] RenderInstructions {
            get { return _currentRenderBatch; }
        }

        public Provinces(RawProvince[] provinces)
        {
            _polys                 = new Poly[provinces.Length];
            _currentRenderBatch    = new RenderInstruction[provinces.Length];
            _allRenderInstructions = new Dictionary<int, Dictionary<RenderState, RenderInstruction>>();
            _mouseOver             = -1;

            _provinceTagIdMapping = new BiDictionary<int, string>();
            _realms = new Realms();


            for (int id = 0; id < _polys.Length; id++) {
                
                _provinceTagIdMapping.Add(id, provinces[id].Tag);
                _realms.AssignOwnership(id, provinces[id].RealmTag);

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
                _currentRenderBatch[id]    = _allRenderInstructions[id][RenderState.Idle];
            }
        }
            
        public void Update(Vector2 mousePos)
        {
            for (int id = 0; id < _polys.Length; id++)
            {
                if (_polys[id].isPointInside(mousePos)) {
                    // If this is not a new mouse over, don't bother.
                    if (id == _mouseOver) return;

                    _currentRenderBatch[id] = _allRenderInstructions[id][RenderState.Hover];
                    if (_mouseOver > -1) {
                        _currentRenderBatch[_mouseOver] = _allRenderInstructions[_mouseOver][RenderState.Idle];    
                    }
                    _mouseOver = id;

                    // Provinces can't overlap so we exit immediately when we find a hit.
                    return;
                }
            }

            // Not hit - clear mouse over.
            if (_mouseOver > -1) {
                _currentRenderBatch[_mouseOver] = _allRenderInstructions[_mouseOver][RenderState.Idle];    
                _mouseOver = -1;
            }
        }

        public void Draw(Renderer renderer, GameTime gameTime)
        {
            renderer.Render(_currentRenderBatch);
        }

        #region geometric

        // TODO: These do not belong in here.

        public class Poly : Hitable
        {
            public readonly Vector2[] Points;
            private readonly AABB _boundary;

            public Poly (Vector2[] points) {
                Points  = points;

                Vector2 max = Vector2.Zero, min = Vector2.Zero;

                foreach (Vector2 vector in Points)
                {
                    max.X = MathHelper.Max(max.X, vector.X);
                    max.Y = MathHelper.Max(max.Y, vector.Y);
                    min.X = MathHelper.Min(min.X, vector.X);
                    min.Y = MathHelper.Min(min.Y, vector.Y);
                }

                _boundary = new AABB(max, min);
            }

            public bool isPointInside(Vector2 point)
            {
                if (!_boundary.isPointInside(point)) return false;

                // http://stackoverflow.com/questions/217578/

                int i, j;
                bool c = false;
                for (i = 0, j = Points.Length - 1; i < Points.Length; j = i++) {
                    if ( ((Points[i].Y > point.Y) != (Points[j].Y > point.Y)) &&
                        (point.X < 
                            (Points[j].X - Points[i].X) * (point.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X) )
                    {
                        c = !c;
                    }
                }

                return c;
            }
        }

        public class AABB : Hitable
        {
            private readonly Vector2 _max, _min;

            public AABB(Vector2 max, Vector2 min)
            {
                _max = max;
                _min = min;
            }

            public bool isPointInside(Vector2 point)
            {
                return _min.X <= point.X && point.X <= _max.X
                    && _min.Y <= point.Y && point.Y <= _max.Y;
            }
        }

        public interface Hitable
        {
            bool isPointInside(Vector2 point);
        }

        #endregion

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
