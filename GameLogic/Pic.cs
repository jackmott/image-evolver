﻿using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GameLogic.GraphUtils;
using static GameLogic.ColorTools;


namespace GameLogic
{
    public enum PicType { RGB, HSV, GRADIENT }
    public enum GradientType { RANDOM, DIAD,DOUBLE_COMPLEMENT,COMPLEMENTARY, SPLIT_COMPLEMENTARY, TRIADIC, TETRADIC, SQUARE, ANALOGOUS }
    [DataContract]
    public class Pic
    {
        [DataMember]
        public PicType type;
        [DataMember]
        public AptNode[] Trees;
        [DataMember]
        public StackMachine[] Machines;
        //Gradients can either be from one color to the next
        //or if one of the nullable colors is null
        // a hard stop
        [DataMember]
        public (float?, float?)[] gradients;
        [DataMember]
        public float[] pos;

        [DataMember]
        public Button picButton;
        [DataMember]
        public Button injectButton;
        [DataMember]
        public Button editEquationButton;
        [DataMember]
        public Button saveEquationButton;
        [DataMember]
        public Button cancelEditButton;

        [DataMember]
        public bool selected = false;
        [DataMember]
        public bool zoomed = false;



        [DataMember]
        public TextBox textBox;

        GraphicsDevice g;
        GameWindow w;

        public Pic(PicType type, GraphicsDevice g, GameWindow w)
        {
            this.g = g;
            this.w = w;
            this.type = type;
            InitButtons();
            switch (type)
            {
                case PicType.RGB:
                case PicType.HSV:
                case PicType.GRADIENT:
                    Trees = new AptNode[3];
                    Machines = new StackMachine[3];
                    break;
            }
        }

        public Pic(PicType type, Random rand, int min, int max, GraphicsDevice graphics, GameWindow w)
        {
            this.g = graphics;
            this.w = w;
            this.type = type;
            InitButtons();
            switch (type)
            {
                case PicType.RGB:
                case PicType.HSV:
                    Trees = new AptNode[3];
                    Machines = new StackMachine[3];
                    for (int i = 0; i < 3; i++)
                    {
                        Trees[i] = AptNode.GenerateTree(rand.Next(min, max), rand);
                        Machines[i] = new StackMachine(Trees[i]);
                    }
                    break;
                case PicType.GRADIENT:
                    Trees = new AptNode[3];
                    Machines = new StackMachine[3];
                    for (int i = 0; i < 3; i++)
                    {
                        Trees[i] = AptNode.GenerateTree(rand.Next(min, max), rand);
                        Machines[i] = new StackMachine(Trees[i]);
                    }

                    float[] hues = null;
                    var enum_size = Enum.GetNames(typeof(GradientType)).Length;
                    var gradType = (GradientType)rand.Next(0, enum_size);
                    gradType = GradientType.TRIADIC;
                    switch (gradType)
                    {
                        case GradientType.ANALOGOUS:
                            {
                                hues = new float[3];
                                (hues[0], hues[1], hues[2]) = GetAnalogousHues((float)rand.NextDouble());
                                break;
                            }
                        case GradientType.COMPLEMENTARY:
                            {
                                hues = new float[2];
                                (hues[0], hues[1]) = GetComplementaryHues((float)rand.NextDouble());
                                break;
                            }
                        case GradientType.SPLIT_COMPLEMENTARY:
                            {
                                hues = new float[3];
                                (hues[0], hues[1], hues[2]) = GetSplitComplementaryHues((float)rand.NextDouble());

                                break;
                            }
                        case GradientType.SQUARE:
                            {
                                hues = new float[4];
                                (hues[0], hues[1], hues[2], hues[3]) = GetSquareHues((float)rand.NextDouble());
                                break;
                            }
                        case GradientType.TETRADIC:
                            {
                                hues = new float[4];
                                (hues[0], hues[1], hues[2], hues[3]) = GetTetradicHues((float)rand.NextDouble());
                                break;
                            }
                        case GradientType.TRIADIC:
                            {
                                hues = new float[3];
                                (hues[0], hues[1], hues[2]) = GetTriadicHues((float)rand.NextDouble());
                                break;
                            }
                        case GradientType.RANDOM:
                            {
                                hues = new float[rand.Next(Settings.MIN_GRADIENTS, Settings.MAX_GRADIENTS)];
                                for (int i = 0; i < hues.Length; i++)
                                {
                                    hues[i] = (float)rand.NextDouble();
                                }
                                break;
                            }
                        case GradientType.DOUBLE_COMPLEMENT:
                            {
                                hues = new float[4];
                                (hues[0], hues[1], hues[2], hues[3]) = GetTetradicHues((float)rand.NextDouble());
                                break;
                            }
                        case GradientType.DIAD:
                            {
                                hues = new float[2];
                                (hues[0], hues[1]) = GetComplementaryHues((float)rand.NextDouble());
                                break;
                            }
                    }


                    var numGradients = hues.Length;
                    gradients = new (float?, float?)[numGradients];
                    pos = new float[numGradients];
                    for (int i = 0; i < gradients.Length; i++)
                    {
                        bool isSuddenShift = rand.Next(0, Settings.CHANCE_HARD_GRADIENT) == 0;
                        if (!isSuddenShift)
                        {
                            gradients[i] = (hues[i], null);
                        }
                        else
                        {
                            gradients[i] = (hues[i],hues[(i+1)%hues.Length]);
                        }
                        pos[i] = (float)(rand.NextDouble() * 2.0 - 1.0);
                        Array.Sort(pos);
                    }
                    pos[0] = -1.0f;
                    pos[pos.Length - 1] = 1.0f;
                    break;
            }
            SetupTextbox();
        }

