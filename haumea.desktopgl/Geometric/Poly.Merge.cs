using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace Haumea.Geometric
{
    /// <summary>
    /// This class makes some big assumptions:
    ///     - In the entire set of polygons which are used, no overlapps except for the rand exists.
    ///     - Shared lines always break with a shared point. E.g if a polygon shares half of a straight line
    ///       of another polygon, the 2nd polygon will have an extra point where the shared line stops.
    ///     - In the entire set of polygons which are used, there exist no borders between polygons which consist
    ///       of a single point.
    /// 
    /// Terminology:
    ///     Mergee: the outlines (or outline + hole) that are being merged.
    ///     Unique point: point that only exist in one of the mergees.
    ///     Neighbor: a point is a neighbor to another point if they are directly connected.
    /// 
    /// The basic steps of the algorithm:
    /// 0.  Figure out if the merge is inverse or normal.
    ///     Normal merge is a merge between two outlines.
    ///     Inverse merge is a merge between outline and hole.
    /// 1.  Pick a unique point as starting point. If inverse merge, pick a unique point on the outline.
    /// 2.  Polygon = Travelling(start, start)
    /// 3.  while (Polygon doesn't contain all unuqie points):
    ///         p = unique point which doesn't exist in Polygon
    ///         Polygon_2 = Travelling(p, p)
    ///         Outline = Biggest(Polygon, Polygon_2)
    ///         Hole = Smallest(Polyon, Polygon_2)

    /// Travelling (start point, end point)
    /// 1.  Current point = start point
    /// 2.  Collect the current point if not already collected
    /// 3.  ns = all neighbors to current point
    /// 4.  if ns contain end point, return collected points as polygon
    /// 5.  if ns.length == 1:
    ///         if un[0] is unique or un[0] doesn't have exactly two neighbors:
    ///             collect un[0]
    ///             goto §2
    ///         else: return collected points as polygon
    /// 6.  if uniques(ns).length > 1:
    ///         set end point to un[0]
    ///         set current point to start point
    ///         invert order of collected points
    ///         goto §2
    /// 7.  if uniques(ns).length = 1:
    ///         collect uniques(ns)[0]
    ///         gptp §2
    /// 8.  if neighbors_three(ns).length > 1
    ///         set end point to un[0]
    ///         set current point to start point
    ///         invert order of collected points
    ///         goto §2
    /// 9.  if neighbors_three(ns).length = 1:
    ///         collect neihbors_three(ns)[0]
    ///         goto §2
    /// 10. return collected points as polygon
    ///             
    /// uniques (points) : returns all points that are unique
    /// neighbors_three (points) : returns all points with exactly three neighbors
    /// 
    /// </summary>
    public static class MergePolygons
    {
        #region well defined methods

        /// <summary>
        /// Finds and returns one value that exist in either
        /// `set1` or `set2`, but not in both.
        /// </summary>
        private static bool TryFindUnique<T>(ICollection<T> set1, ICollection<T> set2, out T unique)
        {
            if (!set1.TryFind(out unique, t => !set2.Contains(t)))
            {
                if (!set2.TryFind(out unique, t => !set1.Contains(t)))
                {
                    return false;
                }
            }

            // `unique` will be set to default in the other try functions, we don't have to bother here.
            return true;
        }

        private static ISet<T> FindAllUniques<T>(ICollection<T> set1, ICollection<T> set2)
        {
            ISet<T> uniques = new HashSet<T>();

            foreach (T t in set1) 
            {
                if (!set2.Contains(t)) uniques.Add(t);
            }

            foreach (T t in set2)
            {
                if (!set1.Contains(t)) uniques.Add(t);
            }

            return uniques;
        }

        /// <summary>
        /// Determines if is a value does not exist in more than one of the sets.
        /// Will return true even if the value doesn't exist in any set.
        /// </summary>
        private static bool IsUnique<T>(T t, params ISet<T>[] sets)
        {
            bool found = false;

            foreach (ISet<T> set in sets)
            {
                if (set.Contains(t))
                {
                    if (found) return false;
                    found = true;
                }
            }

            return true;
        }

        private static int FindIndex(Vector2[] poly, Vector2 point)
        {
            for (int index = 0; index < poly.Length; index++)
            {
                if (poly[index] == point) return index;
            }

            return -1;
        }

        private static int CountNeighbors(Vector2[] poly1, Vector2[] poly2, Vector2 point)
        {
            return FindNeighboringPoints(poly1, poly2, point).Count;
        }

        private static int SanitizeIndex(int index, int len)
        {
            return ((index % len) + len) % len;
        }
            
        private static ISet<Vector2> FindNeighboringPoints(Vector2[] poly1, Vector2[] poly2, Vector2 point)
        {
            ISet<Vector2> neighbors = new HashSet<Vector2>();

            int index1 = FindIndex(poly1, point);
            int index2 = FindIndex(poly2, point);

            // These four are our alternatives.

            if (index1 > -1) 
            {
                int index1Dec = SanitizeIndex(index1 - 1, poly1.Length);
                int index1Inc = SanitizeIndex(index1 + 1, poly1.Length);
                neighbors.Add(poly1[index1Dec]);
                neighbors.Add(poly1[index1Inc]);
            }

            if (index2 > -1)
            {
                int index2Dec = SanitizeIndex(index2 - 1, poly2.Length);
                int index2Inc = SanitizeIndex(index2 + 1, poly2.Length);
                neighbors.Add(poly2[index2Dec]);
                neighbors.Add(poly2[index2Inc]);
            }

            return neighbors;
        }

        /// <summary>
        /// Check if any two polygons are neighbors.
        /// For the context of this method, a very simple defintion of neighbor is used:
        ///     Any two polygons with at least two shared points (which must be in the same sub-poly,
        ///     e.g if there is one shared point on the outline and one on a hole, it doesn't count)
        ///     are considered neighbors.
        /// Note that this simple check __does not__ guarantee that the polygons are mergeable, since
        /// merging also requires the polygons not to overlap. This condition is not checked, 
        /// because it would be to slow. This class should really only be used when no
        /// overlaping polygons can exist.
        /// </summary>
        public static bool IsNeighbor(this IPoly poly1, IPoly poly2)
        {
            if (poly1.IsNeighborOnOutline(poly2))
            {
                return true;
            }

            // Then we check if either of the outlines are neighbor with any of the holes.
            foreach (IPoly hole in poly1.Holes)
            {
                if (hole.IsNeighbor(poly2)) return true;
            }

            foreach (IPoly hole in poly2.Holes)
            {
                if (hole.IsNeighbor(poly1)) return true;
            }  // Now we can be certain that the polygons are not neighbors.
            return false;
        }

        // Check if any two polygons are neighbors on their outline, e.g they have two or more
        //points in their outlines in common.
        public static bool IsNeighborOnOutline(this IPoly poly1, IPoly poly2)
        {
            // Since it's quick and most polygons are not neighbors, we start by just checking the boundaries.
            if (!poly1.Boundary.Intersects(poly2.Boundary)) return false;

            // Then we check if the outlines are neighbors.
            ISet<Vector2> points = new HashSet<Vector2>(poly1.Points);
            int nShared = 0;
            foreach (Vector2 v in poly2.Points)
            {
                if (points.Contains(v)) nShared++;
                if (nShared >= 2) return true;
            }

            return false;
        }


        // Creates a `ComplexPoly` only if needed.
        private static IPoly CreatePoly(Vector2[] points, IPoly[] holes)
        {
            IPoly poly = new Poly(points);

            return holes.Length == 0 ? poly : new ComplexPoly(poly, holes);
        }

        #endregion

        // Handles the actual merging.
        // Merges the polygons in `points1` and `points2`, starting at `nextPoint.
        // Note that this method has zero error checking. If you call this, you better make sure
        // the polygons are actually mergeable.
        // NOTE: `visisted` will be mutated. This is by design.
        // TODO: Possible performance improvements:
        // ..... -  `newPoly` needs to be reversed when finding ambiguous paths. This is unnecessary.
        // .....    I should introduce a `PolyBuilder` helper to make this easier.
        private static Vector2[] DoMerge(
            Vector2[] points1, Vector2[] points2,
            ISet<Vector2> pointSet1, ISet<Vector2> pointSet2,
            ISet<Vector2> visited, Vector2 startPoint)
        {

            Vector2 nextPoint = startPoint;
            List<Vector2> newPoly = new List<Vector2>();
            bool done = false;
            int nVisited0 = visited.Count;

            while (!done)
            {
                Vector2 currentPoint = nextPoint;

                if (visited.Add(currentPoint))
                {
                    newPoly.Add(currentPoint);    
                }

                // We start by finding all neighbors that we haven't used yet.
                // (polygon points that are directly connected to our current point).
                ISet<Vector2> neighbors = FindNeighboringPoints(points1, points2, currentPoint);
                if (visited.Count > nVisited0 + 2  && neighbors.Contains(startPoint))
                {
                    break; // Stop immediatly if we have reached the starting point.
                }
                neighbors.ExceptWith(visited);

                // If there is only one candidate, we just have to make sure we can keep it.
                if (neighbors.Count == 1)
                {
                    // We know that the set isn't empty, so there's no point in using `TryFirst`.
                    Vector2 first = neighbors.First();
                    if (IsUnique(first, pointSet1, pointSet2) ||
                        CountNeighbors(points1, points2, first) != 2)
                    {
                        nextPoint = first;
                    }
                    else
                    {
                        done = true;   
                    }
                }
                // If we have to choose, first check for a unique candidate.
                else if (!neighbors.TryFind(out nextPoint, v => IsUnique(v, pointSet1, pointSet2)))
                {
                    // If no unique candidate exist, check for a candidate with three paths.
                    Vector2[] threePaths = neighbors.FindAll(v => CountNeighbors(points1, points2, v) == 3).ToArray();


                    switch (threePaths.Length)
                    {
                    case 0: // If we still haven't found a candidate, we're done.
                        done = true;
                        break;
                    case 1: // Only one choice, so we can safely pick it.
                        nextPoint = threePaths[0];
                        break;
                    default: // There are multiple choices, so we have to backtrack.
                        // Pretty neat, huh
                        nextPoint = startPoint;
                        startPoint = currentPoint;
                        newPoly.Reverse();
                        break;
                    }
                }
            }

            return newPoly.ToArray();
        }

        private static Vector2[] DoInverseMerge(
            Vector2[]     holePoints, Vector2[] outlinePoints,
            ISet<Vector2> holeSet, ISet<Vector2> outlineSet,
            ISet<Vector2> visited, Vector2 startPoint)
        {
            bool done = false;
            Vector2 nextPoint = startPoint;
            List<Vector2> newPoly = new List<Vector2>();
            int nVisited0 = visited.Count;

            while (!done)
            {
                Vector2 currentPoint = nextPoint;

                if (visited.Add(currentPoint))
                {
                    newPoly.Add(currentPoint);    
                }

                // We start by finding all neighbors that we haven't used yet.
                // (polygon points that are directly connected to our current point).
                ISet<Vector2> neighbors = FindNeighboringPoints(holePoints, outlinePoints, currentPoint);
                if (visited.Count > nVisited0 + 2  && neighbors.Contains(startPoint))
                {
                    break; // Stop immediatly if we have reached the starting point.
                }
                neighbors.ExceptWith(visited);

                // If there is only one candidate, we just have to make sure we can keep it.
                if (neighbors.Count == 1)
                {
                    // We know that the set isn't empty, so there's no point in using `TruFirst`.
                    Vector2 first = neighbors.First();
                    if (IsUnique(first, holeSet, outlineSet) ||
                        CountNeighbors(holePoints, outlinePoints, first) != 2)
                    {
                        nextPoint = first;
                    }
                    else
                    {
                        done = true;   
                    }
                }
                // If we have to choose, first check for a unique candidate.
                else if (!neighbors.TryFind(out nextPoint, v => IsUnique(v, holeSet, outlineSet)))
                {
                    // If no unique candidate exist, check for a candidate with three paths.
                    Vector2[] threePaths = neighbors
                        .FindAll(v => CountNeighbors(holePoints, outlinePoints, v) == 3).ToArray();

                    switch (threePaths.Length)
                    {
                    case 0: // If we still haven't found a candidate, we're done.
                        done = true;
                        break;
                    case 1: // Only one choice, so we can safely pick it.
                        nextPoint = threePaths[0];
                        break;
                    default: // There are multiple choices, so we have to backtrack.
                        // Pretty neat, huh
                        nextPoint = startPoint;
                        startPoint = currentPoint;
                        newPoly.Reverse();
                        break;
                    }
                }
            }

            return newPoly.ToArray();
        }

        public static bool TryInverseMerge(IPoly bigPoly, IPoly smallPoly, out IPoly merged)
        {
            for (int n = 0; n < bigPoly.Holes.Length; n++)
            {
                if (bigPoly.Holes[n].IsNeighborOnOutline(smallPoly))
                {
                    IPoly hole = bigPoly.Holes[n];
                    ISet<Vector2> pointSet2 = new HashSet<Vector2>(smallPoly.Points);
                    IPoly[] allHoles;

                    Vector2 startPoint;

                    // Always start on a unique point on the hole rand.
                    if (!hole.Points.TryFind(out startPoint, v => !pointSet2.Contains(v)))
                    {
                        // If there are no unique points of the hole, the hole will disappear.
                        // (the outline covers the hole exactly)
                        // No actual merging required!
                        allHoles =
                            bigPoly.Holes.Take(n - 1)
                            .Concat(bigPoly.Holes.Skip(n + 1)).ToArray();
                    }
                    else
                    {
                        ISet<Vector2> visited = new HashSet<Vector2>();
                        ISet<Vector2> pointSet1 = new HashSet<Vector2>(bigPoly.Holes[n].Points);

                        Vector2[] newHole = DoInverseMerge(
                            hole.Points, smallPoly.Points, pointSet1, pointSet2, visited, startPoint);


                        ISet<Vector2> uniques   = FindAllUniques(pointSet1, pointSet2);
                        ISet<Vector2> pointSet3 = new HashSet<Vector2>(newHole);

                        List<IPoly> holes = new List<IPoly> { new Poly(newHole) };

                        // All unique points should be used,
                        // so if we have missed any there must be a hole (or several) somewhere...

                        Vector2 unique;

                        while (uniques.TryFind(out unique, v => !pointSet3.Contains(v)))
                        {
                            Vector2[] additionalNewPoints = DoInverseMerge(hole.Points, smallPoly.Points,
                                pointSet1, pointSet2, visited, unique);
                            holes.Add(new Poly(additionalNewPoints));
                            pointSet3.UnionWith(additionalNewPoints);
                        }

                        allHoles =
                            bigPoly.Holes.Take(n - 1)
                            .Concat(holes)
                            .Concat(bigPoly.Holes.Skip(n + 1)).ToArray();
                    }

                    merged = CreatePoly(bigPoly.Points, allHoles);
                    return true;   
                }
            }

            merged = null;
            return false;
        }

        /// <summary>
        /// Merge two neighbouring (but not overlapping) polygons.
        /// The algorithm works the following way:
        /// (A point is unique if it only occurs in on of the polygons)
        /// (The number of paths of a point is the number of other points connected to it)
        /// 1. Find a unique point to start on.
        ///    This becomes our current point. We add it to the merged polygon.
        /// 2. Find all neighbors to the current point.
        ///    Remove any that is already included in the merged polygon.
        /// 3. If 
        ///      there is exactly one neighbor, and it is either unique or doesn't have exactly two paths:
        ///    
        ///      OR there is a unique neighbor:
        /// 
        ///      OR there is a neighbor with three paths:
        /// 
        ///    - Make it the current point, add it to the merged polygon, and go to step 2.
        /// 4. No neighbors suitable. We are done, return the merged polygon.
        /// 
        /// DO NOT:
        ///   - Merge polygons with identical outlines (WONTFIX).
        ///   - Merge polygons that are not neighbors (WONTFIX, duh).
        ///   - Merge polygons which create a hole when merged.
        ///   - Merge polygons where one of them completely surrounds the other.
        /// </summary>
        /// <returns>The merged polygon</returns>
        public static bool TryMerge(this IPoly poly1, IPoly poly2, out IPoly merged)
        {
            if (!poly1.IsNeighborOnOutline(poly2))
            {
                // Even if the polygons isn't neighbors on the outline,
                // they might be neighbors. Here we attempt to merge each polyon
                // with the other polygon, and if it does't work out we give up.
                return TryInverseMerge(poly1, poly2, out merged) || TryInverseMerge(poly2, poly1, out merged);
            }

            IPoly[] allHoles = poly1.Holes.Concat(poly2.Holes).ToArray();

            HashSet<Vector2> pointSet1 = new HashSet<Vector2>(poly1.Points);
            HashSet<Vector2> pointSet2 = new HashSet<Vector2>(poly2.Points);

            // We can start at any unique index.
            // (actually, we can start at any point on the edge, but this is faster)
            Vector2 nextPoint;
            if (!TryFindUnique(pointSet1, pointSet2, out nextPoint))
            {
                // If the outlines are identical then, according to ur assumptions, this should hold.
                // XXX: Of course, if the outlines are identical, then there are probably identical holes to,
                // .... but that would be very expensive to check for...
                // .... Having identical holes is harmless, but it would impact performance.
                merged = CreatePoly(poly1.Points, allHoles);
                return true;
            }

            ISet<Vector2> visited = new HashSet<Vector2>();
            Vector2[] newPoints = DoMerge(poly1.Points, poly2.Points, pointSet1, pointSet2, visited, nextPoint);

            ISet<Vector2> uniques   = FindAllUniques(pointSet1, pointSet2);
            ISet<Vector2> pointSet3 = new HashSet<Vector2>(newPoints);
            IList<IPoly> polys = new List<IPoly> { new Poly(newPoints) };

            // All unique points should be used,
            // so if we have missed any there must be a hole (or several) somewhere...
            Vector2 unique;
            //bool uniqueExist = uniques.TryFind(out unique, v => !pointSet3.Contains(v));

            while (uniques.TryFind(out unique, v => !pointSet3.Contains(v)))
            {
                Vector2[] additionalNewPoints = DoMerge(poly1.Points, poly2.Points,
                    pointSet1, pointSet2, visited, unique);
                polys.Add(new Poly(additionalNewPoints));
                pointSet3.UnionWith(additionalNewPoints);
                
                //uniqueExist = uniques.TryFind(out unique, v => !pointSet3.Contains(v));
            }

            if (polys.Count == 1)
            {
                merged = CreatePoly(polys[0].Points, allHoles);
                return true;
            }
            else
            {
                // If we have multiple polygons, the biggest one must logically be te outline.
                // The rest are holes.
                var sorted = polys.OrderByDescending(p => p.Boundary.Area);
                var holes = sorted.Skip(1).Concat(allHoles).ToArray();
                merged = new ComplexPoly(sorted.First(), holes);
                return true;
            }
        }
    }
}

