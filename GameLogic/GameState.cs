using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

        public GameState Update(KeyboardState keyboard, GameTime gameTime)
        {
            var tree = AptNode.GenerateTree(30, new Random());
            string lisp = tree.ToLisp();
            Console.WriteLine(lisp);
            Lexer lexer = new Lexer{ };
            lexer.BeginLexing(lisp);
            Console.WriteLine(lexer.ToString());


            var stackmachine = new StackMachine(tree);

            
            var s = new Stopwatch();
            var dummy = 0.0f;
            s.Start();
            for (float x = 0.0f; x < 1920.0f*2.0f; x++)
            {
                for (float y = 0.0f; y < 1080.0f*2.0f; y++)
                {
                    //   dummy += stackmachine.Execute(x, y);
                    dummy += stackmachine.Execute(x, y);
                }
            }
            s.Stop();
            var smt = s.ElapsedMilliseconds;
            Console.WriteLine("stackdummy:" + dummy);
            Console.WriteLine("time:" + smt);



            /*  s = new Stopwatch();
              dummy = 0.0f;
              s.Start();
              for (float x = 0.0f; x < 1920.0f; x++)
              {
                  for (float y = 0.0f; y < 1080.0f; y++)
                  {
                      //   dummy += stackmachine.Execute(x, y);
                      dummy += tree.Eval(x, y);
                  }
              }
              s.Stop();
              var tt = s.ElapsedMilliseconds;
              Console.WriteLine("treedummy: " + dummy);

              Console.WriteLine("stack / tree = " + ((double)smt/(double)tt).ToString());
              */

            var elapsed = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            
            if (state == null)
            {
                state = new GameState();
                state.PlayerPos = new Vector2(0.5f, 1.0f);
                state.jumpStart = 0;
            }
            
            if (keyboard.IsKeyDown(Keys.Left))
            {
                state.PlayerPos.X -= MOVE_SPEED / elapsed;
            }
            else if  (keyboard.IsKeyDown(Keys.Right))
            {
                state.PlayerPos.X += MOVE_SPEED / elapsed;
            }
            else if (keyboard.IsKeyDown(Keys.Space))
            {
                if (state.jumpStart == 0.0f)
                {
                    state.jumpStart = (float)gameTime.TotalGameTime.TotalMilliseconds;
                }                
            }

            if (state.jumpStart != 0)
            {
                var jumpTime = ((float)gameTime.TotalGameTime.TotalMilliseconds - state.jumpStart)/1000.0f;
                jumpTime -= (JUMP_DURATION/1000.0f) / 2.0f;
                var jumpAmount = JUMP_SPEED * jumpTime;
                state.PlayerPos.Y += jumpAmount;

                if (state.PlayerPos.Y >= 1.0f)
                {
                    state.PlayerPos.Y = 1.0f;
                    state.jumpStart = 0.0f;
                }

            }
            return state;
        }
    }
}

