using System;
using Microsoft.Xna.Framework;

namespace Haumea.Geometric
{
    public interface IShape
    {
        bool IsPointInside(Vector2 point);
    }
}

