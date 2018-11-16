using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace GameInterface
{
    public struct ExternalImage
    {
        public Color[] data;
        public int w;
        public int h;
    }

    public enum Screen { CHOOSE, ZOOM };
    public class GameState
    {
        public GraphicsDevice g;
        public GameWindow w;
        public InputState inputState;
        public ContentManager content;
        public Button evolveButton;
        public Button reRollButton;
        public Screen screen = Screen.CHOOSE;
        public Random r;        
        public int populationSize;
        public List<Pic> pictures;
        public Pic zoomedPic;
        public static List<ExternalImage> externalImages;
    }

    public class InputState
    {
        public KeyboardState keyboardState;
        public KeyboardState prevKeyboardState;
        public MouseState mouseState;
        public MouseState prevMouseState;
        public int keyboardStateMillis;
    }

    public interface IGameInterface
    {
        GameState Init(GraphicsDevice g, GameWindow w, ContentManager content);
        GameState Update(GameTime gameTime);
        void Draw(SpriteBatch batch, GameTime gameTime);
        void SetState(GameState state);
        void OnResize();
    }

   
}
