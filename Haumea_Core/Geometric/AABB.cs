using System;
using Microsoft.Xna.Framework;

using Haumea_Core.Rendering;

namespace Haumea_Core.Geometric {
    public class AABB : IHitable
    {
        // This is probably to damn many props for this class.
        public Vector2 Max { get; }
        public Vector2 Min { get; }
        public Vector2 Dim { get; }
        public float Area  { get; }
        public Vector2 Center { get;}

        public AABB(Vector2 max, Vector2 min)
        {
            Max = max;
            Min = min;
            Dim = (Max - Min).Abs();
            Area = Dim.X * Dim.Y;
            // FIXME: Something is wrong with the coordinate system.
            Center = new Vector2(Max.X + Dim.X / 2, Max.Y - Dim.Y / 2);
        }

        public bool IsPointInside(Vector2 point)
        {
            return Min.X <= point.X && point.X <= Max.X
                && Min.Y <= point.Y && point.Y <= Max.Y;
        }

        public Rectangle ToRectangle()
        {
            return new Rectangle((int)Min.X, (int)Min.Y, (int)Math.Abs(Max.X - Min.X), (int)Math.Abs(Max.Y - Min.Y));
        }

        public override String ToString()
        {
            return "Max: " + Max + " Min: " + Min;
        }
    }
}
