using System;
using System.Collections.Generic;

namespace Haumea_Core.Collections
{
    // Because it is a major pain to do arethmics with generics in C#,
    // distances are hardcoded as ints.
    public class NodeGraph<N>
    {
        public IDictionary<N, Connector<N>[]> Nodes { get; }

        public NodeGraph(IDictionary<N, Connector<N>[]> nodes)
        {
            Nodes = nodes;
        }

        // Breadth-first search.
        public bool PathExists(N from, N to)
        {
            ISet<N> visited = new HashSet<N>();
            Queue<N> nextLevel = new Queue<N>();

            nextLevel.Enqueue(from);
            visited.Add(from);

            while (nextLevel.Count > 0)
            {
                foreach (Connector<N> conn in Nodes[nextLevel.Dequeue()])
                {
                    if (conn.To.Equals(to)) return true;

                    if (!visited.Contains(conn.To))
                    {
                        nextLevel.Enqueue(conn.To);    
                        visited.Add(conn.To);
                    }
                }
            }

            return false;
        }

        // Find shortest path between from and to.
        public GraphPath<N> Dijkstra(N from, N to)
        {
            IDictionary<N, int> accumulatedCosts = new Dictionary<N, int>(Nodes.Count);
            IDictionary<N, N>   previusInPath    = new Dictionary<N, N>(Nodes.Count);
            ISet<N> done = new HashSet<N>();
            SortedList<Pair> priorityQueue = new SortedList<Pair>();

            accumulatedCosts.Add(from, 0);
            priorityQueue.Add(new Pair(from, 0));

            while (priorityQueue.Count > 0)
            {
                Pair pair = priorityQueue[0];
                priorityQueue.RemoveAt(0);

                if (pair.Node.Equals(to))
                {
                    IList<N> path = new List<N>();
                    path.Add(to);
                    N node = to;
                    while (!node.Equals(from))
                    {
                        node = previusInPath[node];
                        path.Add(node);
                    }

                    return new GraphPath<N>(path, pair.Cost);
                }

                if (!done.Contains(pair.Node))
                {
                    done.Add(pair.Node);
                    foreach (Connector<N> conn in Nodes[pair.Node])
                    {
                        N w = conn.To;
                        int newCost = pair.Cost + conn.Cost;

                        if (!accumulatedCosts.ContainsKey(w) || newCost < accumulatedCosts[w])
                        {
                            accumulatedCosts.Add(w, newCost);
                            priorityQueue.Add(new Pair(w, newCost));
                            previusInPath.Add(w, pair.Node);
                        }
                    }
                }
            }

            return null;
        }

        private struct Pair : IComparable<Pair>
        {
            public N Node   { get; }
            public int Cost { get; }

            public Pair(N node, int cost)
            {
                Node = node;
                Cost = cost;
            }
            public int CompareTo(Pair pair)
            {
                return Cost.CompareTo(pair.Cost);
            }
        }
    }

    public struct Connector<N>
    {
        public int Cost { get; }
        public N   From { get; }
        public N   To   { get; }

        public Connector(N from, N to, int cost)
        {
            Cost = cost;
            From = from;
            To = to;
        }
    }

    public class GraphPath<N>
    {
        public int Cost       { get; }
        public IList<N> Nodes { get; }

        public GraphPath(IList<N> nodes, int cost)
        {
            Nodes = nodes;
            Cost = cost;
        }
    }
}

