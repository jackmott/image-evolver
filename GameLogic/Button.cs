using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Runtime.Serialization;
using System;
using static GameLogic.GraphUtils;
using System.Collections.Generic;

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
        public string tex;        
        public Rectangle bounds;
        Dictionary<string, Texture2D> buttons;
        Color color;
        public Button() { }

        public Button(string tex,Rectangle bounds, Dictionary<string,Texture2D> buttons,Color color)
        {            
            this.tex = tex;
            this.buttons = buttons;
            this.color = color;
            SetBounds(bounds);
        }

        public Button(string tex, Rectangle bounds, Dictionary<string, Texture2D> buttons)
        {
            this.tex = tex;
            this.buttons = buttons;
            color = Color.White;
            SetBounds(bounds);
        }

        public void SetBounds(Rectangle bounds) {
            this.bounds = bounds;
        }

        public int GetWidth()
        {
            return buttons[tex].Width;
        }

        public int GetHeight()
        {
            return buttons[tex].Height;
        }

        public void Draw(SpriteBatch batch,GraphicsDevice g, GameTime gameTime)
        {
            batch.Draw(buttons[tex], bounds,color);                        
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
        public string tex;
        Dictionary<string, Texture2D> buttons;        
        public Rectangle bounds;
        public Color onColor;
        public Color offColor;

        public ToggleButton() { }

        public ToggleButton(string tex, Dictionary<string, Texture2D> buttons,Rectangle bounds, Color offColor, Color onColor)
        {
            this.tex = tex;
            this.offColor = offColor;
            this.onColor = onColor;
            this.bounds = bounds;
            this.buttons = buttons;
        }

        public void Draw(SpriteBatch batch, GameTime gameTime, bool on)
        {
            var c = offColor;
            if (on) c = onColor;
            
            batch.Draw(buttons[tex], bounds, c);
            
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
