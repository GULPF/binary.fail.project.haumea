using System;
using Microsoft.Xna.Framework;

namespace Haumea.Geometric
{
    public interface IHitable
    {
        bool IsPointInside(Vector2 point);
    }
}

