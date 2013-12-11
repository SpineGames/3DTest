using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace _3DTest._2_0.Rendering
{
    /// <summary>
    /// Represents a vertex model
    /// </summary>
    /// <typeparam name="T">The type of vertex to use</typeparam>
    public class VertexElement<T> : IRenderable where T : struct, IVertexType
    {
        private bool Finalized = false;

        List<T> tempBuffer = new List<T>();
        List<int> tempIndices = new List<int>();
        T[] vertices;
        int[] indices;

        /// <summary>
        /// The number of vertices needed to draw (still a W.I.P)
        /// </summary>
        int vCount = 0;
        /// <summary>
        /// The number of primitives to draw
        /// </summary>
        int pCount = 0;
        /// <summary>
        /// Gets the number of primitives in this element
        /// </summary>
        public int PrimitiveCount
        {
            get { return pCount; }
        }

        PrimitiveType primitiveType;
        /// <summary>
        /// Gets or sets the primitive type to draw with
        /// </summary>
        public PrimitiveType PrimitiveType
        {
            get { return primitiveType; }
            set
            {
                primitiveType = value;

                if (PrimitiveTypeChanged != null)
                    PrimitiveTypeChanged.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Gets or sets the event handler invoked when the primitive type is changed
        /// </summary>
        public EventHandler PrimitiveTypeChanged;
        /// <summary>
        /// Gets or sets the event handler invoked when the element is finalized
        /// </summary>
        public EventHandler FinalizedEvent;

        /// <summary>
        /// Adds a vertex to this element
        /// </summary>
        /// <param name="vertex">The vertex to add</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this element was already finalized</exception>
        public void AddVertex(T vertex)
        {
            if (Finalized)
                throw new InvalidOperationException("The vertex element was already finalized");

            tempBuffer.Add(vertex);
        }

        /// <summary>
        /// Adds an array of vertices to this element
        /// </summary>
        /// <param name="vertex">The vertices to add</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this element was already finalized</exception>
        public void AddVertices(T[] vertices)
        {
            if (Finalized)
                throw new InvalidOperationException("The vertex element was already finalized");

            tempBuffer.AddRange(vertices);
        }

        /// <summary>
        /// Adds an index to this element
        /// </summary>
        /// <param name="index">The index to add</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this element was already finalized</exception>
        public void AddIndex(int index)
        {
            if (Finalized)
                throw new InvalidOperationException("The vertex element was already finalized");

            tempIndices.Add(index);
        }

        /// <summary>
        /// Adds an array of indices to this element
        /// </summary>
        /// <param name="index">The indices to add</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this element was already finalized</exception>
        public void AddIndices(int[] indices)
        {
            if (Finalized)
                throw new InvalidOperationException("The vertex element was already finalized");

            tempIndices.AddRange(indices);
        }

        /// <summary>
        /// Adds a non-finalized vertex element to this element
        /// </summary>
        /// <param name="element">The element to add</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this or the other element was already finalized</exception>
        public void AddElement(VertexElement<T> element)
        {
            if (Finalized || element.Finalized)
                throw new InvalidOperationException("The vertex element was already finalized");

            tempBuffer.AddRange(element.tempBuffer);
            tempIndices.AddRange(element.tempIndices);
        }

        ///<summary>
        ///Generates the basic indices for this model
        ///</summary>
        ///<param name="Override">True if the current indices should be overridden</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this element was already finalized</exception>
        public void GenIndices(bool Override = true)
        {
            if (Finalized)
                throw new InvalidOperationException("The vertex element was already finalized");

            if (Override)
            {
                tempIndices.Clear();
                for (int i = 0; i < tempBuffer.Count; i++)
                    tempIndices.Add(i);
            }
            else
            {
                if (tempIndices.Count == 0)
                {
                    tempIndices.Clear();
                    for (int i = 0; i < tempBuffer.Count; i++)
                        tempIndices.Add(i);
                }
            }
        }

        /// <summary>
        /// Finalizes this Vertex Element and makes changes no longer possible
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this element was already finalized</exception>
        public void Finish()
        {
            if (Finalized)
                throw new InvalidOperationException("The vertex element was already finalized");

            Finalized = true;

            if (tempIndices.Count == 0)
                GenIndices();

            vertices = tempBuffer.ToArray();
            indices = tempIndices.ToArray();

            vCount = vertices.Length;

            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    pCount = indices.Length / 2;
                    break;

                case PrimitiveType.LineStrip:
                    pCount = indices.Length - 1;
                    break;

                case PrimitiveType.TriangleList:
                    pCount = indices.Length - 2;
                    break;

                case PrimitiveType.TriangleStrip:
                    pCount = indices.Length/3;
                    break;
            }

            if (FinalizedEvent != null)
                FinalizedEvent.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Pops the finalization and allows changes to be made to this vertex element
        /// </summary>
        public void Pop()
        {
            tempBuffer.AddRange(vertices);
            tempIndices.AddRange(indices);

            vertices = null;
            indices  = null;

            Finalized = false;
        }

        /// <summary>
        /// Draws this vertex element
        /// </summary>
        /// <param name="Graphics">The GraphicsDevice to draw with</param>
        public override void Render(GraphicsDevice Graphics)
        {
            if (Finalized)
            {
                Graphics.DrawUserIndexedPrimitives<T>(primitiveType, vertices, 0, vCount,
                    indices, 0, pCount);
            }
        }
    }
}
