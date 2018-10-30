using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameInterface
{    
    public class GameState
    {
        public Texture2D tex;
    }

    public interface IGameInterface
    {        
        GameState Update(KeyboardState keyboard, GameTime gameTime, GraphicsDevice g);
        void SetState(GameState state);
    }

   
}
