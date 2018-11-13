using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic
{
    public static class WordWrap
    {
        public static int MeasureWidth(string s)
        {
            return (int)Settings.equationFont.MeasureString(s).X;
        }

        public static string Wrap(string s, int width, Func<string, int> widthMeasure)
        {
            int spaceWidth = widthMeasure(" ");
            StringBuilder builder = new StringBuilder(s.Length);
            var text = s.AsSpan();
            int lineStart = 0;
            int lineLen = 0;
            int wordStart = 0;
            int lineWidth = 0;
            for (int i = 0; i < text.Length; i++) {
                
                if (text[i] == ' ')
                {   //0123456789
                    //the poo 
                    int wordWidth = widthMeasure(text.Slice(wordStart, i - wordStart).ToString());
                    if (lineWidth + wordWidth <= width)
                    {                        
                        lineLen = i - lineStart+1;
                        lineWidth += wordWidth + spaceWidth;
                        wordStart = i+1;
                    }
                    else
                    {
                        builder.AppendLine(text.Slice(lineStart,lineLen).ToString());
                        lineLen = i-wordStart;
                        lineStart = wordStart;
                        wordStart = i + 1;
                        lineWidth = wordWidth + spaceWidth;
                        
                    }
                    
                }
                if (text[i] == '\n')
                {
                    builder.AppendLine(text.Slice(lineStart, i-lineStart).ToString());
                    lineLen = 0;
                    lineStart = i + 1;
                    wordStart = i + 1;
                    lineWidth = 0;
                }
            }
            builder.AppendLine(text.Slice(lineStart).ToString());                                            
            return builder.ToString();
            
        }

    }
}
