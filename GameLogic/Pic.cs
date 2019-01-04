using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GameLogic.GraphUtils;
using static GameLogic.ColorTools;
using System.Threading;

namespace GameLogic
{

    public enum PicType { RGB, HSV, GRADIENT }
    public enum GradientType { RANDOM, DIAD, DOUBLE_COMPLEMENT, COMPLEMENTARY, SPLIT_COMPLEMENTARY, TRIADIC, TETRADIC, SQUARE, ANALOGOUS }
    [DataContract]
    public class Pic
    {
        [DataMember]
        bool video;
        [DataMember]
        public float t = 0.0f;
        [DataMember]
        public bool videoForward;
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
        public float[] hues;
        [DataMember]
        public float[] pos;

        [DataMember]
        public Button picButton;
        [DataMember]
        public Button injectButton;
        [DataMember]
        public Button editEquationButton;
        [DataMember]
        public Button constantFoldButton;
        [DataMember]
        public Button saveEquationButton;
        [DataMember]
        public Button cancelEditButton;
        [DataMember]
        public SlidingPanel panel;

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

        public Pic(PicType type, Random rand, int min, int max, GraphicsDevice graphics, GameWindow w, bool video)
        {
            this.video = video;
            this.g = graphics;
            this.w = w;
            this.type = type;
            InitButtons();

            Trees = new AptNode[3];
            Machines = new StackMachine[3];
            for (int i = 0; i < 3; i++)
            {
                Trees[i] = AptNode.GenerateTree(rand.Next(min, max), rand, video);
                Machines[i] = new StackMachine(Trees[i]);
            }

            if (type == PicType.GRADIENT)
            {
                Trees = new AptNode[3];
                Machines = new StackMachine[3];
                for (int i = 0; i < 3; i++)
                {
                    Trees[i] = AptNode.GenerateTree(rand.Next(min, max), rand, video);
                    Machines[i] = new StackMachine(Trees[i]);
                }

               
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

                pos = new float[hues.Length];
                for (int i = 0; i < hues.Length; i++)
                {
                    pos[i] = (float)(rand.NextDouble() * 2.0 - 1.0);
                }
                Array.Sort(pos);

            }
            SetupTextbox();
        }

        public void InitButtons()
        {
            picButton = new Button(null, new Rectangle());
            injectButton = new Button(Settings.injectTexture, new Rectangle());
            editEquationButton = new Button(Settings.equationTexture, new Rectangle());
            saveEquationButton = new Button(Settings.saveEquationTexture, new Rectangle());
            constantFoldButton = new Button(GraphUtils.GetTexture(g, Color.Blue), new Rectangle());
            cancelEditButton = new Button(Settings.cancelEditTexture, new Rectangle());
            panel = new SlidingPanel(Settings.panelTexture, new Rectangle(), new Rectangle(), 1000.0);
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
            if (hues != null)
            {
                var newHues = new float[hues.Length];
                var newPos = new float[pos.Length];

                for (int i = 0; i < hues.Length; i++)
                {
                    newHues[i] = hues[i];
                    newPos[i] = pos[i];
                }
                pic.hues = newHues;
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

        public void Draw(SpriteBatch batch, GameTime gameTime, InputState state)
        {
            if (selected)
            {
                Rectangle rect = new Rectangle(picButton.bounds.X - 5, picButton.bounds.Y - 5, picButton.bounds.Width + 10, picButton.bounds.Height + 10);
                batch.Draw(Settings.selectedTexture, rect, Color.White);
            }
            picButton.Draw(batch, gameTime);
            if (picButton.bounds.Contains(state.mouseState.Position))
            {
                injectButton.Draw(batch, gameTime);
            }
        }

        public void EditDraw(SpriteBatch batch, GameTime gameTime)
        {
            picButton.Draw(batch, gameTime);
            textBox.Draw(batch, gameTime);
            saveEquationButton.Draw(batch, gameTime);
            constantFoldButton.Draw(batch, gameTime);
            cancelEditButton.Draw(batch, gameTime);
        }

        public void ZoomDraw(SpriteBatch batch, GameTime gameTime, InputState state)
        {
            picButton.Draw(batch, gameTime);

            panel.Draw(batch, gameTime, state);
            var panelBounds = panel.GetBounds(state);
            editEquationButton.bounds = FRect(panelBounds.X + panelBounds.Width * .1f, panelBounds.Y + panelBounds.Height * .25f, panelBounds.Width * .1f, panelBounds.Height * .5f);
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
            injectButton.bounds = FRect(bounds.X + bounds.Width * .025, bounds.Y + bounds.Height * .9, bounds.Width * .1, bounds.Height * .1);
            saveEquationButton.bounds = FRect(textBounds.X, bounds.Height * .9f, bounds.Width * .1f, bounds.Height * .05f);
            constantFoldButton.bounds = FRect(textBounds.X + textBounds.Width * 0.4f, bounds.Height * .9f, bounds.Width * .1f, bounds.Height * .05f);

            cancelEditButton.bounds = FRect(textBounds.X + textBounds.Width - bounds.Width * .1f, bounds.Height * .9f, bounds.Width * .1f, bounds.Height * .05f);

            panel.activeBounds = FRect(0, bounds.Height * .85f, bounds.Width, bounds.Height * .15f);
            panel.hiddenBounds = FRect(0, bounds.Height * 1.001, bounds.Width, bounds.Height * .15f);

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
                    result.hues = new float[partner.hues.Length];
                    Array.Copy(partner.hues, result.hues, partner.hues.Length);
                }
                // clear gradient data if we are changing type FROM gradient
                else if (result.type == PicType.GRADIENT)
                {
                    result.pos = null;
                    result.hues = null;
                }
                result.type = partner.type;

                return result;
            }
            else
            {
                var (ft, fs) = result.GetRandomTree(r);
                var (st, ss) = partnerClone.GetRandomTree(r);
                ft.BreedWith(st, r, video);
                return result;
            }
        }

