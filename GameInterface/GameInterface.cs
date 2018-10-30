using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameInterface
{
    [Serializable]
    public class GameState
    {
        public Vector2 PlayerPos;
        public float jumpStart;
    }

    public interface IGameInterface
    {        
        GameState Update(KeyboardState keyboard, GameTime gameTime);
        void SetState(GameState state);
    }

   
}
