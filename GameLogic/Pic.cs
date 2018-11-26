using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace GameLogic
{
    public enum PicType { RGB, HSV, GRADIENT }
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
        public Button button;
        [DataMember]
        public Button inject;
        [DataMember]
        public Button equation;

        [DataMember]
        public bool selected = false;
        [DataMember]
        public bool zoomed = false;
        [DataMember]
        public bool showEquation = false;

        [DataMember]
        public TextBox textBox;

        public Pic(PicType type)
        {
            
            this.type = type;
            button = new Button(null, new Rectangle());
            inject = new Button(Settings.injectTexture, new Rectangle());
            equation = new Button(Settings.equationTexture, new Rectangle());
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

        public Pic(PicType type, Random rand, int min, int max)
        {
            this.type = type;
            button = new Button(null, new Rectangle());
            inject = new Button(Settings.injectTexture, new Rectangle());
            equation = new Button(Settings.equationTexture, new Rectangle());
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

                    int numGradients = rand.Next(Settings.MIN_GRADIENTS, Settings.MAX_GRADIENTS);
                    gradients = new (Color?, Color?)[numGradients];
                    pos = new float[numGradients];
                    for (int i = 0; i < gradients.Length; i++)
                    {
                        bool isSuddenShift = rand.Next(0, Settings.CHANCE_HARD_GRADIENT) == 0;
                        if (!isSuddenShift)
                        {
                            gradients[i] = (GraphUtils.RandomColor(rand), null);
                        }
                        else
                        {
                            gradients[i] = (GraphUtils.RandomColor(rand), GraphUtils.RandomColor(rand));
                        }
                        pos[i] = (float)(rand.NextDouble() * 2.0 - 1.0);
                        Array.Sort(pos);
                    }
                    pos[0] = -1.0f;
                    pos[pos.Length - 1] = 1.0f;
                    break;
            }            
        }

        public void SetupTextbox(GraphicsDevice g, GameWindow window)
        {
            string lisp = ToLisp();
            textBox = new TextBox(lisp, window, GraphUtils.GetTexture(g, new Color(0, 0, 0, 128)), GraphUtils.GetTexture(g, Color.Cyan), button.bounds, Settings.equationFont, Color.White);
        }

        public string ToLisp()
        {
            switch (type)
            {
                case PicType.GRADIENT:
                    return "( Gradient " + Trees[0].ToLisp() + " )\n";
                case PicType.RGB:
                    {
                        string result = "( R " + Trees[0].ToLisp() + " )\n";
                        result += "( G " + Trees[1].ToLisp() + " )\n";
                        result += "( B " + Trees[2].ToLisp() + " )\n";
                        return result;
                    }
                case PicType.HSV:
                    {
                        string result = "( H " + Trees[0].ToLisp() + " )\n";
                        result += "( S " + Trees[1].ToLisp() + " )\n";
                        result += "( V " + Trees[2].ToLisp() + " )\n";
                        return result;
                    }
                default:
                    throw new Exception("Impossible");
            }
        }

        public Pic Clone()
        {
            Pic pic = new Pic(type);
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
            return pic;
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            if (selected)
            {
                Rectangle rect = new Rectangle(button.bounds.X - 5, button.bounds.Y - 5, button.bounds.Width + 20, button.bounds.Height + 10);
                batch.Draw(Settings.selectedTexture, rect, Color.White);
            }
            button.Draw(batch, gameTime);
            inject.Draw(batch, gameTime);                                                           
        }

        public void ZoomDraw(SpriteBatch batch, GameTime gameTime)
        {

            button.Draw(batch, gameTime);
            equation.Draw(batch, gameTime);
            if (textBox.IsActive())
            {
                textBox.Draw(batch, gameTime);                
            }
        }

        public void SetNewBounds(Rectangle bounds, GraphicsDevice g)
        {            
            button.bounds = bounds;
            if (button.tex != null)
            {
                button.tex.Dispose();
            }
            inject.bounds = new Rectangle(bounds.X, bounds.Y + bounds.Height + 5, 20, 20);
            equation.bounds = new Rectangle(bounds.X+30, (int)(bounds.Y + bounds.Height * .9f), 20, 20);            
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

        public (AptNode, StackMachine) GetRandomTree(Random r)
        {
            int index = r.Next(0, Trees.Length);
            return (Trees[index], Machines[index]);
        }


        public Pic Mutate(Random r)
        {
            var result = Clone();
            if (r.Next(0, Settings.MUTATE_CHANCE) == 0) {
                var (t, s) = result.GetRandomTree(r);
                t.Mutate(r);
                s.RebuildInstructions(t);
            }
            return result;
        }

        public void RegenTex(GraphicsDevice graphics)
        {            
            if (button.tex != null) { button.tex.Dispose(); }
            button.tex = ToTexture(graphics, button.bounds.Width, button.bounds.Height);                                                    
        }

        public Texture2D ToTexture(GraphicsDevice graphics, int w, int h)
        {
            switch (type) {
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
        private Texture2D RGBToTexture(GraphicsDevice graphics, int w, int h)
        {
            Color[] colors = new Color[w * h];
            var scale = (float)(255 / 2);
            var offset = -1.0f * scale;
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
                            var r = (byte)(Machines[0].Execute(xf, yf, rStack) * scale - offset);
                            var g = (byte)(Machines[1].Execute(xf, yf, gStack) * scale - offset);
                            var b = (byte)(Machines[2].Execute(xf, yf, bStack) * scale - offset);
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
                            for (; i < pos.Length-2; i++)
                            {
                                if (r >= pos[i] && r <= pos[i+1])
                                {
                                    break;
                                }
                            }
                            

                            var (c1a,c1b) = gradients[i];
                            var (c2a,c2b) = gradients[i + 1];
                            
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
                            var h = Wrap0To1(Machines[0].Execute(xf, yf, rStack) * scale - scale);
                            var s = Wrap0To1(Machines[1].Execute(xf, yf, gStack) * scale - scale);
                            var v = Wrap0To1(Machines[2].Execute(xf, yf, bStack) * scale - scale);
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

