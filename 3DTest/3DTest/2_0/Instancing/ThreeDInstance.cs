using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _3DTest._2_0.Rendering;

namespace _3DTest._2_0.Instancing
{
    public abstract class ThreeDInstance
    {
        protected VertexElement<VertexPositionColor> Model;
        protected BasicEffect Effect;
        Matrix world;

        private Vector3 position;
        /// <summary>
        /// Gets or sets this instance's position
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; UpdateTransform(); }
        }

        private void UpdateTransform()
        {
            world = Matrix.CreateTranslation(Position);
        }

        public abstract void Update(GameTime gameTime);

        public virtual void Render(GraphicsDevice Graphics)
        {
            Effect.World = world;
            Model.Render(Graphics);
        }
    }
}
