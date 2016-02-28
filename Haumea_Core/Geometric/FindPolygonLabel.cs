using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Haumea_Core.Collections;
using Haumea_Core.Rendering;

// http://mapcontext.com/autocarto/proceedings/auto-carto-8/pdf/an-algorithm-for-locating-candidate-labeling-boxes-within-a-polygon.pdf
namespace Haumea_Core.Geometric
{
    public static class FindPolygonLabel
    {
        private struct Strip
        {
            public readonly float YMax, YMin;
            public readonly int Index;

            public Strip(float yMax, float yMin, int index)
            {
                YMax = yMax;
                YMin = yMin;
                Index = index;
            }
        }

        private struct LineEkv
        {
            public readonly float Slope, Offset, YMax, YMin, X0;

            public LineEkv (float slope, float offset, float yMax, float yMin, float x0)
            {
                Slope = slope;
                Offset = offset;
                YMax = yMax;
                YMin = yMin;
                X0 = x0;
            }
        }

        private struct VerticalSegment : IComparable<VerticalSegment>
        {
            public readonly int StripIndex;
            public readonly float X, YMax, YMin;

            public VerticalSegment(float x, int stripIndex, float yMax, float yMin)
            {
                X = x;
                StripIndex = stripIndex;
                YMax = yMax;
                YMin = yMin;
            }

            public int CompareTo(VerticalSegment other)
            {
                return X > other.X ? -1 : 1;
            }
        }

        private struct Box
        {
            public int Top, Left, Right, Bottom;

            public Box(int top, int left, int right, int bottom)
            {
                Top = top;
                Left = left;
                Right = right;
                Bottom = bottom;
            }
        }

        private static IList<VerticalSegment> GetVerticalSegments(this Poly poly)
        {
            var segments = new SortedList<VerticalSegment>(new InvertCompare<VerticalSegment>());

            IList<Strip> strips      = poly.GetStrips();
            IList<LineEkv> ekvations = poly.GetLineEkvations();

            foreach (Strip strip in strips)
            {
                foreach (LineEkv line in ekvations)
                {
                    if (line.YMax >= strip.YMax && strip.YMin >= line.YMin)
                    {
                        float x;
                        if (float.IsInfinity(line.Slope))
                        {
                            x = line.X0;
                        }
                        else
                        {
                            float x1 = (strip.YMax - line.Offset) / line.Slope;
                            float x2 = (strip.YMin - line.Offset) / line.Slope;
                            x = (x1 + x2) / 2f;
                        }

                        segments.Add(new VerticalSegment(x, strip.Index, strip.YMax, strip.YMin));
                    }
                }
            }

            return segments;
        }

        private static LineEkv GetLineEkvation(Vector2 v1, Vector2 v2)
        {
            float slope = v1.X > v2.X
                ? (v1.Y - v2.Y) / (v1.X - v2.X)
                : (v2.Y - v1.Y) / (v2.X - v1.X);

            float yMax = Math.Max(v1.Y, v2.Y);
            float yMin = Math.Min(v1.Y, v2.Y);

            float offset = v1.Y - v1.X * slope;

            return new LineEkv(slope, offset, yMax, yMin, v1.X);
        }

        private static IList<Strip> GetStrips(this Poly poly)
        {
            var yList = new SortedSet<float>(new InvertCompare<float>());

            foreach (Vector2 v in poly.Points)
            {
                yList.Add(v.Y);
            }

            int index = 1;
            var strips = new List<Strip>();
            var enumerator = yList.GetEnumerator();
            enumerator.MoveNext();
            float prev = enumerator.Current;

            while (enumerator.MoveNext())
            {
                float curr = enumerator.Current;
                strips.Add(new Strip(prev, curr, index));
                index++;
                prev = curr;
            }

            return strips;
        }
    
