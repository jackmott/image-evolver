// todo - handle typing beyond edge of text box
// todo - consider filter nodes attached to top level pic nodes (sepia, etc)
// todo - does breed handle warp properly? I think not


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using JM.LinqFaster;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Content;
using System.Runtime.Serialization;
using System.Xml;
using static GameLogic.GraphUtils;
using System.Threading;

namespace GameLogic
{
    //Used when transitioning from one state to another that will take time
    public static class Transition
    {
        //1 to 100000                
        public static int progress;
        public static int goal;        
        public static Screen nextScreen;


        public static void StartTransition(Screen to, int thegoal)
        {                        
            nextScreen = to;
            progress = 0;
            goal = thegoal;
        }

        public static void AddProgress(int amount)
        {
            Interlocked.Add(ref progress, amount);
        }

        public static void Update(GameState state)
        {
            if (progress == goal)
            {
                state.screen = nextScreen;
            }
        }
        public static void Draw(SpriteBatch b, GraphicsDevice g, GameTime gametime)
        {
            int winW = g.Viewport.Width;
            int winH = g.Viewport.Height;
            float pct = (float)progress/(float)goal;
            
            Rectangle rect = CenteredRect(new Rectangle(0, 0, winW, winH), winW / 4, winH / 20);
            b.Begin();            
            ProgressBar.Draw(b, g, rect, Color.Cyan, Color.Blue, pct);
            b.End();
        }
    }

    public class GameLogic
    {
        public GameState state;


        public GameState SetState(string xml)
        {
            using (XmlReader reader = XmlReader.Create(new StringReader(xml)))
            {
                DataContractSerializer formatter0 =
                    new DataContractSerializer(typeof(GameState));
                state = (GameState)formatter0.ReadObject(reader);
                return state;
            }


        }

        public GameState Init(GraphicsDevice g, GameWindow window, ContentManager content)
        {
            state = new GameState();
            state.r = new Random();
            state.g = g;
            state.w = window;
            state.inputState = new InputState();
            Settings.injectTexture = GraphUtils.GetTexture(g, Color.Blue);
            Settings.selectedTexture = GraphUtils.GetTexture(g, Color.Yellow);
            Settings.equationTexture = GraphUtils.GetTexture(g, Color.White);
            Settings.saveEquationTexture = GraphUtils.GetTexture(g, Color.Green);
            Settings.cancelEditTexture = GraphUtils.GetTexture(g, Color.Red);
            Settings.equationFont = content.Load<SpriteFont>("equation-font");
            Settings.panelTexture = GraphUtils.GetTexture(g, new Color(0.0f, 0.0f, 0.0f, 0.5f));


            DirectoryInfo d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Assets");
            var files = d.GetFiles("*.jpg").AsEnumerable().Concat(d.GetFiles("*.png"));
            GameState.externalImages = new List<ExternalImage>();

            foreach (var file in files)
            {
                try
                {
                    var fs = new FileStream(file.FullName, FileMode.Open);
                    var tex = Texture2D.FromStream(g, fs);
                    fs.Close();
                    Color[] colors = new Color[tex.Width * tex.Height];
                    tex.GetData(colors);
                    ExternalImage img = new ExternalImage { filename = file.Name, data = colors, w = tex.Width, h = tex.Height };
                    GameState.externalImages.Add(img);
                    tex.Dispose();
                }
                catch (Exception e)
                {
                    //do something
                    throw e;
                }
            }

            //Tests.BreedingPairs(g, window);
            //Tests.BreedingSelf(g, window);
            //Tests.PicGenerate(g, window,state,this);
            //Console.ReadLine();
            //Parsing(g, window);
            //Optimizing(g, window);
            //Console.ReadLine();


            state.populationSize = Settings.POP_SIZE_COLUMNS * (Math.Max(Settings.POP_SIZE_COLUMNS - 1, 1));
            Random r = state.r;
            state.pictures = GenPics(r);
            LayoutUI();

            return state;
        }

