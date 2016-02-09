using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Triangulator;

namespace Haumea_Core.Rendering
{
    // Contains everything that needs to be known to render a primitive.
    public struct RenderInstruction
    {
        public readonly VertexPositionColor[] Vertices;
        public readonly int[] Indices;
        public readonly PrimitiveType Type;

        public RenderInstruction(VertexPositionColor[] vertices, int[] indices, PrimitiveType type)
        {
            Vertices = vertices;
            Indices  = indices;
            Type     = type;
        }

        #region Creator-methods

        // Uses triangulation. Works for any polygon.
        public static RenderInstruction Polygon(Vector2[] points, Color color)
        {
            int[] ind;
            Vector2[] vectors;

            Triangulator.Triangulator.Triangulate(points, WindingOrder.Clockwise, out vectors, out ind);

            VertexPositionColor[] vertices = new VertexPositionColor[vectors.Length];

            for (int n = 0; n < vectors.Length; n++)
            {
                vertices[n].Position = vectors[n].ToVector3();
                vertices[n].Color = color;
            }

            return new RenderInstruction(vertices, ind, PrimitiveType.TriangleList);
        }

        // Uses triangle-fan method. Might be faster for concave polygons. Probably not though.
        public static RenderInstruction ConcavePolygon(Vector2[] points2d, Color color)
        {
            VertexPositionColor[] polygon = new VertexPositionColor[points2d.Length];

            // It's obviously stupid to do this everytime,
            // but I don't really care.
            for (int n = 0; n < points2d.Length; n++)
            {
                polygon[n].Position = points2d[n].ToVector3();
                polygon[n].Color = color;
            }


            int[] ind = new int[(points2d.Length - 2) * 3];

            // Could be done in the same iteration as above.
            for (int n = 0; n < ind.Length - 2; n += 3)
            {
                int ii = n / 3;

                ind[n] = 0;
                ind[n + 1] = (ii + 1);
                ind[n + 2] = (ii + 2);
            }

            return new RenderInstruction(polygon, ind, PrimitiveType.TriangleList);
        }

        public static RenderInstruction Circle(Vector2 center2d, float rad, double rads,
            bool closeCircle, Color c)
        {
            Vector3 center = center2d.ToVector3();
            // number of vertices used
            const int quality = 43;
            // number of triangles used, -1 because the center vertex 
            const int nTriangles = quality - 1;

            VertexPositionColor[] vertexCircle = new VertexPositionColor[quality];
            vertexCircle[0].Position = center;
            vertexCircle[0].Color = c;

            // Find quality - 1 points on the circle pereferi.
            for (int n = 1; n < quality; n++)
            {
                double frac = n / (float) (quality - 1);
                double r    = frac * rads;

                Vector3 offset = new Vector3(rad * (float)Math.Cos(r), rad * (float)Math.Sin(r), 0);
                vertexCircle[n].Position = center + offset;
                vertexCircle[n].Color = c;
            }

            // Every group of three indexes points to indexes that points to coordinates in vertexCircle.
            // Together, these coordinates form a triangle.
            int[] ind = new int[nTriangles * 3];
            short ii = 0;

            for (; ii < ind.Length - 3; ii += 3)
            {
                // + 1 because ii start at zero, but vertexIndex start at 1 since center has index 0
                int vertexIndex = (ii/ 3) + 1;

                // All triangles points to the center,
                // and two coordinates on the pereferi.
                // Note that the order of the two coordinates are important.
                ind[ii] = 0;
                ind[ii + 2] = vertexIndex;
                ind[ii + 1] = vertexIndex + 1;
            }

            // Last one that glues it together.
            if (closeCircle)
            {
                ind[ii] = 0;
                ind[ii + 2] = (short)(ii / 3 + 1);
                ind[ii + 1] = 1;
            }

            return new RenderInstruction(vertexCircle, ind, PrimitiveType.TriangleList);
        }

        public static RenderInstruction Triangle(Vector2 v1, Vector2 v2, Vector2 v3, Color color)
        {
            VertexPositionColor[] vertexTriangle = new VertexPositionColor[3];
            vertexTriangle[0].Position = v1.ToVector3();
            vertexTriangle[1].Position = v2.ToVector3();
            vertexTriangle[2].Position = v3.ToVector3();
            vertexTriangle[0].Color = vertexTriangle[1].Color = vertexTriangle[2].Color = color;

            int[] ind = { 0, 1, 2 };

            return new RenderInstruction(vertexTriangle, ind, PrimitiveType.TriangleList);
        }

        public static RenderInstruction Line(Vector2 start2d, Vector2 end2d, Color color)
        {
            VertexPositionColor[] line = new VertexPositionColor[2];
            line[0].Position = start2d.ToVector3();
            line[1].Position = end2d.ToVector3();
            line[0].Color = line[1].Color = color;

            int[] ind = { 0, 1 };

            return new RenderInstruction(line, ind, PrimitiveType.LineList);
        }

        public static RenderInstruction PolygonOutline(Vector2[] points2d, Color c)
        {
            VertexPositionColor[] polygon = new VertexPositionColor[points2d.Length];

            // It's obviously stupid to do this everytime,
            // but I don't really care.
            for (int n = 0; n < points2d.Length; n++)
            {
                polygon[n].Position = points2d[n].ToVector3();
                polygon[n].Color = c;
            }

            int[] ind = new int[points2d.Length * 2];

            // Could be done in the same iteration as above.
            for (int n = 0; n < points2d.Length - 1; n++)
            {
                ind[n * 2]     = n;
                ind[n * 2 + 1] = n + 1;
            }

            ind[ind.Length - 2] = (ind.Length / 2 - 1);
            ind[ind.Length - 1] = 0;

            return new RenderInstruction(polygon, ind, PrimitiveType.LineList);
        }

        public static RenderInstruction Rectangle(
            Vector2 topLeft2d, Vector2 dims, Color c) 
        {
            Vector3 topLeft     = topLeft2d.ToVector3();
            Vector3 topRight    = new Vector3(topLeft.X + dims.X, topLeft.Y, 0);
            Vector3 bottomRight = new Vector3(topLeft.X, topLeft.Y + dims.Y, 0);
            Vector3 bottomLeft  = new Vector3(topLeft.X + dims.X, topLeft.Y + dims.Y, 0);

            VertexPositionColor[] vertexRectangle = new VertexPositionColor[4];

            vertexRectangle[0].Position = topLeft;
            vertexRectangle[1].Position = topRight;
            vertexRectangle[2].Position = bottomRight;
            vertexRectangle[3].Position = bottomLeft;

            // Do __not__ touch the order of these.
            int[] ind = new int[6];
            ind[0] = 0;
            ind[1] = 2;
            ind[2] = 1;
            ind[3] = 3;
            ind[4] = 1;
            ind[5] = 2;

            vertexRectangle[0].Color = vertexRectangle[1].Color =
                vertexRectangle[2].Color = vertexRectangle[3].Color = c;

            return new RenderInstruction(vertexRectangle, ind, PrimitiveType.TriangleList);
        }

        #endregion
    }
}

