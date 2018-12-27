using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameLogic
{
    public static class GraphUtils
    {
        private static Dictionary<Color, Texture2D> simple_textures = new Dictionary<Color, Texture2D>();
        public static Texture2D GetTexture(GraphicsDevice g, Color color)
        {
            Texture2D tex;
            if (simple_textures.TryGetValue(color, out tex))
            {
                return tex;
            }
            else
            {
                tex = new Texture2D(g, 1, 1, false, SurfaceFormat.Color);
                tex.SetData(new[] { color });
                simple_textures[color] = tex;
                return tex;
            }            
        }

        public static Color RandomColor(Random r)
        {
            return new Color((byte)r.Next(0, 256), (byte)r.Next(0, 256), (byte)r.Next(0, 256), (byte)255);
        }

        public static Rectangle FRect(float x, float y, float w, float h)
        {
            return new Rectangle((int)x, (int)y, (int)w, (int)h);
        }

        public static Rectangle FRect(double x, double y, double w, double h)
        {
            return new Rectangle((int)x, (int)y, (int)w, (int)h);
        }

        public static Rectangle ScaleCentered(Rectangle r, float scale)
        {
            float newW = r.Width * scale;
            float newH = r.Height * scale;
            float x = r.X + (r.Width - newW) / 2.0f;
            float y = r.Y + (r.Height- newH) / 2.0f;
            return FRect(x, y, newW, newH);
        }
    }
}
