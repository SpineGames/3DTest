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

namespace _3DTest
{
    
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        public static DebugMode Debug;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        BasicEffect basicEffect, mapEffect;
        Effect NormalEffect;
        List<VertexPositionColor> triangles = new List<VertexPositionColor>();
        List<VertexPositionColorTexture> grid = new List<VertexPositionColorTexture>();
        Vector2 cameraPos = new Vector2(0, 0);
        Terrain terrain = new Terrain();

        GraphicsManager graphicsManager = new GraphicsManager();
        Random random = new Random();
        bool debug = false;
        RasterizerState wireFrame = new RasterizerState();
        Vector3 angle;

        Video introVid;
        VideoPlayer vidoePlayer = new VideoPlayer();
        bool isIntroStarted;

        VisualButton Play, LAN;

        NetworkSession netSession;

        byte GAMESTATE = 0;
        const byte GS_INTRO = 0, GS_MENU_MAIN = 1, GS_LOBBY = 2, GS_GAME = 3;
        
        Level level;
        
        public Game1()
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
            wireFrame.FillMode = FillMode.WireFrame;
            this.IsMouseVisible = true;

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

            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;

            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.FogEnabled = false;
            basicEffect.FogColor = Color.DarkGray.ToVector3();
            basicEffect.FogStart = 15F;
            basicEffect.FogEnd = 300F;

            mapEffect = new BasicEffect(graphics.GraphicsDevice);
            mapEffect.VertexColorEnabled = true;
            mapEffect.FogEnabled = false;
            mapEffect.FogColor = Color.DarkGray.ToVector3();
            mapEffect.FogStart = 15F;
            mapEffect.FogEnd = 300F;

            NormalEffect = Content.Load<Effect>("effect");            
            
            Texture2D[] texs = new Texture2D[18];
            for (int i = 0; i < texs.Length; i++)
            {
                texs[i] = Content.Load<Texture2D>("Textures/Terrain/terrain_" + i);
            }

            Texture2D[] guiTexs = new Texture2D[2];
            for (int i = 0; i < guiTexs.Length; i++)
                guiTexs[i] = Content.Load<Texture2D>("Textures/GUI/gui_" + i);

            try
            {
                introVid = Content.Load<Video>("intro");
            }
            catch (ContentLoadException)
            {
                GAMESTATE = GS_MENU_MAIN;
            }

            font = Content.Load<SpriteFont>(@"font");
            FontManager.addFont(font, "GUI");

            //build main menu buttons
            Play = new VisualButton(new Point(400,24), null, VisualButton.T_NORMAL, //play button
                Color.White, Color.Gray, font, "Play", basicEffect);

            LAN = new VisualButton(new Point(400, 64), null, VisualButton.T_NORMAL, //LAN button
                Color.White, Color.Gray, font, "LAN", basicEffect);

