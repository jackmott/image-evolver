using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using static GameLogic.GraphUtils;

namespace GameLogic
{
    [DataContract]
    public class TextBox
    {
        [DataMember]
        public string rawContents;
        [DataMember]
        public List<string> contents;
        [DataMember]
        public Color color;
        [DataMember]
        public Border border;
        [DataMember]
        public Point cursorPos;        
        public SpriteFont font;
        [DataMember]
        private bool active;
        [DataMember]
        public Rectangle bounds;        
        public GameWindow window;
        [DataMember]
        public Vector2 letterSize;
        [DataMember]
        public Texture2D pixelTex;
        [DataMember]
        private bool cursorOn;

        public TextBox(string contents, GameWindow window, Texture2D background, Texture2D pixelTex, Rectangle bounds, SpriteFont font, Color color)
        {
            active = false;
            this.window = window;
            this.bounds = bounds;
            this.color = color;
            this.font = font;
            rawContents = contents;
            this.contents = WordWrap.Wrap(contents, bounds.Width, WordWrap.MeasureWidth);
            letterSize = font.MeasureString("A");
            this.pixelTex = pixelTex;
            border = new Border(pixelTex, bounds, 4);
            cursorPos = new Point(0, 0);

        }

        public void SetNewBounds(Rectangle bounds)
        {
            this.bounds = bounds;
            this.border.Rect = bounds;
            this.contents = WordWrap.Wrap(rawContents, bounds.Width, WordWrap.MeasureWidth);

        }

        public void UpdateRawText()
        {
            rawContents = contents.Aggregate((a, b) => a + b);
        }

        public void Update(InputState state, GameTime gameTime)
        {
            if (IsActive())
            {
                if (TextUtils.IsKey(Keys.Back, state))
                {
                    if (cursorPos.X > 0)
                    {
                        cursorPos.X--;
                        contents[cursorPos.Y] = contents[cursorPos.Y].Remove(cursorPos.X, 1);
                        UpdateRawText();
                    }
                    else if (cursorPos.Y > 0)
                    {
                        cursorPos.X = contents[cursorPos.Y - 1].Length;
                        contents[cursorPos.Y - 1] += contents[cursorPos.Y];
                        contents.RemoveAt(cursorPos.Y);
                        cursorPos.Y--;
                        
                        UpdateRawText();
                    }

                }
                else if (TextUtils.IsKey(Keys.Delete, state))
                {
                    if (cursorPos.X < contents[cursorPos.Y].Length)
                    {
                        contents[cursorPos.Y] = contents[cursorPos.Y].Remove(cursorPos.X, 1);
                    }
                    else if (cursorPos.Y < contents.Count - 1)
                    {
                        for (int i = cursorPos.Y + 1; i < contents.Count; i++)
                        {
                            contents[i - 1] = contents[i];
                        }
                        contents.RemoveAt(contents.Count - 1);
                    }
                    //todo remove text
                }
                else if (TextUtils.IsKey(Keys.Home, state))
                {
                    cursorPos.X = 0;
                }
                else if (TextUtils.IsKey(Keys.End, state))
                {
                    cursorPos.X = contents[cursorPos.Y].Length;
                }
                else if (TextUtils.IsKey(Keys.Right, state))
                {
                    if (cursorPos.X < contents[cursorPos.Y].Length)
                        cursorPos.X++;
                }
                else if (TextUtils.IsKey(Keys.Left, state))
                {
                    if (cursorPos.X > 0)
                    {
                        cursorPos.X--;
                    }
                }
                else if (TextUtils.IsKey(Keys.Up, state))
                {
                    if (cursorPos.Y > 0)
                    {
                        cursorPos.Y--;
                    }
                    if (cursorPos.X > contents[cursorPos.Y].Length)
                    {
                        cursorPos.X = contents[cursorPos.Y].Length;
                    }
                }
                else if (TextUtils.IsKey(Keys.Down, state))
                {
                    if (cursorPos.Y < contents.Count)
                    {
                        cursorPos.Y++;
                    }
                    if (cursorPos.X > contents[cursorPos.Y].Length)
                    {
                        cursorPos.X = contents[cursorPos.Y].Length;
                    }
                }
                else if (TextUtils.IsKey(Keys.Enter, state))
                {
                    contents.Insert(cursorPos.Y+1, contents[cursorPos.Y].Substring(cursorPos.X-1));
                    contents[cursorPos.Y] = contents[cursorPos.Y].Substring(0, cursorPos.X-1);
                    cursorPos.X = 0;
                    cursorPos.Y++;
                }
            }
        }


        private void HandleInput(object sender, TextInputEventArgs e)
        {
            if (active)
            {
                if (Char.IsLetterOrDigit(e.Character) || Char.IsPunctuation(e.Character) || Char.IsSymbol(e.Character) || Char.IsWhiteSpace(e.Character))
                {
                    string toInsert = string.Empty;
                    if (e.Character == '\r')
                    {
                        //ignore?

                    }
                    else if (e.Character == '\t')
                    {
                        toInsert = "  ";
                    }
                    else
                    {
                        toInsert = e.Character.ToString();
                    }
                    
                    contents[cursorPos.Y] = contents[cursorPos.Y].Insert(cursorPos.X, toInsert);
                    cursorPos.X++;
                    UpdateRawText();                                        
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
            
            batch.Draw(pixelTex, bounds, new Color(0.0f, 0.0f, 0.0f, 0.75f));
            if (active && gameTime.TotalGameTime.Milliseconds % 250 == 0)
            {
                cursorOn = !cursorOn;
            }
            if (cursorOn)
            {
                // var letterSize = font.MeasureString("" + contents[cursorPos.Y][cursorPos.X]);
                batch.Draw(pixelTex, FRect(cursorPos.X * letterSize.X + bounds.X, cursorPos.Y * letterSize.Y + bounds.Y, letterSize.X, letterSize.Y), Color.White);
            }
            for (int i = 0; i < contents.Count; i++)
            {
                batch.DrawString(font, contents[i], new Vector2(bounds.X, bounds.Y+letterSize.Y*i), c);
            }
            border.Draw(batch, c);


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
