using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _3DTest._2_0.Rendering
{
    /// <summary>
    /// Some simple utilities for rendering things
    /// </summary>
    public class Utils
    {
        private static VertexPositionColor[] lineVerts = new VertexPositionColor[2];
        private static int[] lineIndices = new int[2] { 0, 1 };

        private static VertexPositionColor[] quadCVerts = new VertexPositionColor[4];
        private static VertexPositionColorTexture[] quadTVerts = new VertexPositionColorTexture[4];
        private static int[] quadIndices = new int[4] { 0, 1, 2, 3};

        /// <summary>
        /// Draws a single line segment between two points
        /// </summary>
        /// <param name="Point1">The first point</param>
        /// <param name="Point2">The second point</param>
        /// <param name="color">The color of the line to draw</param>
        /// <param name="Graphics">The GraphicsDevice to draw with</param>
        public static void DrawLine
            (Vector3 Point1, Vector3 Point2, Color color, GraphicsDevice Graphics)
        {
            lineVerts[0].Position = Point1;
            lineVerts[0].Color = color;

            lineVerts[1].Position = Point2;
            lineVerts[1].Color = color;

            Graphics.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineList,
                lineVerts, 0, 2, lineIndices, 0, 1);
        }

        /// <summary>
        /// Draws a quad
        /// </summary>
        /// <param name="Point1">The first point</param>
        /// <param name="Point2">The second point</param>
        /// <param name="color">The color of the line to draw</param>
        /// <param name="Graphics">The GraphicsDevice to draw with</param>
        public static void DrawQuad2D
            (Rectangle rectangle, Color color, 
            GraphicsDevice Graphics)
        {
            quadCVerts[0].Position = 
                new Vector3(rectangle.X, rectangle.Y, 0);
            quadCVerts[0].Color = color;

            quadCVerts[1].Position = 
                new Vector3(rectangle.X + rectangle.Width, rectangle.Y, 0);
            quadCVerts[1].Color = color;

            quadCVerts[2].Position = 
                new Vector3(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height, 0);
            quadCVerts[2].Color = color;

            quadCVerts[3].Position =
                new Vector3(rectangle.X, rectangle.Y + rectangle.Height, 0);
            quadCVerts[3].Color = color;

            Graphics.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleStrip,
                quadCVerts, 0, 2);
        }
    }
}
