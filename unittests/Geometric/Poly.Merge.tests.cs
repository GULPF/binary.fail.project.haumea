using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.Xna.Framework;
using Haumea.Geometric;

namespace unittests
{
    public class MergePolygons : XnaTests
    {
        // Simple case: polygons share one border, no points inside the merged polygon.
        [Test]
        public void MergeNeighborsSimple()
        {
            IPoly poly = new Poly(new [] { V2(0, 0), V2(3, 2), V2(3, 6), V2(0, 4) });
            IPoly toMerge = new Poly(new [] {
                V2(0, 0),
                V2(3, 0),
                V2(3, 2) });

            IPoly merged = merge(poly, toMerge);
            ISet<Vector2> points = new HashSet<Vector2>(merged.Points);

            Assert.AreEqual(5, merged.Points.Length);

            Assert.True(points.Contains(V2(0, 0)));
            Assert.True(points.Contains(V2(3, 0)));
            Assert.True(points.Contains(V2(3, 6)));
            Assert.True(points.Contains(V2(0, 4)));
            // This one should not be included in a normalized polygon,
            // but I'll ignore that for now.
            Assert.True(points.Contains(V2(3, 2)));
        }

        // Medium case: Polygons share three borders, two points inside the merged polygon.
        [Test]
        public void MergeNeighborsMedium()
        {
            Poly poly1 = new Poly(new [] {
                V2(0, 0),
                V2(5, 0),
                V2(5, 2),
                V2(3, 4),
                V2(2, 2),
                V2(0, 4)
            });

            Poly poly2 = new Poly(new [] {
                V2(5, 2),
                V2(3, 4),
                V2(2, 2),
                V2(0, 4),
                V2(0, 9),
                V2(5, 9)
            });

            IPoly merged = merge(poly1, poly2);
            ISet<Vector2> points = new HashSet<Vector2>(merged.Points);

            Assert.AreEqual(6, merged.Points.Length);
            Assert.True(points.Contains(V2(0, 0)));
            Assert.True(points.Contains(V2(5, 0)));
            Assert.True(points.Contains(V2(5, 9)));
            Assert.True(points.Contains(V2(0, 9)));

            // These two should be included in a normalized polygon.
            Assert.True(points.Contains(V2(5, 2)));
            Assert.True(points.Contains(V2(0, 4)));

        }

        // Medium case: one polygon completly surrounds the other
        // #####
        // #####
        // #¤¤##
        // #¤¤##
        // #####
        [Test]
        public void MergeInsideCompletely()
        {
            Vector2[] points1 = Rectangle(0, 0, 5, 5);
            Vector2[] points2 = Rectangle(1, 2, 2, 2);
            IPoly poly1 = new ComplexPoly(points1, new [] { points2 });
            IPoly poly2 = new Poly(points2);
            IPoly merged = merge(poly1, poly2);
            Assert.AreEqual(0, merged.Holes.Length, "Hole should be gone");
            Assert.AreEqual(25, merged.Boundary.Area);
        }

        // Complex case: One polygon almost surrounds the other (2nd polygon only have one unshared border).
        [Test]
        public void MergeNeighborsComplex()
        {
            Poly poly1 = new Poly(new [] {
                V2(0, 0),
                V2(8, 0),
                V2(8, 10),
                V2(3, 10),
                V2(3, 7),
                V2(5, 5),
                V2(6, 6),
                V2(6, 2),
                V2(2, 2),
                V2(2, 6),
                V2(0, 6)
            });

            Poly poly2 = new Poly(new [] {
                V2(2, 2),
                V2(6, 2),
                V2(6, 6),
                V2(5, 5),
                V2(3, 7),
                V2(2, 6)
            });

            IPoly merged = merge(poly1, poly2);
            ISet<Vector2> points = new HashSet<Vector2>(merged.Points);

            Assert.AreEqual(7, merged.Points.Length);
            Assert.True(points.Contains(V2(0, 0)));
            Assert.True(points.Contains(V2(8, 0)));
            Assert.True(points.Contains(V2(8, 10)));
            Assert.True(points.Contains(V2(3, 10)));
            Assert.True(points.Contains(V2(3, 7)));
            Assert.True(points.Contains(V2(2, 6)));
            Assert.True(points.Contains(V2(0, 6)));
        }

        // Complex case: One polygon surrounds the other.
        // ######
        // #----#
        // #--¤¤#
        // #--¤¤#
        // #----#
        // ######
        [Test]
        public void MergeInside()
        {
            Vector2[] points1 = Rectangle(0, 0, 6, 6);
            Vector2[] hole    = { V2(1, 1), V2(5, 1), V2(5, 2), V2(5, 4), V2(5, 5), V2(1, 5) };
            Vector2[] points2 = Rectangle(3, 2, 2, 2);
            IPoly poly1 = new ComplexPoly(points1, new [] { hole });
            IPoly poly2 = new Poly(points2);

            IPoly merged = merge(poly1, poly2);

            Assert.True(merged.IsPointInside(V2(0, 0)), "Border");
            Assert.True(merged.IsPointInside(V2(3, 2)), "Inner border");
            Assert.False(merged.IsPointInside(V2(2, 2)), "Inside hole");
        }

