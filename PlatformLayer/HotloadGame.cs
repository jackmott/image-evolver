using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using GameInterface;

namespace HotloadPong
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class HotloadGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Hotloader hotloader;
        GameState state;        
        
        int screenWidth;
        int screenHeight;


        public HotloadGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Window.AllowUserResizing = true;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            screenWidth = graphics.PreferredBackBufferWidth;
            screenHeight = graphics.PreferredBackBufferHeight;                        
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            hotloader = new Hotloader(Content, GraphicsDevice);
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);                                
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            hotloader.CheckDLL(state);
#if DEBUG
            hotloader.CheckShaders();
#endif
            state = hotloader.Update(Keyboard.GetState(),gameTime,GraphicsDevice);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            int winW = GraphicsDevice.Viewport.Width;
            int winH = GraphicsDevice.Viewport.Height;

            int hSpace = (int)(winW * GameState.HORIZONTAL_SPACING);
            int vSpace = (int)(winW * GameState.VERTICAL_SPACING);

            int numPerRow = (int)Math.Sqrt(state.populationSize);
            int spaceRemaining = winW - hSpace * (numPerRow + 1);
            int picW = spaceRemaining / numPerRow;

            spaceRemaining = winH - vSpace * (numPerRow + 1);
            int picH = spaceRemaining / numPerRow;


            GraphicsDevice.Clear(Color.Black);            
            spriteBatch.Begin();
            var pos = new Vector2(0,0);
            int index = 0;
            for (int y = 0; y < numPerRow; y++)
            {
                pos.Y += vSpace;
                for (int x = 0; x < numPerRow; x++)
                {
                    pos.X += hSpace;
                    spriteBatch.Draw(state.pictures[index].GetTex(GraphicsDevice, picW, picH), pos, Color.White);
                    index++;
                    pos.X += picW;
                }
                pos.Y += picH;
                pos.X = 0;
            }
            
            spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
