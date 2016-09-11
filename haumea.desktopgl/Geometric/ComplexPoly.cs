using System;
using System.Linq;
using Microsoft.Xna.Framework;


namespace Haumea.Geometric
{
    /// <summary>
    /// Polygon with any number of holes.
    /// NOTE: Does not allow holes to holes of their own. Only enforced at runtime, and only in debug.
    /// </summary>
    public class ComplexPoly : IPoly
    {
        public AABB Boundary
        {
            get { return _poly.Boundary; }
        }

        public Vector2[] Points
        {
            get { return _poly.Points; }
        }

        public IPoly[] Holes { get; }

        private readonly IPoly _poly;

        public ComplexPoly(Vector2[] points, Vector2[][] holes)
        {
            Holes = new Poly[holes.Length];

            for (int n = 0; n < holes.Length; n++)
            {
                Holes[n] = new Poly(holes[n]);
            }

            _poly  = new Poly(points);
        }

        public ComplexPoly(IPoly poly, IPoly[] holes)
        {
            Debug.Assert(poly.Holes.Length == 0, "Outline can not have holes");
            Debug.Assert(holes.All(p => p.Holes.Length == 0), "Holes can not have holes");
            Holes = holes;
            _poly = poly;
        }

        public bool IsPointInside(Vector2 v)
        {
            return IsPointInside(v, true);
        }

        public bool IsPointInside(Vector2 v, bool includeBorder)
        {
            return _poly.IsPointInside(v, includeBorder) &&
                Holes.All(p => !p.IsPointInside(v, !includeBorder));
        }

        public Vector2 CalculateCentroid()
        {
            return _poly.CalculateCentroid();
        }
    }
}