using System;
using System.IO;
using NUnit.Framework;
using Haumea.Parsing;

namespace unittests
{
    [TestFixture]
    public class GameFileTests
    {
        [Test]
        public void EmptyFile()
        {
            const string file = "";

            Assert.DoesNotThrow(
                () => GameFile.Parse(new StringReader(file)), "Empty file should be parseable");
        }

        [Test]
        public void EmptyGroups()
        {
            const string file =
@"
[provinces]
[water]
[realms]
[graph]
[armies]
";
            Assert.DoesNotThrow(
                () => GameFile.Parse(new StringReader(file)), "Empty groups should be parseable");
        }

        [Test]
        public void IncompleteGroups()
        {
            const string file =
@"
[water]
[graph]
";

            Assert.DoesNotThrow(
                () => GameFile.Parse(new StringReader(file)), "Missing groups should be parseable");
        }

        [Test]
        public void Comments()
        {
            const string file =
@"
// And this!
[provinces]
// This is a comment!
[water]
[realms]
[graph]
[armies]
// All should be ignored!
";            

            Assert.DoesNotThrow(
                () => GameFile.Parse(new StringReader(file)), "Comments should be ignored");
        }
    }
}

