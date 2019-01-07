using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GameLogic.GraphUtils;
using static GameLogic.ColorTools;
using System.Threading.Tasks;

namespace GameLogic
{

    public enum PicType { RGB, HSV, GRADIENT }
    public enum GradientType { RANDOM, DIAD, DOUBLE_COMPLEMENT, COMPLEMENTARY, SPLIT_COMPLEMENTARY, TRIADIC, TETRADIC, SQUARE, ANALOGOUS }
    [DataContract]
    public class Pic
    {
        public Texture2D[] videoFrames;
        [DataMember]
        bool video;
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
        public Button previewButton;
        [DataMember]
        public Button playButton;
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
            previewButton = new Button(GraphUtils.GetTexture(g, Color.Blue), new Rectangle());
            playButton = new Button(GraphUtils.GetTexture(g, Color.Red), new Rectangle());
            cancelEditButton = new Button(Settings.cancelEditTexture, new Rectangle());
            panel = new SlidingPanel(Settings.panelTexture, new Rectangle(), new Rectangle(), 500.0);
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
                        result += "( Hues";
                        foreach (var hue in hues)
                        {
                            result += " " + hue.ToString("0.000");
                        }
                        result += " )\n";
                        result += "( Positions";
                        foreach (var p in pos)
                        {
                            result += " " + p.ToString("0.000");
                        }
                        result += " )\n";
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
            // only draw if the tex is ready
            if (picButton.tex != null)
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
        }

        public void EditDraw(SpriteBatch batch, GameTime gameTime)
        {
            picButton.Draw(batch, gameTime);
            textBox.Draw(batch, gameTime);
            saveEquationButton.Draw(batch, gameTime);
            cancelEditButton.Draw(batch, gameTime);
        }



        public void PanelDraw(SpriteBatch batch, GameTime gameTime, InputState state)
        {
            panel.Draw(batch, gameTime, state);
            var panelBounds = panel.GetBounds(state);
            editEquationButton.bounds = FRect(panelBounds.X + panelBounds.Width * .1f, panelBounds.Y + panelBounds.Height * .25f, panelBounds.Width * .1f, panelBounds.Height * .5f);
            editEquationButton.Draw(batch, gameTime);

            if (video)
            {
                previewButton.bounds = editEquationButton.bounds;
                previewButton.bounds.X += (int)(previewButton.bounds.Width * 1.1f);
                previewButton.Draw(batch, gameTime);

                playButton.bounds = previewButton.bounds;
                playButton.bounds.X += (int)(previewButton.bounds.Width * 1.1f);
                playButton.Draw(batch, gameTime);
            }
        }

        public void VideoPlayingDraw(SpriteBatch batch, GameTime gameTime, InputState state)
        {
            var seconds = gameTime.TotalGameTime.TotalSeconds % (Settings.VIDEO_LENGTH * 2.0f);
            var frameIndex = (int)(seconds * Settings.FPS);
            if (frameIndex >= videoFrames.Length)
            {
                var backIndex = frameIndex - videoFrames.Length;
                frameIndex = videoFrames.Length - backIndex - 1;
            }
            batch.Draw(videoFrames[frameIndex], picButton.bounds, Color.White);
            PanelDraw(batch, gameTime, state);
        }

        public void ZoomDraw(SpriteBatch batch, GameTime gameTime, InputState state)
        {
            picButton.Draw(batch, gameTime);
            PanelDraw(batch, gameTime, state);
        }


