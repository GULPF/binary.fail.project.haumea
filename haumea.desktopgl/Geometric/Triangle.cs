using System;
using Microsoft.Xna.Framework;

namespace Haumea.Geometric
{
    class Triangle
    {
        public Vertex V1 { get; }
        public Vertex V2 { get; }
        public Vertex V3 { get; }

        public Vector2 B1 { get; }
        public Vector2 B2 { get; }

        public Triangle(Vertex v1, Vertex v2, Vertex v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;

            // Boundary box.
            float b1x = Math.Min(Math.Min(v1.Position.X, v2.Position.X), v3.Position.X);
            float b1y = Math.Min(Math.Min(v1.Position.Y, v2.Position.Y), v3.Position.Y);
            float b2x = Math.Max(Math.Max(v1.Position.X, v2.Position.X), v3.Position.X);
            float b2y = Math.Max(Math.Max(v1.Position.Y, v2.Position.Y), v3.Position.Y);
            B1 = new Vector2(b1x, b1y);
            B2 = new Vector2(b2x, b2y);
        }
            
        public bool ContainsPoint(Vertex point)
        {
            if (B1.X <= point.Position.X && point.Position.X <= B2.X
                && B1.Y <= point.Position.Y && point.Position.Y <= B2.Y)
            {
                bool oddNodes = false;

                if (CheckPointToSegment(V3, V1, point))
                    oddNodes = !oddNodes;
                if (CheckPointToSegment(V1, V2, point))
                    oddNodes = !oddNodes;
                if (CheckPointToSegment(V2, V3, point))
                    oddNodes = !oddNodes;

                return oddNodes;
            }

            return false;
        }

        private static bool CheckPointToSegment(Vertex sA, Vertex sB, Vertex point)
        {
            if ((sA.Position.Y < point.Position.Y && sB.Position.Y >= point.Position.Y) ||
                (sB.Position.Y < point.Position.Y && sA.Position.Y >= point.Position.Y))
            {
                float x = 
                    sA.Position.X + 
                    (point.Position.Y - sA.Position.Y) / 
                    (sB.Position.Y - sA.Position.Y) * 
                    (sB.Position.X - sA.Position.X);

                if (x < point.Position.X)
                    return true;
            }

            return false;
        }

        public bool HavePoint(Vertex point)
        {
            return V1.Equals(point) || V2.Equals(point) || V3.Equals(point);
        }
    }
}