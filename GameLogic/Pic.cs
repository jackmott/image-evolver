using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static GameLogic.GraphUtils;
using static GameLogic.ColorTools;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using System.Threading;

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
        public float[] hues;
        [DataMember]
        public float[] pos;

        [DataMember]
        public Button injectButton;
        [DataMember]
        public Button editEquationButton;
        [DataMember]
        public Button previewButton;
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

        private void SharedConstructor(PicType type, GraphicsDevice g, GameWindow w)
        {            
            this.g = g;
            this.w = w;
            this.type = type;
            imageCancellationSource = new CancellationTokenSource();
            smallImage = GraphUtils.GetTexture(g, Color.Black);
            bigImage = GraphUtils.GetTexture(g, Color.Black);
            InitButtons();
            Trees = new AptNode[3];
            Machines = new StackMachine[3];
        }

       
        public void GenBigImage()
        {
            int chunkSize = Math.Min(64, bounds.Height);
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
                chunkSize = (int)(chunkSize* 2);
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
        
        public void GenVideoRec(GameState state, int w, int h,int index, float t)
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
                foreach (var frameData in frameDatas) {                    
                    var start = frameData.start;
                    var len = frameData.end - frameData.start;
                    videoFrames[index].SetData(0, new Rectangle(0, frameData.start, w, len), frameData.colors, 0, frameData.colors.Length);
                    index++;
                    Transition.AddProgress(1);
                }
                GenVideoRec(state, w, h, index, t);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void GenVideo(GameState state, int w, int h)
        {
            //limit to 1080p or its unbearably slow
            if (h > 1080) h = 1080;
            if (w > 1920) w = 1920;

            const int frameCount = Settings.FPS * Settings.VIDEO_LENGTH;
            Transition.StartTransition(Screen.VIDEO_PLAYING, frameCount);
            state.screen = Screen.VIDEO_GENERATING;
                        
            //Just play the video if its already there
            if (videoFrames != null) {
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
            

            GenVideoRec(state, w, h,0, -1.0f);
                                     
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
            injectButton = new Button(Settings.injectTexture, new Rectangle());
            editEquationButton = new Button(Settings.equationTexture, new Rectangle());
            saveEquationButton = new Button(Settings.saveEquationTexture, new Rectangle());
            previewButton = new Button(GraphUtils.GetTexture(g, Color.Blue), new Rectangle());
            playButton = new Button(GraphUtils.GetTexture(g, Color.Red), new Rectangle());
            exportGIFButton = new Button(GraphUtils.GetTexture(g, Color.Yellow), new Rectangle());
            exportPNGButton = new Button(GraphUtils.GetTexture(g, Color.Yellow), new Rectangle());
            cancelVideoGenButton = new Button(GraphUtils.GetTexture(g, Color.Green), new Rectangle());
            cancelEditButton = new Button(Settings.cancelEditTexture, new Rectangle());
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
                injectButton.Draw(batch, gameTime);
            }            

        }

        public void EditDraw(SpriteBatch batch, GraphicsDevice g, GameWindow w, GameTime gameTime)
        {
            batch.Draw(bigImage, bounds, Color.White);
            textBox.Draw(batch, gameTime);
            saveEquationButton.Draw(batch, gameTime);
            cancelEditButton.Draw(batch, gameTime);
        }



        public void PanelDraw(SpriteBatch batch, GameTime gameTime, InputState state, bool videoGenerating, bool videoPlaying)
        {
            panel.Draw(batch, gameTime, state);
            var panelBounds = panel.GetBounds(state);
            var leftButtonBounds = FRect(panelBounds.X + panelBounds.Width * .1f, panelBounds.Y + panelBounds.Height * .25f, panelBounds.Width * .1f, panelBounds.Height * .5f);

            if (videoGenerating)
            {
                cancelVideoGenButton.bounds = leftButtonBounds;
                cancelVideoGenButton.Draw(batch, gameTime);
                return;
            }

            editEquationButton.bounds = leftButtonBounds;
            editEquationButton.Draw(batch, gameTime);

            if (video)
            {
                previewButton.bounds = editEquationButton.bounds;
                previewButton.bounds.X += (int)(previewButton.bounds.Width * 1.1f);
                previewButton.Draw(batch, gameTime);

                playButton.bounds = previewButton.bounds;
                playButton.bounds.X += (int)(previewButton.bounds.Width * 1.1f);
                playButton.Draw(batch, gameTime);

                if (videoPlaying)
                {
                    exportGIFButton.bounds = playButton.bounds;
                    exportGIFButton.bounds.X += (int)(playButton.bounds.Width * 1.1f);
                    exportGIFButton.Draw(batch, gameTime);
                }
            }
            else
            {
                if (bigImage != null && bigImage.Width == bounds.Width && bigImage.Height == bounds.Height)
                {
                    exportPNGButton.bounds = editEquationButton.bounds;
                    exportPNGButton.bounds.X += (int)(editEquationButton.bounds.Width * 1.1f);
                    exportPNGButton.Draw(batch, gameTime);
                }
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
            batch.Draw(videoFrames[frameIndex], bounds, Color.White);
            PanelDraw(batch, gameTime, state, false,true);
        }

        public void ZoomDraw(SpriteBatch batch, GraphicsDevice g, GameWindow w, GameTime gameTime, InputState state)
        {
            batch.Draw(smallImage, bounds, Color.White);
            batch.Draw(bigImage, bounds, Color.White);
            PanelDraw(batch, gameTime, state, false,false);
        }

        public void VideoGeneratingDraw(SpriteBatch batch, GraphicsDevice g, GameWindow w, GameTime gameTime, InputState state)
        {
            batch.Draw(smallImage, bounds, Color.White);
            batch.Draw(bigImage, bounds, Color.White);
            PanelDraw(batch, gameTime, state, true,false);
        }


        public void SetNewBounds(Rectangle bounds)
        {
            this.bounds = bounds;

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
                    for (int i = 0; i < Trees.Length; i++)
                    {
                        if (Trees[i] == t)
                        {
                            Trees[i] = rootMutated;
                        }
                        t = rootMutated;
                    }
                }
                s = new StackMachine(t);
            }
            return result;
        }
          
        private async Task<TextureData> ImageGenAsync(
            int w, int h,
            int start, int end,
            float t)
        {
            var ct = imageCancellationSource.Token;
            return  await Task.Run(() =>
            {
                Color[] colors = new Color[(end-start) * w];
                switch (type)
                {
                    case PicType.RGB:
                        RGBToTexture(w, h, start, end, t, colors,ct);
                        break;
                    case PicType.HSV:
                        HSVToTexture(w, h, start, end, t, colors,ct);
                        break;
                    case PicType.GRADIENT: 
                        GradientToTexture(w, h, start, end, t, colors,ct);
                        break;
                    default:                        
                        throw new Exception("wat");
                }
                return new TextureData { colors = colors, start = start, end = end } ;
            },ct);
        }

        private void RGBToTexture(int w, int h, int start, int end, float t, Color[] colors,CancellationToken ct)
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


        private void GradientToTexture(int w, int h, int start, int end, float t, Color[] colors,CancellationToken ct)
        {
            unsafe
            {
                const float scale = 0.5f;
                var hStack = stackalloc float[Machines[0].nodeCount];
                var sStack = stackalloc float[Machines[1].nodeCount];
                var vStack = stackalloc float[Machines[2].nodeCount];

                for (int y = start; y < end; y++)
                {
                    float yf = ((float)y / (float)h) * 2.0f - 1.0f;
                    int yw = (y - start) * w;
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
                    ct.ThrowIfCancellationRequested();
                }
            }
        }

        private void HSVToTexture(int width, int height, int start, int end, float t, Color[] colors,CancellationToken ct)
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

