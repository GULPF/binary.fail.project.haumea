using System;

using Microsoft.Xna.Framework;

namespace Haumea.Geometric
{
    public interface IPoly : IHitable
    {
        // The boundary box of the polygon,
        // defined as the smallest rectangle the polygon fits in.
        AABB Boundary { get; }

        /// <summary>
        /// All points on the polygons outline, in order.
        /// </summary>
        Vector2[] Points { get; }

        IPoly[] Holes { get; }

        /// <summary>
        /// Check if a point is within the polygon.
        /// </summary>
        /// <returns><c>true</c> if the points is within the polygon, otherwise <c>false</c>.</returns>
        /// <param name="point">The point</param>
        /// <param name="includeBorder">Indicates wether the border of the polygon should be considered "inside" or "outside"</param>
        bool IsPointInside(Vector2 point, bool includeBorder);

        Vector2 CalculateCentroid();
    }
}

