using Microsoft.Xna.Framework;

namespace Haumea_Core
{
    public static class VectorExtensions
    {
        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static Vector2 ToVector2(this Vector3 v3d)
        {
            return new Vector2(v3d.X, v3d.Y);
        }

        public static Vector3 ToVector3(this Vector2 v2d)
        {
            return new Vector3(v2d.X, v2d.Y, 0);
        }
    }
}

