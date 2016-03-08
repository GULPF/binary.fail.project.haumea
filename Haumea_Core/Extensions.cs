using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Haumea_Core
{
    public static class Extensions
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

        public static Vector2 Abs(this Vector2 v2)
        {
            return new Vector2(Math.Abs(v2.X), Math.Abs(v2.Y));
        }

        public static Vector2 RotateLeft90(this Vector2 v) {
            return new Vector2(-v.Y, v.X);
        }

        public static Vector2 RotateRight90(this Vector2 v) {
            return new Vector2(v.Y, -v.X);
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

        public static string[] Split(this string str, char khar)
        {
            return str.Split(new char[] { khar });
        }

        public static bool IsPointInside(this Rectangle rect, Point point)
        {
            return rect.Left <= point.X && point.X <= rect.Right
                && rect.Top <= point.Y && point.Y <= rect.Bottom;
        }

        //
        // IEnumerable
        //

        // These two are a bit silly...

        // Creates a single IEnumerable<T> from several others.
        public static IEnumerable<T> Union<T>(this IEnumerable<T> enumer1, params IEnumerable<T>[] enumer2)
        {
            foreach (T t in enumer1)
            {
                yield return t;
            }

            foreach (IEnumerable<T> enumerx in enumer2)
            {
                foreach (T t in enumerx)
                {
                    yield return t;
                }
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumers)
        {
            foreach (IEnumerable<T> enumer in enumers)
            {
                foreach(T t in enumer)
                {
                    yield return t;
                }
            }
        }

        public static string Join<T>(this IEnumerable<T> enumer, string sep)
        {
            StringBuilder sb = new StringBuilder();
            var itr = enumer.GetEnumerator();
            itr.MoveNext();
            sb.Append(itr.Current);

            while (itr.MoveNext()){
                sb.Append(sep);
                sb.Append(itr.Current.ToString());
            }

            return sb.ToString();
        }
    }
}
