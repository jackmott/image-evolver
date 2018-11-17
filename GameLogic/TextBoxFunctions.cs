using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameLogic
{
    public static class TextBoxFunctions
    {
        public static void Update(TextBox box, InputState state, GameTime gameTime)
        {
            if (box.IsActive())
            {
                if (TextUtils.IsKey(Keys.Back, state))
                {
                    if (box.cursor.Pos > 0)
                    {
                        box.contents = box.contents.Remove(box.cursor.Pos - 1, 1);
                        box.cursor.Pos--;
                    }
                }
                else if (TextUtils.IsKey(Keys.Delete, state))
                {
                    if (box.cursor.Pos < box.contents.Length)
                    {
                        box.contents = box.contents.Remove(box.cursor.Pos, 1);
                    }
                }
                else if (TextUtils.IsKey(Keys.Home, state))
                {
                    box.cursor.Pos = 0;
                }
                else if (TextUtils.IsKey(Keys.End, state))
                {
                    box.cursor.Pos = box.contents.Length;
                }
                else if (TextUtils.IsKey(Keys.Right, state))
                {
                    if (box.cursor.Pos < box.contents.Length)
                        box.cursor.Pos++;
                }
                else if (TextUtils.IsKey(Keys.Left, state))
                {
                    box.cursor.Pos--;
                }
            }
        }
    }
}
