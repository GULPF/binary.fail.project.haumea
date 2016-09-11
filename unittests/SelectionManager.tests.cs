using System;
using System.Collections.Generic;
using NUnit.Framework;
using Haumea.Components;

namespace unittests
{
    [TestFixture]
    public class SelectionManagerTests
    {
        [Test]
        public void EmptySelection()
        {
            SelectionManager<int> selection = new SelectionManager<int>();
            Assert.True(selection.IsEmpty);
            Assert.AreEqual(0, selection.Selected.Count);
        }

        [Test]
        public void MakeSelection()
        {
            var selection  = new SelectionManager<int>();

            Assert.True(true);
        }
    }
}

