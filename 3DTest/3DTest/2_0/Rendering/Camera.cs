using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace _3DTest._2_0.Rendering
{
    public class Camera
    {
        protected Matrix view;
        /// <summary>
        /// Gets the view matrix;
        /// </summary>
        public virtual Matrix View
        {
            get { return view; }
        }

        protected Matrix projection;
        /// <summary>
        /// Gets the view matrix;
        /// </summary>
        public virtual Matrix Projection
        {
            get { return projection; }
        }

        protected Vector3 position;
        /// <summary>
        /// Gets or set's the camera's position
        /// </summary>
        public virtual Vector3 Position
        {
            get { return position; }
            set { position = value; lookAt = Position + normal; RebuildView(); }
        }

        protected Vector3 normal;
        /// <summary>
        /// Gets or sets the camera's view normal
        /// </summary>
        public virtual Vector3 Normal
        {
            get { return normal; }
            set { normal = value; lookAt = Position + normal; RebuildView(); }
        }

        protected Vector3 UpVector;

        protected Vector3 lookAt;

        /// <summary>
        /// Creates a new camera
        /// </summary>
        /// <param name="projection">The projection matrix to use</param>
        public Camera(Matrix projection)
        {
            this.projection = projection;
        }

        /// <summary>
        /// Rebuilds the view matrix
        /// </summary>
        protected void RebuildView()
        {
            view = Matrix.CreateLookAt(Position, lookAt, UpVector);
        }
    }
}
