using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _3DTest._2_0.Rendering;

namespace _3DTest._2_0.Instancing
{
    /// <summary>
    /// Represents a 3D instance
    /// </summary>
    public abstract class ThreeDInstance
    {
        /// <summary>
        /// The object rendered for this instance
        /// </summary>
        protected IRenderable Model;
        /// <summary>
        /// The effect to render with. NOTE: will be replaced with a shader implementation
        /// </summary>
        protected BasicEffect Effect;
        /// <summary>
        /// The world transformation for this instance
        /// </summary>
        protected Matrix world;

        private Vector3 position;
        /// <summary>
        /// Gets or sets this instance's position
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; UpdateTransform(); }
        }

        /// <summary>
        /// Called when the transformation matrix should update
        /// </summary>
        private void UpdateTransform()
        {
            world = Matrix.CreateTranslation(Position);
        }

        /// <summary>
        /// Called when this instance should be updated
        /// </summary>
        /// <param name="gameTime">The current game Time</param>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        /// Renders this instance
        /// </summary>
        /// <param name="Graphics">The GraphicsDevice to render with</param>
        public virtual void Render(GraphicsDevice Graphics)
        {
            if (Model != null)
            {
                Effect.World = world;

                foreach (EffectPass p in Effect.CurrentTechnique.Passes)
                {
                    p.Apply();
                    Model.Render(Graphics);
                }
            }
        }
    }
}
