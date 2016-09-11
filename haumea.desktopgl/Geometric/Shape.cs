using System;
using System.Linq;
using Microsoft.Xna.Framework;


namespace Haumea.Geometric
{
    public class Shape : IShape
    {
        public IShape[] Shapes { get; }

        public Shape(IShape[] shapes)
        {
            Shapes = shapes;
        }
            
        public bool IsPointInside(Vector2 point)
        {
            return Shapes.Any(s => s.IsPointInside(point));
        }
    }
}

