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
    public class GameState
    {
        public const float HORIZONTAL_SPACING = .01f;
        public const float VERTICAL_SPACING = .01f;
        public int populationSize;
        public List<Pic> pictures;
        public List<ExternalImage> externalImages;
    }

    public interface IGameInterface
    {        
        GameState Update(KeyboardState keyboard, MouseState mouseState, MouseState prevMouseState, GameTime gameTime, GraphicsDevice g);
        void Draw(GraphicsDevice g, SpriteBatch batch, GameTime gameTime);
        void SetState(GameState state);
    }

   
}
