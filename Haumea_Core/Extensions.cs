using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Haumea_Core
{
    public static class VectorExtensions
    {

        //
        // Vectors
        //

        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static Vector2 ToVector2(this Vector3 v3d)
        {
            return new Vector2(v3d.X, v3d.Y);
        }

        public static Vector3 ToVector3(this Vector2 v2d, float z = 0)
        {
            return new Vector3(v2d.X, v2d.Y, z);
        }

        //
        // Color
        //

        public static Color Darken(this Color color, float factor = 0.8f)
        {
            return new Color(
                (int)(color.R * factor),
                (int)(color.G * factor),
                (int)(color.B * factor));
        }

        public static Color Lighten(this Color color, float factor = 0.4f)
        {
            return new Color(
                (int)(color.R + (255 - color.R) * factor),
                (int)(color.G + (255 - color.G) * factor),
                (int)(color.B + (255 - color.B) * factor));
        }

        //
        // Other
        //

        public static Vector2 GetScreenDimensions(this GraphicsDevice device)
        {
            return new Vector2(device.PresentationParameters.BackBufferWidth,
                device.PresentationParameters.BackBufferHeight);
        }
    }
}
