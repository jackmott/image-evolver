using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

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
        
        
        int screenWidth;
        int screenHeight;


        public HotloadGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
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
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            hotloader = new Hotloader(Content,Window, GraphicsDevice);                        
            spriteBatch = new SpriteBatch(GraphicsDevice);                        
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
            // TODO: Add your update logic here
            hotloader.CheckDLL();
#if DEBUG
            hotloader.CheckShaders();
#endif
            if (hotloader.state != null && IsActive)
            {
                hotloader.state.inputState.keyboardState = Keyboard.GetState();
                hotloader.state.inputState.mouseState = Mouse.GetState();
            }
            hotloader.Update(gameTime);


            //Keep track of how long the keyboard state has not changed
            //So that you can hold arrows/backspace and have them repeat
            if (hotloader.state.inputState.keyboardState != hotloader.state.inputState.prevKeyboardState)
            {
                hotloader.state.inputState.keyboardStateMillis = 0;
            }
            else
            {
                hotloader.state.inputState.keyboardStateMillis += gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            hotloader.state.inputState.prevKeyboardState = hotloader.state.inputState.keyboardState;
            hotloader.state.inputState.prevMouseState = hotloader.state.inputState.mouseState;
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
