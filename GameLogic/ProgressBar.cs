using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using static GameLogic.GraphUtils;

namespace GameLogic
{

    //Used when transitioning from one state to another that will take time
    public static class Transition
    {
        //1 to 100000                
        public static object translock = new object();
        public static int progress;
        public static int goal;
        public static Screen nextScreen;
        public static string message;

        public static void StartTransition(Screen to, int theGoal, string theMessage)
        {
            nextScreen = to;
            progress = 0;
            goal = theGoal;
            message = theMessage;
        }

        public static void AddProgress(int amount)
        {
            Interlocked.Add(ref progress, amount);
        }

        public static void SetProgress(int amount)
        {
            lock (translock)
            {
                progress = amount;
            }
        }


        public static void Update(GameState state)
        {
            if (progress == goal)
            {
                state.screen = nextScreen;
            }
        }
        public static void Draw(SpriteBatch batch, GraphicsDevice g, GameTime gametime)
        {
            int winW = g.Viewport.Width;
            int winH = g.Viewport.Height;
            float pct = (float)progress / (float)goal;

            Rectangle Rect = CenteredRect(new Rectangle(0, 0, winW, winH), winW / 4, winH / 20);           
            var tex = GraphUtils.GetTexture(g, Color.Blue);
            var background = GraphUtils.GetTexture(g, new Color(0.0f, 0.0f, 0.0f, 0.5f));
            var progRect = new Rectangle(Rect.X, Rect.Y, (int)(Rect.Width * ((float)progress/(float)goal)), Rect.Height);
            batch.Draw(background, progRect, Color.White);
            batch.Draw(tex, progRect, Color.White);
            batch.DrawString(Settings.equationFont, message, new Vector2(progRect.X + 10.0f, progRect.Y + 10.0f), Color.White);
            var Thickness = 2;
            tex = GraphUtils.GetTexture(g, Color.Cyan);
            // Draw top line
            batch.Draw(tex, new Rectangle(Rect.X, Rect.Y, Rect.Width, Thickness), Color.White);
            batch.Draw(tex, new Rectangle(Rect.X, Rect.Y, Thickness, Rect.Height), Color.White);
            batch.Draw(tex, new Rectangle((Rect.X + Rect.Width - Thickness),
                                            Rect.Y,
                                            Thickness,
                                            Rect.Height), Color.White);
            batch.Draw(tex, new Rectangle(Rect.X,
                                            Rect.Y + Rect.Height - Thickness,
                                            Rect.Width,
                                            Thickness), Color.White);            
        }
    }


}
