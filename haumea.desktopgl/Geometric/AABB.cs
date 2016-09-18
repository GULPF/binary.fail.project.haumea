using System;
using Microsoft.Xna.Framework;

using Haumea.Rendering;

namespace Haumea.Geometric
{
    /// <summary>
    /// Axis Aligned Boundary Box.
    /// Borders are considered inside of the box.
    /// </summary>
    public class AABB : IHitable
    {
        public Vector2 TopLeft     { get; }
        public Vector2 BottomRight { get; }

        // These three props only have to be calculated once, 
        // but I believe the increased object size will have a greater performance impact.

        public Vector2 Center {
            get
            {
                return new Vector2(TopLeft.X + Dim.X / 2, TopLeft.Y + Dim.Y / 2);    
            }
        }

        public float Area {
            get
            {
                return Dim.X * Dim.Y;
            }
        }

        public Vector2 Dim {
            get
            {
                return BottomRight - TopLeft;
            }
        }

        public AABB(Vector2 v0, float width, float height) :
                this(v0, v0 + new Vector2(width, height)) {}

        /// <param name="v1">A corner point of the box</param>
        /// <param name="v2">The mirrored corner point of v1</param>
        public AABB(Vector2 v1, Vector2 v2)
        {
            TopLeft     = Vector2.Min(v1, v2);
            BottomRight = Vector2.Max(v1, v2);
        }

        public bool IsPointInside(Vector2 point)
        {
            return TopLeft.X <= point.X && point.X <= BottomRight.X
                && TopLeft.Y <= point.Y && point.Y <= BottomRight.Y;
        }

        // Scales the AABB while keeping the center of gravity.
        public AABB Scale(float scale)
        {
            Vector2 dimd = - Dim + new Vector2(
                (float)(Dim.X * Math.Sqrt(scale)),
                (float)(Dim.Y * Math.Sqrt(scale)));
            return new AABB(TopLeft - dimd / 2, BottomRight + dimd / 2);
        }

        public bool Intersects(AABB aabb)
        {
            if (aabb.Area > Area) return aabb.Intersects(this);

            return 
                   IsPointInside(aabb.TopLeft)
                || IsPointInside(aabb.BottomRight)
                || IsPointInside(new Vector2(aabb.TopLeft.X, aabb.BottomRight.Y))
                || IsPointInside(new Vector2(aabb.TopLeft.Y, aabb.BottomRight.X));
        }

        public AABB Move(Vector2 vd)
        {
            return new AABB(TopLeft + vd, BottomRight + vd);
        }

        public override string ToString()
        {
            return "Top left: " + TopLeft + " Bottom right: " + BottomRight;
        }
    }

}

