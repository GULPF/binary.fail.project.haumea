using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

using Haumea.Geometric;

namespace unittests
{
    [TestFixture]
    public class ComplexPolyTests : XnaTests
    {
        [Test]
        public void IsPointInside()
        {
            Vector2[] points     = { V2(0, 0), V2(5, 0), V2(5, 8), V2(0, 8) };
            Vector2[] holePoints = { V2(2, 3), V2(4, 3), V2(4, 5), V2(2, 5) };

            IPoly poly = new ComplexPoly(points, new Vector2[][] { holePoints });

            Assert.AreEqual(1, poly.Holes.Length);
            Assert.True(poly.IsPointInside(V2(1, 1)),  "Inside");
            Assert.True(poly.IsPointInside(V2(0, 0)),  "Border");
            Assert.False(poly.IsPointInside(V2(3, 4)), "Inside hole");
            Assert.True(poly.IsPointInside(V2(2, 3)),  "Hole border");
            Assert.False(poly.IsPointInside(V2(2, 3), false),  "Hole border with ignoreborder");
        }

        // Nested holes, e.g a lake with an island
        // ##################
        // #####------#######
        // #####-##---#######
        // #####-##---#######
        // #####------#######
        // ##################
        [Test]
        public void Holeception()
        {
            Vector2[] points       = { V2(0, 0), V2(10, 0), V2(10, 10), V2(0, 10) };
            Vector2[] lakePoints   = { V2(2, 2), V2(7, 2), V2(7, 8), V2(2, 8) };
            Vector2[] islandPoints = { V2(4, 4), V2(6, 4), V2(6, 7), V2(4, 7) };

            IPoly innerPoly = new ComplexPoly(lakePoints, new [] {islandPoints });
            IPoly poly      = new ComplexPoly(new Poly(points), new [] { innerPoly });

            Assert.False(poly.IsPointInside(V2(3, 3)), "In the lake");
            Assert.True(poly.IsPointInside(V2(5, 5)), "On the island");
        }
    }
}