        public void LayoutUI()
        {
            Console.WriteLine("layout ui");
            var g = state.g;
            int winW = g.Viewport.Width;
            int winH = g.Viewport.Height;


            state.undoButton = new Button(GetTexture(g, Color.White), FRect(winW * .01f, winH * .91f, winW * .1f, winH * 0.05f));
            state.reRollButton = new Button(GetTexture(g, Color.Blue), FRect(winW * .201f, winH * .91f, winW * .1f, winH * 0.05f));
            state.evolveButton = new Button(GetTexture(g, Color.Red), FRect(winW * .401f, winH * .91f, winW * .1f, winH * 0.05f));
            state.videoModeButton = new ToggleButton(GetTexture(g, Color.Green), GetTexture(g, Color.DarkGreen), FRect(winW * .601f, winH * .91f, winW * .1f, winH * 0.05f));


            int UISpace = (int)(winH * 0.1f);
            winH -= UISpace;

            int hSpace = (int)(winW * Settings.HORIZONTAL_SPACING);
            int vSpace = (int)(winH * Settings.VERTICAL_SPACING);

            int numPerRow = Settings.POP_SIZE_COLUMNS;
            int numPerColumn = Math.Max(1, Settings.POP_SIZE_COLUMNS - 1);
            int spaceRemaining = winW - hSpace * (numPerRow + 1);
            int picW = spaceRemaining / numPerRow;

            spaceRemaining = winH - vSpace * (numPerColumn + 1);
            int picH = spaceRemaining / numPerColumn;

            var pos = new Vector2(0, 0);
            int index = 0;
            for (int y = 0; y < numPerColumn; y++)
            {
                pos.Y += vSpace;
                for (int x = 0; x < Settings.POP_SIZE_COLUMNS; x++)
                {
                    pos.X += hSpace;
                    if (state.pictures[index] != state.zoomedPic)
                    {
                        var newBounds = new Rectangle((int)pos.X, (int)pos.Y, picW, picH);

                        if (state.pictures[index].bounds != newBounds)
                        {
                            state.pictures[index].SetNewBounds(newBounds);
                            _ = state.pictures[index].GenSmallImageAsync();
                        }
                    }
                    else
                    {
                        var newBounds = new Rectangle(0, 0, state.g.Viewport.Width, state.g.Viewport.Height);
                        if (state.zoomedPic.bounds != newBounds)
                        {
                            state.zoomedPic.SetNewBounds(newBounds);
                            state.zoomedPic.GenBigImage();
                        }
                    }
                    index++;
                    pos.X += picW;

                }
                pos.Y += picH;
                pos.X = 0;
            }


        }

        public void OnResize()
        {
            Console.WriteLine("resize");
            LayoutUI();
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            var screen = state.screen;
            if (screen == Screen.VIDEO_GENERATING)
            {                
                VideoGeneratingDraw(batch, gameTime);
                Transition.Draw(batch, state.g, gameTime);
            }
            else if (state.screen == Screen.CHOOSE)
            {
                ChooseDraw(batch, gameTime);
            }
            else if (state.screen == Screen.ZOOM)
            {
                ZoomDraw(batch, gameTime);
            }
            else if (state.screen == Screen.VIDEO_PLAYING)
            {
                VideoPlayingDraw(batch, gameTime);
            }
            else if (state.screen == Screen.EDIT)
            {
                EditDraw(batch, gameTime);
            }

        }


        public void EditDraw(SpriteBatch batch, GameTime gameTime)
        {
            batch.Begin();
            state.zoomedPic.EditDraw(batch, state.g, state.w, gameTime);
            batch.End();
        }

        public void ZoomDraw(SpriteBatch batch, GameTime gameTime)
        {
            batch.Begin();
            state.zoomedPic.ZoomDraw(batch, state.g, state.w, gameTime, state.inputState);
            batch.End();
        }

        public void VideoGeneratingDraw(SpriteBatch batch, GameTime gameTime)
        {
            batch.Begin();
            state.zoomedPic.VideoGeneratingDraw(batch, state.g, state.w, gameTime, state.inputState);
            batch.End();
        }

        public void VideoPlayingDraw(SpriteBatch batch, GameTime gameTime)
        {
            batch.Begin();
            state.zoomedPic.VideoPlayingDraw(batch, gameTime, state.inputState);
            batch.End();
        }


        public void ChooseDraw(SpriteBatch batch, GameTime gameTime)
        {
            var g = state.g;

            g.Clear(Color.Black);
            batch.Begin();
            state.undoButton.Draw(batch, gameTime);
            state.reRollButton.Draw(batch, gameTime);
            state.evolveButton.Draw(batch, gameTime);
            state.videoModeButton.Draw(batch, gameTime, state.videoMode);


            foreach (var pic in state.pictures)
            {
                pic.Draw(batch, state.g, state.w, gameTime, state.inputState);
            }
            batch.End();

        }

