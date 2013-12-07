using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Spine_Library.Graphics;
using Spine_Library.Input;
using Spine_Library.Instances;
using Spine_Library.Inventories;
using Spine_Library.SkeletalAnimation;
using Spine_Library.Tools;
using Spine_Library._3DFuncs;
using Particle3DSample;

namespace _3DTest._2_0
{
    
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game2 : Game
    {
        public static DebugMode Debug;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game2()
        {
            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";

            this.TargetElapsedTime = TimeSpan.FromMilliseconds((1000 / 120));
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            base.Draw(gameTime);
        }

        /// <summary>
        /// Represents a debugging state
        /// </summary>
        public enum DebugMode
        {
            None,
            Partial,
            Full
        }
    }
}