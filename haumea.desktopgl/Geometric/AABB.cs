﻿using System;
using Microsoft.Xna.Framework;

using Haumea.Rendering;

namespace Haumea.Geometric
{
    /// <summary>
    /// Axis Aligned Boundary Box.
    /// Borders are considered inside of the box.
    /// </summary>
    public class AABB : IShape
    {
        // This is probably to damn many props for this class.
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

        /// <param name="v1">A corner point of the box</param>
        /// <param name="v2">Another corner point of the box</param>
        public AABB(Vector2 v1, Vector2 v2)
        {
            float x0, x1, y0, y1;

            if (v1.X <= v2.X)
            {
                x0 = v1.X;
                x1 = v2.X;
            } else {
                x0 = v2.X;
                x1 = v1.X;
            }

            if (v1.Y <= v2.Y)
            {
                y0 = v1.Y;
                y1 = v2.Y;
            } else {
                y0 = v2.Y;
                y1 = v1.Y;
            }

            TopLeft = new Vector2(x0, y0);
            BottomRight = new Vector2(x1, y1);
        }

        public bool IsPointInside(Vector2 point)
        {
            return TopLeft.X <= point.X && point.X <= BottomRight.X
                && TopLeft.Y <= point.Y && point.Y <= BottomRight.Y;
        }
            
        public Rectangle ToRectangle()
        {
            return new Rectangle(
                (int)TopLeft.X, (int)TopLeft.Y,
                (int)(Dim.X), (int)(Dim.Y));
        }

        // Scales the AABB while keeping the center of gravity.
        public AABB Scale(float scale)
        {
            Vector2 dimd = - Dim + new Vector2(
                (float)(Dim.X * Math.Sqrt(scale)),
                (float)(Dim.Y * Math.Sqrt(scale)));
            return new AABB(TopLeft - dimd / 2, BottomRight + dimd / 2);
        }

        public bool DoOverlap(AABB aabb)
        {
            if (aabb.Area > Area) return aabb.DoOverlap(this);

            return 
                   IsPointInside(aabb.TopLeft)
                || IsPointInside(aabb.BottomRight)
                || IsPointInside(new Vector2(aabb.TopLeft.X, aabb.BottomRight.Y))
                || IsPointInside(new Vector2(aabb.TopLeft.Y, aabb.BottomRight.X));
        }

        public override string ToString()
        {
            return "Top left: " + TopLeft + " Bottom right: " + BottomRight;
        }
    }

}
