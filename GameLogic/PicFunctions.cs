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
        public static Pic NewPic(PicType type)
        {
            Pic pic = new Pic();
            pic.type = type;
            pic.button = new Button(null, new Rectangle());
            pic.inject = new Button(Settings.injectTexture, new Rectangle());
            pic.equation = new Button(Settings.equationTexture, new Rectangle());
            switch (type)
            {
                case PicType.RGB:
                case PicType.HSV:
                    pic.Trees = new AptNode[3];
                    pic.Machines = new StackMachine[3];                   
                    break;
                case PicType.GRADIENT:
                    pic.Trees = new AptNode[1];
                    pic.Machines = new StackMachine[1];                    
                    break;
            }
            return pic;
        }

        public static Pic NewPic(PicType type, Random rand, int min, int max)
        {
            Pic pic = NewPic(type);
                        
            switch (type)
            {
                case PicType.RGB:
                case PicType.HSV:                    
                    for (int i = 0; i < 3; i++)
                    {
                        pic.Trees[i] = AptNode.GenerateTree(rand.Next(min, max), rand);
                        pic.Machines[i] = new StackMachine(pic.Trees[i]);
                    }
                    break;
                case PicType.GRADIENT:                    
                    pic.Trees[0] = AptNode.GenerateTree(rand.Next(min, max), rand);
                    pic.Machines[0] = new StackMachine(pic.Trees[0]);

                    int numGradients = rand.Next(Settings.MIN_GRADIENTS, Settings.MAX_GRADIENTS);
                    pic.gradients = new (Color?, Color?)[numGradients];
                    pic.pos = new float[numGradients];
                    for (int i = 0; i < pic.gradients.Length; i++)
                    {
                        bool isSuddenShift = rand.Next(0, Settings.CHANCE_HARD_GRADIENT) == 0;
                        if (!isSuddenShift)
                        {
                            pic.gradients[i] = (GraphUtils.RandomColor(rand), null);
                        }
                        else
                        {
                            pic.gradients[i] = (GraphUtils.RandomColor(rand), GraphUtils.RandomColor(rand));
                        }
                        pic.pos[i] = (float)(rand.NextDouble() * 2.0 - 1.0);
                        Array.Sort(pic.pos);
                    }
                    pic.pos[0] = -1.0f;
                    pic.pos[pic.pos.Length - 1] = 1.0f;
                    break;
            }
            return pic;
        }

        public static void SetupTextbox(Pic p, GraphicsDevice g, GameWindow window)
        {
            string lisp = p.ToLisp();
            p.textBox = new TextBox(lisp, window, GraphUtils.GetTexture(g, new Color(0, 0, 0, 128)), GraphUtils.GetTexture(g, Color.Cyan), p.button.bounds, Settings.equationFont, Color.White);
        }

        public static Pic Clone(Pic toClone)
        {
            Pic pic = NewPic(toClone.type);
            if (toClone.gradients != null)
            {
                var newGradients = new (Color?, Color?)[toClone.gradients.Length];
                var newPos = new float[toClone.pos.Length];

                for (int i = 0; i < newGradients.Length; i++)
                {
                    newGradients[i] = toClone.gradients[i];
                    newPos[i] = toClone.pos[i];
                }
                pic.gradients = newGradients;
                pic.pos = newPos;
            }
            for (int i = 0; i < toClone.Trees.Length; i++)
            {
                pic.Trees[i] = toClone.Trees[i].Clone();
                pic.Machines[i] = new StackMachine(toClone.Trees[i]);
            }            
            return pic;
        }

        public static void Draw(Pic pic, SpriteBatch batch, GameTime gameTime)
        {
            if (pic.selected)
            {
                Rectangle rect = new Rectangle(pic.button.bounds.X - 5, pic.button.bounds.Y - 5, pic.button.bounds.Width + 10, pic.button.bounds.Height + 10);
                batch.Draw(Settings.selectedTexture, rect, Color.White);
            }
            pic.button.Draw(batch, gameTime);
            pic.inject.Draw(batch, gameTime);                                                           
        }

        public static void ZoomDraw(Pic pic, SpriteBatch batch, GameTime gameTime)
        {

            pic.button.Draw(batch, gameTime);
            pic.equation.Draw(batch, gameTime);
            if (pic.textBox.IsActive())
            {
                pic.textBox.Draw(batch, gameTime);                
            }
        }

        public static void SetNewBounds(Pic pic, Rectangle bounds, GraphicsDevice g)
        {            
            pic.button.bounds = bounds;
            if (pic.button.tex != null)
            {
                pic.button.tex.Dispose();
            }
            pic.inject.bounds = new Rectangle(bounds.X, bounds.Y + bounds.Height + 5, 20, 20);
            pic.equation.bounds = new Rectangle(bounds.X+30, (int)(bounds.Y + bounds.Height * .9f), 20, 20);            
            RegenTex(pic, g);            
        }

        public static Pic BreedWith(Pic pic, Pic partner, Random r)
        {
            
            var result = Clone(pic);

            if (result.type != partner.type && r.Next(0, Settings.CROSSOVER_ROOT_CHANCE) == 0)
            {
                result.type = partner.type;
                if (result.Trees.Length != partner.Trees.Length)
                {
                    var newTrees = new AptNode[partner.Trees.Length];
                    var newMachines = new StackMachine[partner.Trees.Length];
                    for (int i = 0; i < partner.Trees.Length; i++)
                    {
                        var randomIndex = r.Next(0, result.Trees.Length);
                        newTrees[i] = result.Trees[randomIndex];
                        newMachines[i] = result.Machines[randomIndex];
                    }
                    result.Trees = newTrees;
                    result.Machines = newMachines;
                    if (partner.type == PicType.GRADIENT)
                    {
                        result.gradients = ((Color?,Color?)[])partner.gradients.Clone();
                        result.pos = (float[])partner.pos.Clone();
                    }
                    
                }
                return result;
            }
            else
            {

                var (ft, fs) = result.GetRandomTree(r);
                var (st, ss) = partner.GetRandomTree(r);
                ft.BreedWith(st, r);
                fs.RebuildInstructions(ft);
                return result;
            }
        }

        public static Pic Mutate(Pic p, Random r)
        {
            var result = Clone(p);
            if (r.Next(0, Settings.MUTATE_CHANCE) == 0) {
                var (t, s) = result.GetRandomTree(r);
                APTFunctions.Mutate(t, r);
                s.RebuildInstructions(t);
            }
            return result;
        }

        public static void RegenTex(Pic p,GraphicsDevice graphics)
        {            
                if (p.button.tex != null) { p.button.tex.Dispose(); }
                p.button.tex = ToTexture(p, graphics, p.button.bounds.Width, p.button.bounds.Height);                                                    
        }

        public static Texture2D ToTexture(Pic pic, GraphicsDevice graphics, int w, int h)
        {
            switch (pic.type) {
                case PicType.RGB:
                    return RGBToTexture(pic, graphics, w, h);
                case PicType.HSV:
                    return HSVToTexture(pic, graphics, w, h);
                case PicType.GRADIENT:
                    return GradientToTexture(pic, graphics, w, h);
                default:
                    throw new Exception("wat");

           }
        }
        private static Texture2D RGBToTexture(Pic pic,GraphicsDevice graphics, int w, int h)
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

       

        private static Texture2D GradientToTexture(Pic pic, GraphicsDevice graphics, int w, int h)
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
                            

                            var (c1a,c1b) = pic.gradients[i];
                            var (c2a,c2b) = pic.gradients[i + 1];
                            
                            float posDiff = r - pic.pos[i];
                            float totalDiff = pic.pos[i + 1] - pic.pos[i];
                            float pct = posDiff / totalDiff;

                            Color c1;
                            if (c1b == null) c1 = c1a.Value;
                            else c1 = c1b.Value;
                            
                            colors[yw + x] = Color.Lerp(c1, c2a.Value, pct);
                                                        
                        }
                    }

                });
            Texture2D tex = new Texture2D(graphics, w, h);
            var tex2 = new Texture2D(graphics, w, h);
            tex.SetData(colors);
            return tex;
        }


        private static Texture2D HSVToTexture(Pic pic,GraphicsDevice graphics, int width, int height)
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

