using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using GameInterface;
using System.Diagnostics;

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
            }
            if (state.tex == null)
            {
                Random r = new Random();
                var rTree = AptNode.GenerateTree(15, r);
                var rStackmachine = new StackMachine(rTree);

                var gTree = AptNode.GenerateTree(15, r);
                var gStackmachine = new StackMachine(gTree);

                var bTree = AptNode.GenerateTree(15, r);
                var bStackmachine = new StackMachine(bTree);

                var rgbTree = new RGBTree();
                rgbTree.R = rStackmachine;
                rgbTree.G = gStackmachine;
                rgbTree.B = bStackmachine;
                state.tex = rgbTree.ToTexture(g, 1920, 1080);
            }
            return state;
        }
    }
}

