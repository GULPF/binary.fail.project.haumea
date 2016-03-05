using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Haumea_Core.Geometric;
using Haumea_Core.Collections;

namespace Haumea_Core.Game
{
    public partial class Provinces
    {
        // Create the world.
        public static RawProvince[] CreateRaw()
        {
            // Just to make the polygon initialization a bit prettier
            Func<double, double, Vector2> V = (x, y) => new Vector2(20 * (float)x,  20 * (float)y);

            Poly[] polys = {
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
                }),
                new Poly(new Vector2[] {
                    V(-15, -15), V(-10, -12), V(-16, -5)
                })
            };

            Provinces.RawProvince[] rawProvinces = {
                new Provinces.RawProvince(polys[0], "P1", "DAN", Color.Red, 1),
                new Provinces.RawProvince(polys[1], "P2", "TEU", Color.DarkGoldenrod, 2),
                new Provinces.RawProvince(polys[2], "P3", "TEU", Color.Brown, 0),
                new Provinces.RawProvince(polys[3], "P4", "GOT", Color.BurlyWood, 1)}; 

            return rawProvinces;
        }

        public static NodeGraph<int> CreateMapGraph()
        {
            IList<Connector<int>> data = new List<Connector<int>>{
                new Connector<int>(0, 1, 2),
                new Connector<int>(0, 2, 1),
                new Connector<int>(2, 1, 3),
                new Connector<int>(2, 3, 5)
            };

            NodeGraph<int> graph = new NodeGraph<int>(data, true);

            return graph;
        }
    }
}

