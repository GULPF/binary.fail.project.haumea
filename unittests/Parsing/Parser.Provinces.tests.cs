using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Microsoft.Xna.Framework;
using Haumea.Parsing;

namespace unittests
{
    [TestFixture]
    public class ParserProvincesTests
    {
        [Test]
        public void SimpleProps()
        {
            const string parseText =
@"
§P1 #03020F
(0, 0) % (0, 5) % (5, 5) % (5, 0)
\ (1, 1) % (1, 2) % (2, 2) % (2, 1)
\ (3, 3) % (3, 4) % (4, 4) % (4, 3)";
        
            var rProvinces = Parser.Provinces(parseText);

            Assert.AreEqual(1, rProvinces.Count);
            RawProvince rProvince = rProvinces[0];
            Assert.AreEqual("P1", rProvince.Tag);
            Assert.AreEqual(new Color(3, 2, 15), rProvince.Color);
        }

        [Test]
        public void TwoPolygon()
        {
            const string parseText =
@"§P1 #03020F
(0, 0) % (0, 5) % (5, 5) % (5, 0)
\ (1, 1) % (1, 2) % (2, 2) % (2, 1)
\ (3, 3) % (3, 4) % (4, 4) % (4, 3)
(10, 10) % (20, 10) % (20, 20) % (15, 20) % (10, 20)";

            var rProvince = Parser.Provinces(parseText)[0];
            Assert.AreEqual(2, rProvince.Polys.Count);
            Assert.AreEqual(2, rProvince.Polys[0].Holes.Length);
            Assert.AreEqual(0, rProvince.Polys[1].Holes.Length);
            Assert.AreEqual(4, rProvince.Polys[0].Points.Length);
            Assert.AreEqual(5, rProvince.Polys[1].Points.Length);
        }

        [Test]
        public void OnePolygon()
        {
            const string parseText =
@"§P1 #B82E00
(0, 0) % (0, 5) % (5, 5) % (5, 0)
\ (1, 1) % (1, 2) % (2, 2) % (2, 1)
\ (3, 3) % (3, 4) % (4, 4) % (4, 3)";

            var rProvince = Parser.Provinces(parseText)[0];
            Assert.AreEqual(1, rProvince.Polys.Count);
            Assert.AreEqual(2, rProvince.Polys[0].Holes.Length);;
        }

        [Test]
        public void TwoProvinces()
        {
            const string parseText =
@"§P1 #B82E00
(0, 0) % (2, 1) % (3, 1) % (3, 3) % (2, 6) % (-2, 6) % (-3, 4) % (-1, 2)
§P4 #F5B815
(16, 3) % (17, 5) % (16, 7) % (16, 9) % (14, 10) % (12, 7) % (13, 4)";

            var rProvinces = Parser.Provinces(parseText);
            Assert.AreEqual(2, rProvinces.Count);
            Assert.AreEqual(1, rProvinces[0].Polys.Count);
            Assert.AreEqual(1, rProvinces[1].Polys.Count);
            Assert.AreEqual(8, rProvinces[0].Polys[0].Points.Length);
            Assert.AreEqual(7, rProvinces[1].Polys[0].Points.Length);
        }
    }
}

