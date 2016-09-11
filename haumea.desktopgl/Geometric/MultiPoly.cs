using System;
using System.Linq;
using Microsoft.Xna.Framework;


namespace Haumea.Geometric
{
    public class MultiPoly : IHitable
    {
        public IPoly[] Polys { get; }

        public MultiPoly(IPoly[] polys)
        {
            Polys = polys;
        }
            
        public bool IsPointInside(Vector2 point)
        {
            return Polys.Any(s => s.IsPointInside(point));
        }
    }
}

