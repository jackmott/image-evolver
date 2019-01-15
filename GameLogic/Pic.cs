using System;
using JM.LinqFaster;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GameLogic.GraphUtils;
using static GameLogic.ColorTools;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Threading;
using System.IO;


namespace GameLogic
{
    public struct TextureData
    {
        public Color[] colors;
        public int start;
        public int end;
    }

    public enum PicType { RGB, HSV, GRADIENT }
    public enum GradientType { RANDOM, DIAD, DOUBLE_COMPLEMENT, COMPLEMENTARY, SPLIT_COMPLEMENTARY, TRIADIC, TETRADIC, SQUARE, ANALOGOUS }
    [DataContract]
    public class Pic : IDisposable
    {

        public Rectangle bounds;
        public Texture2D[] videoFrames;

        private Texture2D smallImage;
        public Texture2D bigImage;


        public CancellationTokenSource imageCancellationSource;

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
        public Color[] colors;
        [DataMember]
        public float[] pos;

        [DataMember]
        public Button injectButton;
        [DataMember]
        public Button editEquationButton;        
        [DataMember]
        public Button playButton;
        [DataMember]
        public Button exportGIFButton;
        [DataMember]
        public Button exportPNGButton;
        [DataMember]
        public Button saveEquationButton;
        [DataMember]
        public Button cancelEditButton;
        [DataMember]
        public Button cancelVideoGenButton;
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
            SharedConstructor(type, g, w);
        }