        public void InitButtons()
        {
            picButton = new Button(null, new Rectangle());
            injectButton = new Button(Settings.injectTexture, new Rectangle());
            editEquationButton = new Button(Settings.equationTexture, new Rectangle());
            saveEquationButton = new Button(Settings.saveEquationTexture, new Rectangle());
            cancelEditButton = new Button(Settings.cancelEditTexture, new Rectangle());
        }

        public void SetupTextbox()
        {
            string lisp = ToLisp();
            textBox = new TextBox(lisp, w, GetTexture(g, new Color(0, 0, 0, 128)), GetTexture(g, Color.Cyan), ScaleCentered(picButton.bounds, 0.75f), Settings.equationFont, Color.White);
        }

        public string ToLisp()
        {
            switch (type)
            {
                case PicType.GRADIENT:
                    {
                        string result = "( Gradient \n";
                        result += Trees[0].ToLisp() + "\n";
                        result += Trees[1].ToLisp() + "\n";
                        result += Trees[2].ToLisp() + " )";
                        return result;
                    }
                case PicType.RGB:
                    {
                        string result = "( RGB \n";
                        result += Trees[0].ToLisp() + "\n";
                        result += Trees[1].ToLisp() + "\n";
                        result += Trees[2].ToLisp() + " )";
                        return result;
                    }
                case PicType.HSV:
                    {
                        string result = "( HSV \n";
                        result += Trees[0].ToLisp() + "\n";
                        result += Trees[1].ToLisp() + "\n";
                        result += Trees[2].ToLisp() + " )";
                        return result;
                    }
                default:
                    throw new Exception("Impossible");
            }
        }

        public Pic Clone()
        {
            Pic pic = new Pic(type, g, w);
            if (gradients != null)
            {
                var newGradients = new (float?, float?)[gradients.Length];
                var newPos = new float[pos.Length];

                for (int i = 0; i < newGradients.Length; i++)
                {
                    newGradients[i] = gradients[i];
                    newPos[i] = pos[i];
                }
                pic.gradients = newGradients;
                pic.pos = newPos;
            }
            for (int i = 0; i < Trees.Length; i++)
            {
                pic.Trees[i] = Trees[i].Clone();
                pic.Machines[i] = new StackMachine(Trees[i]);
            }
            pic.SetupTextbox();
            return pic;
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            if (selected)
            {
                Rectangle rect = new Rectangle(picButton.bounds.X - 5, picButton.bounds.Y - 5, picButton.bounds.Width + 10, picButton.bounds.Height + 10);
                batch.Draw(Settings.selectedTexture, rect, Color.White);
            }
            picButton.Draw(batch, gameTime);
            injectButton.Draw(batch, gameTime);
        }

        public void EditDraw(SpriteBatch batch, GameTime gameTime)
        {
            picButton.Draw(batch, gameTime);
            textBox.Draw(batch, gameTime);
            saveEquationButton.Draw(batch, gameTime);
            cancelEditButton.Draw(batch, gameTime);
        }

