using System;

using NUnit.Framework;
using Microsoft.Xna.Framework;

using Haumea.Geometric;

namespace unittests
{
    [TestFixture]
    public class AABBTests
    {
        private AABB _aabb;

        [SetUp]
        public void Setup()
        {
            _aabb = new AABB(new Vector2(0, 0), new Vector2(10, 10));
        }

        [Test]
        public void Dimensions()
        {
            Assert.AreEqual(new Vector2(10, 10), _aabb.Dim);
            Assert.AreEqual(100, _aabb.Area);
            Assert.AreEqual(new Vector2(5f, 5f), _aabb.Center);
        }

        [Test]
        public void IsPointInside()
        {
            Vector2 v1 = new Vector2(0, 0);     // On the border
            Vector2 v2 = new Vector2(1, 1);     // Inside
            Vector2 v3 = new Vector2(14, 10);   // Outside
            Vector2 v4 = new Vector2(-10, -10); // Outside
            Vector2 v5 = new Vector2(10, 10);   // On the border

            Assert.True(_aabb.IsPointInside(v1),  "Border point");
            Assert.True(_aabb.IsPointInside(v2),  "Basic inside");
            Assert.False(_aabb.IsPointInside(v3), "Basic outside");
            Assert.False(_aabb.IsPointInside(v4), "Negative outside");
            Assert.True(_aabb.IsPointInside(v5),  "Border point");
        }

        [Test]
        public void ToRectangle()
        {
            Rectangle rect = _aabb.ToRectangle();
            Assert.AreEqual(10, rect.Width);
            Assert.AreEqual(10, rect.Height);
            Assert.AreEqual(0, rect.Left);
            Assert.AreEqual(0, rect.Top);
            Assert.AreEqual(10, rect.Right);
            Assert.AreEqual(10, rect.Bottom);
        }

        [Test]
        public void Scale() 
        {
            AABB bigger = _aabb.Scale(2);
            Assert.AreEqual(_aabb.Area * 2, bigger.Area, 0.0005);
            Assert.AreEqual(_aabb.Center, bigger.Center);
        }
    }
}