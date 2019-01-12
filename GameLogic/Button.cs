using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Runtime.Serialization;
using System;
using static GameLogic.GraphUtils;
namespace GameLogic
{
    public static class ProgressBar
    {
        //progress 0.0f to 1.0f
        public static void Draw(SpriteBatch batch, GraphicsDevice g,Rectangle Rect, Color borderColor, Color barColor, float progress)
        {

            var tex = GraphUtils.GetTexture(g, barColor);
            var background = GraphUtils.GetTexture(g, new Color(0.0f, 0.0f, 0.0f, 0.5f));
            var progRect = new Rectangle(Rect.X, Rect.Y, (int)(Rect.Width * progress), Rect.Height);
            batch.Draw(background, progRect, Color.White);
            batch.Draw(tex, progRect, Color.White);
            batch.DrawString(Settings.equationFont, "Loading...", new Vector2(progRect.X + 10.0f, progRect.Y + 10.0f), Color.White);
            var Thickness = 2;
            tex = GraphUtils.GetTexture(g, borderColor);
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
        public SpriteFont font;
        public string text;
        public Color borderColor;
        public Color textColor;
        private Rectangle bounds;
        private float fontScale;

        public Button() { }

        public Button(string text, SpriteFont font, Rectangle bounds, Color borderColor, Color textColor)
        {
            this.text = text;
            this.font = font;
            this.borderColor = borderColor;
            this.textColor = textColor;
            SetBounds(bounds);
        }
        
        public void SetBounds(Rectangle bounds) {
            if (this.bounds != bounds)
            {
                this.bounds = bounds;
                var fontSize = font.MeasureString(text);

                var fontScaleW = (float)bounds.Width / (float)fontSize.X;
                var fontScaleH = (float)bounds.Height / (float)fontSize.Y;

                fontScale = Math.Min(fontScaleH, fontScaleW);

            }
        }

        public void Draw(SpriteBatch batch,GraphicsDevice g, GameTime gameTime)
        {


            var tex = GraphUtils.GetTexture(g, borderColor);
            var Rect = bounds;
            var Thickness = 2;
            batch.Draw(tex, new Rectangle(Rect.X, Rect.Y, Rect.Width, Thickness), borderColor);
            batch.Draw(tex, new Rectangle(Rect.X, Rect.Y, Thickness, Rect.Height), borderColor);
            batch.Draw(tex, new Rectangle((Rect.X + Rect.Width - Thickness),
                                            Rect.Y,
                                            Thickness,
                                            Rect.Height), borderColor);
            batch.Draw(tex, new Rectangle(Rect.X,
                                            Rect.Y + Rect.Height - Thickness,
                                            Rect.Width,
                                            Thickness), borderColor);

            batch.DrawString(font, text, new Vector2(bounds.X, bounds.Y), textColor,0,Vector2.Zero,fontScale,SpriteEffects.None,0);

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
