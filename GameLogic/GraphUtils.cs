using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GameLogic
{
    public static class GraphUtils
    {
        private static Dictionary<Color, Texture2D> simple_textures = new Dictionary<Color, Texture2D>();
        public static Texture2D GetTexture(SpriteBatch spriteBatch, Color color)
        {
            Texture2D tex;
            if (simple_textures.TryGetValue(color, out tex))
            {
                return tex;
            }
            else
            {
                tex = new Texture2D(spriteBatch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                tex.SetData(new[] { color });
                simple_textures[color] = tex;
                return tex;
            }            
        }

        public static Color RandomColor(Random r)
        {
            return new Color((byte)r.Next(0, 256), (byte)r.Next(0, 256), (byte)r.Next(0, 256), (byte)255);
        }
    }
}
