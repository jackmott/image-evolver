using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace GameInterface
{
    public class Button
    {
        public Texture2D tex;
        public Rectangle bounds;

        public Button(Texture2D tex, Rectangle bounds) {
            this.tex = tex;
            this.bounds = bounds;
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            batch.Draw(tex, bounds, Color.White);
        }

        public bool WasLeftClicked(MouseState current, MouseState prev)
        {
            if (prev.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released)
            {
                if (bounds.Contains(prev.Position) && bounds.Contains(current.Position))
                {
                    Debug.WriteLine("left click");
                    return true;
                }
            }
            return false;
        }

        public bool WasRightClicked(MouseState current, MouseState prev)
        {
            if (prev.RightButton == ButtonState.Pressed && current.RightButton == ButtonState.Released)
            {
                if (bounds.Contains(prev.Position) && bounds.Contains(current.Position))
                {
                    Debug.WriteLine("right click");
                    return true;
                }
            }
            return false;
        }



    }
}