        public void ZoomDraw(SpriteBatch batch, GameTime gameTime)
        {
            picButton.Draw(batch, gameTime);
            editEquationButton.Draw(batch, gameTime);
        }

        public void SetNewBounds(Rectangle bounds, GraphicsDevice g)
        {
            picButton.bounds = bounds;
            if (picButton.tex != null)
            {
                picButton.tex.Dispose();
            }

            var textBounds = ScaleCentered(bounds, 0.75f);
            textBox.SetNewBounds(textBounds);
            injectButton.bounds = FRect(bounds.X, bounds.Y + bounds.Height + 5, bounds.Width * .1, bounds.Height * .1);
            editEquationButton.bounds = FRect(bounds.X + bounds.Width * .1, bounds.Y + bounds.Height * .9f, bounds.Width * .1, bounds.Height * .05);

            saveEquationButton.bounds = FRect(textBounds.X, bounds.Height * .9f, bounds.Width * .1f, bounds.Height * .05f);
            cancelEditButton.bounds = FRect(textBounds.X + textBounds.Width - bounds.Width * .1f, bounds.Height * .9f, bounds.Width * .1f, bounds.Height * .05f);


            RegenTex(g);
        }

        public Pic BreedWith(Pic partner, Random r)
        {

            var result = Clone();
            var partnerClone = partner.Clone();

            if (result.type != partner.type && r.Next(0, Settings.CROSSOVER_ROOT_CHANCE) == 0)
            {
                //Copy the gradient data if we are changing type TO gradient
                if (partner.type == PicType.GRADIENT)
                {
                    result.pos = new float[partner.pos.Length];
                    Array.Copy(partner.pos, result.pos, partner.pos.Length);
                    result.gradients = new (float?,float?)[partner.gradients.Length];
                    Array.Copy(partner.gradients, result.gradients, partner.gradients.Length);
                }
                // clear gradient data if we are changing type FROM gradient
                else if (result.type == PicType.GRADIENT)
                {
                    result.pos = null;
                    result.gradients = null;
                }
                result.type = partner.type;
                
                return result;
            }
            else
            {
                var (ft, fs) = result.GetRandomTree(r);
                var (st, ss) = partnerClone.GetRandomTree(r);
                ft.BreedWith(st, r);
                fs.RebuildInstructions(ft);
                return result;
            }
        }

        public (AptNode, StackMachine) GetRandomTree(Random r)
        {
            int index = r.Next(0, Trees.Length);
            return (Trees[index], Machines[index]);
        }


        public Pic Mutate(Random r)
        {
            var result = Clone();
            if (r.Next(0, Settings.MUTATE_CHANCE) == 0)
            {
                var (t, s) = result.GetRandomTree(r);
                t.Mutate(r);
                s.RebuildInstructions(t);
            }
            return result;
        }

        public void RegenTex(GraphicsDevice graphics)
        {
            if (picButton.tex != null) { picButton.tex.Dispose(); }
            picButton.tex = ToTexture(graphics, picButton.bounds.Width, picButton.bounds.Height);
        }

        public Texture2D ToTexture(GraphicsDevice graphics, int w, int h)
        {
            switch (type)
            {
                case PicType.RGB:
                    return RGBToTexture(graphics, w, h);
                case PicType.HSV:
                    return HSVToTexture(graphics, w, h);
                case PicType.GRADIENT:
                    return GradientToTexture(graphics, w, h);
                default:
                    throw new Exception("wat");

            }
        }

        public void RangeTest()
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            float[][] stacks = new float[Machines.Length][];
            for (int i = 0; i < stacks.Length; i++)
            {
                stacks[i] = new float[Machines[i].nodeCount];
            }

            int h = 500;
            int w = 500;
            for (int y = 0; y < h; y++)
            {
                float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                int yw = y * w;
                for (int x = 0; x < w; x++)
                {
                    float xf = ((float)x / (float)w) * 2.0f - 1.0f;

                    for (int i = 0; i < Machines.Length; i++)
                    {
                        var f = Machines[i].Execute(xf, yf, stacks[i]);
                        if (f < min) min = f;
                        if (f > max) max = f;
                    }
                }

            }

