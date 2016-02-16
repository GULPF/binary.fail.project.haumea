using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Haumea_Core.Collections;

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
            public readonly float X;

            public VerticalSegment(float x, int stripIndex)
            {
                X = x;
                StripIndex = stripIndex;
            }

            public int CompareTo(VerticalSegment other)
            {
                return X > other.X ? -1 : 1;
            }

        }

        private static IList<VerticalSegment> GetVerticalSegments(this Poly poly)
        {
            var segments = new SortedList<VerticalSegment>();

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

                        segments.Add(new VerticalSegment(x, strip.Index));
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
            var yList = new SortedSet<float>();

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
                strips.Add(new Strip(curr, prev, index));
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
    
        public static IList<AABB> LabelBoxCondidates(this Poly poly)
        {
            var segments = poly.GetVerticalSegments();    

            return null;
        }
    }
}

