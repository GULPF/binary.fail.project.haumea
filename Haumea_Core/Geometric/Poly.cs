using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Haumea_Core.Geometric
{

    public class Poly : IHitable
    {
        public AABB Boundary { get; }
        public Vector2[] Points { get; }

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

            Boundary = new AABB(max, min);
        }

        public bool IsPointInside(Vector2 point)
        {
            if (!Boundary.IsPointInside(point)) return false;

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

        public Poly RotateLeft90()
        {
            Vector2[] points = new Vector2[Points.Length];
            int index = 0;

            foreach (Vector2 v in Points)
            {
                points[index++] = v.RotateLeft90();
            }

            return new Poly(points);
        }

        // For simplicity, we assume that the polygons are not overlapping.
        // This is not exactly fast (O(n²)). There should be a faster way to do this.
        // The poblem is that the order of vertices in the new poly matters.
        public Poly Merge(Poly p)
        {
            var newPoints = new List<Vector2>();

            foreach (Vector2 v1 in p.Points)
            {
                bool unique = true;

                foreach (Vector2 v2 in this.Points)
                {
                    if (v1 == v2)
                    {
                        unique = false;
                        break;
                    }
                }

                if (unique)
                {
                    newPoints.Add(v1);
                }
            }

            foreach (Vector2 v1 in this.Points)
            {
                bool unique = true;

                foreach (Vector2 v2 in p.Points)
                {
                    if (v1 == v2)
                    {
                        unique = false;
                        break;
                    }
                }

                if (unique)
                {
                    newPoints.Add(v1);
                }
            }

            return new Poly(newPoints.ToArray());
        }

        // http://stackoverflow.com/questions/5271583/
        public Vector2 CalculateCentroid()
        {
            float sum = 0.0f;
            Vector2 vsum = Vector2.Zero;

            for (int i = 0; i < Points.Length; i++){
                Vector2 v1 = Points[i];
                Vector2 v2 = Points[(i + 1) % Points.Length];
                float cross = v1.X * v2.Y - v1.Y * v2.X;
                sum += cross;
                vsum = new Vector2(((v1.X + v2.X) * cross) + vsum.X, ((v1.Y + v2.Y) * cross) + vsum.Y);
            }

            float z = 1.0f / (3.0f * sum);
            return new Vector2(vsum.X * z, vsum.Y * z);
        }
    }
}

