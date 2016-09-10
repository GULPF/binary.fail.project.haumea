using System;
using System.Collections.Generic;

using NUnit.Framework;

using Haumea.Collections;

namespace unittests
{
    [TestFixture]
    public class NodeGraphTests : XnaTests
    {
        [Test]
        public void PathExists() 
        {                
            NodeGraph<int> graph = CreateGraph(false);

            Assert.True(graph.PathExists(1, 4), "One way graph");
            Assert.False(graph.PathExists(2, 3), "One way graph");

            graph = CreateGraph(true);

            Assert.True(graph.PathExists(1, 4), "Two way graph");
            Assert.False(graph.PathExists(2, 3), "Two way graph");
        }

        [Test]
        public void NeighborDistance()
        {
            NodeGraph<int> graph = CreateGraph(false);
            Assert.AreEqual(-1, graph.NeighborDistance(1, 4), "Non-neighbour");
            Assert.AreEqual( 4, graph.NeighborDistance(3, 6), "One jump");
        }

        [Test]
        public void Dijkstra()
        {
            NodeGraph<int> graph = CreateGraph(false);
            GraphPath<int> path  = graph.Dijkstra(1, 4);

            Assert.AreEqual(5, path.Cost);
            Assert.AreEqual(2, path.NJumps, "Jumps between 1 and 2, 2 and 4");
            Assert.AreEqual(1, path.Nodes[0]);
            Assert.AreEqual(2, path.Nodes[1]);
            Assert.AreEqual(4, path.Nodes[2]);

            Assert.Null(graph.Dijkstra(2, 6), "Failed dijkstra");

            graph = CreateGraph(true);
            path = graph.Dijkstra(4, 1);

            Assert.AreEqual(5, path.Cost,     "(twoway");
            Assert.AreEqual(2, path.NJumps,   "Jumps between 1 and 2, 2 and 4 (twoway)");
            Assert.AreEqual(4, path.Nodes[0], "(twoway");
            Assert.AreEqual(2, path.Nodes[1], "(twoway");
            Assert.AreEqual(1, path.Nodes[2], "(twoway");
        }

        private static NodeGraph<int> CreateGraph(bool twoway)
        {
            IList<Connector<int>> graphData = new List<Connector<int>> {
                new Connector<int>(1, 2, 2),
                new Connector<int>(2, 4, 3),
                new Connector<int>(3, 6, 4)
            };

            return new NodeGraph<int>(graphData, twoway);
        }
    }
}