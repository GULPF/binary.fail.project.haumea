using System;
using System.Collections.Generic;

namespace Haumea_Core.Collections
{
    // Because it is a major pain to do arethmics with generics in C#,
    // distances are hardcoded as ints.
    public class NodeGraph<N> where N : IEquatable<N>
    {
        public IDictionary<N, IList<Connector<N>>> Nodes { get; }

        public NodeGraph(IDictionary<N, IList<Connector<N>>> nodes)
        {
            Nodes = nodes;
        }

        // This constructor can be used if no lonely node islands exists.
        public NodeGraph(IEnumerable<Connector<N>> connectors, bool twoway)
        {
            Nodes = new Dictionary<N, IList<Connector<N>>>();

            foreach (Connector<N> connector in connectors)
            {
                if (!Nodes.ContainsKey(connector.From))
                {
                    Nodes.Add(connector.From, new List<Connector<N>>());   
                }

                Nodes[connector.From].Add(connector);

                if (twoway)
                {
                    if (!Nodes.ContainsKey(connector.To))
                    {
                        Nodes.Add(connector.To, new List<Connector<N>>());       
                    }

                    Nodes[connector.To].Add(connector.Invert());
                }
            }
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

        public int NeighborDistance(N from, N to)
        {
            foreach (Connector<N> conn in Nodes[from])
            {
                if (conn.To.Equals(to))
                {
                    return conn.Cost;
                }
            }

            return -1;
        }

        // Find shortest path between from and to.
        public GraphPath<N> Dijkstra(N from, N to)
        {
            IDictionary<N, int> accumulatedCosts = new Dictionary<N, int>(Nodes.Count);
            IDictionary<N, N>   previusInPath    = new Dictionary<N, N>(Nodes.Count);
            ISet<N> done = new HashSet<N>();
            // TODO: If it's worth it, switch to a PriorityQueue.
            // .... https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/
            IList<Pair> priorityQueue = new SortedList<Pair>();

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
                        path.Insert(0, node);
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

        public Connector<N> Invert()
        {
            return new Connector<N>(To, From, Cost);
        }
    }

    public class GraphPath<N> : IEnumerable<N>
    {
        public int Cost       { get; }
        public IList<N> Nodes { get; }

        public int NJumps {
            get { return Nodes.Count; }
        }

        public GraphPath(IList<N> nodes, int cost)
        {
            Nodes = nodes;
            Cost = cost;
        }

        public IEnumerator<N> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

