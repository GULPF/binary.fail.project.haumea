using System;
using NUnit.Framework;
using Microsoft.Xna.Framework;
using Haumea.Geometric;

namespace unittests
{
    [TestFixture]
    public class PolyTests : XnaTests
    {
        private Poly _poly;
        private Poly _nonOrigoPoly;

        [SetUp]
        public void Setup()
        {
            _poly = new Poly(new [] { V2(0, 0), V2(3, 2), V2(3, 6), V2(0, 4) });
            _nonOrigoPoly = new Poly(Rectangle(1, 1, 1, 1));
        }

        [Test]
        public void Properties()
        {
            Assert.AreEqual(4, _poly.Points.Length);
            Assert.AreEqual(0, _poly.Holes.Length);
            Assert.AreNotEqual(V2(0, 0), _nonOrigoPoly.Boundary.TopLeft);
            Assert.AreEqual(1, _nonOrigoPoly.Boundary.Area);
            Assert.AreEqual(V2(1.5f, 1.5f), _nonOrigoPoly.Boundary.Center);
        }

        [Test]
        public void IsPointInside()
        {
            Assert.True(_poly.IsPointInside(V2(1, 2)),  "Basic inside");
            Assert.False(_poly.IsPointInside(V2(2, 1)), "Basic outside");
            Assert.True(_poly.IsPointInside(V2(3, 2)),  "Border");
            Assert.True(_poly.IsPointInside(V2(0, 0)),  "Border");
            Assert.False(_poly.IsPointInside(V2(0, 0), false), "Border with ignoreborder");
        }

        [Test]
        public void CalculateCentroid()
        {
            // Calculated with wolfram alpha.
            Vector2 answer = V2(1.5f, 3f);
            Assert.AreEqual(answer, _poly.CalculateCentroid());
        }
        }
}

