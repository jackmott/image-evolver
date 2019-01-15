using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Runtime.Serialization;
using System;
using static GameLogic.GraphUtils;
namespace GameLogic
{
   

    [DataContract]
    public class SlidingPanel
    {
        public Texture2D tex;
        public Rectangle activeBounds;
        public Rectangle hiddenBounds;
        private Rectangle lastBounds;
        double lastStateChange = 0;
        bool lastState;
        double tranisitionTime;


        public SlidingPanel(Texture2D tex, Rectangle activeBounds, Rectangle hiddenBounds, double transitionTime)
        {
            this.tex = tex;
            this.activeBounds = activeBounds;
            this.hiddenBounds = hiddenBounds;
            lastBounds = hiddenBounds;
            this.tranisitionTime = transitionTime;
        }

        public Rectangle GetBounds(InputState state)
        {
            return lastBounds;
        }

        public void Draw(SpriteBatch batch, GameTime gameTime, InputState state)
        {
            bool active = activeBounds.Contains(state.mouseState.Position);
            var deltaT = gameTime.TotalGameTime.TotalMilliseconds - lastStateChange;
            var pct = (float)Math.Min(deltaT / tranisitionTime, 1.0f);
            if (active != lastState)
            {
                deltaT = 0.0;
                pct = (float)Math.Min(deltaT / tranisitionTime, 1.0f);
                lastStateChange = gameTime.TotalGameTime.TotalMilliseconds;
                lastState = active;
            }
            if (active)
            {
                lastBounds = RectLerp(lastBounds, activeBounds, pct);
                batch.Draw(tex, lastBounds, Color.White);
            }
            else
            {
                lastBounds = RectLerp(lastBounds, hiddenBounds, pct);
                batch.Draw(tex, lastBounds, Color.White);
            }
        }
    }

    [DataContract]
    public class Button
    {
        public Texture2D tex;        
        private Rectangle bounds;        

        public Button() { }

        public Button(Texture2D tex,Rectangle bounds)
        {
            this.tex = tex;
            SetBounds(bounds);
        }
        
        public void SetBounds(Rectangle bounds) {
            this.bounds = bounds;
        }

        public void Draw(SpriteBatch batch,GraphicsDevice g, GameTime gameTime)
        {
            batch.Draw(tex, bounds, Color.White);                        
        }

        public bool WasLeftClicked(InputState state)
        {
            if (state.prevMouseState.LeftButton == ButtonState.Pressed && state.mouseState.LeftButton == ButtonState.Released)
            {
                if (bounds.Contains(state.prevMouseState.Position) && bounds.Contains(state.mouseState.Position))
                {
                    Debug.WriteLine("left click");
                    return true;
                }
            }
            return false;
        }

        public bool WasRightClicked(InputState state)
        {
            if (state.prevMouseState.RightButton == ButtonState.Pressed && state.mouseState.RightButton == ButtonState.Released)
            {
                if (bounds.Contains(state.prevMouseState.Position) && bounds.Contains(state.mouseState.Position))
                {
                    Debug.WriteLine("right click");
                    return true;
                }
            }
            return false;
        }



    }

    [DataContract]
    public class ToggleButton
    {
        public Texture2D onTex;
        public Texture2D offTex;
        [DataMember]
        public Rectangle bounds;

        public ToggleButton() { }

        public ToggleButton(Texture2D onTex, Texture2D offTex, Rectangle bounds)
        {
            this.onTex = onTex;
            this.offTex = offTex;
            this.bounds = bounds;
        }

        public void Draw(SpriteBatch batch, GameTime gameTime, bool on)
        {
            if (on)
            {
                batch.Draw(onTex, bounds, Color.White);
            }
            else
            {
                batch.Draw(offTex, bounds, Color.White);
            }
        }



        public bool WasLeftClicked(InputState state)
        {
            if (state.prevMouseState.LeftButton == ButtonState.Pressed && state.mouseState.LeftButton == ButtonState.Released)
            {
                if (bounds.Contains(state.prevMouseState.Position) && bounds.Contains(state.mouseState.Position))
                {
                    Debug.WriteLine("left click");
                    return true;
                }
            }
            return false;
        }

        public bool WasRightClicked(InputState state)
        {
            if (state.prevMouseState.RightButton == ButtonState.Pressed && state.mouseState.RightButton == ButtonState.Released)
            {
                if (bounds.Contains(state.prevMouseState.Position) && bounds.Contains(state.mouseState.Position))
                {
                    Debug.WriteLine("right click");
                    return true;
                }
            }
            return false;
        }



    }
}