        public Pic(PicType type, Random rand, int min, int max, GraphicsDevice g, GameWindow w, bool video)
        {
            this.video = video;
            SharedConstructor(type, g, w);

            for (int i = 0; i < Trees.Length; i++)
            {
                Trees[i] = AptNode.GenerateTree(rand.Next(min, max), rand, video);
                Machines[i] = new StackMachine(Trees[i]);
            }


            if (type == PicType.GRADIENT)
            {

                var enum_size = Enum.GetNames(typeof(GradientType)).Length;
                var gradType = (GradientType)rand.Next(0, enum_size);                
                float[] hues;
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
                    default:
                        throw new Exception("hues broke");
                }

                colors = hues.SelectF(h =>
                {
                    float s = (float)rand.NextDouble();
                    float v = (float)rand.NextDouble();
                    var (red, green, blue) = HSV2RGB(h, s, v);
                    return new Color(red, green, blue);
                });

                pos = new float[colors.Length];
                int chance = Settings.STOP_GRADIENT_CHANCE * pos.Length;
                for (int i = 0; i < colors.Length; i++)
                {

                    if (i > 0 && rand.Next(0, chance) == 0)
                    {
                        pos[i] = pos[i - 1];
                    }
                    else
                    {
                        pos[i] = (float)(rand.NextDouble() * 2.0 - 1.0);
                    }

                }
                Array.Sort(pos);

            }
            SetupTextbox();
        }

        private void SharedConstructor(PicType type, GraphicsDevice g, GameWindow w)
        {
            this.g = g;
            this.w = w;
            this.type = type;
            imageCancellationSource = new CancellationTokenSource();
            smallImage = GraphUtils.GetTexture(g, Color.Black);
            bigImage = GraphUtils.GetTexture(g, Color.Black);
            InitButtons();
            if (type != PicType.GRADIENT)
            {
                Trees = new AptNode[3];
                Machines = new StackMachine[3];
            }
            else
            {
                Trees = new AptNode[1];
                Machines = new StackMachine[1];
            }
        }


        public void GenBigImage()
        {
            int chunkSize =  Math.Min(64, bounds.Height);
            int lineCount = 0;

            while (lineCount < bounds.Height)
            {
                ImageGenAsync(bounds.Width, bounds.Height, lineCount, lineCount + chunkSize, -1.0f).ContinueWith(task =>
                 {
                     if (bigImage.Width != bounds.Width || bigImage.Height != bounds.Height)
                     {
                         bigImage.Dispose();
                         bigImage = new Texture2D(g, bounds.Width, bounds.Height, false, SurfaceFormat.Color);
                         bigImage.SetData(new Color[bigImage.Width * bigImage.Height]); //will be transparent so smallImage will show beneath
                     }

                     var imageData = task.Result;
                     var start = imageData.start;
                     var len = imageData.end - imageData.start;
                     bigImage.SetData(0, new Rectangle(0, imageData.start, bigImage.Width, len), imageData.colors, 0, imageData.colors.Length);

                 }, TaskScheduler.FromCurrentSynchronizationContext());
                lineCount += chunkSize;
                chunkSize = (int)(chunkSize * 2);
                chunkSize = Math.Min(chunkSize, bounds.Height - lineCount);
            }

        }


        public async Task GenSmallImageAsync()
        {
            var result = await ImageGenAsync(bounds.Width, bounds.Height, 0, bounds.Height, -1.0f);

            smallImage.Dispose(); // dispose of the old texture
            smallImage = new Texture2D(g, bounds.Width, bounds.Height, false, SurfaceFormat.Color);
            var colors = result.colors;
            //todo this assert fails sometimes when flipping between states quickly
            Debug.Assert(colors.Length == smallImage.Width * smallImage.Height);
            smallImage.SetData(colors);

        }

        public void GenVideoRec(GameState state, int w, int h, int index, float t)
        {
            const int frameCount = Settings.FPS * Settings.VIDEO_LENGTH;

            //base case of the recursion, we done
            if (index == frameCount) return;

            //logical core count
            var cpuCount = Environment.ProcessorCount;
            var taskCount = Math.Min(cpuCount, frameCount - index);

            var stepSize = 2.0f / frameCount;
            var tasks = new Task<TextureData>[taskCount];

            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = ImageGenAsync(w, h, 0, h, t);
                t += stepSize;
            }
            // Wait until this batch is done before firing off more, so we get progress bar updates
            Task.WhenAll(tasks).ContinueWith(task =>
            {
                var frameDatas = task.Result;
                foreach (var frameData in frameDatas)
                {
                    var start = frameData.start;
                    var len = frameData.end - frameData.start;
                    videoFrames[index].SetData(0, new Rectangle(0, frameData.start, w, len), frameData.colors, 0, frameData.colors.Length);
                    index++;
                    Transition.AddProgress(1);
                }
                GenVideoRec(state, w, h, index, t);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async Task exportGIF(GameState state)
        {
            //start generating the gif while they file dialog is open
            Transition.StartTransition(state.screen, videoFrames.Length, "Saving Gif");
            var task = Task.Run(genGIF);
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "gifs (*.gif)|*.gif";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = false;
            Stream fileStream;
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if ((fileStream = saveFileDialog.OpenFile()) != null)
                {
                    state.screen = Screen.GIF_EXPORTING;
                    var gifStream = await task;
                    Transition.SetProgress(videoFrames.Length / 2);
                    gifStream.Position = 0;
                    gifStream.CopyTo(fileStream);
                    fileStream.Close();
                    gifStream.Close();
                    Transition.SetProgress(videoFrames.Length);
                }
            }
        }

        public Stream genGIF()
        {
            var store = new FrameStore(videoFrames.Length, videoFrames[0].Width, videoFrames[0].Height);
            foreach (var frame in videoFrames)
            {
                store.PushFrame(frame);
            }

            float delay = 1.0f / Settings.FPS;
            delay *= 100.0f;

            Stream myStream = new MemoryStream(1024);
            store.ExportGif(myStream, (int)delay);
            return myStream;
        }

        public void GenVideo(GameState state)
        {

            const int w = Settings.VIDEO_WIDTH;
            const int h = Settings.VIDEO_HEIGHT;

            const int frameCount = Settings.FPS * Settings.VIDEO_LENGTH;
            Transition.StartTransition(Screen.VIDEO_PLAYING, frameCount, "Generating...");
            state.screen = Screen.VIDEO_GENERATING;

            //Just play the video if its already there
            if (videoFrames != null)
            {
                var frame = videoFrames[0];
                if (frame == null)
                {
                    ClearVideo();
                }
                else if (frame.Width == w && frame.Height == h)
                {
                    Transition.AddProgress(frameCount);
                    return;
                }
                else
                {
                    ClearVideo();
                }

            }


            videoFrames = new Texture2D[frameCount];
            for (int i = 0; i < videoFrames.Length; i++)
            {
                videoFrames[i] = new Texture2D(g, w, h, false, SurfaceFormat.Color);
            }


            GenVideoRec(state, w, h, 0, -1.0f);

        }

        public bool WasLeftClicked(InputState state)
        {
            if (state.prevMouseState.LeftButton == ButtonState.Pressed && state.mouseState.LeftButton == ButtonState.Released)
            {
                if (bounds.Contains(state.prevMouseState.Position) && bounds.Contains(state.mouseState.Position))
                {
                    return true;
                }
            }
            return false;
        }

        public bool WasRightClicked(InputState state)
        {
            if (state.prevMouseState.RightButton == ButtonState.Pressed && state.mouseState.RightButton == ButtonState.Released)
            {
                if (bounds.Contains(state.prevMouseState.Position) && bounds.Contains(state.mouseState.Position))
                {
                    return true;
                }
            }
            return false;
        }

        public void InitButtons()
        {
            injectButton = new Button("New", Settings.buttonFont, new Rectangle(), Color.Cyan, Color.White);
            editEquationButton = new Button("Edit", Settings.buttonFont, new Rectangle(), Color.Cyan, Color.White);
            saveEquationButton = new Button("Save", Settings.buttonFont, new Rectangle(), Color.Cyan, Color.White);           
            playButton = new Button("Play", Settings.buttonFont, new Rectangle(), Color.Cyan, Color.White);
            exportGIFButton = new Button("Export", Settings.buttonFont, new Rectangle(), Color.Cyan, Color.White);
            exportPNGButton = new Button("Export", Settings.buttonFont, new Rectangle(), Color.Cyan, Color.White);
            cancelVideoGenButton = new Button("Cancel", Settings.buttonFont, new Rectangle(), Color.Cyan, Color.White);
            cancelEditButton = new Button("Cancel", Settings.buttonFont, new Rectangle(), Color.Cyan, Color.White);
            panel = new SlidingPanel(Settings.panelTexture, new Rectangle(), new Rectangle(), 500.0);
        }

        public void SetupTextbox()
        {
            string lisp = ToLisp();
            textBox = new TextBox(lisp, w, GetTexture(g, new Color(0, 0, 0, 128)), GetTexture(g, Color.Cyan), ScaleCentered(bounds, 0.75f), Settings.equationFont, Color.White);
        }

        public string ToLisp()
        {
            switch (type)
            {
                case PicType.GRADIENT:
                    {
                        string result = "( Gradient \n";
                        result += "( Colors";
                        foreach (var c in colors)
                        {
                            float r = (float)c.R / 255.0f;
                            float g = (float)c.G / 255.0f;
                            float b = (float)c.B / 255.0f;
                            result += " (  " + r.ToString("0.000") + " " + g.ToString("0.000") + " " + b.ToString("0.000") + " ) ";
                        }
                        result += " )\n";
                        result += "( Positions";
                        foreach (var p in pos)
                        {
                            result += " " + p.ToString("0.000");
                        }
                        result += " )\n";
                        result += Trees[0].ToLisp() + " )";
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
            pic.video = video;
            if (colors != null)
            {
                var newColors = new Color[colors.Length];
                var newPos = new float[pos.Length];

                for (int i = 0; i < colors.Length; i++)
                {
                    newColors[i] = colors[i];
                    newPos[i] = pos[i];
                }
                pic.colors = newColors;
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

        public void Draw(SpriteBatch batch, GraphicsDevice g, GameWindow w, GameTime gameTime, InputState state)
        {
            // only draw if the tex is ready            
            if (selected)
            {
                Rectangle rect = new Rectangle(bounds.X - 5, bounds.Y - 5, bounds.Width + 10, bounds.Height + 10);
                batch.Draw(Settings.selectedTexture, rect, Color.White);
            }
            batch.Draw(smallImage, bounds, Color.White);
            if (bounds.Contains(state.mouseState.Position))
            {
                injectButton.Draw(batch, g, gameTime);
            }

        }

        public void EditDraw(SpriteBatch batch, GraphicsDevice g, GameWindow w, GameTime gameTime)
        {
            batch.Draw(bigImage, bounds, Color.White);
            textBox.Draw(batch, gameTime);
            saveEquationButton.Draw(batch, g, gameTime);
            cancelEditButton.Draw(batch, g, gameTime);
        }



        public void PanelDraw(SpriteBatch batch, GraphicsDevice g, GameTime gameTime, InputState state, bool videoGenerating, bool videoPlaying)
        {
            panel.Draw(batch, gameTime, state);
            var panelBounds = panel.GetBounds(state);
            var leftButtonBounds = FRect(panelBounds.X + panelBounds.Width * .1f, panelBounds.Y + panelBounds.Height * .25f, panelBounds.Width * .1f, panelBounds.Height * .5f);

            if (videoGenerating)
            {
                cancelVideoGenButton.SetBounds(leftButtonBounds);
                cancelVideoGenButton.Draw(batch, g, gameTime);
                return;
            }

            editEquationButton.SetBounds(leftButtonBounds);
            editEquationButton.Draw(batch, g, gameTime);

            if (video)
            {
                var bounds = leftButtonBounds;
              
                bounds.X += (int)(bounds.Width * 1.1f);
                playButton.SetBounds(bounds);
                playButton.Draw(batch, g, gameTime);

                if (videoPlaying)
                {
                    bounds.X += (int)(bounds.Width * 1.1f);
                    exportGIFButton.SetBounds(bounds);
                    exportGIFButton.Draw(batch, g, gameTime);
                }
            }
            else
            {
                if (bigImage != null && bigImage.Width == bounds.Width && bigImage.Height == bounds.Height)
                {
                    var bounds = leftButtonBounds;
                    bounds.X += (int)(bounds.Width * 1.1f);
                    exportPNGButton.SetBounds(bounds);
                    exportPNGButton.Draw(batch, g, gameTime);
                }
            }
        }

        public void VideoPlayingDraw(SpriteBatch batch, GraphicsDevice g, GameTime gameTime, InputState state)
        {
            var seconds = gameTime.TotalGameTime.TotalSeconds % (Settings.VIDEO_LENGTH * 2.0f);
            var frameIndex = (int)(seconds * Settings.FPS);
            if (frameIndex >= videoFrames.Length)
            {
                var backIndex = frameIndex - videoFrames.Length;
                frameIndex = videoFrames.Length - backIndex - 1;
            }
            batch.Draw(videoFrames[frameIndex], bounds, Color.White);
            PanelDraw(batch, g, gameTime, state, false, true);
        }

        public void GifGeneratingDraw(SpriteBatch batch, GraphicsDevice g, GameWindow w, GameTime gameTime)
        {
            batch.Begin();
            batch.Draw(smallImage, bounds, Color.White);
            batch.Draw(bigImage, bounds, Color.White);
            batch.End();
        }

        public void ZoomDraw(SpriteBatch batch, GraphicsDevice g, GameWindow w, GameTime gameTime, InputState state)
        {
            batch.Draw(smallImage, bounds, Color.White);
            batch.Draw(bigImage, bounds, Color.White);
            PanelDraw(batch, g, gameTime, state, false, false);
        }

        public void VideoGeneratingDraw(SpriteBatch batch, GraphicsDevice g, GameWindow w, GameTime gameTime, InputState state)
        {
            batch.Draw(smallImage, bounds, Color.White);
            batch.Draw(bigImage, bounds, Color.White);
            PanelDraw(batch, g, gameTime, state, true, false);
        }


        public void SetNewBounds(Rectangle bounds)
        {
            this.bounds = bounds;

            var textBounds = ScaleCentered(bounds, 0.75f);
            textBox.SetNewBounds(textBounds);
            injectButton.SetBounds(FRect(bounds.X + bounds.Width * .025, bounds.Y + bounds.Height * .9, bounds.Width * .1, bounds.Height * .1));
            saveEquationButton.SetBounds(FRect(textBounds.X, bounds.Height * .9f, bounds.Width * .1f, bounds.Height * .05f));            
            cancelEditButton.SetBounds(FRect(textBounds.X + textBounds.Width - bounds.Width * .1f, bounds.Height * .9f, bounds.Width * .1f, bounds.Height * .05f));


            panel.activeBounds = FRect(0, bounds.Height * .85f, bounds.Width, bounds.Height * .15f);
            panel.hiddenBounds = FRect(0, bounds.Height * 1.001, bounds.Width, bounds.Height * .15f);
        }

        public Pic BreedWith(Pic partner, Random r)
        {

            var result = Clone();
            var partnerClone = partner.Clone();

            if (result.type != partner.type && r.Next(0, Settings.CROSSOVER_ROOT_CHANCE) == 0)
            {
                //Not gradient -> gradient
                if (partner.type == PicType.GRADIENT)
                {
                    result.pos = new float[partner.pos.Length];
                    Array.Copy(partner.pos, result.pos, partner.pos.Length);
                    result.colors = new Color[partner.colors.Length];
                    Array.Copy(partner.colors, result.colors, partner.colors.Length);
                    var newMachines = new StackMachine[1];
                    var newTrees = new AptNode[1];                    
                    var (tree, machine) = result.GetRandomTree(r);
                    newTrees[0] = tree.Clone();
                    newMachines[0] = new StackMachine(newTrees[0]);
                    result.Machines = newMachines;
                    result.Trees = newTrees;

                }
                //Gradient -> not gradient
                else if (result.type == PicType.GRADIENT)
                {
                    result.pos = null;
                    result.colors = null;
                    var newMachines = new StackMachine[3];
                    var newTrees = new AptNode[3];
                    var i = r.Next(0, newTrees.Length);                    
                    newTrees[i] = result.Trees[0].Clone();
                    newMachines[i] = new StackMachine(newTrees[i]);
                    for (i = 0; i < newTrees.Length; i++)
                    {
                        if (newTrees[i] == null)
                        {
                            newTrees[i] = AptNode.GetRandomLeaf(r, video);
                            newMachines[i] = new StackMachine(newTrees[i]);
                        }
                    }
                    
                    result.Trees = newTrees;
                    result.Machines = newMachines;
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
                Machines[i] = new StackMachine(Trees[i]);
            }
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
                    for (int i = 0; i < result.Trees.Length; i++)
                    {
                        if (result.Trees[i] == t)
                        {
                            result.Trees[i] = rootMutated;
                        }
                        t = rootMutated;
                    }
                }
                for (int i = 0; i < result.Machines.Length; i++)
                {
                    result.Machines[i] = new StackMachine(result.Trees[i]);
                }
            }
            return result;
        }

        private async Task<TextureData> ImageGenAsync(
            int w, int h,
            int start, int end,
            float t)
        {
            var ct = imageCancellationSource.Token;
            return await Task.Run(() =>
           {
               Color[] colors = new Color[(end - start) * w];
               switch (type)
               {
                   case PicType.RGB:
                       RGBToTexture(w, h, start, end, t, colors, ct);
                       break;
                   case PicType.HSV:
                       HSVToTexture(w, h, start, end, t, colors, ct);
                       break;
                   case PicType.GRADIENT:
                       GradientToTexture(w, h, start, end, t, colors, ct);
                       break;
                   default:
                       throw new Exception("wat");
               }
               return new TextureData { colors = colors, start = start, end = end };
           }, ct);
        }

        private void RGBToTexture(int w, int h, int start, int end, float t, Color[] colors, CancellationToken ct)
        {
            unsafe
            {
                const float scale = 0.5f;

                var rStack = stackalloc float[Machines[0].nodeCount];
                var gStack = stackalloc float[Machines[1].nodeCount];
                var bStack = stackalloc float[Machines[2].nodeCount];

                for (int y = start; y < end; y++)
                {

                    float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                    int yw = (y - start) * w;
                    for (int x = 0; x < w; x++)
                    {
                        float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                        var rf = Wrap0To1(Machines[0].Execute(xf, yf, t, rStack) * scale + scale);
                        var gf = Wrap0To1(Machines[1].Execute(xf, yf, t, gStack) * scale + scale);
                        var bf = Wrap0To1(Machines[2].Execute(xf, yf, t, bStack) * scale + scale);
                        colors[yw + x] = new Color(rf, gf, bf);
                    }
                    ct.ThrowIfCancellationRequested();
                }
            }
        }


        private void GradientToTexture(int w, int h, int start, int end, float t, Color[] c, CancellationToken ct)
        {
            unsafe
            {
                const float scale = 0.5f;
                var stack = stackalloc float[Machines[0].nodeCount];

                for (int y = start; y < end; y++)
                {
                    float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                    int yw = (y - start) * w;
                    for (int x = 0; x < w; x++)
                    {
                        float xf = ((float)x / (float)w) * 2.0f - 1.0f;
                        var colorIndex = Machines[0].Execute(xf, yf, t, stack);
                        colorIndex = MathUtils.WrapMinMax(colorIndex, -1.0f, 1.0f);
                        Color c1 = new Color { };
                        Color c2 = new Color { };
                        float pct = 0.0f;
                        //[-.9 0.2  .8]
                        if (colorIndex < pos[0] || colorIndex > pos[pos.Length - 1])
                        {
                            c1 = colors[colors.Length - 1];
                            c2 = colors[0];
                            float distToEnd = (1.0f - pos[pos.Length - 1]);
                            float distToStart = Math.Abs(-1.0f - pos[0]);
                            if (colorIndex < pos[0])
                            {
                                pct = (distToEnd + (colorIndex - -1.0f )) / (distToEnd + distToStart);
                            }
                            else
                            {
                                pct = (colorIndex - pos[pos.Length - 1]) / (distToEnd + distToStart);
                            }
                        }
                        else 
                        {
                            for (int i = 0; i < pos.Length-1; i++)
                            {
                                if (colorIndex > pos[i] && colorIndex < pos[i+1])
                                {
                                    c1 = colors[i];
                                    c2 = colors[i + 1];
                                    float dist = pos[i + 1] - pos[i];
                                    pct = (colorIndex - pos[i]) / dist;
                                    break;
                                }
                            }
                        }
                        
                        c[yw + x] = Color.Lerp(c1, c2, pct);

                    }
                    ct.ThrowIfCancellationRequested();
                }
            }
        }

        private void HSVToTexture(int width, int height, int start, int end, float t, Color[] colors, CancellationToken ct)
        {
            unsafe
            {
                const float scale = 0.5f;

                var hStack = stackalloc float[Machines[0].nodeCount];
                var sStack = stackalloc float[Machines[1].nodeCount];
                var vStack = stackalloc float[Machines[2].nodeCount];

                for (int y = start; y < end; y++)
                {
                    float yf = ((float)y / (float)height) * 2.0f - 1.0f;
                    int yw = (y - start) * width;
                    for (int x = 0; x < width; x++)
                    {
                        float xf = ((float)x / (float)width) * 2.0f - 1.0f;
                        var h = Wrap0To1(Machines[0].Execute(xf, yf, t, hStack) * scale + scale);
                        var s = Wrap0To1(Machines[1].Execute(xf, yf, t, sStack) * scale + scale);
                        var v = Wrap0To1(Machines[2].Execute(xf, yf, t, vStack) * scale + scale);
                        var (rf, gf, bf) = HSV2RGB(h, s, v);
                        colors[yw + x] = new Color(rf, gf, bf);
                    }
                    ct.ThrowIfCancellationRequested();

                }
            }

        }

        public static float Wrap0To1(float v)
        {
            return v % 1.0001f;
        }

        public void ClearVideo()
        {
            if (videoFrames != null)
            {
                foreach (var frame in videoFrames)
                {
                    if (frame != null && !frame.IsDisposed)
                    {
                        frame.Dispose();
                    }
                }
            }
            videoFrames = null;
        }
        public void Dispose()
        {
            if (bigImage != null && !bigImage.IsDisposed)
            {
                bigImage.Dispose();
            }
            if (smallImage != null && !smallImage.IsDisposed)
            {
                smallImage.Dispose();
            }
            ClearVideo();

        }
    }
}

