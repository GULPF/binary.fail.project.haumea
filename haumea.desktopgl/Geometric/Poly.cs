using System;
using Microsoft.Xna.Framework;


namespace Haumea.Geometric
{

    public class Poly : IPoly
    {
        public AABB Boundary { get; }
        public Vector2[] Points { get; }
        public IPoly[] Holes { get; } = {};

        public Poly (Vector2[] points) {
            Debug.Assert(points.Length > 2, "A polygon requires atleast three points");
            Points  = points;
            Boundary = CalculateBoundary(Points);
        }

        public bool IsPointInside(Vector2 v)
        {
            return IsPointInside(v, true);
        }

        public bool IsPointInside(Vector2 v, bool includeBorder)
        {

            if (!Boundary.IsPointInside(v)) return false;

            // http://stackoverflow.com/questions/217578/

            int i, j;
            bool c = false;
            for (i = 0, j = Points.Length - 1; i < Points.Length; j = i++) {

                // There are some unpredictable behavior for some points (like 0, 0) when they are on top
                // of a polygon point. This seems to work.
                if (Points[j] == v || Points[i] == v) return includeBorder;
                    
                float monsterexpr =
                    (Points[j].X - Points[i].X)
                    *   (v.Y - Points[i].Y) / (Points[j].Y - Points[i].Y)
                    +   Points[i].X;
                if ( ((Points[i].Y > v.Y) != (Points[j].Y > v.Y)) &&
                    // NOTE: The source uses `<`, not `<=`.
                    //       `<=` means that the border is considered part of the polygon.
                    ((includeBorder && v.X <= monsterexpr) || (v.X < monsterexpr)))
                {
                    c = !c;
                }
            }

            return c;
        }

        // TODO: I'm fairly certain that this is currently bullshit.
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
            
        public static AABB CalculateBoundary(Vector2[] points)
        {
            Vector2 max = float.MinValue * Vector2.One;
            Vector2 min = float.MaxValue * Vector2.One;

            foreach (Vector2 vector in points)
            {
                max.X = MathHelper.Max(max.X, vector.X);
                max.Y = MathHelper.Max(max.Y, vector.Y);
                min.X = MathHelper.Min(min.X, vector.X);
                min.Y = MathHelper.Min(min.Y, vector.Y);
            }

            return new AABB(max, min);
        }
    }
}

