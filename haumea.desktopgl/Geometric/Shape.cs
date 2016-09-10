using System;
using System.Linq;
using Microsoft.Xna.Framework;


namespace Haumea.Geometric
{
    public class Shape : IHitable
    {
        public IHitable[] Shapes { get; }

        public Shape(IHitable[] shapes)
        {
            Shapes = shapes;
        }
            
        public bool IsPointInside(Vector2 point)
        {
            return Shapes.Any(s => s.IsPointInside(point));
        }
    }
}

