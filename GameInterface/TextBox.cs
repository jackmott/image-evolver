using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameInterface
{
    public class TextBox
    {
        Rectangle bounds;
        string text;
        SpriteFont font;
        public TextBox(Rectangle bounds, SpriteFont font, string text = "")
        {
            this.bounds = bounds;
            this.text = text;
            this.font = font;
        }

        public void Update(KeyboardState keys, KeyboardState prevKeys, GameTime gameTime)
        {

        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            batch.DrawString(font, text, new Vector2(bounds.X, bounds.Y), Color.White);            
        }
    }
}
