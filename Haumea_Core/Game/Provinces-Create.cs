using System;

using Microsoft.Xna.Framework;

using Haumea_Core.Geometric;

namespace Haumea_Core.Game
{
    public partial class Provinces
    {
        // Create the world.
        public static RawProvince[] CreateRaw()
        {
            // Just to make the polygon initialization a bit prettier
            Func<double, double, Vector2> V = (x, y) => new Vector2(20 * (float)x,  20 * (float)y);

            var polys = new Poly[]{
                new Poly(new Vector2[] { 
                    V(0, 0), V(1, 2), V(2, 2), V(3, 1), V(4, 1), V(5, 3),
                    V(7, 3), V(9, 4), V(12, 3), V(12, 1), V(9, 0), V(8, -1), V(8, -2),
                    V(6, -3), V(5, -2), V(3, -2), V(1, -1)
                }),
                new Poly(new Vector2[] {
                    V(0, 0), V(1, -1), V(3, -2), V(5, -2), V(6, -3), V(5, -4), V(5, -5), V(4, -6),
                    V(2, -6), V(0, -5), V(-2, -3), V(-2, -2), V(-3, -1), V(-2, 0)
                }),
                new Poly(new Vector2[] {
                    V(0, 0), V(1, 2), V(2, 2), V(3, 1), V(4, 1), V(5, 3),
                    V(3, 4), V(2, 4), V(0, 5), V(0, 6), V(-2, 6), V(-3, 5),
                    V(-5, 5), V(-7, 3), V(-7, 1), V(-8, 0), V(-8, -1), V(-7, -2), V(-4, -3),
                    V(0, -5), V(-2, -3), V(-2, -2), V(-3, -1), V(-2, 0)
                })
            };

            var rawProvinces = new Provinces.RawProvince[3];
            rawProvinces[0] = new Provinces.RawProvince(polys[0], "P1", "DAN", Color.Red);
            rawProvinces[1] = new Provinces.RawProvince(polys[1], "P2", "TEU", Color.DarkGoldenrod);
            rawProvinces[2] = new Provinces.RawProvince(polys[2], "P3", "TEU", Color.Brown); 

            return rawProvinces;
        }
    }
}