        public void SetNewBounds(Rectangle bounds)
        {
            picButton.bounds = bounds;


            var textBounds = ScaleCentered(bounds, 0.75f);
            textBox.SetNewBounds(textBounds);
            injectButton.bounds = FRect(bounds.X + bounds.Width * .025, bounds.Y + bounds.Height * .9, bounds.Width * .1, bounds.Height * .1);
            saveEquationButton.bounds = FRect(textBounds.X, bounds.Height * .9f, bounds.Width * .1f, bounds.Height * .05f);
            previewButton.bounds = FRect(textBounds.X + textBounds.Width * 0.4f, bounds.Height * .9f, bounds.Width * .1f, bounds.Height * .05f);

            cancelEditButton.bounds = FRect(textBounds.X + textBounds.Width - bounds.Width * .1f, bounds.Height * .9f, bounds.Width * .1f, bounds.Height * .05f);

            panel.activeBounds = FRect(0, bounds.Height * .85f, bounds.Width, bounds.Height * .15f);
            panel.hiddenBounds = FRect(0, bounds.Height * 1.001, bounds.Width, bounds.Height * .15f);
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
                var rootBred = ft.BreedWith(st, r, video);
                if (rootBred != null)
                {
                    for (int i = 0; i < result.Trees.Length; i++)
                    {
                        if (result.Trees[i] == ft)
                        {
                            result.Trees[i] = rootBred;
                        }
                    }

                }
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

        public void GenerateVideo(int w, int h)
        {
            new Task(() =>
            {
                //clear all old video textures
                if (videoFrames != null)
                {
                    foreach (var frame in videoFrames)
                    {
                        frame.Dispose();
                    }
                    videoFrames = null;
                }
                //5 seconds at 30fps
                const int frameCount = Settings.FPS * Settings.VIDEO_LENGTH;
                videoFrames = new Texture2D[frameCount];
                Task[] tasks = new Task[frameCount];
                var stepSize = 2.0f / frameCount;
                float t = -1.0f;
                for (int i = 0; i < videoFrames.Length; i++)
                {
                    int index = i;
                    float time = t;
                    tasks[i] = new Task(() =>
                    {
                        videoFrames[index] = new Texture2D(g, w, h, false, SurfaceFormat.Color);
                        ToTexture(videoFrames[index], false, time);
                        Console.WriteLine("frame done");
                        Transition.AddProgress(1.0f / frameCount);
                    });
                    tasks[i].Start();
                    t += stepSize;                    
                }
                Task.WaitAll(tasks);
                Transition.Complete();
            }).Start();
        }


        public Pic Mutate(Random r)
        {
            var result = Clone();
            if (r.Next(0, Settings.MUTATE_CHANCE) == 0)
            {
                var (t, s) = result.GetRandomTree(r);
                var rootMutated = t.Mutate(r, video);
                if (rootMutated != null)
                {
                    //t = rootMutated;
                    for (int i = 0; i < Trees.Length; i++)
                    {
                        if (Trees[i] == t)
                        {
                            Trees[i] = rootMutated;
                        }
                        t = rootMutated;
                    }
                }
                s.RebuildInstructions(t);
            }
            return result;
        }

        public void RegenTex(GraphicsDevice graphics, bool parallel)
        {

            if (picButton.tex != null) { picButton.tex.Dispose(); }
            picButton.tex = new Texture2D(graphics, picButton.bounds.Width, picButton.bounds.Height, false, SurfaceFormat.Color);

            new Task(() =>
            {
                ToTexture(picButton.tex, parallel);

            }).Start();
        }

        public void ToTexture(Texture2D tex, bool parallel, float t = -1.0f)
        {
            switch (type)
            {
                case PicType.RGB:
                    if (parallel)
                    {
                        ParallelImageGen(tex, t, RGBToTexture);
                    }
                    else
                    {
                        ScalarImageGen(tex, t, RGBToTexture);
                    }
                    break;
                case PicType.HSV:
                    if (parallel)
                    {
                        ParallelImageGen(tex, t, HSVToTexture);
                    }
                    else
                    {
                        ScalarImageGen(tex, t, HSVToTexture);
                    }
                    break;
                case PicType.GRADIENT:
                    if (parallel)
                    {
                        ParallelImageGen(tex, t, GradientToTexture);
                    }
                    else
                    {
                        ScalarImageGen(tex, t, GradientToTexture);
                    }
                    break;
                default:
                    throw new Exception("wat");

            }
        }

        private bool AllTasksComplete(Task[] tasks)
        {
            foreach (var task in tasks)
            {
                if (!task.IsCompleted) return false;
            }
            return true;
        }

        private void ScalarImageGen(Texture2D tex, float t, Action<int, int, float, Color[], Texture2D> f)
        {
            int w = tex.Width;
            int h = tex.Height;
            Color[] colors = new Color[w * h];
            tex.SetData(colors); //need to clear data or we get old random texture data
            f.Invoke(0, h, t, colors, tex);
            tex.SetData(colors);

        }

        private void ParallelImageGen(Texture2D tex, float t, Action<int, int, float, Color[], Texture2D> f)
        {
            int w = tex.Width;
            int h = tex.Height;
            Color[] colors = new Color[w * h];
            var cpuCount = Environment.ProcessorCount * 3;
            int chunk = (int)Math.Ceiling((float)h / (float)cpuCount);
            Task[] tasks = new Task[cpuCount];
            var extRange = (0, chunk);
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Task(o =>
                {
                    var range = (ValueTuple<int, int>)o;
                    f.Invoke(range.Item1, range.Item2, t, colors, tex);
                    //todo do we need to lock?
                    tex.SetData(0, new Rectangle(0, range.Item1, w, range.Item2 - range.Item1), colors, range.Item1 * w, (range.Item2 - range.Item1) * w);

                }, extRange);

                tasks[i].Start();
                extRange.Item1 += chunk;
                extRange.Item2 += chunk;
                extRange.Item2 = Math.Min(h, extRange.Item2);
              }

            Task.WaitAll(tasks);
        }

