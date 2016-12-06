using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Haumea.Geometric;

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

        public static Rectangle ToRectangle(this AABB aabb)
        {
            Vector2 dim = aabb.Dim;
            return new Rectangle(
                (int)aabb.TopLeft.X, (int)aabb.TopLeft.Y,
                (int)(dim.X), (int)(dim.Y));
        }

        public static Vector2 GetScreenDimensions(this SpriteBatch spriteBatch)
        {
            return new Vector2(spriteBatch.GraphicsDevice.PresentationParameters.BackBufferWidth,
                spriteBatch.GraphicsDevice.PresentationParameters.BackBufferHeight);
        }

        public static string[] Split(this string str, char khar)
        {
            return str.Split(new [] { khar });
        }

        public static string[] Split(this string str, string khar)
        {
            return str.Split(new [] { khar }, StringSplitOptions.None);
        }

        public static bool IsPointInside(this Rectangle rect, Point point)
        {
            return rect.Left <= point.X && point.X <= rect.Right
                && rect.Top <= point.Y && point.Y <= rect.Bottom;
        }

        public static int Area(this Rectangle rect)
        {
            return rect.Width * rect.Height;
        }

        public static AABB[] Borders(this AABB aabb, int thickness)
        {
            Vector2 topLeft = aabb.TopLeft;
            Vector2 dim     =aabb.Dim;

            var top    = new AABB(topLeft - thickness * Vector2.One, dim.X + 2 * thickness, thickness);
            var left   = new AABB(topLeft                          , -thickness           , dim.Y + thickness);
            var right  = new AABB(topLeft + dim.X * Vector2.UnitX  , thickness            , dim.Y);
            var bottom = new AABB(topLeft + dim.Y * Vector2.UnitY  , dim.X + thickness    , thickness);

            return new [] { top, left, bottom, right };
        }

        private static Texture2D _pixel;

        public static void Draw(this SpriteBatch spriteBatch, AABB aabb, Color color)
        {
            if (_pixel == null)
            {
                _pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _pixel.SetData<Color>(new[] { Color.White });
            }

            spriteBatch.Draw(_pixel, aabb.ToRectangle(), color);
        }

        public static void Draw(this SpriteBatch spritebatch, AABB abbb, Color color,
            int borderWidth, Color borderColor)
        {
            spritebatch.Draw(abbb, color);
            spritebatch.Draw(abbb.Borders(borderWidth), borderColor);
        }

        public static void Draw(this SpriteBatch spriteBatch, AABB[] aabbs, Color color)
        {
            foreach (AABB aabb in aabbs)
            {
                spriteBatch.Draw(aabb, color);
            }
        }

        //
        // IEnumerable
        //

        // In general, Linq should be prefered over defining methods here.
        // In the cases where Linq uses exceptions that are replaceable with the `Try...` pattern,
        // it's ok to make a new method though (see TryFind, TryFirst).

        public static void ForEach<T>(this IEnumerable<T> enumer, Action<T> func)
        {
            foreach (T t in enumer)
            {
                func(t);
            }
        }

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

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumer)
        {
            return new HashSet<T>(enumer);
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
