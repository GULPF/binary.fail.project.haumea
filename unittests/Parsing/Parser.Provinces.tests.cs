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
        public void Polygon()
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

        private static StreamReader ToStreamReader(string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            return new StreamReader(new MemoryStream(bytes));
        }
    }
}