        private void RGBToTexture(int start, int end, float t, Color[] colors, Texture2D tex)
        {
            int w = tex.Width;
            int h = tex.Height;
            var scale = 0.5f;
            var rStack = new float[Machines[0].nodeCount];
            var gStack = new float[Machines[1].nodeCount];
            var bStack = new float[Machines[2].nodeCount];

            for (int y = start; y < end; y++)
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
        }


        private void GradientToTexture(int start, int end, float t, Color[] colors, Texture2D tex)
        {
            int w = tex.Width;
            int h = tex.Height;
            var scale = 0.5f;
            var hStack = new float[Machines[0].nodeCount];
            var sStack = new float[Machines[1].nodeCount];
            var vStack = new float[Machines[2].nodeCount];

            for (int y = start; y < end; y++)
            {
                float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                int yw = y * w;
                for (int x = 0; x < w; x++)
                {
                    float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                    var hueIndex = Machines[0].Execute(xf, yf, t, hStack);
                    hueIndex = MathUtils.WrapMinMax(hueIndex, -1.0f, 1.0f);
                    int i = 0;
                    for (; i < pos.Length - 1; i++)
                    {
                        if (hueIndex >= pos[i] && hueIndex <= pos[i + 1])
                        {
                            break;
                        }
                    }
                    var s = Machines[1].Execute(xf, yf, t, sStack) * scale + scale;
                    var v = Machines[2].Execute(xf, yf, t, vStack) * scale + scale;

                    var f1 = hues[i];
                    var f2 = hues[(i + 1) % hues.Length];

                    float posDiff = hueIndex - pos[i];
                    float totalDiff = pos[(i + 1) % hues.Length] - pos[i];
                    float pct = posDiff / totalDiff;

                    var (c1r, c1g, c1b) = HSV2RGB(f1, s, v);
                    var (c2r, c2g, c2b) = HSV2RGB(f2, s, v);
                    var c1 = new Color(c1r, c1g, c1b);
                    var c2 = new Color(c2r, c2g, c2b);

                    colors[yw + x] = Color.Lerp(c1, c2, pct);

                }

            }
        }


        private void HSVToTexture(int start, int end, float t, Color[] colors, Texture2D tex)
        {
            var scale = 0.5f;
            int width = tex.Width;
            int height = tex.Height;

            var hStack = new float[Machines[0].nodeCount];
            var sStack = new float[Machines[1].nodeCount];
            var vStack = new float[Machines[2].nodeCount];

            for (int y = start; y < end; y++)
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

        }

        public static float Wrap0To1(float v)
        {
            return v % 1.0001f;
        }


    }
}

