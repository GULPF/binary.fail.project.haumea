using System;
using Microsoft.Xna.Framework;

namespace Haumea_Core.Geometric
{
    public interface IHitable
    {
        bool IsPointInside(Vector2 point);
    }
}

