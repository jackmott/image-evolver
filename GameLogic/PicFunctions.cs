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

        public static Texture2D GetTex(Pic p,GraphicsDevice graphics, int width, int height)
        {
            if (p.button.tex == null || (p.button.tex.Width != width || p.button.tex.Height != height))
            {
                if (p.button.tex != null) { p.button.tex.Dispose(); }
                if (p is RGBTree)
                {
                    var rgb = (RGBTree)p;
                    p.button.tex = ToTexture(rgb, graphics, width, height);
                }
                else if (p is HSVTree)
                {
                    var hsv = (HSVTree)p;
                    p.button.tex = ToTexture(hsv, graphics, width, height);
                }
                else if (p is GradientTree)
                {
                    var grad = (GradientTree)p;
                    grad.button.tex = ToTexture(grad, graphics, width, height);
                }
            }
            
            return p.button.tex;
        }

        public static Texture2D ToTexture(RGBTree pic,GraphicsDevice graphics, int w, int h)
        {
            Color[] colors = new Color[w * h];
            var scale = (float)(255 / 2);
            var offset = -1.0f * scale;
            var partition = Partitioner.Create(0, h);

            Parallel.ForEach(
                partition,
                (range, state) =>
                {
                    var rStack = new float[pic.Machines[0].nodeCount];
                    var gStack = new float[pic.Machines[1].nodeCount];
                    var bStack = new float[pic.Machines[2].nodeCount];
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                        int yw = y * w;
                        for (int x = 0; x < w; x++)
                        {
                            float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                            var r = (byte)(Execute(pic.Machines[0], xf, yf, rStack) * scale - offset);
                            var g = (byte)(Execute(pic.Machines[1], xf, yf, gStack) * scale - offset);
                            var b = (byte)(Execute(pic.Machines[2], xf, yf, bStack) * scale - offset);
                            colors[yw + x] = new Color(r, g, b, (byte)255);
                        }
                    }

                });
            Texture2D tex = new Texture2D(graphics, w, h);
            var tex2 = new Texture2D(graphics, w, h);
            tex.SetData(colors);
            return tex;
        }

        public static Texture2D ToTextureSingleThread(RGBTree pic, GraphicsDevice graphics, int w, int h)
        {
            Color[] colors = new Color[w * h];
            var scale = (float)(255 / 2);
            var offset = -1.0f * scale;
            var partition = Partitioner.Create(0, h);

            var rStack = new float[pic.Machines[0].nodeCount];
            var gStack = new float[pic.Machines[1].nodeCount];
            var bStack = new float[pic.Machines[2].nodeCount];
            for (int y = 0; y < h; y++)
                    {
                        float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                        int yw = y * w;
                        for (int x = 0; x < w; x++)
                        {
                            float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                            var r = (byte)(Execute(pic.Machines[0], xf, yf, rStack) * scale - offset);
                            var g = (byte)(Execute(pic.Machines[1], xf, yf, gStack) * scale - offset);
                            var b = (byte)(Execute(pic.Machines[2], xf, yf, bStack) * scale - offset);
                            colors[yw + x] = new Color(r, g, b, (byte)255);
                        }
                    }

            
            Texture2D tex = new Texture2D(graphics, w, h);
            var tex2 = new Texture2D(graphics, w, h);
            tex.SetData(colors);
            return tex;
        }

        public static Texture2D ToTexture(GradientTree pic, GraphicsDevice graphics, int w, int h)
        {
            Color[] colors = new Color[w * h];            
            var partition = Partitioner.Create(0, h);

            Parallel.ForEach(
                partition,
                (range, state) =>
                {
                    var stack = new float[pic.Machines[0].nodeCount];
                    
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                        int yw = y * w;
                        for (int x = 0; x < w; x++)
                        {
                            float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                            var r = Execute(pic.Machines[0], xf, yf, stack);
                            int i = 0;
                            for (; i < pic.pos.Length-2; i++)
                            {
                                if (r >= pic.pos[i] && r <= pic.pos[i+1])
                                {
                                    break;
                                }
                            }
                            

                            var c1 = pic.gradients[i];
                            var c2 = pic.gradients[i + 1];

                            float posDiff = r - pic.pos[i];
                            float totalDiff = pic.pos[i + 1] - pic.pos[i];
                            float pct = posDiff / totalDiff;
                            colors[yw + x] = Color.Lerp(c1, c2, pct);
                                                        
                        }
                    }

                });
            Texture2D tex = new Texture2D(graphics, w, h);
            var tex2 = new Texture2D(graphics, w, h);
            tex.SetData(colors);
            return tex;
        }


        public static Texture2D ToTexture(HSVTree pic,GraphicsDevice graphics, int width, int height)
        {
            Color[] colors = new Color[width * height];
            var scale = 0.5f;

            var partition = Partitioner.Create(0, height);
            Parallel.ForEach(
                partition,
                (range, state) =>
                {
                    var rStack = new float[pic.Machines[0].nodeCount];
                    var gStack = new float[pic.Machines[1].nodeCount];
                    var bStack = new float[pic.Machines[2].nodeCount];
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)height) * 2.0f - 1.0f;
                        int yw = y * width;
                        for (int x = 0; x < width; x++)
                        {
                            float xf = ((float)x / (float)width) * 2.0f - 1.0f;
                            var h = Wrap0To1(Execute(pic.Machines[0], xf, yf, rStack) * scale - scale);
                            var s = Wrap0To1(Execute(pic.Machines[1], xf, yf, gStack) * scale - scale);
                            var v = Wrap0To1(Execute(pic.Machines[2], xf, yf, bStack) * scale - scale);
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