        // Complex case: One polygon surrounds the other, which splits the the other polygons hole in two.
        // ######
        // #----#
        // #¤¤¤¤#
        // #¤¤¤¤#
        // #----#
        // ######
        [Test]
        public void MergeInsideDivider()
        {
            Vector2[] points1 = Rectangle(0, 0, 6, 6);
            Vector2[] hole    = { V2(1, 1), V2(5, 1), V2(5, 2), V2(5, 4), V2(5, 5), V2(1, 5), V2(1, 4), V2(1, 2) };
            Vector2[] points2 = Rectangle(1, 2, 4, 2);
            IPoly poly1 = new ComplexPoly(points1, new [] { hole });
            IPoly poly2 = new Poly(points2);
            IPoly merged = merge(poly1, poly2);

            Assert.AreEqual(2, merged.Holes.Length, "Another hole should be be created");
            Assert.False(merged.IsPointInside(V2(1.5f, 1.5f)), "First hole");
            Assert.False(merged.IsPointInside(V2(1.5f, 4.5f)), "2nd hole");
            Assert.True(merged.IsPointInside(V2(1, 3)), "Merged area");

            foreach (var mergedHole in merged.Holes)
            {
                Assert.AreEqual(new HashSet<Vector2>(mergedHole.Points).Count, mergedHole.Points.Length,
                    "Merged polygon should only have unique points");    
            }
        }

        // Simple case: Two ordinary neighbors with holes.
        // ¤¤¤
        // ¤-¤
        // ¤¤¤
        // #####
        // #####
        // #####
        // ##--#
        // ##--#
        // #####
        // #####
        // #####
        [Test]
        public void PreserveHoles()
        {
            // Needs additional point (3, 0) so can't use `Rectangle`.
            Vector2[] points1 = { V2(0, 0), V2(3, 0), V2(5, 0), V2(5, 8), V2(0, 8) };
            Vector2[] holePoints = Rectangle(2, 3, 2, 2);

            Vector2[] points2 = Rectangle(0, -3, 3, 3);
            Vector2[] hole2Points = Rectangle(1, -2, 1, 1);

            IPoly poly1 = new ComplexPoly(points1, new [] { holePoints });
            IPoly poly2 = new ComplexPoly(points2, new [] { hole2Points });
            IPoly merged = merge(poly1, poly2);

            Assert.True(merged.IsPointInside(V2(1, 1)), "Inside");
            Assert.True(merged.IsPointInside(V2(1, -1)), "Inside merged");
            Assert.AreEqual(2, merged.Holes.Length);
            Assert.False(merged.IsPointInside(V2(3, 4)), "Inside hole");
            Assert.False(merged.IsPointInside(V2(1.5f, -1.5f)), "Inside 2nd hole");
        }

        // Complex case: Polygons with no hole that create a hole when merged.
        // ######
        // ######
        // ##--##
        // ¤¤--¤¤
        // ¤¤¤¤¤¤
        // ¤¤¤¤¤¤
        [Test]
        public void MergeWithHoleCreation()
        {
            Vector2[] points1 = { V2(0, 0), V2(6, 0), V2(6, 3), V2(4, 3), V2(4, 2), V2(2, 2), V2(2, 3), V2(0, 3) };
            Vector2[] points2 = { V2(0, 3), V2(0, 6), V2(6, 6), V2(6, 3), V2(4, 3), V2(4, 4), V2(2, 4), V2(2, 3) };

            IPoly poly1 = new Poly(points1);
            IPoly poly2 = new Poly(points2);
            IPoly merged = merge(poly1, poly2);

            Assert.AreEqual(1, merged.Holes.Length, "Merged polygon should have a hole");
            Assert.True(merged.IsPointInside(V2(0, 0)), "Border");
            Assert.False(merged.IsPointInside(V2(3, 3)), "Inside hole");
        }

