﻿using System;
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
        Cursor cursor;
        SpriteFont font;        
        bool active;
        Rectangle bounds;
        EventHandler<TextInputEventArgs> textEvents;

        public TextBox(string contents, EventHandler<TextInputEventArgs> textEvents, Texture2D background, Texture2D pixelTex, Rectangle bounds, SpriteFont font, Color color)
        {            
            active = false;
            this.textEvents = textEvents;
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
                textEvents += HandleInput;
            }
            else if (!a && active)
            {
                active = false;
                textEvents -= HandleInput;
            }
        }

        public void Update(GameTime gameTime)
        {
            /*
            if (active)
            {
                if (game.IsKey(Keys.Back, gameTime))
                {
                    if (cursor.Pos > 0)
                    {
                        contents = contents.Remove(cursor.Pos - 1, 1);
                        cursor.Pos--;
                    }
                }
                else if (game.IsKey(Keys.Delete, gameTime))
                {
                    if (cursor.Pos < contents.Length)
                    {
                        contents = contents.Remove(cursor.Pos, 1);
                    }
                }
                else if (game.IsKey(Keys.Home, gameTime))
                {
                    cursor.Pos = 0;
                }
                else if (game.IsKey(Keys.End, gameTime))
                {
                    cursor.Pos = contents.Length;
                }
                else if (game.IsKey(Keys.Right, gameTime))
                {
                    if (cursor.Pos < contents.Length)
                        cursor.Pos++;
                }
                else if (game.IsKey(Keys.Left, gameTime))
                {
                    cursor.Pos--;
                }
            }
            */
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
