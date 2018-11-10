using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
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
        public Button evolveButton;
        public Button reRollButton;
        public Screen screen = Screen.CHOOSE;
        public Random r;        
        public int populationSize;
        public List<Pic> pictures;
        public Pic zoomedPic;
        public static List<ExternalImage> externalImages;
    }

    public interface IGameInterface
    {
        GameState Init(GraphicsDevice g,SpriteBatch batch);
        GameState Update(KeyboardState keyboard, MouseState mouseState, MouseState prevMouseState, GameTime gameTime, GraphicsDevice g);
        void Draw(SpriteBatch batch, GameTime gameTime);
        void SetState(GameState state);
        void OnResize();
    }

   
}