        // Complex case: Like above, but with multiple holes.
        // ##########
        // ##########
        // ##--##--##
        // ¤¤--¤¤--¤¤
        // ¤¤¤¤¤¤¤¤¤¤
        // ¤¤¤¤¤¤¤¤¤¤
        [Test]
        public void MergeWithMultipleHoleCreation()
        {
            Vector2[] points1 = { V2(0, 0),  V2(10, 0), V2(10, 3), V2(8, 3), V2(8, 2), V2(6, 2),
                V2(6, 3), V2(4, 3), V2(4, 2), V2(2, 2), V2(2, 3), V2(0, 3) };
            Vector2[] points2 = { V2(0, 3), V2(0, 6), V2(10, 6), V2(10, 3), V2(8, 3), V2(8, 4),
                V2(6, 4), V2(6, 3), V2(4, 3), V2(4, 4), V2(2, 4), V2(2, 3) };

            IPoly poly1 = new Poly(points1);
            IPoly poly2 = new Poly(points2);
            IPoly merged = merge(poly1, poly2);

            Assert.AreEqual(2, merged.Holes.Length, "Merged polygon should have two holes");
            Assert.True(merged.IsPointInside(V2(0, 0)), "Border");
            Assert.False(merged.IsPointInside(V2(3, 3)), "Inside first hole");
            Assert.False(merged.IsPointInside(V2(7, 3)), "Inside 2nd hole");
            //Assert.True(merged.IsPointInside(V2(0, 0)), "Border");
            //Assert.False(merged.IsPointInside(V2(3, 3)), "Inside hole");
        }

        // Really fucking complex case: hole creation where it's extra hard to distinguish the areas.
        // ##¤¤¤####
        // #-------#
        // #-------#
        // #########
        [Test]
        public void MergeWithHoleCreationHard()
        {
            Vector2[] points1 = { V2(0, 0), V2(2, 0), V2(2, 1), V2(1, 1), V2(1, 3), V2(8, 3), V2(8, 1),
                V2(5, 1), V2(5, 0), V2(9, 0), V2(9, 4), V2(0, 4) };
            Vector2[] points2 = { V2(2, 0), V2(2, 1), V2(5, 1), V2(5, 0) };

            IPoly poly1 = new Poly(points1);
            IPoly poly2 = new Poly(points2);
            IPoly merged = merge(poly1, poly2);

            Assert.AreEqual(1, merged.Holes.Length, "Merged polygon should have a hole");
            Assert.True(merged.IsPointInside(V2(2, 0)), "Border");
            Assert.True(merged.IsPointInside(V2(5, 1)), "Inner border");
            Assert.False(merged.IsPointInside(V2(2, 2)), "Hole");
        }

        // Even more complex case: like above, but with multiple new holes created.
        // ##¤¤¤####
        // #--¤----#
        // #--¤----#
        // #########
        [Test]
        public void MergeWithMultipleHoleCreationHard()
        {
            Vector2[] points1 = { V2(0, 0), V2(2, 0), V2(2, 1), V2(1, 1), V2(1, 3), V2(8, 3), V2(8, 1),
                V2(5, 1), V2(5, 0), V2(9, 0), V2(9, 4), V2(0, 4) };
            Vector2[] points2 = { V2(2, 0), V2(2, 1), V2(3, 1), V2(3, 3), V2(4, 3), V2(4, 1), V2(5, 1), V2(5, 0) };

            IPoly poly1 = new Poly(points1);
            IPoly poly2 = new Poly(points2);
            IPoly merged = merge(poly1, poly2);

            Assert.AreEqual(1, merged.Holes.Length, "Merged polygon should have two holes");
            Assert.False(merged.IsPointInside(V2(2, 2)), "Inside first hole");
            Assert.False(merged.IsPointInside(V2(5, 2)), "Inside 2nd hole");
        }

        [Test]
        public void IsNeighbor()
        {
            Vector2[] points1 = { V2(0, 0), V2(1, 0),  V2(1, 1),  V2(0, 1) };
            Vector2[] points2 = { V2(0, 0), V2(1, 0),  V2(1, -1), V2(0, -1) };
            Vector2[] points3 = { V2(0, 0), V2(1, -1), V2(0, -1) };
            Vector2[] points4 = { V2(5, 0), V2(6, 0),  V2(6, -1), V2(5, -1) };

            Vector2[] outlinePoints = { V2(-2, -3), V2(2, -3), V2(2, 1), V2(-2, 1) };
            Vector2[] holePoints    = { V2(-1, -2), V2(1, -2), V2(1, 0), V2(0, 0), V2(-1, 0) };

            IPoly poly1 = new Poly(points1);
            IPoly poly2 = new Poly(points2);
            IPoly poly3 = new Poly(points3);
            IPoly poly4 = new Poly(points4);
            IPoly poly5 = new ComplexPoly(outlinePoints, new []{ holePoints });

            Assert.True(poly1.IsNeighbor(poly2), "Simple neighbors");
            Assert.False(poly1.IsNeighbor(poly3), "Only one shared point");
            Assert.False(poly1.IsNeighbor(poly4), "Simple not neighbors");
            Assert.True(poly2.IsNeighbor(poly5), "Border with the hole");
        }

        private IPoly merge(IPoly poly1, IPoly poly2)
        {
            IPoly merged;
            Assert.True(poly1.TryMerge(poly2, out merged), "Should succeed with merge");
            return merged;
        }
    }
}

