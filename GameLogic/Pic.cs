using System;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GameLogic.GraphUtils;
using static GameLogic.ColorTools;


namespace GameLogic
{
    public enum PicType { RGB, HSV, GRADIENT }
    public enum GradientType { RANDOM, COMPLEMENTARY, SPLIT_COMPLEMENTARY, TRIADIC, TETRADIC, SQUARE, ANALOGOUS }
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
        public (Color?, Color?)[] gradients;
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
                    Trees = new AptNode[3];
                    Machines = new StackMachine[3];
                    break;
                case PicType.GRADIENT:
                    Trees = new AptNode[1];
                    Machines = new StackMachine[1];
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
                    Trees = new AptNode[1];
                    Machines = new StackMachine[1];
                    Trees[0] = AptNode.GenerateTree(rand.Next(min, max), rand);
                    Machines[0] = new StackMachine(Trees[0]);
                    
                    Color[] colors = null;
                    var enum_size = Enum.GetNames(typeof(GradientType)).Length;
                    var gradType = (GradientType)rand.Next(0, enum_size);                    
                    float saturation = (float)rand.NextDouble();
                    float value = (float)rand.NextDouble();
                    switch (gradType)
                    {
                        case GradientType.ANALOGOUS:
                            {                                
                                colors = new Color[3];
                                var (h1, h2, h3) = ColorTools.GetAnalogousHues((float)rand.NextDouble());

                                var (r, g, b) = HSV2RGB(h1, saturation, value);
                                colors[0] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h2, saturation, value);
                                colors[1] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h3, saturation, value);
                                colors[2] = new Color(r, g, b);
                                break;
                            }
                        case GradientType.COMPLEMENTARY:
                            {                                
                                colors = new Color[2];
                                var (h1, h2) = ColorTools.GetComplementaryHues((float)rand.NextDouble());

                                var (r, g, b) = HSV2RGB(h1, saturation, value);
                                colors[0] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h2, saturation, value);
                                colors[1] = new Color(r, g, b);
                                break;
                            }
                        case GradientType.SPLIT_COMPLEMENTARY:
                            {                                
                                colors = new Color[3];
                                var (h1, h2, h3) = ColorTools.GetSplitComplementaryHues((float)rand.NextDouble());

                                var (r, g, b) = HSV2RGB(h1, saturation, value);
                                colors[0] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h2, saturation, value);
                                colors[1] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h3, saturation, value);
                                colors[2] = new Color(r, g, b);
                                break;
                            }
                        case GradientType.SQUARE:
                            {                                
                                colors = new Color[4];
                                var (h1, h2, h3, h4) = ColorTools.GetSquareHues((float)rand.NextDouble());

                                var (r, g, b) = HSV2RGB(h1, saturation, value);
                                colors[0] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h2, saturation, value);
                                colors[1] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h3, saturation, value);
                                colors[2] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h4, saturation, value);
                                colors[3] = new Color(r, g, b);
                                break;
                            }
                        case GradientType.TETRADIC:
                            {                                
                                colors = new Color[4];
                                var (h1, h2, h3, h4) = ColorTools.GetTetradicHues((float)rand.NextDouble());

                                var (r, g, b) = HSV2RGB(h1, saturation, value);
                                colors[0] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h2, saturation, value);
                                colors[1] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h3, saturation, value);
                                colors[2] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h4, saturation, value);
                                colors[3] = new Color(r, g, b);
                                break;
                            }
                        case GradientType.TRIADIC:
                            {                                
                                colors = new Color[3];
                                var (h1, h2, h3) = ColorTools.GetTriadicHues((float)rand.NextDouble());

                                var (r, g, b) = HSV2RGB(h1, saturation, value);
                                colors[0] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h2, saturation, value);
                                colors[1] = new Color(r, g, b);

                                (r, g, b) = HSV2RGB(h3, saturation, value);
                                colors[2] = new Color(r, g, b);
                                break;
                            }
                        case GradientType.RANDOM:
                            {
                                colors = new Color[rand.Next(Settings.MIN_GRADIENTS, Settings.MAX_GRADIENTS)];
                                for (int i = 0; i < colors.Length; i++)
                                {
                                    colors[i] = GraphUtils.RandomColor(rand);
                                }
                                break;
                            }                            
                    }


                    var numGradients = colors.Length;
                    gradients = new (Color?, Color?)[numGradients];
                    pos = new float[numGradients];
                    for (int i = 0; i < gradients.Length; i++)
                    {
                        bool isSuddenShift = rand.Next(0, Settings.CHANCE_HARD_GRADIENT) == 0;
                        if (!isSuddenShift)
                        {
                            gradients[i] = (colors[i], null);
                        }
                        else
                        {
                            gradients[i] = (colors[i],colors[(i+1)%colors.Length]);
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
                    return "( Gradient " + Trees[0].ToLisp() + " )";
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
                var newGradients = new (Color?, Color?)[gradients.Length];
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
                        result.gradients = ((Color?, Color?)[])partner.gradients.Clone();
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
                            byte r = (byte)(rf * 255.0f);
                            byte g = (byte)(gf * 255.0f);
                            byte b = (byte)(bf * 255.0f);
                            colors[yw + x] = new Color(r, g, b, (byte)255);
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

            Parallel.ForEach(
                partition,
                (range, state) =>
                {

                    var stack = new float[Machines[0].nodeCount];

                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                        int yw = y * w;
                        for (int x = 0; x < w; x++)
                        {
                            float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                            var r = Machines[0].Execute(xf, yf, stack);
                            int i = 0;
                            for (; i < pos.Length - 2; i++)
                            {
                                if (r >= pos[i] && r <= pos[i + 1])
                                {
                                    break;
                                }
                            }


                            var (c1a, c1b) = gradients[i];
                            var (c2a, c2b) = gradients[i + 1];

                            float posDiff = r - pos[i];
                            float totalDiff = pos[i + 1] - pos[i];
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


        private Texture2D HSVToTexture(GraphicsDevice graphics, int width, int height)
        {
            Color[] colors = new Color[width * height];
            var scale = 0.5f;

            var partition = Partitioner.Create(0, height);
            Parallel.ForEach(
                partition,
                (range, state) =>
                {
                    var rStack = new float[Machines[0].nodeCount];
                    var gStack = new float[Machines[1].nodeCount];
                    var bStack = new float[Machines[2].nodeCount];
                    for (int y = range.Item1; y < range.Item2; y++)
                    {
                        float yf = ((float)y / (float)height) * 2.0f - 1.0f;
                        int yw = y * width;
                        for (int x = 0; x < width; x++)
                        {
                            float xf = ((float)x / (float)width) * 2.0f - 1.0f;
                            var h = Wrap0To1(Machines[0].Execute(xf, yf, rStack) * scale + scale);
                            var s = Wrap0To1(Machines[1].Execute(xf, yf, gStack) * scale + scale);
                            var v = Wrap0To1(Machines[2].Execute(xf, yf, bStack) * scale + scale);
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

      
    }
}

