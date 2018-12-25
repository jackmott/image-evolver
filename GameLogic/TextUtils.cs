using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace GameLogic
{
    public static class TextUtils
    {
        public static int prevRepeat = 0;
        public static bool IsKey(Keys key, InputState state)
        {

            if (state.keyboardState.IsKeyDown(key))
            {
                if (!state.prevKeyboardState.IsKeyDown(key))
                {
                    prevRepeat = 0;
                    return true;
                }
                if (state.keyboardStateMillis > 500)
                {
                    if (state.keyboardStateMillis - prevRepeat > 20)
                    {
                        prevRepeat = state.keyboardStateMillis;
                        return true;
                    }

                }
            }
            return false;
        }
        public static bool IsShift(InputState state)
        {
            return state.keyboardState.IsKeyDown(Keys.LeftShift) || state.keyboardState.IsKeyDown(Keys.RightShift);
        }

        public static bool IsControl(InputState state)
        {
            return state.keyboardState.IsKeyDown(Keys.LeftControl) || state.keyboardState.IsKeyDown(Keys.RightControl);

        }

        public static bool IsCopy(InputState state)
        {
            //todo make this work for mac/linux idioms?
            return IsControl(state) && IsKey(Keys.C, state);

        }
        public static bool IsCut(InputState state)
        {
            //todo make this work for mac/linux idioms?
            return IsControl(state) && IsKey(Keys.X, state);

        }

        public static bool IsPaste(InputState state)
        {
            //todo make this work for mac/linux idioms?
            return IsControl(state) && IsKey(Keys.V, state);

        }


    }

    public static class WordWrap
    {
        public static int MeasureWidth(string s)
        {
            return (int)Settings.equationFont.MeasureString(s).X;
        }

        public static List<string> Wrap(string s, int width, Func<string, int> widthMeasure)
        {
            int spaceWidth = widthMeasure(" ");
            var result = new List<string>();
            var text = s.AsSpan();
            int lineStart = 0;
            int lineLen = 0;
            int wordStart = 0;
            int lineWidth = 0;
            for (int i = 0; i < text.Length; i++)
            {

                if (text[i] == ' ')
                {   //0123456789
                    //the poo 
                    int wordWidth = widthMeasure(text.Slice(wordStart, i - wordStart).ToString());
                    if (lineWidth + wordWidth <= width)
                    {
                        lineLen = i - lineStart + 1;
                        lineWidth += wordWidth + spaceWidth;
                        wordStart = i + 1;
                    }
                    else
                    {
                        result.Add(text.Slice(lineStart, lineLen).ToString());
                        lineLen = i - wordStart;
                        lineStart = wordStart;
                        wordStart = i + 1;
                        lineWidth = wordWidth + spaceWidth;

                    }

                }
                if (text[i] == '\n')
                {
                    result.Add(text.Slice(lineStart, i - lineStart).ToString());
                    lineLen = 0;
                    lineStart = i + 1;
                    wordStart = i + 1;
                    lineWidth = 0;
                }
            }
            result.Add(text.Slice(lineStart).ToString());
            return result;

        }

    }
}