            Console.WriteLine("min:" + min + " max:" + max + " range:" + (max - min));
        }
        private Texture2D RGBToTexture(GraphicsDevice graphics, int w, int h)
        {
            Color[] colors = new Color[w * h];
            var scale = 0.5f;
            var partition = Partitioner.Create(0, h);
                                    
            Parallel.ForEach(
                partition,
                (range, state) =>
                {
                    var rStack = new float[Machines[0].nodeCount];
                    var gStack = new float[Machines[1].nodeCount];
                    var bStack = new float[Machines[2].nodeCount];
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                        int yw = y * w;
                        for (int x = 0; x < w; x++)
                        {
                            float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                            var rf = Wrap0To1(Machines[0].Execute(xf, yf, rStack) * scale + scale);
                            var gf = Wrap0To1(Machines[1].Execute(xf, yf, gStack) * scale + scale);
                            var bf = Wrap0To1(Machines[2].Execute(xf, yf, bStack) * scale + scale);                                                       
                            colors[yw + x] = new Color(rf,gf,bf);
                        }
                    }
                });
            Texture2D tex = new Texture2D(graphics, w, h);
            var tex2 = new Texture2D(graphics, w, h);
            tex.SetData(colors);
            return tex;
        }



        private Texture2D GradientToTexture(GraphicsDevice graphics, int w, int h)
        {
            Color[] colors = new Color[w * h];
            var partition = Partitioner.Create(0, h);
            var scale = 0.5f;
            Parallel.ForEach(
                partition,
                (range, state) =>
                {

                    var hStack = new float[Machines[0].nodeCount];
                    var sStack = new float[Machines[1].nodeCount];
                    var vStack = new float[Machines[2].nodeCount];

                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                        int yw = y * w;
                        for (int x = 0; x < w; x++)
                        {
                            float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                            var hues = Machines[0].Execute(xf, yf, hStack);
                            int i = 0;
                            for (; i < pos.Length - 2; i++)
                            {
                                if (hues >= pos[i] && hues <= pos[i + 1])
                                {
                                    break;
                                }
                            }
                            var s = Machines[1].Execute(xf, yf, sStack)*scale+scale;
                            var v = Machines[2].Execute(xf, yf, vStack)*scale+scale;

                            var (h1a, h1b) = gradients[i];
                            var (h2a, h2b) = gradients[i + 1];

                            float posDiff = hues - pos[i];
                            float totalDiff = pos[i + 1] - pos[i];
                            float pct = posDiff / totalDiff;

                            float h1;
                            if (h1b == null) h1 = h1a.Value;
                            else h1 = h1b.Value;

                            var (c1r,c1g,c1b) = HSV2RGB(h1, s, v);
                            var (c2ar,c2ag,c2ab) = HSV2RGB(h2a.Value, s, v);
                            Color c1 = new Color(c1r, c1g, c1b);
                            Color c2a = new Color(c2ar, c2ag, c2ab);

                            colors[yw + x] = Color.Lerp(c1, c2a, pct);

                        }
                    }
                });
            Texture2D tex = new Texture2D(graphics, w, h);
            var tex2 = new Texture2D(graphics, w, h);
            tex.SetData(colors);
            return tex;
        }


        private Texture2D HSVToTexture(GraphicsDevice graphics, int width, int height)
        {
            Color[] colors = new Color[width * height];
            var scale = 0.5f;

            var partition = Partitioner.Create(0, height);
            Parallel.ForEach(
                partition,
                (range, state) =>
                {
                    var hStack = new float[Machines[0].nodeCount];
                    var sStack = new float[Machines[1].nodeCount];
                    var vStack = new float[Machines[2].nodeCount];
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)height) * 2.0f - 1.0f;
                        int yw = y * width;
                        for (int x = 0; x < width; x++)
                        {
                            float xf = ((float)x / (float)width) * 2.0f - 1.0f;
                            var h = Wrap0To1(Machines[0].Execute(xf, yf, hStack) * scale + scale);
                            var s = Wrap0To1(Machines[1].Execute(xf, yf, sStack) * scale + scale);
                            var v = Wrap0To1(Machines[2].Execute(xf, yf, vStack) * scale + scale);
                            var (rf, gf, bf) = HSV2RGB(h, s, v);                            
                            colors[yw + x] = new Color(rf,gf,bf);
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

      
    }
}

