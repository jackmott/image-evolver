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
        public Screen screen = Screen.CHOOSE;
        public const float HORIZONTAL_SPACING = .01f;
        public const float VERTICAL_SPACING = .01f;
        public int populationSize;
        public List<Pic> pictures;
        public Pic zoomedPic;
        public List<ExternalImage> externalImages;
    }

    public interface IGameInterface
    {
        GameState Init(GraphicsDevice g);
        GameState Update(KeyboardState keyboard, MouseState mouseState, MouseState prevMouseState, GameTime gameTime, GraphicsDevice g);
        void Draw(GraphicsDevice g, SpriteBatch batch, GameTime gameTime);
        void SetState(GameState state);
    }

   
}
