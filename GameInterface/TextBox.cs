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
        public string contents;
        Color color;
        Border border;
        public Cursor cursor;
        SpriteFont font;        
        private bool active;
        Rectangle bounds;
        GameWindow window;

        public TextBox(string contents, GameWindow window, Texture2D background, Texture2D pixelTex, Rectangle bounds, SpriteFont font, Color color)
        {            
            active = false;
            this.window = window;
            this.bounds = bounds;
            this.color = color;                        
            this.font = font;
            this.contents = contents;
            border = new Border(pixelTex,bounds, 1);
            cursor = new Cursor(pixelTex);            

        }

        private void HandleInput(object sender, TextInputEventArgs e)
        {
            if (active)
            {
                if (Char.IsLetterOrDigit(e.Character) || Char.IsPunctuation(e.Character) || Char.IsSymbol(e.Character))
                {
                    contents = contents.Insert(cursor.Pos, e.Character.ToString());
                    cursor.Pos++;
                }
            }
        }

        public void SetActive(bool a)
        {
            if (a && !active)
            {
                active = true;
                window.TextInput += HandleInput;
            }
            else if (!a && active)
            {
                active = false;
                window.TextInput -= HandleInput;
            }
        }

        public bool IsActive()
        {
            return active;
        }

      
        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            Color c = color;                        
            border.Draw(batch,c);
            batch.DrawString(font, contents, new Vector2(bounds.X, bounds.Y), c);
            if (active && gameTime.TotalGameTime.Seconds % 2 == 0)
            {
                string substring = contents.Substring(0, cursor.Pos);
                float contentWidth = font.MeasureString(substring).X;
                int cursorX = (int)(bounds.X + contentWidth);
                var cursorSize = font.MeasureString("A");
                cursor.Draw(batch,new Rectangle(cursorX, (int)bounds.Y, (int)cursorSize.X, (int)cursorSize.Y),gameTime);
            }


        }
    }

    public class Cursor
    {        
        Texture2D tex;
       
        private int pos;
        public int Pos {
            get { return pos; }
            set { pos = Math.Max(0, value); }
        }
        public Cursor(Texture2D tex)
        {
            this.tex = tex;            
            Pos = 0;
        }

        public void Draw(SpriteBatch batch, Rectangle rectangleToDraw,GameTime gameTime)
        {
            batch.Draw(tex, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, rectangleToDraw.Height), Color.White);
        }
    }

    public class Border
    {        
        Texture2D tex;
        Rectangle Rect;
        int Thickness;
        public Border(Texture2D tex, Rectangle rectangleToDraw, int thicknessOfBorder)
        {            
            this.Thickness = thicknessOfBorder;
            this.Rect = rectangleToDraw;
            this.tex = tex;
            
        }

        public void Draw(SpriteBatch batch, Color borderColor)
        {
            // Draw top line
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
        }
    }
}
