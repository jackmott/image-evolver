using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using GameInterface;
using System.Diagnostics;
using System.Collections.Generic;

namespace GameLogic
{
    
    public class GameLogic : IGameInterface
    {
        public GameState state;
        public void SetState(GameState state)
        {
            this.state = state;
        }

        private const float MOVE_SPEED = 0.05f;
        private const float JUMP_DURATION = 1000.0f;
        private const float JUMP_SPEED = 0.02f;

        public GameState Update(KeyboardState keyboard, GameTime gameTime, GraphicsDevice g)
        {
            if (state == null)
            {                
                state = new GameState();
                state.populationSize = 9;
                state.pictures = new List<Pic>();
                Random r = new Random();

                for (int i = 0; i < state.populationSize; i++)
                {
                    int chooser = r.Next(0, 2);
                    if (chooser == 0)
                    {
                        var rgbTree = new RGBTree(3, 30, r);
                        state.pictures.Add(rgbTree);
                    }
                    else
                    {
                        var hsvTree = new HSVTree(3, 30, r);
                        state.pictures.Add(hsvTree);
                    }
                }

            }
            
            return state;
        }
    }
}

