using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Haumea
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

        public static Vector2 Floor(this Vector2 v2)
        {
            return new Vector2((int)v2.X, (int)v2.Y);
        }

        public static Vector2 RotateLeft90(this Vector2 v) {
            return new Vector2(-v.Y, v.X);
        }

        public static Vector2 RotateRight90(this Vector2 v) {
            return new Vector2(v.Y, -v.X);
        }

        // Angle between vectors
        public static double Angle(this Vector2 v1, Vector2 v2)
        {
            return Math.Atan2(v2.Y, v2.X) - Math.Atan2(v1.Y, v1.X);
        }

        // Angle between vector-points
        public static double Angle(this Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Vector2 u1 = p2 - p1;
            Vector2 u2 = p3 - p1;
            return Angle(u1, u2);
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
            return str.Split(new [] { khar });
        }

        public static bool IsPointInside(this Rectangle rect, Point point)
        {
            return rect.Left <= point.X && point.X <= rect.Right
                && rect.Top <= point.Y && point.Y <= rect.Bottom;
        }

        public static Rectangle[] Borders(this Rectangle rect, int thickness)
        {
            return new []{
                new Rectangle(rect.Left, rect.Top, rect.Width, thickness),
                new Rectangle(rect.Right, rect.Top, thickness, rect.Height),
                new Rectangle(rect.Left, rect.Top, thickness, rect.Height),
                new Rectangle(rect.Left, rect.Bottom, rect.Width + thickness, thickness)
            };
        }

        public static void Draw(this SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            Texture2D texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            texture.SetData<Color>(new [] { Color.White });

            spriteBatch.Draw(texture, rect, color);

        }

        public static void Draw(this SpriteBatch spriteBatch, Rectangle[] rects, Color color)
        {
            foreach (Rectangle rect in rects)
            {
                spriteBatch.Draw(rect, color);
            }
        }

        //
        // IEnumerable
        //

        // In general, Linq should be prefered over defining methods here.
        // In the cases where Linq uses exceptions that are replaceable with the `Try...` pattern,
        // it's ok to make a new method though (see TryFind, TryFirst).

        public static string Join<T>(this IEnumerable<T> enumer, string sep)
        {
            StringBuilder sb = new StringBuilder();

            using (var itr = enumer.GetEnumerator())
            {
                itr.MoveNext();
                sb.Append(itr.Current);

                while (itr.MoveNext()){
                    sb.Append(sep);
                    sb.Append(itr.Current.ToString());
                }    
            }

            return sb.ToString();
        }

        public static bool TryFind<T>(this IEnumerable<T> enumer, out T found, Func<T, bool> predicate)
        {
            foreach (T t in enumer)
            {
                if (predicate(t))
                {
                    found = t;
                    return true;
                }
            }

            found = default(T);
            return false;
        }

        public static bool TryFirst<T>(this IEnumerable<T> enumer, out T first)
        {
            using (var itr = enumer.GetEnumerator())
            {
                if (itr.MoveNext())
                {
                    first = itr.Current;
                    return true;
                }
            }

            first = default(T);
            return false;
        }

        public static IEnumerable<T> FindAll<T>(this IEnumerable<T> enumer, Func<T, bool> predicate)
        {
            //List<T> foundList = new List<T>(1);

            foreach (T t in enumer)
            {
                if (predicate(t)) yield return t;
            }

            //return foundList.ToArray();
        }

        // Non-extenion methods

        private static readonly Random rnd = new Random();

        public static Color RndColor()
        {
            return new Color(
                (float)rnd.NextDouble(),
                (float)rnd.NextDouble(),
                (float)rnd.NextDouble());
        }
    }
}
