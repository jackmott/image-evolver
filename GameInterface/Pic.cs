﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;

namespace GameInterface
{
    public abstract class Pic
    {
        protected Texture2D tex;
        public abstract Texture2D GetTex(GraphicsDevice graphics, int width, int height);        
    }

    public class RGBTree : Pic
    {
        public AptNode RTree;
        public AptNode GTree;
        public AptNode BTree;
        public StackMachine RSM;
        public StackMachine GSM;
        public StackMachine BSM;

        public RGBTree(int min, int max, Random rand)
        {

            RTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            RSM = new StackMachine(RTree);

            GTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            GSM = new StackMachine(GTree);

            BTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            BSM = new StackMachine(BTree);

        }

        public override Texture2D GetTex(GraphicsDevice graphics, int width, int height)
        {
            if (tex == null)
            {
                tex = ToTexture(graphics, width, height);
            }
            else if (tex.Width != width || tex.Height != height)
            {
                tex.Dispose();
                tex = ToTexture(graphics, width, height);
            }
            return tex;
        }

 

        public Texture2D ToTexture(GraphicsDevice graphics, int w, int h)
        {
            Color[] colors = new Color[w * h];
            var scale = (float)(255 / 2);
            var offset = -1.0f * scale;
            var partition = Partitioner.Create(0, h);
            Parallel.ForEach(
                partition,
                (range, state) =>
                {
                    var rStack = new float[RSM.nodeCount];
                    var gStack = new float[GSM.nodeCount];
                    var bStack = new float[BSM.nodeCount];
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                        int yw = y * w;
                        for (int x = 0; x < w; x++)
                        {
                            float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                            var r = (byte)(RSM.Execute(xf, yf, rStack) * scale - offset);
                            var g = (byte)(this.GSM.Execute(xf, yf, gStack) * scale - offset);
                            var b = (byte)(this.GSM.Execute(xf, yf, bStack) * scale - offset);
                            colors[yw + x] = new Color(r, g, b, (byte)255);
                        }
                    }
                });
            Texture2D tex = new Texture2D(graphics, w, h);
            var tex2 = new Texture2D(graphics, w, h);
            tex.SetData(colors);
            return tex;
        }
    }

    public class HSVTree : Pic
    {
        public AptNode HTree;
        public AptNode STree;
        public AptNode VTree;
        public StackMachine HSM;
        public StackMachine SSM;
        public StackMachine VSM;

        public HSVTree(int min, int max, Random rand)
        {

            HTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            HSM = new StackMachine(HTree);

            STree = AptNode.GenerateTree(rand.Next(min, max), rand);
            SSM = new StackMachine(STree);

            VTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            VSM = new StackMachine(VTree);

        }

        public override Texture2D GetTex(GraphicsDevice graphics, int width, int height)
        {
            if (tex == null)
            {
                tex = ToTexture(graphics, width, height);
            }
            else if (tex.Width != width || tex.Height != height)
            {
                tex.Dispose();
                tex = ToTexture(graphics, width, height);
            }
            return tex;
        }

        public Texture2D ToTexture(GraphicsDevice graphics, int width, int height)
        {
            Color[] colors = new Color[width * height];
            var scale = 0.5f;
            
            var partition = Partitioner.Create(0, height);
            Parallel.ForEach(
                partition,
                (range, state) =>
                {
                    var rStack = new float[HSM.nodeCount];
                    var gStack = new float[SSM.nodeCount];
                    var bStack = new float[VSM.nodeCount];
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)height) * 2.0f - 1.0f;
                        int yw = y * width;
                        for (int x = 0; x < width; x++)
                        {
                            float xf = ((float)x / (float)width) * 2.0f - 1.0f;
                            var h = Wrap0To1(HSM.Execute(xf, yf, rStack) * scale - scale);
                            var s = Wrap0To1(SSM.Execute(xf, yf, gStack) * scale - scale);
                            var v = Wrap0To1(VSM.Execute(xf, yf, bStack) * scale - scale);
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