        public (AptNode, StackMachine) GetRandomTree(Random r)
        {
            int index = r.Next(0, Trees.Length);
            return (Trees[index], Machines[index]);
        }

        public void Optimize()
        {
            for (int i = 0; i < Trees.Length; i++)
            {
                Trees[i] = AptNode.ConstantFolding(Trees[i]);
                Machines[i].RebuildInstructions(Trees[i]);
            }
        }


        public Pic Mutate(Random r)
        {
            var result = Clone();
            if (r.Next(0, Settings.MUTATE_CHANCE) == 0)
            {
                var (t, s) = result.GetRandomTree(r);
                t.Mutate(r, video);
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
                    return RGBToTexture(graphics, w, h, t);
                case PicType.HSV:
                    return HSVToTexture(graphics, w, h, t);
                case PicType.GRADIENT:
                    return GradientToTexture(graphics, w, h, t);
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


        private Texture2D RGBToTexture(GraphicsDevice graphics, int w, int h, float t)
        {
            Color[] colors = new Color[w * h];
            var scale = 0.5f;

            var cpuCount = Environment.ProcessorCount;
            int chunk = h / cpuCount;

            Thread[] threads = new Thread[cpuCount+1];
            var extRange = (0, chunk);
            for (int i = 0; i < threads.Length; i++) {

                threads[i] = new Thread(o =>
                {
                    var range = (ValueTuple<int,int>)o;
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
                            var rf = Wrap0To1(Machines[0].Execute(xf, yf, t, rStack) * scale + scale);
                            var gf = Wrap0To1(Machines[1].Execute(xf, yf, t, gStack) * scale + scale);
                            var bf = Wrap0To1(Machines[2].Execute(xf, yf, t, bStack) * scale + scale);
                            colors[yw + x] = new Color(rf, gf, bf);
                        }
                    }
                });
                threads[i].Start(extRange);
                extRange.Item1 += chunk;
                extRange.Item2 += chunk;
                extRange.Item2 = Math.Min(h, extRange.Item2);
            }

            foreach (var thread in threads) {
                thread.Join();
            }
            
            Texture2D tex = new Texture2D(graphics, w, h);
            var tex2 = new Texture2D(graphics, w, h);
            tex.SetData(colors);
            return tex;
        }



        private Texture2D GradientToTexture(GraphicsDevice graphics, int w, int h, float t)
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
                            var hueIndex = Machines[0].Execute(xf, yf, t, hStack);
                            hueIndex = MathUtils.WrapMinMax(hueIndex, -1.0f, 1.0f);
                            int i = 0;
                            for (; i < pos.Length-1; i++)
                            {
                                if (hueIndex >= pos[i] && hueIndex <= pos[i + 1])
                                {
                                    break;
                                }
                            }
                            var s = Machines[1].Execute(xf, yf, t, sStack) * scale + scale;
                            var v = Machines[2].Execute(xf, yf, t, vStack) * scale + scale;

                            var f1 = hues[i];
                            var f2 = hues[(i + 1)%hues.Length];

                            float posDiff = hueIndex - pos[i];
                            float totalDiff = pos[(i + 1)%hues.Length] - pos[i];
                            float pct = posDiff / totalDiff;

                            var (c1r, c1g, c1b) = HSV2RGB(f1, s, v);
                            var (c2r, c2g, c2b) = HSV2RGB(f2, s, v);
                            var c1 = new Color(c1r, c1g, c1b);
                            var c2 = new Color(c2r, c2g, c2b);

                            colors[yw + x] = Color.Lerp(c1, c2, pct);

                        }
                    }
                });
            Texture2D tex = new Texture2D(graphics, w, h);
            var tex2 = new Texture2D(graphics, w, h);
            tex.SetData(colors);
            return tex;
        }


        private Texture2D HSVToTexture(GraphicsDevice graphics, int width, int height, float t)
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
                            var h = Wrap0To1(Machines[0].Execute(xf, yf, t, hStack) * scale + scale);
                            var s = Wrap0To1(Machines[1].Execute(xf, yf, t, sStack) * scale + scale);
                            var v = Wrap0To1(Machines[2].Execute(xf, yf, t, vStack) * scale + scale);
                            var (rf, gf, bf) = HSV2RGB(h, s, v);
                            colors[yw + x] = new Color(rf, gf, bf);
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
            return v % 1.0001f;
        }


    }
}

