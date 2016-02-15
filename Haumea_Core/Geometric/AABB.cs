using System;
using Microsoft.Xna.Framework;

namespace Haumea_Core.Geometric {
    public class AABB : IHitable
    {
        private readonly Vector2 _max, _min;

        public AABB(Vector2 max, Vector2 min)
        {
            _max = max;
            _min = min;
        }

        public bool IsPointInside(Vector2 point)
        {
            return _min.X <= point.X && point.X <= _max.X
                && _min.Y <= point.Y && point.Y <= _max.Y;
        }
    }
}
