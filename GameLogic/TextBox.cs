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
        [DataMember]
        public Point highlightStart;
        [DataMember]
        public Point highlightEnd;

        public ParseException error;

        public TextBox(string contents, GameWindow window, Texture2D background, Texture2D pixelTex, Rectangle bounds, SpriteFont font, Color color)
        {
            active = false;
            this.window = window;
            this.bounds = bounds;
            this.color = color;
            this.font = font;
            SetText(contents);
            letterSize = font.MeasureString("A");
            this.pixelTex = pixelTex;
            border = new Border(pixelTex, bounds, 4);
            cursorPos = new Point(0, 0);

        }

        public void SetText(string text)
        {
            rawContents = text;
            this.contents = WordWrap.Wrap(rawContents, bounds.Width, WordWrap.MeasureWidth);
        }

        public void SetNewBounds(Rectangle bounds)
        {
            this.bounds = bounds;
            this.border.Rect = bounds;
            this.contents = WordWrap.Wrap(rawContents, bounds.Width, WordWrap.MeasureWidth);

        }

        public void UpdateRawText()
        {
            if (contents.Count > 0)
                rawContents = contents.Aggregate((a, b) => a + "\n" + b);
            else
                rawContents = string.Empty;
        }

        public void PreHighlight(InputState state)
        {
            if (TextUtils.IsKey(Keys.Up, state) ||
                TextUtils.IsKey(Keys.Down, state) ||
                TextUtils.IsKey(Keys.Left, state) ||
                TextUtils.IsKey(Keys.Right, state) ||
                TextUtils.IsKey(Keys.Home, state) ||
              TextUtils.IsKey(Keys.End, state))
            {


                if (TextUtils.IsShift(state))
                {
                    if (highlightStart == highlightEnd)
                    {
                        highlightStart = cursorPos;
                        highlightEnd = cursorPos;
                    }
                }
                else
                {
                    highlightStart = Point.Zero;
                    highlightEnd = Point.Zero;
                }
            }

        }

        public void PostHighlight(InputState state)
        {
            if (TextUtils.IsKey(Keys.Up, state) ||
              TextUtils.IsKey(Keys.Down, state) ||
              TextUtils.IsKey(Keys.Left, state) ||
              TextUtils.IsKey(Keys.Right, state) ||
              TextUtils.IsKey(Keys.Home, state) ||
              TextUtils.IsKey(Keys.End, state))
            {

                if (TextUtils.IsShift(state))
                {
                    highlightEnd = cursorPos;
                }
            }

        }

        public void CheckCopy(InputState state)
        {
            if (TextUtils.IsCopy(state))
            {
                System.Windows.Forms.Clipboard.SetText(ProcessHighlight(state));

            }
        }


        public void CheckCut(InputState state)
        {
            if (TextUtils.IsCut(state))
            {
                System.Windows.Forms.Clipboard.SetText(ProcessHighlight(state, true));
            }

        }

        public void CheckPaste(InputState state)
        {
            if (TextUtils.IsPaste(state))
            {
                if (highlightStart != highlightEnd)
                {
                    ProcessHighlight(state, true);
                }
                contents[cursorPos.Y] = contents[cursorPos.Y].Insert(cursorPos.X, System.Windows.Forms.Clipboard.GetText());
                UpdateRawText();
                contents = WordWrap.Wrap(rawContents, bounds.Width, WordWrap.MeasureWidth);

            }
        }

        public void CheckSelectAll(InputState state)
        {
            if (TextUtils.IsSelectAll(state))
            {
                highlightStart = Point.Zero;
                highlightEnd = new Point(contents.Last().Length - 1, contents.Count - 1);
                cursorPos = highlightEnd;
            }
        }

        public string ProcessHighlight(InputState state, bool delete = false)
        {
            string text = string.Empty;
            var (start, end) = GetHighlightSorted();
            if (delete) cursorPos = start;
            for (int y = start.Y; y <= end.Y; y++)
            {
                int xStart = 0;
                int xEnd = contents[y].Length - 1;
                if (y == start.Y)
                {
                    xStart = start.X;
                }
                if (y == end.Y)
                {
                    xEnd = end.X;
                }


                text += contents[y].Substring(xStart, Math.Min(contents[y].Length, xEnd + 1) - xStart);

                if (delete)
                {
                    var rightSide = string.Empty;
                    if (xEnd + 1 < contents[y].Length) rightSide = contents[y].Substring(xEnd);
                    contents[y] = contents[y].Substring(0, xStart) + rightSide;

                }
                if (y != end.Y) { text += "\n"; }
            }


            if (delete && start.Y != end.Y)
            {
                contents[start.Y] += contents[end.Y];
                contents.RemoveAt(end.Y);
                contents = contents.Where(s => s.Length != 0).ToList();
                if (contents.Count == 0)
                {
                    contents.Add(string.Empty);
                }
            }
            if (delete)
            {
 
                highlightStart = Point.Zero;
                highlightEnd = Point.Zero;

            }
            return text;
        }

        public Point TextPosFromScreenPos(Point p)
        {
            p.X -= bounds.X;
            p.Y -= bounds.Y;
            p.X = (int)(p.X / letterSize.X);
            p.Y = (int)(p.Y / letterSize.Y);
            if (p.Y < 0) p.Y = 0;
            if (p.Y > contents.Count - 1) p.Y = contents.Count - 1;
            if (p.X < 0) p.X = 0;
            if (p.X > contents[p.Y].Length - 1) p.X = contents[p.Y].Length - 1;
            return p;
        }

        public int MaxLetters()
        {
            return (int)(bounds.Width / letterSize.X);
        }

        public void HandleMouse(InputState state)
        {
            // Set the cursor position on press then release
            if (state.prevMouseState.LeftButton == ButtonState.Pressed && state.mouseState.LeftButton == ButtonState.Released)
            {
                if (bounds.Contains(state.prevMouseState.Position) && bounds.Contains(state.mouseState.Position))
                {
                    cursorPos = TextPosFromScreenPos(state.mouseState.Position);
                }
            }

            // Clear the highlight on a new press
            if (state.prevMouseState.LeftButton == ButtonState.Released && state.mouseState.LeftButton == ButtonState.Pressed)
            {
                highlightStart = Point.Zero;
                highlightEnd = Point.Zero;
            }

            // Change the highlighted range on click->drag
            if (state.prevMouseState.LeftButton == ButtonState.Pressed && state.mouseState.LeftButton == ButtonState.Pressed)
            {
                Point pre = TextPosFromScreenPos(state.prevMouseState.Position);
                Point cur = TextPosFromScreenPos(state.mouseState.Position);
                if (pre != cur)
                {
                    if (highlightStart == highlightEnd)
                    {
                        highlightStart = pre;

                    }
                    highlightEnd = cur;

                }
            }
        }

        public void Update(InputState state, GameTime gameTime)
        {
            if (IsActive())
            {
                HandleMouse(state);
                CheckCopy(state);
                CheckCut(state);
                CheckPaste(state);
                CheckSelectAll(state);
                PreHighlight(state);

                if (TextUtils.IsKey(Keys.Back, state))
                {
                    if (highlightStart != highlightEnd)
                    {
                        ProcessHighlight(state, true);
                    }
                    else
                    {
                        if (cursorPos.X > 0)
                        {
                            cursorPos.X--;
                            contents[cursorPos.Y] = contents[cursorPos.Y].Remove(cursorPos.X, 1);
                        }
                        else if (cursorPos.Y > 0)
                        {
                            cursorPos.X = contents[cursorPos.Y - 1].Length;
                            contents[cursorPos.Y - 1] += contents[cursorPos.Y];
                            contents.RemoveAt(cursorPos.Y);
                            cursorPos.Y--;
                        }
                    }
                    UpdateRawText();
                }
                else if (TextUtils.IsKey(Keys.Delete, state))
                {
                    if (highlightStart != highlightEnd)
                    {
                        ProcessHighlight(state, true);
                    }
                    else
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
                    }
                    UpdateRawText();
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
                    else if (cursorPos.Y < contents.Count - 1)
                    {
                        cursorPos.Y++;
                        cursorPos.X = 0;
                    }
                }
                else if (TextUtils.IsKey(Keys.Left, state))
                {

                    if (cursorPos.X > 0)
                    {
                        cursorPos.X--;
                    }
                    else if (cursorPos.Y > 0)
                    {
                        cursorPos.Y--;
                        cursorPos.X = contents[cursorPos.Y].Length;
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

                    if (cursorPos.Y < contents.Count - 1)
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
                    contents.Insert(cursorPos.Y + 1, contents[cursorPos.Y].Substring(cursorPos.X - 1));
                    contents[cursorPos.Y] = contents[cursorPos.Y].Substring(0, cursorPos.X - 1);
                    cursorPos.X = 0;
                    cursorPos.Y++;
                }


                PostHighlight(state);
            }
        }


        private void addLetter(char c, int y)
        {
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

                    var maxLetters = MaxLetters();
                    var curLine = contents[cursorPos.Y];
                    curLine = curLine.Insert(cursorPos.X, toInsert).TrimEnd(' ');
                    contents[cursorPos.Y] = curLine;

                    int y = cursorPos.Y;
                    while (curLine.Length > maxLetters)
                    {
                        int lastSpace = curLine.LastIndexOf(' ');

                        var removed = curLine.Substring(lastSpace + 1);
                        curLine = curLine.Remove(lastSpace + 1);
                        contents[y] = curLine;
                        y++;
                        if (y >= contents.Count)
                        {
                            contents.Add("");
                        }
                        contents[y] = contents[y].Insert(0, removed);
                        curLine = contents[y]; 
                    }


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

        public (Point, Point) GetHighlightSorted()
        {
            if (highlightStart.Y > highlightEnd.Y || (highlightStart.X > highlightEnd.X && highlightStart.Y == highlightEnd.Y))
            {
                return (highlightEnd, highlightStart);
            }
            return (highlightStart, highlightEnd);
        }


        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            Color c = color;

            batch.Draw(pixelTex, bounds, new Color(0.0f, 0.0f, 0.0f, 0.75f));

            if (highlightStart != highlightEnd)
            {
                var (start, end) = GetHighlightSorted();
                for (int y = start.Y; y <= end.Y; y++)
                {
                    int maxX = end.X;
                    int minX = start.X;
                    if (y != end.Y)
                    {
                        maxX = contents[y].Length - 1;
                    }
                    if (y != start.Y)
                    {
                        minX = 0;
                    }
                    for (int x = minX; x <= maxX; x++)
                    {
                        batch.Draw(pixelTex, FRect(x * letterSize.X + bounds.X, y * letterSize.Y + bounds.Y, letterSize.X, letterSize.Y), Color.DarkBlue);
                    }
                }
            }

            if (active && gameTime.TotalGameTime.Milliseconds % 250 == 0)
            {
                cursorOn = !cursorOn;
            }
            if (cursorOn)
            {
                // var letterSize = font.MeasureString("" + contents[cursorPos.Y][cursorPos.X]);
                batch.Draw(pixelTex, FRect(cursorPos.X * letterSize.X + bounds.X, cursorPos.Y * letterSize.Y + bounds.Y, letterSize.X, letterSize.Y), Color.White);
            }

            if (error != null)
            {
                batch.DrawString(font, error.Message, new Vector2(bounds.X + bounds.Width * .01f, bounds.Y + bounds.Height - letterSize.Y), Color.Red);
            }


            for (int i = 0; i < contents.Count; i++)
            {
                batch.DrawString(font, contents[i], new Vector2(bounds.X, bounds.Y + letterSize.Y * i), c);
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
