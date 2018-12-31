using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GameLogic
{
    [DataContract]
    public class Button
    {        
        public Texture2D tex;
        [DataMember]
        public Rectangle bounds;

        public Button() { }

        public Button(Texture2D tex, Rectangle bounds) {
            this.tex = tex;
            this.bounds = bounds;
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
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

        public ToggleButton(Texture2D onTex,Texture2D offTex, Rectangle bounds)
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