        public GameState Update(GameTime gameTime)
        {
            if (state.screen == Screen.VIDEO_GENERATING)
            {
                Transition.Update(state);
                return VideoGeneratingUpdate(gameTime);                
            }
            else if (state.screen == Screen.CHOOSE)
            {
                return ChooseUpdate(gameTime);
            }
            else if (state.screen == Screen.ZOOM || state.screen == Screen.VIDEO_PLAYING)
            {
                return ZoomUpdate(gameTime);
            }
            else if (state.screen == Screen.EDIT)
            {
                return EditUpdate(gameTime);
            }

            return state;
        }

        public GameState ChooseUpdate(GameTime gameTime)
        {
            var r = state.r;

            if (state.undoButton.WasLeftClicked(state.inputState))
            {
                if (state.prevPictures != null)
                {
                    var temp = state.pictures;
                    state.pictures = state.prevPictures;
                    state.prevPictures = temp;
                }
            }
            if (state.reRollButton.WasLeftClicked(state.inputState))
            {            
                ClearPics(state.prevPictures);
                state.prevPictures = state.pictures;
                state.pictures = null;
                state.pictures = GenPics(r);
                LayoutUI();
            }
            if (state.videoModeButton.WasLeftClicked(state.inputState))
            {
                state.videoMode = !state.videoMode;
                ClearPics(state.prevPictures);
                state.prevPictures = state.pictures;
                state.pictures = GenPics(r);
                LayoutUI();
            }

            if (state.evolveButton.WasLeftClicked(state.inputState))
            {
                if (!Array.Exists(state.pictures, p => p.selected))
                {
                    //no pics selected
                    return state;
                }

                // Build the next generation of pictures
                ClearPics(state.prevPictures);
                state.prevPictures = state.pictures; // save the current for undo
                var nextGeneration = new Pic[state.pictures.Length];
                var breeders = state.pictures.WhereF(p => p.selected);
                // Keep adding new pictures by breeding random combos of the selected ones
                int nextGenIndex = 0;
                while (nextGenIndex < state.pictures.Length)
                {
                    var first = breeders[state.r.Next(0, breeders.Length)];
                    var second = breeders[state.r.Next(0, breeders.Length)];

                    var child = first.BreedWith(second, state.r);
                    child = child.Mutate(state.r);
                    child.Optimize();
                    child.textBox.SetText(child.ToLisp());
                    nextGeneration[nextGenIndex] = child;
                    nextGenIndex++;

                }
                state.pictures = nextGeneration;
                LayoutUI();

            }
            for (int i = 0; i < state.pictures.Length; i++)
            {
                var pic = state.pictures[i];

                if (pic.WasLeftClicked(state.inputState))
                {
                    pic.selected = !pic.selected;
                }

                if (pic.injectButton.WasLeftClicked(state.inputState))
                {
                    state.pictures[i] = GenTree(r);
                }

                if (pic.WasRightClicked(state.inputState))
                {
                    state.zoomedPic = pic;
                    pic.zoomed = true;
                    pic.SetNewBounds(new Rectangle(0, 0, state.g.Viewport.Width, state.g.Viewport.Height));
                    pic.GenBigImage();
                    state.screen = Screen.ZOOM;
                }
            }

            return state;
        }

        public GameState EditUpdate(GameTime gameTime)
        {
            state.zoomedPic.textBox.Update(state.inputState, gameTime);
            if (state.zoomedPic.cancelEditButton.WasLeftClicked(state.inputState))
            {

                state.screen = Screen.ZOOM;
                state.zoomedPic.textBox.SetActive(false);
                state.zoomedPic.textBox.SetText(state.zoomedPic.ToLisp());
                state.zoomedPic.textBox.error = null;
                return state;
            }
            if (state.zoomedPic.saveEquationButton.WasLeftClicked(state.inputState))
            {
                // todo parse and render new image
                Lexer lexer = new Lexer(state.zoomedPic.textBox.rawContents);
                try
                {
                    lexer.BeginLexing();
                    var p = lexer.ParsePic(state.g, state.w);
                    state.zoomedPic.Trees = p.Trees;
                    state.zoomedPic.Machines = p.Machines;
                    state.zoomedPic.type = p.type;
                    state.zoomedPic.hues = p.hues;
                    state.zoomedPic.pos = p.pos;
                    state.zoomedPic.textBox.SetText(state.zoomedPic.ToLisp());
                    state.screen = Screen.ZOOM;
                    state.zoomedPic.textBox.SetActive(false);
                    state.zoomedPic.ClearVideo();
                    _ = state.zoomedPic.GenSmallImageAsync();
                    state.zoomedPic.GenBigImage();
                }
                catch (ParseException ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine(ex.token.type);
                    Console.WriteLine("start:" + ex.token.start + " len:" + ex.token.len);
                    state.zoomedPic.textBox.error = ex;
                    return state;
                }

                state.zoomedPic.textBox.error = null;
            }

            return state;
        }

