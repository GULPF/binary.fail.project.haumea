using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Haumea_Core.Rendering;

namespace Haumea_Core
{
    // This will be a central class in the game - it should handle all province functionality at a high level,
    // either directly or by delegating it to another class. 
    // It is implemented using Data Oriented Design (DOD, see http://www.dataorienteddesign.com/dodmain/).
    // Any delegators should also prefer DOD.

    public class Provinces
    {
        // Rendering related - maybe move to a delegate if it gets hairy.
        private RenderInstruction[] _currentRenderBatch;
        private enum RenderState {Hover, Idle};
        private readonly Dictionary<int, Dictionary<RenderState, RenderInstruction>> _allRenderInstructions;

        public RenderInstruction[] RenderInstructions {
            get { return _currentRenderBatch; }
        }

        // Hit detection.
        private readonly Poly[] _polys;

        public Provinces(Poly[] polys)
        {
            _polys = polys;
            _currentRenderBatch    = new RenderInstruction[_polys.Length];
            _allRenderInstructions = new Dictionary<int, Dictionary<RenderState, RenderInstruction>>();

            // Initialize the render states - all provinces start in `Idle`.
            for (int id = 0; id < _polys.Length; id++) {
                var renderInstructions = new Dictionary<RenderState, RenderInstruction>();

                // This means that the polygons are triangulated at run time, which is not optimal.
                // It shouldn't *really* matter though.
                renderInstructions[RenderState.Idle] = RenderInstruction.
                    Polygon(_polys[id].Points, Color.Black);
                renderInstructions[RenderState.Hover] = RenderInstruction.
                    Polygon(_polys[id].Points, Color.Red);

                _allRenderInstructions[id] = renderInstructions; 
                _currentRenderBatch[id]    = _allRenderInstructions[id][RenderState.Idle];
            }
        }
            
        public void Update(Vector2 mousePos)
        {
            for (int id = 0; id < _polys.Length; id++)
            {
                if (_polys[id].isPointInside(mousePos))
                {
                    _currentRenderBatch[id] = _allRenderInstructions[id][RenderState.Hover];
                } else
                {
                    _currentRenderBatch[id] = _allRenderInstructions[id][RenderState.Idle];
                }
            }
        }

        public void Draw(Renderer renderer, GameTime gameTime)
        {
            renderer.Render(_currentRenderBatch);
        }

        // TODO: These do not belong in here.

        public class Poly : Hitable
        {
            public readonly Vector2[] Points;
            private readonly AABB _boundary;

            public Poly (Vector2[] points) {
                Points  = points;

                Vector2 max = new Vector2(0, 0), min = new Vector2(0, 0);

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
    }
}

