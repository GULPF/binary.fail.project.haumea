using System;
using NUnit.Framework;
using Microsoft.Xna.Framework;
using Haumea;

namespace unittests
{
    [TestFixture]
    public class ExtensionsTests : XnaTests  
    {
        [Test]
        public void VectorAngle()
        {
            Vector2 v1 = V2(1, 0);
            Vector2 v2 = V2(0, 1);
            Assert.AreEqual(MathHelper.ToRadians(90), v1.Angle(v2), 0.0005);
        }

        [Test]
        public void PointAngle()
        {
            Vector2 v1 = Vector2.Zero;
            Vector2 v2 = V2(1, 0);
            Vector2 v3 = V2(0, 1);
            Assert.AreEqual(MathHelper.ToRadians(90), v1.Angle(v2, v3), 0.0005);
        }
    }
}

