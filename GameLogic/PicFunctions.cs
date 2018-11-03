using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GameLogic.StackMachineFunctions;
namespace GameLogic
{
    public static class PicFunctions
    {

        public static Texture2D GetTex(Pic p,List<ExternalImage> images, GraphicsDevice graphics, int width, int height)
        {
            if (p.tex == null || (p.tex.Width != width || p.tex.Height != height))
            {
                if (p.tex != null) { p.tex.Dispose(); }
                if (p is RGBTree)
                {
                    var rgb = (RGBTree)p;
                    p.tex = ToTexture(rgb,images, graphics, width, height);
                }
                else if (p is HSVTree)
                {
                    var hsv = (HSVTree)p;
                    p.tex = ToTexture(hsv, images,graphics, width, height);
                }
            }
            
            return p.tex;
        }

        public static Texture2D ToTexture(RGBTree tree,List<ExternalImage> images, GraphicsDevice graphics, int w, int h)
        {
            Color[] colors = new Color[w * h];
            var scale = (float)(255 / 2);
            var offset = -1.0f * scale;
            var partition = Partitioner.Create(0, h);
            Parallel.ForEach(
                partition,
                (range, state) =>
                {
                    var rStack = new float[tree.RSM.nodeCount];
                    var gStack = new float[tree.GSM.nodeCount];
                    var bStack = new float[tree.BSM.nodeCount];
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                        int yw = y * w;
                        for (int x = 0; x < w; x++)
                        {
                            float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                            var r = (byte)(Execute(tree.RSM, xf, yf, rStack,images) * scale - offset);
                            var g = (byte)(Execute(tree.GSM, xf, yf, gStack,images) * scale - offset);
                            var b = (byte)(Execute(tree.BSM, xf, yf, bStack,images) * scale - offset);
                            colors[yw + x] = new Color(r, g, b, (byte)255);
                        }
                    }

                });
            Texture2D tex = new Texture2D(graphics, w, h);
            var tex2 = new Texture2D(graphics, w, h);
            tex.SetData(colors);
            return tex;
        }


        public static Texture2D ToTexture(HSVTree tree, List<ExternalImage> images,GraphicsDevice graphics, int width, int height)
        {
            Color[] colors = new Color[width * height];
            var scale = 0.5f;

            var partition = Partitioner.Create(0, height);
            Parallel.ForEach(
                partition,
                (range, state) =>
                {
                    var rStack = new float[tree.HSM.nodeCount];
                    var gStack = new float[tree.SSM.nodeCount];
                    var bStack = new float[tree.VSM.nodeCount];
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)height) * 2.0f - 1.0f;
                        int yw = y * width;
                        for (int x = 0; x < width; x++)
                        {
                            float xf = ((float)x / (float)width) * 2.0f - 1.0f;
                            var h = Wrap0To1(Execute(tree.HSM, xf, yf, rStack,images) * scale - scale);
                            var s = Wrap0To1(Execute(tree.SSM, xf, yf, gStack,images) * scale - scale);
                            var v = Wrap0To1(Execute(tree.VSM, xf, yf, bStack,images) * scale - scale);
                            var (rf, gf, bf) = HSV2RGB(h, s, v);
                            byte r = (byte)(rf * 255.0f);
                            byte g = (byte)(gf * 255.0f);
                            byte b = (byte)(bf * 255.0f);
                            colors[yw + x] = new Color(r, g, b, (byte)255);
                        }
                    }
                });
            Texture2D tex = new Texture2D(graphics, width, height);
            var tex2 = new Texture2D(graphics, width, height);
            tex.SetData(colors);
            return tex;
        }

        public static float Wrap0To1(float v)
        {
            return v - (float)Math.Floor(v);
        }

        public static (float, float, float) HSV2RGB(float h, float s, float v)
        {
            var hh = h / 0.1666666f;
            var i = (int)hh;
            var ff = hh - (float)i;
            var p = v * (1.0f - s);
            var q = v * (1.0f - (s * ff));
            var t = v * (1.0f - (s * (1.0f - ff)));

            float r, g, b;
            switch (i)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = p;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                case 5:
                default:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }

            if (s <= 0.0f)
            {
                r = v;
                b = v;
                g = v;
            }

            return (r, g, b);
        }
    }
}

