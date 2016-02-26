using System;
using Microsoft.Xna.Framework;

namespace Haumea_Core.Geometric {
    public class AABB : IHitable
    {
        public Vector2 Max { get; }
        public Vector2 Min { get; }
        public Vector2 Dim {
            get {
                return (Max - Min).Abs();
            }
        }

        public AABB(Vector2 max, Vector2 min)
        {
            Max = max;
            Min = min;
        }

        public bool IsPointInside(Vector2 point)
        {
            return Min.X <= point.X && point.X <= Max.X
                && Min.Y <= point.Y && point.Y <= Max.Y;
        }
    }
}
