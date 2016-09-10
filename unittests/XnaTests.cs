using System;
using Microsoft.Xna.Framework;

namespace unittests
{
    public class XnaTests
    {
        public Vector2 V2(float x, float y)
        {
            return new Vector2(x, y);
        }

        public Vector2[] Rectangle(int x0, int y0, int xd, int yd)
        {
            return new [] { V2(x0, y0), V2(x0 + xd, y0), V2(x0 + xd, y0 + yd), V2(x0, y0 + yd) };
        }
    }
}

