using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameLogic
{
    [DataContract]
    public class TextBox
    {
        [DataMember]
        public string contents;
        [DataMember]
        public Color color;
        [DataMember]
        public Border border;
        [DataMember]
        public Cursor cursor;        
        public SpriteFont font;
        [DataMember]
        private bool active;
        [DataMember]
        public Rectangle bounds;        
        public GameWindow window;
        [DataMember]
        public Vector2 letterSize;

        public TextBox(string contents, GameWindow window, Texture2D background, Texture2D pixelTex, Rectangle bounds, SpriteFont font, Color color)
        {
            active = false;
            this.window = window;
            this.bounds = bounds;
            this.color = color;
            this.font = font;
            this.contents = contents;
            letterSize = font.MeasureString("A");
            border = new Border(pixelTex, bounds, 1);
            cursor = new Cursor(pixelTex);

        }

        public void Update(InputState state, GameTime gameTime)
        {
            if (IsActive())
            {
                if (TextUtils.IsKey(Keys.Back, state))
                {
                    if (cursor.Pos > 0)
                    {
                        contents = contents.Remove(cursor.Pos - 1, 1);
                        cursor.Pos--;
                    }
                }
                else if (TextUtils.IsKey(Keys.Delete, state))
                {
                    if (cursor.Pos < contents.Length)
                    {
                        contents = contents.Remove(cursor.Pos, 1);
                    }
                }
                else if (TextUtils.IsKey(Keys.Home, state))
                {
                    cursor.Pos = 0;
                }
                else if (TextUtils.IsKey(Keys.End, state))
                {
                    cursor.Pos = contents.Length;
                }
                else if (TextUtils.IsKey(Keys.Right, state))
                {
                    if (cursor.Pos < contents.Length)
                        cursor.Pos++;
                }
                else if (TextUtils.IsKey(Keys.Left, state))
                {
                    cursor.Pos--;
                }
            }
        }


        private void HandleInput(object sender, TextInputEventArgs e)
        {
            if (active)
            {
                if (Char.IsLetterOrDigit(e.Character) || Char.IsPunctuation(e.Character) || Char.IsSymbol(e.Character) || Char.IsWhiteSpace(e.Character))
                {
                    char c;
                    if (e.Character == '\r')
                    {
                        c = '\n';

                    }
                    else
                    {
                        c = e.Character;
                    }
                    contents = contents.Insert(cursor.Pos, c.ToString());
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
            border.Draw(batch, c);
            batch.DrawString(font, contents, new Vector2(bounds.X, bounds.Y), c);
            if (active && gameTime.TotalGameTime.Milliseconds % 500 != 0)
            {
                string substring = contents.Substring(0, cursor.Pos);
                float contentWidth = font.MeasureString(substring).X;
                int cursorX = (int)(bounds.X + contentWidth);
                cursor.Draw(batch, new Rectangle(cursorX, (int)bounds.Y, (int)letterSize.X, (int)letterSize.Y), gameTime);
            }


        }
    }

    [DataContract]
    public class Cursor
    {
        
        public Texture2D tex;
        [DataMember]
        public int pos;
        [DataMember]
        public int Pos {
            get { return pos; }
            set { pos = Math.Max(0, value); }
        }
        public Cursor(Texture2D tex)
        {
            this.tex = tex;
            Pos = 0;
        }

        public void Draw(SpriteBatch batch, Rectangle rectangleToDraw, GameTime gameTime)
        {
            batch.Draw(tex, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, rectangleToDraw.Height), Color.White);
        }
    }

    [DataContract]
    public class Border
    {
        public Texture2D tex;
        [DataMember]
        public Rectangle Rect;
        [DataMember]
        public int Thickness;
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
