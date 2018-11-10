using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using GameInterface;

namespace ImageEvolver
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
        MouseState prevMouseState;
        
        int screenWidth;
        int screenHeight;


        public HotloadGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;
            Content.RootDirectory = "Content";
        }


        public void OnResize(Object sender, EventArgs e)
        {
            if (hotloader != null)
            {
                hotloader.OnResize();
            }

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
            this.IsMouseVisible = true;
            
            screenWidth = graphics.PreferredBackBufferWidth;
            screenHeight = graphics.PreferredBackBufferHeight;
            prevMouseState = Mouse.GetState();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            hotloader = new Hotloader(Content, GraphicsDevice);                        
            spriteBatch = new SpriteBatch(GraphicsDevice);
            hotloader.Init(GraphicsDevice, spriteBatch);
            
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
            MouseState mouse = Mouse.GetState();
            state = hotloader.Update(Keyboard.GetState(),mouse,prevMouseState,gameTime,GraphicsDevice);
            prevMouseState = mouse;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            hotloader.Draw(spriteBatch,gameTime);

            base.Draw(gameTime);
        }
    }
}
