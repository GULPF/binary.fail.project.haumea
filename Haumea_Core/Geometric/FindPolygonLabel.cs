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

            using (var enumerator = yList.GetEnumerator())
            {
                enumerator.MoveNext();
                float prev = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    float curr = enumerator.Current;
                    strips.Add(new Strip(prev, curr, index));
                    index++;
                    prev = curr;
                }    
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

            Debug.AddInstructions(lines);
        }

        /// <summary>
        /// Finds a list of label box candidates.
        /// </summary>
        /// <returns>The label condidates.</returns>
        public static IList<AABB> LabelBoxCondidates(this Poly poly)
        {
            return poly.LabelBoxCondidates(false);
        }

        private static IList<AABB> LabelBoxCondidates(this Poly poly, bool suppressDebug)
        {
            var segments = poly.GetVerticalSegments();    
            var strips   = poly.GetStrips(); // TODO: don't run this twice

            #if DEBUG_LABEL_BOXES
            if (!suppressDebug) poly.DrawDebug(strips, segments);
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

                while (w[b].Left > 0) {
                    // I don't know how or why this is happening, but nonetheless.
                    if (w[b].Bottom == b - 1) break;
                    b = w[b].Bottom + 1;
                }

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
    
        /// <summary>
        /// Find the best label box among the candidates.
        /// <para> </para><para> </para>
        /// Basicly, this method runs <c>LabelBoxCandidates</c> twice.
        /// Once with the normal polygon, and once with a rotated polygon (90 deg).
        /// The reason is that the algorithm is pretty shitty and this helps a bit.
        /// <para> </para><para> </para>
        /// Because the label boxes from the rotated can be a bit weird,
        /// it is only choosen if it's bigger __and__ it's closer to the polygon centroid.
        /// </summary>
        /// <returns>The best label box (hopefully).</
        public static AABB FindBestLabelBox(this Poly poly)
        {
            IList<AABB> boxes  = poly.LabelBoxCondidates(false);
            IList<AABB> rBoxes = new List<AABB>();

            // Here be dragons.
            foreach (AABB box in poly.RotateLeft90().LabelBoxCondidates(true))
            {
                Vector2 rmin = box.Min.RotateRight90();
                Vector2 rmax = box.Max.RotateRight90();
                Vector2 min  = rmin - (rmin - rmax).Abs() * Vector2.UnitX;
                Vector2 max  = rmax - (rmin - rmax).Abs() * Vector2.UnitX;

                rBoxes.Add(new AABB(max, min));
            }


            float dmax = 0;
            AABB choosen  = new AABB(Vector2.Zero, Vector2.Zero);
            float rdmax = 0;
            AABB rChoosen = new AABB(Vector2.Zero, Vector2.Zero);

            foreach (AABB box in boxes)
            {
                Vector2 dim = (box.Max - box.Min).Abs();
                if (dim.X * dim.Y > dmax)
                {
                    dmax = dim.X * dim.Y;
                    choosen = box;
                }
            }

            foreach (AABB box in rBoxes)
            {
                Vector2 dim = (box.Max - box.Min).Abs();
                if (dim.X * dim.Y > rdmax)
                {
                    rdmax = dim.X * dim.Y;
                    rChoosen = box;
                }
            }

            Vector2 centroid = poly.CalculateCentroid();

            if (rChoosen.Area > choosen.Area)
            {
                // Even if we find a bigger box on the rotated polygon, 
                // we only switch if it's closer to the centroid.
                // The reason is that the rotated boxes are more often than not a bit messed up.
                if (Vector2.DistanceSquared(rChoosen.Center, centroid)
                    < Vector2.DistanceSquared(choosen.Center, centroid))
                {
                    choosen = rChoosen;    
                }
            }

            return choosen;
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

            return aabbs;
        }
    
        // Reverses the sorting order.
        private class InvertCompare<T> : IComparer<T> where T : IComparable<T> {
            public int Compare(T x, T y)
            {
                return -x.CompareTo(y);
            }
        }
    }
}

