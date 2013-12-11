using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _3DTest._2_0.Rendering
{
    /// <summary>
    /// The base class for classes that can be rendered
    /// </summary>
    public abstract class IRenderable
    {
        /// <summary>
        /// Renders this Renderable
        /// </summary>
        /// <param name="Graphics">The GraphicsDevice to render with</param>
        public abstract void Render(GraphicsDevice Graphics);
    }
}
