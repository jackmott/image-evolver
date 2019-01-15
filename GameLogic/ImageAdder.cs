using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameLogic
{
    public static class ImageAdder
    {
        public static void Draw(SpriteBatch batch, GraphicsDevice g, GameState state, GameTime gameTime)
        {
            int h = (int)(g.Viewport.Height * 0.75f);
            int w = (int)(g.Viewport.Width * 0.5f);
            var back = GraphUtils.GetTexture(g, new Color(0.0f, 0.0f, 0.0f, 0.85f));
            var bounds = GraphUtils.CenteredRect(g.Viewport.Bounds, w, h);
            batch.Draw(back,bounds,  Color.White);
            var btn = state.buttons["cancel-btn"];
            batch.Draw(btn, new Rectangle(bounds.X, bounds.Y+2,btn.Width/2,btn.Height/2), Color.White);
        }
    }
}
