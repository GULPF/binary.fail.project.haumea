using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

using ListNode = System.Collections.Generic.LinkedListNode<Haumea.Geometric.Vertex>;

namespace Haumea.Geometric
{
    public struct Triangulation
    {
        public Vector2[] Vertices { get; }
        public int[]     Indices  { get; }

        public Triangulation(Vector2[] vertices, int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }

    public static class Triangulator
    {
        private struct Angles
        {
            public HashSet<ListNode> Convex { get; }
            public HashSet<ListNode> Reflex { get; }

            public Angles(HashSet<ListNode> convex, HashSet<ListNode> reflex)
            {
                Convex = convex;
                Reflex = reflex;
            }
        }

        public static Triangulation Triangulate(Vector2[] inputVertices)
        {
            Vector2[] outputVertices;

            List<Triangle> triangles = new List<Triangle>();

            // TODO: Since we have full controll over the input,
            // ..... we should just use a Debug.Assert() to force a single vertex order.
            outputVertices = DetermineVertexOrder(inputVertices) == VertexOrder.Clockwise
                ? inputVertices.Reverse().ToArray()
                : (Vector2[])inputVertices.Clone();

            short index = 0;
            var enumer = outputVertices.Select(vec => new Vertex(vec, index++));
            LinkedList<Vertex> polygonVertices = new LinkedList<Vertex>(enumer);

            // Classify all vertices as convex, reflex and ear.
            // The classifications are not final, they need to be updated when clipping.
            // See `UpdateClassification()`.
            Angles angles = ClassifyAngles(polygonVertices);
            HashSet<ListNode> ears = GetEarVertices(angles);

            // Clip ears.
            while (polygonVertices.Count > 3 && ears.Count > 0)
            {
                ClipNextEar(ears.First(), triangles, polygonVertices, ears, angles);   
            }

            // After ear clipping, three points will remain (unless the polyon is messed up).
            //Debug.Assert(polygonVertices.Count == 3);
            var a = polygonVertices.First;
            var b = a.Next;
            var c = b.Next;
            triangles.Add(new Triangle(b.Value, c.Value, a.Value));

            int[] indices = new int[triangles.Count * 3];

            for (int i = 0; i < triangles.Count; i++)
            {
                indices[(i * 3)] = triangles[i].V3.Index;
                indices[(i * 3) + 1] = triangles[i].V2.Index;
                indices[(i * 3) + 2] = triangles[i].V1.Index;
            }

            return new Triangulation(outputVertices, indices);
        }
            
        public static VertexOrder DetermineVertexOrder(Vector2[] verts)
        {
            int nClockWise = 0;
            int nCounterClockwise = 0;

            Action<Vector2,Vector2, Vector2> count = (v1, v2, v3) =>
            {
                Vector2 e1 = v1 - v2;
                Vector2 e2 = v3 - v2;

                float val = e1.X * e2.Y - e1.Y * e2.X;

                if      (val > 0) nClockWise++;
                else if (val < 0) nCounterClockwise++;
            };

            for (int i = 0; i < verts.Length; i++)
            {
                count(verts[i], verts[(i + 1) % verts.Length], verts[(i + 2) % verts.Length]);
            }

            return (nClockWise > nCounterClockwise)
                ? VertexOrder.Clockwise
                : VertexOrder.CounterClockwise;
        }

        private static void ClipNextEar(ListNode ear, ICollection<Triangle> triangles,
            LinkedList<Vertex> vertices, HashSet<ListNode> ears, Angles angles)
        {
            var prev = ear.PreviousOrLast();
            var next = ear.NextOrFirst();
            triangles.Add(new Triangle(ear.Value, next.Value, prev.Value));

            ears.Remove(ear);
            vertices.Remove(ear);

            UpdateClassification(prev, ears, angles);
            UpdateClassification(next, ears, angles);
        }

        private static void UpdateClassification(ListNode node, HashSet<ListNode> ears, Angles angles)
        {
            if (angles.Reflex.Contains(node))
            {
                if (IsConvex(node))
                {
                    angles.Reflex.Remove(node);
                    angles.Convex.Add(node);
                }
            }

            if (angles.Convex.Contains(node))
            {
                bool wasEar = ears.Contains(node);
                bool isEar = IsEar(node, angles.Reflex);

                if (wasEar && !isEar)
                {
                    ears.Remove(node);
                }
                else if (!wasEar && isEar)
                {
                    ears.Add(node);
                }
            }
        }

        private static Angles ClassifyAngles(LinkedList<Vertex> vertices)
        {
            HashSet<ListNode> convex = new HashSet<ListNode>();
            HashSet<ListNode> reflex = new HashSet<ListNode>();

            var node = vertices.First;
            while (node != null)
            {
                if (IsConvex(node))
                {
                    convex.Add(node);
                }
                else
                {
                    reflex.Add(node);
                }

                node = node.Next;
            }

            return new Angles(convex, reflex);
        }
            
        private static HashSet<ListNode> GetEarVertices(Angles angles)
        {
            var ears = new HashSet<ListNode>();

            foreach (var node in angles.Convex.Where(n => IsEar(n, angles.Reflex)))
            {
                ears.Add(node);
            }

            return ears;
        }

        private static bool IsEar(ListNode node, HashSet<ListNode> reflex)
        {
            Vertex c = node.Value;
            Vertex p = node.PreviousOrLast().Value;
            Vertex n = node.NextOrFirst().Value;

            Triangle triangle = new Triangle(p, c, n);

            foreach (var node2 in reflex)
            {
                if (triangle.ContainsPoint(node2.Value) && !triangle.HavePoint(node2.Value))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsConvex(ListNode node)
        {
            Vertex p = node.PreviousOrLast().Value;
            Vertex n = node.NextOrFirst().Value; 
            
            // Vector magic
            Vector2 d1 = Vector2.Normalize(node.Value.Position - p.Position);
            Vector2 d2 = Vector2.Normalize(n.Position - node.Value.Position);
            Vector2 n2 = new Vector2(-d2.Y, d2.X);
            return (Vector2.Dot(d1, n2) <= 0f);
        }
    }

    public enum VertexOrder
    {
        Clockwise,
        CounterClockwise
    }

    public static class CircularLinkedList {
        public static LinkedListNode<T> NextOrFirst<T>(this LinkedListNode<T> current)
        {
            return current.Next ?? current.List.First;
        }

        public static LinkedListNode<T> PreviousOrLast<T>(this LinkedListNode<T> current)
        {
            return current.Previous ?? current.List.Last;
        }
    }
}