        private static IList<LineEkv> GetLineEkvations(this Poly poly)
        {
            var ekvations = new List<LineEkv>();
            if (poly.Points.Length < 2) return ekvations;

            var enumerator = poly.Points.GetEnumerator();
            enumerator.MoveNext();
            Vector2 prev = (Vector2)enumerator.Current;
            Vector2 first = prev;
            Vector2 curr  = prev;
        
            while (enumerator.MoveNext())
            {
                curr = (Vector2)enumerator.Current;
                ekvations.Add(GetLineEkvation(prev, curr));
                prev = curr;
            }
            ekvations.Add(GetLineEkvation(curr, first));

            return ekvations;
        }
    
        private static void DrawDebug(this Poly poly, IList<Strip> strips, IList<VerticalSegment> segments)
        {
            int i = 0;
            RenderInstruction[] lines = new RenderInstruction[segments.Count + strips.Count + 1];
            Vector2 start, end;
            float xMin = poly.Boundary.Min.X;
            float xMax = poly.Boundary.Max.X;

            foreach (VerticalSegment seg in segments)
            {
                start = new Vector2(seg.X, strips[seg.StripIndex - 1].YMin);
                end   = new Vector2(seg.X, strips[seg.StripIndex - 1].YMax);
                lines[i++] = RenderInstruction.Line(start, end, Color.Black);
            }

            foreach (Strip strip in strips)
            {
                start = new Vector2(xMin, strip.YMax);
                end   = new Vector2(xMax, strip.YMax);
                lines[i++] = RenderInstruction.Line(start, end, Color.Black);
            }

            start = new Vector2(xMin, strips[strips.Count - 1].YMin);
            end   = new Vector2(xMax, strips[strips.Count - 1].YMin);
            lines[i] = RenderInstruction.Line(start, end, Color.Black);

            Game1.DebugInstructions = lines;
        }

        public static IList<AABB> LabelBoxCondidates(this Poly poly)
        {
            var segments = poly.GetVerticalSegments();    
            var strips   = poly.GetStrips(); // TODO: don't run this twice

            #if DEBUG
            //poly.DrawDebug(strips, segments);
            #endif 

            Box[] w = new Box[strips.Count + 2];
            IList<Box> boxes = new List<Box>();

            int i = 0;  // vertical segment index
            int s;      // strip index
            int t, b;   // top & bottom array index constraints

            foreach (VerticalSegment seg in segments)
            {
                i = i + 1;
                s = seg.StripIndex; 
                b = s + 1;
                while (w[b].Left > 0) b = w[b].Bottom + 1;
                b = b - 1;
                t = s - 1;
                while (w[t].Left > 0) t = w[t].Top - 1;
                t = t + 1;

                if (w[s].Left == 0)
                {
                    w[s].Left = i;
                    w[s].Top = t;
                    w[s].Bottom = b;
                    w[s].Right = 0;
                }
                else
                {
                    for (int m = t; m <= b; m = m + 1)
                    {
                        if (w[m].Bottom >= s && s >= w[m].Top)
                        {
                            Console.WriteLine("Found!");
                            boxes.Add(new Box(w[m].Top, w[m].Left, i, w[m].Bottom));

                            if (s > m)  w[m].Top    = s + 1;
                            if (s == m) w[m].Left   = 0;
                            if (s < m)  w[m].Bottom = s - 1; 
                        }
                    }
                }
            }

            return BoxesToAABB(boxes, segments, strips);
        }
    
        private static IList<AABB> BoxesToAABB (
            IList<Box> boxes, IList<VerticalSegment> segments, IList<Strip> strips)
        {
            IList<AABB> aabbs = new List<AABB>();
           
            foreach (Box box in boxes)
            {
                var min = new Vector2(segments[box.Right - 1].X, strips[box.Bottom - 1].YMin);
                var max = new Vector2(segments[box.Left - 1].X, strips[box.Top - 1].YMax);
                aabbs.Add(new AABB(max, min));
            }
            Console.WriteLine(aabbs.Count);
            return aabbs;
        }
    }
        
    public class InvertCompare<T> : IComparer<T> where T : IComparable<T> {
        public int Compare(T x, T y)
        {
            int comp = x.CompareTo(y);
            if (comp < 0) return 1;
            if (comp > 0) return -1;
            return 0;
        }
    }
}