            TextureManager.addTexture(texs[0], "tex_blank");
            TextureManager.addTexture(texs[1], "tex_heightmap");
            TextureManager.addTexture(texs[2], "tex_greyBrick");
            TextureManager.addTexture(texs[3], "tex_verticalWood");
            TextureManager.addTexture(texs[4], "tex_orangeGravel");
            TextureManager.addTexture(texs[5], "tex_fenceTex");
            TextureManager.addTexture(texs[6], "tex_pavingStones");
            TextureManager.addTexture(texs[7], "tex_roadStraight");
            TextureManager.addTexture(texs[8], "tex_grass");
            TextureManager.addTexture(texs[9], "tex_roadIntersection");
            TextureManager.addTexture(texs[10], "tex_leaves");
            TextureManager.addTexture(texs[11], "tex_water");
            TextureManager.addTexture(texs[12], "tex_helipad");
            TextureManager.addTexture(texs[13], "tex_blankBlack");
            TextureManager.addTexture(texs[14], "tex_roadTurnLeftDown");
            TextureManager.addTexture(texs[15], "tex_roadTurnRightDown");
            TextureManager.addTexture(texs[16], "tex_roadTurnLeftUp");
            TextureManager.addTexture(texs[17], "tex_roadTurnRightUp");
            TextureManager.addTexture(Content.Load<Texture2D>("Textures/Terrain/grid"), "tex_grid");
            TextureManager.addTexture(guiTexs[0], "gui_noteScribbles");
            TextureManager.addTexture(guiTexs[1], "gui_noteBackdrop");

                        
            level = new Level(this, basicEffect, Content);
            buildLevel(font, Content);
            level.player.position = new Vector3(-500, -500, 1);
        }

        void buildLevel(SpriteFont font, ContentManager Content)
        {
            TextureManager.loadToGlobalModel();

            #region Add Models
            //buildings
            level.addModel(new Building(new Vector3(0, 0, 0), "tex_greyBrick", 20, 20, 10), "house");

            level.addModel(new Building(new Vector3(0, 0, 0), "tex_greyBrick", 5, 5, 5), "crate");
            //trees
            level.addModel(new Tree(new Vector3(0, 0, 0), "tex_verticalWood"), "tree");
            
            //sidewalks
            level.addModel(new Block("tex_pavingStones", new Vector3(-1.5F, -1.5F, 0F), new Vector3(1.5F, 1.5F, 0.04F), Color.White, 1F), "sidewalkSingle");
            level.addModel(new Block("tex_pavingStones", new Vector3(-1F, -1.5F, 0F), new Vector3(1F, 1.5F, 0.04F), Color.White, 1F), "sidewalkSmall_02");
            level.addModel(new Block("tex_pavingStones", new Vector3(-2F, -1.5F, 0F), new Vector3(2F, 1.5F, 0.04F), Color.White, 1F), "sidewalkSmall");
            level.addModel(new Block("tex_pavingStones", new Vector3(-4F, -1.5F, 0F), new Vector3(4F, 1.5F, 0.04F), Color.White, 1F), "sidewalkMedium");
            level.addModel(new Block("tex_pavingStones", new Vector3(-8F, -1.5F, 0F), new Vector3(8F, 1.5F, 0.04F), Color.White, 1F), "sidewalkLong");
            level.addModel(new Block("tex_pavingStones", new Vector3(-53F, -1.5F, 0F), new Vector3(53F, 1.5F, 0.04F), Color.White, 1F), "sidewalk106");
            level.addModel(new Block("tex_pavingStones", new Vector3(-103F, -1.5F, 0F), new Vector3(103F, 1.5F, 0.04F), Color.White, 1F), "sidewalk206");

            //roads
            level.addModel(new Block("tex_roadStraight", new Vector3(-103, -8, 0.0F), new Vector3(103F, 8F, 0.02F), Color.White), "road206");
            level.addModel(new Block("tex_roadStraight", new Vector3(-53, -8, 0.0F), new Vector3(53F, 8F, 0.02F), Color.White), "road106");
            level.addModel(new Block("tex_roadStraight", new Vector3(-8, -8, 0.0F), new Vector3(8F, 8F, 0.02F), Color.White), "roadMedium");
            level.addModel(new Block("tex_roadStraight", new Vector3(-4, -8, 0.0F), new Vector3(4F, 8F, 0.02F), Color.White), "roadSmall");
            level.addModel(new Block("tex_roadStraight", new Vector3(-1, -8, 0.0F), new Vector3(1F, 8F, 0.02F), Color.White), "roadTiny");
                        
            //intersections
            level.addModel(new Block("tex_roadIntersection", new Vector3(-8F, -8F, 0.0F), new Vector3(8F, 8F, 0.02F), Color.White), "intersection");

            //grass
            level.addModel(new Block("tex_grass", new Vector3(-8F, -8F, 0F), new Vector3(8F, 8F, 0.08F), Color.White, 1F), "grass16");
            level.addModel(new Block("tex_grass", new Vector3(-16F, -16F, 0F), new Vector3(16F, 16F, 0.08F), Color.White, 1F), "grass32");
            level.addModel(new Block("tex_grass", new Vector3(-32F, -32F, 0F), new Vector3(32F, 32F, 0.08F), Color.White, 1F), "grass64");
            level.addModel(new Block("tex_grass", new Vector3(-50F, -50F, 0F), new Vector3(50F, 50F, 0.08F), Color.White, 1F), "grass100");
            level.addModel(new Block("tex_grass", new Vector3(-100F, -50F, 0F), new Vector3(100F, 50F, 0.08F), Color.White, 1F), "grass200x100");

            //leaf piles
            level.addModel(new Cone(new Vector3(0, 0, 0), "tex_leaves", Color.White, 5, 2, 20), "leafPile");
            
            //fences
            level.addModel(new Fence("tex_fenceTex", new Vector3(0, 0, 0), new Vector3(0, 100, 0), 2F), "FenceA");
            level.addModel(new Fence("tex_fenceTex", new Vector3(0, 0, 0), new Vector3(200, 0, 0), 2F), "FenceB");

            #endregion

            #region Add Instances
            //grid
            level.addUniqueModel(new Quad("tex_grid", new Vector3(-500, -500, 0), new Vector3(500, 500, 0), Color.Green));

            for (int x = -400; x < 450; x+=222)
                for (int y = -500 + 22; y < 450; y+=122)
                {
                    //adds the fences
                    level.addStaticModel("FenceA", new Vector3(x - 60, y, 0));
                    level.addStaticModel("FenceA", new Vector3(x - 20, y, 0));
                    level.addStaticModel("FenceA", new Vector3(x - 100, y, 0));
                    level.addStaticModel("FenceA", new Vector3(x + 20, y, 0));
                    level.addStaticModel("FenceA", new Vector3(x + 60, y, 0));
                    level.addStaticModel("FenceA", new Vector3(x + 100, y, 0));
                    level.addStaticModel("FenceB", new Vector3(x - 100, y + 50, 0));

                    //houses
                    level.addStaticModel("house", new Vector3(x + -80, y + 80, 0), new Vector2(0, 0.3F));
                    level.addStaticModel("house", new Vector3(x + -80, y + 20, 0));
                    level.addStaticModel("house", new Vector3(x + -40, y + 20, 0));
                    level.addStaticModel("house", new Vector3(x + -40, y + 80, 0));
                    level.addStaticModel("house", new Vector3(x, y + 20, 0));
                    level.addStaticModel("house", new Vector3(x, y + 80, 0));
                    level.addStaticModel("house", new Vector3(x + 40, y + 20, 0));
                    level.addStaticModel("house", new Vector3(x + 40, y + 80, 0));
                    level.addStaticModel("house", new Vector3(x + 80, y + 20, 0));
                    level.addStaticModel("house", new Vector3(x + 80, y + 80, 0));


                    level.addStaticModel("crate", new Vector3(x - 65, y + 80, 0));

                    //Add smoke
                    level.addEmitter(1, new Vector3(x, y, 0), 5);
                    //Add fire
                    level.addEmitter(2, new Vector3(x, y, 0), 10);
                    //Add sparks
                    level.addEmitter(3, new Vector3(x + 20, y, 0), 10);

                    //roads & sidewalks E/W set 1
                    level.addStaticModel("road206", new Vector3(x, y -11, 0));
                    level.addStaticModel("sidewalk206", new Vector3(x, y - 1.5F, 0));
                    level.addStaticModel("sidewalk206", new Vector3(x, y - 20.5F, 0));


                    //roads & sidewalks N/S set 1
                    level.addStaticModel("road106", new Vector3(x + 111F, y + 50F, 0), new Vector2(0, MathHelper.ToRadians(90)));
                    level.addStaticModel("sidewalk106", new Vector3(x + 101.5F, y + 50F, 0), new Vector2(0, MathHelper.ToRadians(90)));
                    level.addStaticModel("sidewalk106", new Vector3(x + 120.5F, y + 50F, 0), new Vector2(0, MathHelper.ToRadians(90)));

                    //intersections
                    level.addStaticModel("intersection", new Vector3(x + 111, y - 11, 0));

                    //trees
                    level.addStaticModel("tree", new Vector3(x + 20, y + 40, 0));

                    //grass builds
                    level.addStaticModel("grass200x100", new Vector3(x, y + 50, 0));
                }

            //notes
            level.addNote(new Note(new Vector3(30, 40, 0.2F), font, "gui_noteScribbles", "gui_noteBackdrop",
                Content.Load<String>("Notes/Note_0")
                ));

            level.addNote(new Note(new Vector3(-30.1F, 15, 2), font, "gui_noteScribbles", "gui_noteBackdrop",
                Content.Load<String>("Notes/Note_1"), 0));
            level.notes.Last().yaw = MathHelper.ToRadians(90);
            level.notes.Last().pitch = MathHelper.ToRadians(0);
            level.notes.Last().roll = MathHelper.ToRadians(270);
            #endregion          

            level.finalize();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            switch (GAMESTATE)
            {
                case GS_INTRO:
                    updateIntro(gameTime);
                    break;
                case GS_MENU_MAIN:
                    updateMenu(gameTime);
                    break;
                case GS_GAME:
                    updateGame(gameTime);
                    break;
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            switch (GAMESTATE)
            {
                case GS_INTRO:
                    drawIntro(gameTime);
                    break;
                case GS_MENU_MAIN:
                    drawMenu(gameTime);
                    break;
                case GS_GAME:
                    drawGame(gameTime);
                    break;
            }

            base.Draw(gameTime);
        }

        private void updateGame(GameTime gameTime)
        {
            if (!IsActive)
                level.setGameSpeed(0F);

            changePos();
            updateViews();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            level.tick(basicEffect, gameTime);
        }

        private void drawGame(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);
            level.render(basicEffect, gameTime, spriteBatch);
        }

        void updateIntro(GameTime gameTime)
        {
            if (!isIntroStarted)
            {
                vidoePlayer.Play(introVid);
                isIntroStarted = true;
            }

            if (Keyboard.GetState().GetPressedKeys().Count() > 0)
                GAMESTATE = GS_MENU_MAIN;
        }

        void drawIntro(GameTime gameTime)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(vidoePlayer.GetTexture(),
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height),
                Color.White);
            spriteBatch.End();
        }

        void updateMenu(GameTime gameTime)
        {
            Play.tick();
            LAN.tick();

            if (Play.getPressed())
                GAMESTATE = GS_GAME;
            if (LAN.getPressed())
                GAMESTATE = GS_LOBBY;
        }

        void drawMenu(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            Play.render(spriteBatch);
            LAN.render(spriteBatch);

            spriteBatch.End();
        }
        
        void updateLobby(GameTime gameTime)
        {
            Play.tick();
            LAN.tick();

            if (Play.getPressed())
                GAMESTATE = GS_GAME;
            if (LAN.getPressed())
                GAMESTATE = GS_LOBBY;
        }

        protected void changePos()
        {
            KeyboardState key = Keyboard.GetState();

            if (key.IsKeyDown(Keys.NumPad7))
                angle.Z += 0.01F;
            if (key.IsKeyDown(Keys.NumPad4))
                angle.Z -= 0.01F;
            if (key.IsKeyDown(Keys.NumPad1))
                angle.Z = 0F;

            if (key.IsKeyDown(Keys.NumPad8))
                angle.Y += 0.01F;
            if (key.IsKeyDown(Keys.NumPad5))
                angle.Y -= 0.01F;
            if (key.IsKeyDown(Keys.NumPad2))
                angle.Y = 0F;

            if (key.IsKeyDown(Keys.NumPad9))
                angle.X += 0.01F;
            if (key.IsKeyDown(Keys.NumPad6))
                angle.X -= 0.01F;
            if (key.IsKeyDown(Keys.NumPad3))
                angle.X = 0F;
        }

        protected void updateViews()
        {
            basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(60),
                (float)graphics.GraphicsDevice.Viewport.Width /
                (float)graphics.GraphicsDevice.Viewport.Height,
                0.1f, 400.0f);
        }        

        static class DrawFunctions3D
        {
            #region cubeMap
            static VertexPositionColorTexture[] cubeMap = new VertexPositionColorTexture[]{
                //top face
                new VertexPositionColorTexture(new Vector3(0, 1, 1), Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), Color.White, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(0, 0, 1), Color.White, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(0, 1, 1), Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(1, 1, 1), Color.White, new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), Color.White, new Vector2(1, 0)),

                //bottom face
                new VertexPositionColorTexture(new Vector3(0, 1, 0), Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(1, 0, 0), Color.White, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(0, 1, 0), Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(1, 0, 0), Color.White, new Vector2(1, 0)),

                //N face
                new VertexPositionColorTexture(new Vector3(0, 1, 0), Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(0, 1, 1), Color.White, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(0, 1, 1), Color.White, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(1, 1, 1), Color.White, new Vector2(1, 0)),
                
                //E face
                new VertexPositionColorTexture(new Vector3(0, 0, 1), Color.White, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0, 1, 0), Color.White, new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(0, 1, 0), Color.White, new Vector2(1, 1)),
                new VertexPositionColorTexture(new Vector3(0, 1, 1), Color.White, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(0, 0, 1), Color.White, new Vector2(0, 0)),

                //W face
                new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), Color.White, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(1, 1, 1), Color.White, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), Color.White, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(1, 1, 0), Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(1, 0, 0), Color.White, new Vector2(1, 1)),
                
                //S face
                new VertexPositionColorTexture(new Vector3(0, 0, 1), Color.White, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(1, 0, 1), Color.White, new Vector2(0, 0)),
                new VertexPositionColorTexture(new Vector3(1, 0, 0), Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0, 0, 1), Color.White, new Vector2(1, 0)),
                new VertexPositionColorTexture(new Vector3(1, 0, 0), Color.White, new Vector2(0, 1)),
                new VertexPositionColorTexture(new Vector3(0, 0, 0), Color.White, new Vector2(1, 1))
            };
            #endregion

            public static void renderTexturedCube(BasicEffect basicEffect, Texture2D tex, Vector3 position, float scale = 1F)
            {
                basicEffect.CurrentTechnique.Passes[0].Apply();
                basicEffect.World = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
                basicEffect.Texture = tex;
                basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, cubeMap, 0, cubeMap.Length / 3);
            }

            public static void renderFace(BasicEffect basicEffect, Texture2D tex, Vector3 position1, Vector3 position2)
            {
            }
        }

        public class GraphicsManager
        {
            List<VertexPositionColorTexture> list = new List<VertexPositionColorTexture>();

            public void addToList(VertexPositionColorTexture[] verticies)
            {
                foreach (VertexPositionColorTexture v in verticies)
                    list.Add(v);
            }

            public void render(BasicEffect basicEffect, bool debug)
            {
                if (list.Count > 0)
                {
                    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        basicEffect.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, list.ToArray(), 0, list.Count / 3);
                    }
                }
                list.Clear();
            }
        }
        
        public class Vehicle : MultiModel
        {
            public float direction, engineYaw, wheelAngle, topSpeed, speed, minZSpeed, topZSpeed, zSpeed;

            /// <summary>
            /// Makes a new vehicle. Note, models[0] is the body, models[1-4] is wheels, and
            /// models[5] is rotated by zSpeed along the yaw every step
            /// </summary>
            /// <param name="position">The position of this vehicle</param>
            /// <param name="tex">The texture to map to</param>
            /// <param name="color">The color of this vehicle</param>
            /// <param name="topSpeed">The top horizontal velocity</param>
            /// <param name="topZSpeed">The top vertical velocity</param>
            public Vehicle(Vector3 position, float topSpeed = 1, float minZSpeed = 0, float topZSpeed = 0, float direction = 0)
            {
                this.position = position;
                this.topSpeed = topSpeed;
                this.minZSpeed = minZSpeed;
                this.topZSpeed = topZSpeed;
            }

            public void tick(float gameSpeed)
            {
                if (speed > 0)
                {
                    position += new Vector3(extraMath.calculateVectorOffset(direction, speed), zSpeed);
                }
                engineYaw += zSpeed;
                wheelAngle = direction;

                if (models[5] != null)
                    models[5].yaw = engineYaw;

                if (models[0] != null)
                    models[0].yaw = direction;

                for (int i = 1; i < 5; i++)
                    if (models[i] != null)
                        models[i].yaw = direction;

                position.Z += zSpeed;

                if (position.Z < 0)
                {
                    position.Z = 0;
                    zSpeed = 0;
                }

                if (position.Z > 0)
                {
                    zSpeed -= 1F;
                }
            }

            public void changeSpeed(float amount)
            {
                speed += amount;
                speed = MathHelper.Clamp(speed, -topSpeed, topSpeed);
            }

            public void changeZSpeed(float amount)
            {
                zSpeed += amount;
                zSpeed = MathHelper.Clamp(zSpeed, -topZSpeed, topZSpeed);
            }

            public void setZSpeed(float speed)
            {
                zSpeed = MathHelper.Clamp(speed, -topZSpeed, topZSpeed);
            }

            public void changeDirection(float amount)
            {
                direction += amount;
                direction = MathHelper.WrapAngle(direction);
            }
        }

        public class Laser : VertexModel
        {
            public Level currentLevel;
            public Vector2 direction;
            public float speed, life;
            public int power = 1;
            public Ray ray;

            public Laser(Level level, Vector3 pos, Vector2 direction, Color color, float speed = 1F, float life = 100)
            {
                this.currentLevel = level;
                this.position = pos;
                this.direction = direction;
                this.speed = speed;
                this.life = life;

                lineList.Add(new VertexPositionColor(new Vector3(0, 0, 0), color));
                lineList.Add(new VertexPositionColor(new Vector3(speed, 0, 0), color));

                yaw = direction.X;
                pitch = -direction.Y;

                 ray = new Ray(position, new Vector3(roll, pitch, yaw));
            }

            public Laser(Level level, Vector3 pos, Vector2 direction, Color color, int size, float speed = 1F, float life = 100)
            {
                this.currentLevel = level;
                this.position = pos;
                this.direction = direction;
                this.speed = speed;
                this.life = life;
                this.power = size;

                ModelAdditions.addLaserToModel(colorVerts, color, size, speed);

                yaw = direction.X;
                pitch = -direction.Y;

                ray = new Ray(position, new Vector3(roll, pitch, yaw));
            }

            public void tick(float gameSpeed = 0)
            {
                ray.Position = position;

                life -= gameSpeed;
                if (life <= 0)
                {
                    currentLevel.disposeLaser(this);
                }
                position += extraMath.getVector3(direction, speed * gameSpeed);
            }
        }

        public class Note : VertexModel
        {
            public string text;
            public Texture2D noteBack;
            public SpriteFont font;

            public Note(Vector3 position, SpriteFont font, string tex, string noteBack, string text = "", float yaw = 0.2F)
            {
                this.position = position;
                this.yaw = yaw;
                this.font = font;
                texID = TextureManager.getID(tex);
                this.noteBack = TextureManager.getTex(noteBack);
                this.text = text;

                verts.Add(new VertexPositionColorTexture(new Vector3(-0.1F, -0.1F, 0), Color.Tan, new Vector2(1, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(-0.1F, 0.2F, 0), Color.Tan, new Vector2(1, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(0.1F, 0.2F, 0), Color.Tan, new Vector2(0, 1)));

                verts.Add(new VertexPositionColorTexture(new Vector3(-0.1F, -0.1F, 0), Color.Tan, new Vector2(1, 0)));
                verts.Add(new VertexPositionColorTexture(new Vector3(0.1F, 0.2F, 0), Color.Tan, new Vector2(0, 1)));
                verts.Add(new VertexPositionColorTexture(new Vector3(0.1F, -0.1F, 0), Color.Tan, new Vector2(0, 0)));
            }
        }

        public class Player : VertexModel
        {
            Level currentLevel;
            float height = 2;
            Vector3 prevPos;
            public double zoom = 45, cameraAngle = 0, cameraPitch = 0;
            public CameraModes cameraMode = CameraModes.CM_FPS;
            public Vector3 direction;
            Gun currentGun;
            float zAcc, maxSpeed = 0.2F, speed = 0, hSpeed;
            public enum CameraModes { CM_FPS = 0, CM_3P = 1 }
            KeyWatcher space = new KeyWatcher(Keys.Space);
            Timer spaceTapTimer = new Timer(TimeSpan.Zero);

            public Player(Level level, Vector3 position, Texture2D gunTex, float height = 2)
            {
                texID = 0;
                this.currentLevel = level;
                this.position = position;
                this.boundingBox = new BoundingBox(position - new Vector3(2, 2, 0), position + new Vector3(2, 2, height));
                this.height = height;

                this.currentGun = new MachineGun(position, "tex_greyBrick");

                Vector2 center = Vector2.Zero;
                float radius = 1.3F;
                //figure out the difference
                double increment = (Math.PI * 2) / 20;

                float texOff = 10F / 20;
                //render
                double angle = 0;
                for (int i = 0; i < 20; i++, angle += increment)
                {
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), height), Color.Brown, new Vector2(texOff * (float)(angle), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), Color.Brown, new Vector2(texOff * (float)(angle + increment), -height)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), Color.Brown, new Vector2(texOff * (float)(angle + increment), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle + increment), 0)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), height), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, height), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle + increment, radius)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle + increment, radius)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, 0), Color.Brown, extraMath.calculateVector(new Vector2(0.5F, 0.5F), angle, 0)));
                }
                lineList.Add(new VertexPositionColor(new Vector3(0, 0, height / 2), Color.Red));
                lineList.Add(new VertexPositionColor(new Vector3(5, 0, height / 2), Color.Red));
            }

            public void tickThis(BasicEffect effect, OrientedBoundingBox[] collisions, BoundingSphere[] sCollisions, BoundingPlane[] pCollisions, GameTime gameTime, float gameSpeed = 0F)
            {
                if (gameSpeed > 0)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                        maxSpeed = 0.5F;
                    else
                        maxSpeed = 0.2F;

                    currentGun.yaw = direction.X;
                    currentGun.pitch = -direction.Y;
                    currentGun.tick(gameSpeed);
                    boundingBox = new BoundingBox(position - new Vector3(2, 2, 0), position + new Vector3(2, 2, height));
                    KeyboardState key = Keyboard.GetState();
                    handleKeyPresses(key, gameTime);
                    physics(collisions, sCollisions, pCollisions);
                    currentGun.position = position + extraMath.getVector3(new Vector2(direction.X, 0), 0.1F) + 
                        new Vector3(0,0,height - 0.5F);
                }

                currentGun.render(effect, position + new Vector3(0,0,3), direction);

                if (cameraMode == CameraModes.CM_FPS)
                {
                    effect.View = Matrix.CreateLookAt(position + new Vector3(0, 0, height),
                    position + extraMath.getVector3(new Vector2((float)cameraAngle, (float)cameraPitch), 100),
                    new Vector3(0, 0, 1)
                    );
                }
                else if (cameraMode == CameraModes.CM_3P)
                {
                    cameraPitch = 0;
                    effect.View = Matrix.CreateLookAt(new Vector3(extraMath.calculateVector(
                        new Vector2(position.X, position.Y),
                        MathHelper.WrapAngle((float)cameraAngle - (float)Math.PI),
                        zoom / 6.125F),
                        position.Z + height + 1),
                        position + new Vector3(0, 0, height), // player's head
                        new Vector3(0, 0, 1));
                }
                prevPos = position;
            }

            public Gun getGun()
            {
                return currentGun;
            }

            private void handleKeyPresses(KeyboardState key, GameTime gameTime)
            {
                MouseState mouse = Mouse.GetState();
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    currentGun.fire(currentLevel, new Vector2(direction.Z, direction.X));
                }
                space.update();

                if (space.wasPressed & zAcc == 0)
                {
                    zAcc = -0.5F;
                    position.Z += 0.51F;
                }

                if (space.wasReleased)
                    spaceTapTimer = new Timer(new TimeSpan(0, 0, 0, 0, 80));

                if (spaceTapTimer.tick(gameTime))
                    if (key.IsKeyDown(Keys.Space))
                        position.Z = 0;

                cameraAngle += (400F - mouse.X) / 180F;
                cameraPitch += (240F - mouse.Y) / 90F;
                cameraAngle = MathHelper.WrapAngle((float)cameraAngle);
                cameraPitch = MathHelper.Clamp((float)cameraPitch, -MathHelper.PiOver2 + 0.01F, MathHelper.PiOver2 - 0.01F);

                Mouse.SetPosition(400, 240);

                if (key.IsKeyDown(Keys.Left))
                {
                    cameraAngle += 0.01F;
                }
                if (key.IsKeyDown(Keys.Right))
                {
                    cameraAngle -= 0.01F;
                }
                if (key.IsKeyDown(Keys.Up))
                {
                    cameraPitch += 0.01F;
                }
                if (key.IsKeyDown(Keys.Down))
                {
                    cameraPitch -= 0.01F;
                }

                direction = new Vector3(0, 0, (float)cameraAngle);

                if (key.IsKeyDown(Keys.W))
                {
                    speed = maxSpeed;
                }
                else if (key.IsKeyDown(Keys.S))
                {
                    speed = -maxSpeed;
                }
                else
                {
                    speed = 0;
                }

                if (key.IsKeyDown(Keys.A))
                {
                    hSpeed = 0.25F;
                }
                else if (key.IsKeyDown(Keys.D))
                {
                    hSpeed = -0.25F;
                }
                else
                {
                    hSpeed = 0;
                }
                if (hSpeed > 0)
                    position += new Vector3(extraMath.calculateVectorOffset(cameraAngle + MathHelper.PiOver2, 0.25F), 0);
                if (hSpeed < 0)
                    position += new Vector3(extraMath.calculateVectorOffset(cameraAngle - MathHelper.PiOver2, 0.25F), 0);
                position += new Vector3(extraMath.calculateVectorOffset(cameraAngle, speed), 0);
                
                if (key.IsKeyDown(Keys.OemPlus))
                {
                    if (zoom > 1F)
                        zoom -= 1F;
                }
                if (key.IsKeyDown(Keys.OemMinus))
                    if (zoom < 179)
                        zoom += 1F;
            }

            private void physics(OrientedBoundingBox[] collisionChecks, BoundingSphere[] sCollisions, BoundingPlane[] pCollisions)
            {
                if (position.Z > 0)
                {
                    position.Z -= zAcc;
                    zAcc += 0.02F;
                }
                else
                {
                    zAcc = 0;
                    position.Z = 0;
                }

                Vector3 forwardCheck = new Vector3(extraMath.calculateVectorOffset(cameraAngle, speed), 0) + position;
                Vector3 backCheck = new Vector3(extraMath.calculateVectorOffset(cameraAngle - MathHelper.Pi, speed), 0) + position;
                Vector3 rightCheck = new Vector3(extraMath.calculateVectorOffset(cameraAngle + MathHelper.PiOver2, speed), 0) + position;
                Vector3 leftCheck = new Vector3(extraMath.calculateVectorOffset(cameraAngle - MathHelper.PiOver2, speed), 0) + position;
                Vector3 downCheck = new Vector3(0,0, -zAcc) + position;

                foreach (OrientedBoundingBox model in collisionChecks)
                {
                    if (speed > 0)
                        if (model.Contains(ref forwardCheck))
                        {
                            position += new Vector3(extraMath.calculateVectorOffset(cameraAngle - MathHelper.Pi, speed), 0);
                        }
                    if (speed < 0)
                        if (model.Contains(ref backCheck))
                        {
                            position += new Vector3(extraMath.calculateVectorOffset(cameraAngle, -speed), 0);
                        }
                    if (hSpeed > 0)
                        if (model.Contains(ref rightCheck))
                        {
                            position += new Vector3(extraMath.calculateVectorOffset((cameraAngle - MathHelper.PiOver2), 0.25F), 0);
                        }
                    if (hSpeed < 0)
                        if (model.Contains(ref leftCheck))
                        {
                            position += new Vector3(extraMath.calculateVectorOffset((cameraAngle + MathHelper.PiOver2), 0.25F), 0);
                        }
                    if (zAcc > 0)
                    {
                        if (model.Contains(ref downCheck))
                        {
                            zAcc = 0;
                        }
                    }
                }
            }
        }

        public class Gun : VertexModel
        {
            Color bulletColor;
            float speed, damage = 10, zoom, shootSpeed, timer;

            public Gun(Vector3 position, string tex, Color bulletColor, float zoom = 45, float shootSpeed = 1, float speed = 2F, float damage = 1)
            {
                texID = TextureManager.getID(tex);
                this.bulletColor = bulletColor;
                this.zoom = zoom;
                this.speed = speed;
                this.shootSpeed = shootSpeed;
                this.damage = damage;
            }

            public void tick(float gameSpeed)
            {
                if (timer > 0)
                    timer -= gameSpeed;
            }

            public void fire(Level level, Vector2 direction)
            {
                if (timer <= 0)
                {
                    level.addLaser(new Laser(level, position, direction, bulletColor, (int)damage, 2F, 100));
                    timer = shootSpeed;
                }
            }
        }

        public class MachineGun : Gun
        {
            public MachineGun(Vector3 position, string tex)
                : base(position, tex, Color.Yellow, 30, 10, 3, 2)
            {
                ModelAdditions.AddCubeToModel(verts, new Vector3(0, -0.1F, 0), new Vector3(2, 0.1F, 0.2F), Color.Gray);
            }
        }

        public class Chopper : Vehicle
        {
            Random rand = new Random();
            float rotorSpeed;

            public Chopper(Vector3 pos, Color color)
                : base(pos, 1, 3, 5, 0)
            {
                models.Add(new Block("tex_blankBlack", pos + new Vector3(-8, -3, 0), pos + new Vector3(4, 3, 3), color));
                models.Last().orgin = new Vector3(-3, -3, 0);
                models.Add(new ChopperBlade("tex_greyBrick", pos + new Vector3(0, 0, 3.5F), Color.White, 1F));
                models.Last().orgin = new Vector3(0, 0, 3.5F);
            }

            new public void tick(float gameSpeed)
            {
                if (speed > 0)
                {
                    position += new Vector3(extraMath.calculateVectorOffset(direction, speed), zSpeed);
                }

                engineYaw += rotorSpeed;
                models[1].yaw = engineYaw;

                if (position.Z > 0)
                    zSpeed -= 0.02F;

                if (position.Z < 0)
                {
                    setZSpeed(0);
                    position.Z = 0;
                }

                if (rotorSpeed >= minZSpeed)
                {
                    zSpeed = rotorSpeed - minZSpeed;
                }

                position.Z += zSpeed;
            }

            public void changeRotorSpeed(float amount)
            {
                rotorSpeed += amount;
            }

            public void setRotorSpeed(float speed)
            {
                rotorSpeed = speed;
            }
        }

        public class Pool : MultiModel
        {
            float waterLevel, midLevel, height, t;

            public Pool(Vector3 position, string side, string water, Color sideColor, Color waterColor, float radius = 8, float height = 4F, float waterLevel = 0.75F)
            {
                boundingSphere = new BoundingSphere(position, radius + 0.1F);
                this.position = position;
                this.waterLevel = waterLevel;
                this.midLevel = waterLevel;
                this.height = height;
                models.Add(new BackFaceCylinder(position, side, sideColor, radius, height, 20));
                models.Add(new CylinderFace(position, side, sideColor, radius, 20));
                models.Add(new CylinderFace(position + new Vector3(0, 0, height * waterLevel), water, waterColor, radius, 20));
            }

            public void tick(float gameSpeed)
            {
                t += 0.5F * gameSpeed;
                waterLevel = 0.75F + (float)Math.Sin(t / 20) / 30;
                models.Last().orgin.Z = height * waterLevel;
            }
        }

        public class Building : VertexModel
        {
            public Building(Vector3 position, string skin, int length = 10, int width = 10, int height = 5, float yaw = 0F)
            {
                VertexModel m = new VertexModel();
                texID = TextureManager.getID(skin);

                this.yaw = yaw;
                orgin = new Vector3(-width / 2, -length / 2, 0);
                this.boundingBox = new BoundingBox(position + orgin - new Vector3(1, 1, 1), position + new Vector3(length, width, height * 1.5F) + orgin + new Vector3(1, 1, 1));

                Vector3 BLB = new Vector3(0, 0, 0),
                     BLF = new Vector3(0, length, 0),
                     BRB = new Vector3(width, 0, 0),
                     BRF = new Vector3(width, length, 0),
                     TLB = new Vector3(0, 0, height),
                     TLF = new Vector3(0, length, height),
                     TRB = new Vector3(width, 0, height),
                     TRF = new Vector3(width, length, height),
                     PEAK = new Vector3(width / 2, length / 2, height + height / 1.5F);

                this.verts.Add(new VertexPositionColorTexture(BLB, Color.White, new Vector2(0, height)));
                this.verts.Add(new VertexPositionColorTexture(BLF, Color.White, new Vector2(length, height)));
                this.verts.Add(new VertexPositionColorTexture(TLB, Color.White, new Vector2(0, 0)));
                this.verts.Add(new VertexPositionColorTexture(BLF, Color.White, new Vector2(length, height)));
                this.verts.Add(new VertexPositionColorTexture(TLF, Color.White, new Vector2(length, 0)));
                this.verts.Add(new VertexPositionColorTexture(TLB, Color.White, new Vector2(0, 0)));

                this.verts.Add(new VertexPositionColorTexture(TRF, Color.White, new Vector2(0, 0)));
                this.verts.Add(new VertexPositionColorTexture(BRF, Color.White, new Vector2(0, height)));
                this.verts.Add(new VertexPositionColorTexture(TRB, Color.White, new Vector2(length, 0)));
                this.verts.Add(new VertexPositionColorTexture(BRB, Color.White, new Vector2(length, height)));
                this.verts.Add(new VertexPositionColorTexture(TRB, Color.White, new Vector2(length, 0)));
                this.verts.Add(new VertexPositionColorTexture(BRF, Color.White, new Vector2(0, height)));

                this.verts.Add(new VertexPositionColorTexture(BLF, Color.White, new Vector2(0, height)));
                this.verts.Add(new VertexPositionColorTexture(TRF, Color.White, new Vector2(length, 0)));
                this.verts.Add(new VertexPositionColorTexture(TLF, Color.White, new Vector2(0, 0)));
                this.verts.Add(new VertexPositionColorTexture(BLF, Color.White, new Vector2(0, height)));
                this.verts.Add(new VertexPositionColorTexture(BRF, Color.White, new Vector2(length, height)));
                this.verts.Add(new VertexPositionColorTexture(TRF, Color.White, new Vector2(length, 0)));


                this.verts.Add(new VertexPositionColorTexture(TRB, Color.White, new Vector2(0, 0)));
                this.verts.Add(new VertexPositionColorTexture(BRB, Color.White, new Vector2(0, height)));
                this.verts.Add(new VertexPositionColorTexture(BLB, Color.White, new Vector2(length, height)));
                this.verts.Add(new VertexPositionColorTexture(TLB, Color.White, new Vector2(length, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRB, Color.White, new Vector2(0, 0)));
                this.verts.Add(new VertexPositionColorTexture(BLB, Color.White, new Vector2(length, height)));

                this.verts.Add(new VertexPositionColorTexture(PEAK, new Color(16, 16, 16), new Vector2(length / 2, 0)));
                this.verts.Add(new VertexPositionColorTexture(TLF + new Vector3(-1, 1, 0), new Color(16, 16, 16), new Vector2(0, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(TRF + new Vector3(1, 1, 0), new Color(16, 16, 16), new Vector2(length, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(PEAK, new Color(16, 16, 16), new Vector2(length / 2, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRB + new Vector3(1, -1, 0), new Color(16, 16, 16), new Vector2(length, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(TLB + new Vector3(-1, -1, 0), new Color(16, 16, 16), new Vector2(0, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(PEAK, new Color(16, 16, 16), new Vector2(length / 2, 0)));
                this.verts.Add(new VertexPositionColorTexture(TLB + new Vector3(-1, -1, 0), new Color(16, 16, 16), new Vector2(0, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(TLF + new Vector3(-1, 1, 0), new Color(16, 16, 16), new Vector2(length, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(PEAK, new Color(16, 16, 16), new Vector2(length / 2, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRF + new Vector3(1, 1, 0), new Color(16, 16, 16), new Vector2(length, height / 2)));
                this.verts.Add(new VertexPositionColorTexture(TRB + new Vector3(1, -1, 0), new Color(16, 16, 16), new Vector2(0, height / 2)));

                //bottom of roof
                this.verts.Add(new VertexPositionColorTexture(TLB + new Vector3(-1, -1, 0), new Color(16, 16, 16), new Vector2(0, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRF + new Vector3(1, 1, 0), new Color(16, 16, 16), new Vector2(width, length)));
                this.verts.Add(new VertexPositionColorTexture(TLF + new Vector3(-1, 1, 0), new Color(16, 16, 16), new Vector2(0, length)));
                this.verts.Add(new VertexPositionColorTexture(TLB + new Vector3(-1, -1, 0), new Color(16, 16, 16), new Vector2(0, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRB + new Vector3(1, -1, 0), new Color(16, 16, 16), new Vector2(width, 0)));
                this.verts.Add(new VertexPositionColorTexture(TRF + new Vector3(1, 1, 0), new Color(16, 16, 16), new Vector2(width, length)));
            }
        }

        public class Tree : VertexModel
        {
            public Tree(Vector3 position, string texture, float radius = 1, int height = 20, int stepping = 10)
            {
                this.boundingBox = new BoundingBox(new Vector3(position.X - radius * 1.5F, position.Y - radius * 1.5F, position.Z),
                    new Vector3(position.X + radius * 1.5F, position.Y + radius * 1.5F, position.Z + height));
                Vector2 center = new Vector2(0, 0);
                this.position = position;
                //figure out the difference
                double increment = (Math.PI * 2) / stepping;

                float texOff = 10F / stepping;
                //render
                double angle = 0;
                for (int i = 0; i < stepping; i++, angle += increment)
                {
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), height), Color.Brown, new Vector2(texOff * (float)(angle), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), Color.Brown, new Vector2(texOff * (float)(angle + increment), -height)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), height), Color.Brown, new Vector2(texOff * (float)(angle + increment), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius), 0), Color.Brown, new Vector2(texOff * (float)(angle + increment), 0)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius * 4), height - 10), Color.Green, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, height + 10), Color.Green, new Vector2(texOff * (float)(angle + increment), -height)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius * 4), height - 10), Color.Green, new Vector2(texOff * (float)(angle + increment), 0)));

                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle, radius * 4), height - 10), Color.Green, new Vector2(texOff * (float)(angle), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(extraMath.calculateVector(center, angle + increment, radius * 4), height - 10), Color.Green, new Vector2(texOff * (float)(angle + increment), 0)));
                    verts.Add(new VertexPositionColorTexture(new Vector3(center, height - 10), Color.Green, new Vector2(texOff * (float)(angle + increment / 2), -height)));
                }
            }
        }

        /// <summary>  
        /// Provides a set of methods for the rendering BoundingBoxs.  
        /// </summary>  
        public static class BoundingBoxRenderer
        {
            #region Fields

            static VertexPositionColor[] verts = new VertexPositionColor[8];
            static Int16[] indices = new Int16[]  
        {  
            0, 1,  
            1, 2,  
            2, 3,  
            3, 0,  
            0, 4,  
            1, 5,  
            2, 6,  
            3, 7,  
            4, 5,  
            5, 6,  
            6, 7,  
            7, 4,  
        };
            #endregion

            /// <summary>  
            /// Renders the bounding box for debugging purposes.  
            /// </summary>  
            /// <param name="box">The box to render.</param>  
            /// <param name="graphicsDevice">The graphics device to use when rendering.</param>  
            /// <param name="view">The current view matrix.</param>  
            /// <param name="projection">The current projection matrix.</param>  
            /// <param name="color">The color to use drawing the lines of the box.</param>  
            public static void Render(BoundingBox box, BasicEffect effect, Color color)
            {
                effect.TextureEnabled = false;

                Vector3[] corners = box.GetCorners();
                for (int i = 0; i < 8; i++)
                {
                    verts[i].Position = corners[i];
                    verts[i].Color = color;
                }
                
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    effect.GraphicsDevice.DrawUserIndexedPrimitives(
                        // TEST PrimitiveType.LineList,  
                        PrimitiveType.LineList,
                       verts,
                        0,
                        8,
                        indices,
                        0,
                        indices.Length / 2);

                }

                effect.TextureEnabled = true;
            }

            /// <summary>  
            /// Renders the bounding box for debugging purposes.  
            /// </summary>  
            /// <param name="box">The box to render.</param>  
            /// <param name="graphicsDevice">The graphics device to use when rendering.</param>  
            /// <param name="view">The current view matrix.</param>  
            /// <param name="projection">The current projection matrix.</param>  
            /// <param name="color">The color to use drawing the lines of the box.</param>  
            public static void Render(OrientedBoundingBox box, BasicEffect effect, Color color)
            {
                effect.TextureEnabled = false;

                Vector3[] corners = box.GetCorners();
                for (int i = 0; i < 8; i++)
                {
                    verts[i].Position = corners[i];
                    verts[i].Color = color;
                }

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    effect.GraphicsDevice.DrawUserIndexedPrimitives(
                        // TEST PrimitiveType.LineList,  
                        PrimitiveType.LineList,
                       verts,
                        0,
                        8,
                        indices,
                        0,
                        indices.Length / 2);

                }

                effect.TextureEnabled = true;
            }
        } 

        public class Level
        {
            public Player player;
            public List<Note> notes = new List<Note>();
            public List<Note> pickedUpNotes = new List<Note>();
            List<Laser> lasers = new List<Laser>();
            List<OrientedBoundingBox> collisions = new List<OrientedBoundingBox>();
            //List<BoundingBox> collisions = new List<BoundingBox>();
            List<BoundingSphere> sCollisions = new List<BoundingSphere>();
            List<BoundingPlane> pCollisions = new List<BoundingPlane>();

            List<Instance3D> staticInstances = new List<Instance3D>();

            KeyWatcher debug;
            KeyWatcher cameraMode;
            KeyWatcher pause;
            FPSHandler fps = new FPSHandler();
            FPSHandler cps = new FPSHandler();

            ExplosionParticleSystem explosions;
            SmokePlumeParticleSystem smoke;
            FireParticleSystem fire;
            SparksParticleSystem sparks;
            List<ParticleEmitter> emitters = new List<ParticleEmitter>();
            Effect effect2;
            
            bool boolDebug = false;
            float gameSpeed = 1F;
            RasterizerState wireFrame;
            BasicEffect effect;
            Note drawingNote;
            Random rand = new Random();
            int scroll = 0;

            Texture2D blank;
            Model turret;
            GameTime time, prevTime;
            int fenceCount = 0, uniqueModels = 0, staticInstanceCount, polyCount;

            public const byte C_EXPLOSION = 0, C_SMOKE = 1, C_FIRE = 2; 

            public Level(Game1 game, BasicEffect effect, ContentManager Content)
            {
                explosions = new ExplosionParticleSystem(game, game.Content);
                explosions.Initialize();
                smoke = new SmokePlumeParticleSystem(game, game.Content);
                smoke.Initialize();
                fire = new FireParticleSystem(game, game.Content);
                fire.Initialize();
                sparks = new SparksParticleSystem(game, game.Content);
                sparks.Initialize();

                effect2 = game.Content.Load<Effect>("effect");
                game.Components.Add(explosions);
                game.Components.Add(smoke);
                game.Components.Add(fire);

                this.effect = effect;
                player = new Player(this, new Vector3(0, 0, 0), TextureManager.getTex("txt_greyBrick"), 2.5F);
                wireFrame = new RasterizerState();
                wireFrame.FillMode = FillMode.WireFrame;
                
                blank = new Texture2D(game.GraphicsDevice, 1, 1);
                blank.SetData(new [] {Color.White});
                
                debug = new KeyWatcher(Keys.F1, new EventHandler(debugPressed));
                cameraMode = new KeyWatcher(Keys.F5, new EventHandler(cameraChange));
                pause = new KeyWatcher(Keys.P, new EventHandler(pausePressed));
            }

            public void finalize()
            {
                foreach (Instance3D instance in staticInstances)
                {
                    GlobalStaticModel.addModel(instance);
                    polyCount += ModelManager.getModel(instance.model).verts.Count;
                }
                staticInstanceCount = staticInstances.Count;
                staticInstances.Clear();
            }

            public void addStaticModel(string modelName, Vector3 position)
            {
                staticInstances.Add(new Instance3D() { model = ModelManager.getIndex(modelName), position = position,
                pitch = ModelManager.getModel(modelName).pitch, 
                yaw = ModelManager.getModel(modelName).yaw, 
                roll = ModelManager.getModel(modelName).roll});
                staticInstances.Last().rebuildMatrix(ModelManager.getModel(modelName).orgin, 1F);
                collisions.Add(new OrientedBoundingBox(extraMath.shiftBox(ModelManager.getModel(modelName).boundingBox, position),
                    Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), 0)));
            }
                        
            public void addStaticModel(string modelName, Vector3 position, Vector2 direction)
            {
                staticInstances.Add(new Instance3D() { model = ModelManager.getIndex(modelName), position = position,
                pitch = direction.X, yaw = direction.Y});
                staticInstances.Last().rebuildMatrix(ModelManager.getModel(modelName).orgin, 1F);
                collisions.Add(new OrientedBoundingBox(extraMath.shiftBox(ModelManager.getModel(modelName).boundingBox, position), 
                    Quaternion.CreateFromAxisAngle(new Vector3(0,0,1),direction.Y)));
            }

            public void addModel(VertexModel model, string name)
            {
                ModelManager.addModel(model, name);
            }

            public void addFence(Fence fence)
            {
                ModelManager.addModel((VertexModel)fence, "AGFence_" + fenceCount);

                staticInstances.Add(new Instance3D() {model = ModelManager.getIndex("AGFence_" + fenceCount), position  = fence.position,
                yaw = fence.yaw, pitch = fence.pitch
                });
                staticInstances.Last().rebuildMatrix(ModelManager.getModel("AGFence_" + fenceCount).orgin, 1F);

                BoundingBox b = fence.boundingBox;
                //Plane p = new Plane(fence.start, fence.end + new Vector3(0, 0, fence.height), fence.end);
                //p.D = 0;
                //pCollisions.Add(new BoundingPlane(b, p));
                collisions.Add(new OrientedBoundingBox(b));

                fenceCount++;
            }

            public void addUniqueModel(VertexModel model)
            {
                ModelManager.addModel(model, "AGUNIQUE_" + uniqueModels);
                staticInstances.Add(new Instance3D() { model = ModelManager.getIndex("AGUNIQUE_" + uniqueModels), position = model.position });
                staticInstances.Last().rebuildMatrix(ModelManager.getModel("AGUNIQUE_" + uniqueModels).orgin, 1F);
                uniqueModels++;
            }

            public void addBoundingBox(Vector3 min, Vector3 max)
            {
                min = new Vector3(Math.Min(min.X, max.X), Math.Min(min.Y, max.Y), Math.Min(min.Z, max.Z));
                max = new Vector3(Math.Max(min.X, max.X), Math.Max(min.Y, max.Y), Math.Max(min.Z, max.Z));
                collisions.Add(new OrientedBoundingBox(new BoundingBox(min,max)));
            }

            public void addEmitter(byte type, Vector3 position, float partsPerSecond = 1)
            {
                switch (type)
                {
                    case 0:
                        emitters.Add(new ParticleEmitter(explosions, partsPerSecond, position));
                        break;
                    case 1:
                        emitters.Add(new ParticleEmitter(smoke, partsPerSecond, position));
                        break;
                    case 2:
                        emitters.Add(new ParticleEmitter(fire, partsPerSecond, position));
                        break;
                    case 3:
                        emitters.Add(new ParticleEmitter(sparks, partsPerSecond, position));
                            break;
                }
            }

            public void addEmitter(byte type, Vector3 position, Vector3 endPos, float partsPerSecond = 1)
            {
                switch (type)
                {
                    case 0:
                        emitters.Add(new ParticleEmitter(explosions, partsPerSecond, position, endPos));
                        break;
                    case 1:
                        emitters.Add(new ParticleEmitter(smoke, partsPerSecond, position, endPos));
                        break;
                    case 2:
                        emitters.Add(new ParticleEmitter(fire, partsPerSecond, position, endPos));
                        break;
                    case 3:
                        emitters.Add(new ParticleEmitter(sparks, partsPerSecond, position, endPos));
                        break;
                }
            }
            
            public void addLaser(Laser laser)
            {
                lasers.Add(laser);
            }

            public void addNote(Note note)
            {
                notes.Add(note);
            }

            public void addExplosion(Vector3 position, int size)
            {
                for (int i = 0; i < size; i++ )
                    explosions.AddParticle(position, new Vector3(0, 0, 0.1F));
            }

            public void tick(BasicEffect effect, GameTime gameTime, SpriteBatch batch = null)
            {
                time = gameTime;
                handleKeys();

                player.tickThis(effect, collisions.ToArray(), sCollisions.ToArray(), pCollisions.ToArray(), gameTime, gameSpeed);
                //lasers
                for (int i = 0; i < lasers.Count; i++)
                    lasers[i].tick(gameSpeed);
                //emittters
                foreach (ParticleEmitter e in emitters)
                    e.Update(gameTime);

                //explosion objects
                explosions.SetCamera(effect.View, effect.Projection);
                explosions.Update(gameTime);
                smoke.SetCamera(effect.View, effect.Projection);
                smoke.Update(gameTime);
                fire.SetCamera(effect.View, effect.Projection);
                fire.Update(gameTime);
                
                prevTime = gameTime;
            }

            private void handleKeys()
            {
                debug.update();
                cameraMode.update();
                pause.update();


                if (Keyboard.GetState().IsKeyDown(Keys.E))
                {
                    foreach (Note note in notes)
                    {
                        if (Vector3.Distance(note.position, player.position) < 5)
                        {
                            pickedUpNotes.Add(note);
                            notes.Remove(note);
                            gameSpeed = 0F;
                            drawingNote = note;
                            break;
                        }
                    }
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    if (drawingNote != null)
                    {
                        drawingNote = null;
                        scroll = 0;
                        gameSpeed = 1F;
                    }
                }
            }

            /// <summary>
            /// Renders the entire level
            /// </summary>
            /// <param name="effect">The basicEffec to draw with</param>
            /// <param name="gameTime">The current GameTime</param>
            /// <param name="batch">The spriteBatch to draw with</param>
            public void render(BasicEffect effect, GameTime gameTime, SpriteBatch batch = null)
            {
                fps.onDraw(gameTime);
                effect.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                effect.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                effect.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;

                if (Debug == DebugMode.Full)
                {
                    effect.GraphicsDevice.RasterizerState = wireFrame;  
                }
                else
                    effect.GraphicsDevice.RasterizerState = new RasterizerState();

                effect.CurrentTechnique.Passes[0].Apply();

                foreach (Instance3D model in staticInstances)
                    ModelManager.drawModel(model, effect);

                if (Debug != DebugMode.None)
                {
                    foreach (OrientedBoundingBox b in collisions)
                        BoundingBoxRenderer.Render(b, effect, Color.Red);
                }

                GlobalStaticModel.render(effect, Matrix.CreateTranslation(0, 0, 0));

                Note[] tempNotes = notes.ToArray();
                for (int i = 0; i < notes.Count; i++)
                {
                    Note note = tempNotes[i];
                    note.render(effect, note.position, new Vector3(note.roll, note.pitch, note.yaw));
                }

                player.render(effect, player.position, player.direction);

                explosions.Draw(gameTime);
                smoke.Draw(gameTime);
                fire.Draw(gameTime);
                sparks.Draw(gameTime);
                
                batch.Begin();

                batch.DrawString(FontManager.getFont("GUI"), "FPS: " + fps.getFrameRate().ToString(), new Vector2(10, 85), Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.None, 0);
                if (fps.getFrameRate() < 60)
                    batch.DrawString(FontManager.getFont("GUI"), "FPS: " + fps.getFrameRate().ToString(), new Vector2(10, 85), Color.Red, 0F, Vector2.Zero, 1F, SpriteEffects.None, 0);


                if (boolDebug)
                {
                    batch.DrawString(FontManager.getFont("GUI"), "X: " + player.position.X.ToString(), new Vector2(10, 10), Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.None, 0);
                    batch.DrawString(FontManager.getFont("GUI"), "Y: " + player.position.Y.ToString(), new Vector2(10, 25), Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.None, 0);
                    batch.DrawString(FontManager.getFont("GUI"), "Z: " + player.position.Z.ToString(), new Vector2(10, 40), Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.None, 0);
                    batch.DrawString(FontManager.getFont("GUI"), "Yaw: " + player.cameraAngle.ToString(), new Vector2(10, 55), Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.None, 0);
                    batch.DrawString(FontManager.getFont("GUI"), "Pitch: " + player.cameraPitch.ToString(), new Vector2(10, 70), Color.White, 0F, Vector2.Zero, 1F, SpriteEffects.None, 0);
                    batch.DrawString(FontManager.getFont("GUI"), "Zoom: " + player.zoom.ToString(), new Vector2(10, 100), Color.White);
                    batch.DrawString(FontManager.getFont("GUI"), "Instances: " + staticInstanceCount, new Vector2(10, 115), Color.White);
                    batch.DrawString(FontManager.getFont("GUI"), "Poly Count: " + polyCount, new Vector2(10, 130), Color.White);
                }

                if (gameSpeed == 0)
                    DrawFunctions.drawCenteredText(batch, FontManager.getFont("GUI"), "[Paused]", new Vector2(400, 240), Color.White);

                foreach (Note note in notes)
                {
                    if (Vector3.Distance(note.position, player.position) < 5)
                    {
                        DrawFunctions.drawCenteredText(batch, FontManager.getFont("GUI"), "Press [E] to read letter", new Vector2(400, 450), 
                            Color.White);
                    }
                }

                if (drawingNote != null)
                {
                    batch.Draw(drawingNote.noteBack, new Rectangle(50, 25 - scroll, 700, (int)drawingNote.font.MeasureString(drawingNote.text).Y + 40),
                        Color.White);
                    DrawFunctions.drawHorizontalCenteredText(batch, drawingNote.font, drawingNote.text, new Vector2(400, 45 - scroll), Color.Black);
                }
                batch.End();

                effect.GraphicsDevice.RasterizerState = new RasterizerState();
            }

            public void disposeLaser(Laser laser)
            {
                lasers.Remove(laser);
            }
            
            public void setGameSpeed(float gameSpeed)
            {
                this.gameSpeed = gameSpeed;
            }

            void debugPressed(object o, EventArgs args)
            {
                Debug = (DebugMode)((int)Debug + 1 >= 3 ? 0 : (int)Debug + 1);
            }

            void cameraChange(object o, EventArgs args)
            {
                if (player.cameraMode == Player.CameraModes.CM_FPS)
                {
                    player.cameraMode = Player.CameraModes.CM_3P;
                    return;
                }
                if (player.cameraMode == Player.CameraModes.CM_3P)
                {
                    player.cameraMode = Player.CameraModes.CM_FPS;
                    return;
                }
            }

            void pausePressed(object o, EventArgs args)
            {
                if (gameSpeed == 0)
                {
                    Mouse.SetPosition(400, 240);
                    gameSpeed = 1;
                }
                else
                    gameSpeed = 0;
            }
        }

        public class Terrain
        {
            float[,] heightmap = new float[100,100];
            VertexModel model = new VertexModel();

            public Terrain()
            {
                Random rand = new Random();
                for (int x = 0; x < 100; x++)
                {
                    for (int y = 0; y < 100; y++)
                    {
                        heightmap[x, y] = rand.Next(3,4);
                    }
                }
                for (int x = 0; x < 100 - 1; x++)
                {
                    for (int y = 0; y < 100 -1; y++)
                    {
                        model.verts.Add(new VertexPositionColorTexture(new Vector3(x + 1, y, heightmap[x + 1, y]), Color.Green, new Vector2(1, 0)));
                        model.verts.Add(new VertexPositionColorTexture(new Vector3(x, y, heightmap[x, y]), Color.Green, new Vector2(0, 0)));
                        model.verts.Add(new VertexPositionColorTexture(new Vector3(x, y + 1, heightmap[x, y + 1]), Color.Green, new Vector2(0, 1)));

                        model.verts.Add(new VertexPositionColorTexture(new Vector3(x + 1, y + 1, heightmap[x + 1, y]), Color.Green, new Vector2(1, 1)));
                        model.verts.Add(new VertexPositionColorTexture(new Vector3(x + 1, y, heightmap[x, y]), Color.Green, new Vector2(1, 0)));
                        model.verts.Add(new VertexPositionColorTexture(new Vector3(x, y + 1, heightmap[x, y + 1]), Color.Green, new Vector2(0, 1)));
                    }
                }

                model.texID = 8;
            }

            public void render(BasicEffect effect)
            {
                model.render(effect, Vector3.Zero, effect.World);
            }
        }

        public enum DebugMode
        {
            None,
            Partial,
            Full
        }
    }
}