        public GameState VideoGeneratingUpdate(GameTime gameTime)
        {
            if (state.zoomedPic.cancelVideoGenButton.WasLeftClicked(state.inputState))
            {
                state.zoomedPic.imageCancellationSource.Cancel();
                state.zoomedPic.ClearVideo();
                state.zoomedPic.imageCancellationSource = new CancellationTokenSource();
                state.screen = Screen.ZOOM;
            }
            return state;
        }
        public GameState ZoomUpdate(GameTime gameTime)
        {


            if (state.zoomedPic.editEquationButton.WasLeftClicked(state.inputState))
            {
                state.screen = Screen.EDIT;
                state.zoomedPic.textBox.cursorPos = new Point(0, 0);
                state.zoomedPic.textBox.SetActive(true);
                return state;
            }

            if (state.zoomedPic.previewButton.WasLeftClicked(state.inputState))
            {                
                state.zoomedPic.GenVideo(state,Settings.PREVIEW_VIDEO_WIDTH, Settings.PREVIEW_VIDEO_HEIGHT);
            }

            if (state.zoomedPic.playButton.WasLeftClicked(state.inputState))
            {                
                state.zoomedPic.GenVideo(state, state.zoomedPic.bounds.Width, state.zoomedPic.bounds.Height);
            }

            if (state.zoomedPic.WasRightClicked(state.inputState))
            {
                state.zoomedPic.imageCancellationSource.Cancel();
                state.zoomedPic.imageCancellationSource = new CancellationTokenSource();
                state.screen = Screen.CHOOSE;
                state.zoomedPic.zoomed = false;
                state.zoomedPic = null;
                LayoutUI();
                return state;
            }



            return state;
        }


        // for testing
        public Pic GenTree(NodeType type, Random r)
        {
            AptNode root = AptNode.MakeNode(type);
            if (root.children.Length >= 2)
            {
                root.AddLeaf(new AptNode { type = NodeType.X });
                root.AddLeaf(new AptNode { type = NodeType.Y });
            }
            while (root.AddLeaf(new AptNode { type = NodeType.CONSTANT, value = (float)r.NextDouble() * 2.0f - 1.0f })) { }
            //while (root.AddLeaf(new AptNode { type = NodeType.CONSTANT, value = 0.5f })) { }

            int chooser = r.Next(0, 3);
            Pic p;
            // chooser = 0;
            if (chooser == 0)
            {
                p = new Pic(PicType.RGB, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w, state.videoMode);
            }
            else if (chooser == 1)
            {
                p = new Pic(PicType.HSV, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w, state.videoMode);
            }
            else
            {
                p = new Pic(PicType.GRADIENT, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w, state.videoMode);
            }

            for (int i = 0; i < p.Trees.Length; i++)
            {
                p.Trees[i] = root;
                p.Machines[i] = new StackMachine(root);
            }
            p.SetupTextbox();

            return p;


        }

        public Pic GenTree(Random r)
        {
            int chooser = r.Next(0, 3);            
            PicType type = (PicType)chooser;

            return new Pic(type, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w, state.videoMode);
        }

        public void ClearPics(Pic[] pics)
        {
            if (pics == null) return;
            for (int i = 0; i < pics.Length; i++)
            {
                if (pics[i] != null)
                {
                    pics[i].imageCancellationSource.Cancel();
                    pics[i].Dispose();
                    pics[i] = null;
                }
            }

        }

        public Pic[] GenPics(Random r)
        {

            Pic[] pics = new Pic[state.populationSize];
            for (int i = 0; i < state.populationSize; i++)
            {
                pics[i] = GenTree(r);
            }
            return pics;
        }


    }
}

