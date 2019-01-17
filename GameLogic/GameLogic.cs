// todo - wconsider filter nodes attached to top level pic nodes (sepia, etc)
// todo - does breed handle warp properly? I think not
// todo - toggle button


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
using System.Windows.Forms;
using Svg;

namespace GameLogic
{


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

        public void LoadButtons()
        {
            DirectoryInfo d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Content");
            state.svgs = d.GetFiles("*.svg").SelectF(file => (file.Name.Replace(".svg", string.Empty), SvgDocument.Open(file.FullName)));
            ResizeButtons();
        }

        public void ResizeButtons()
        {
            if (state.buttons != null)
            {
                foreach (var tex in state.buttons.Values)
                {
                    tex.Dispose();
                }
                state.buttons.Clear();
            }
            else
            {
                state.buttons = new Dictionary<string, Texture2D>();
            }
            
            foreach (var (name, svg) in state.svgs)
            {
                var size = (int)(Math.Min(state.g.Viewport.Width, state.g.Viewport.Width) * 0.1f);
                var svgimg = svg.Draw(size,size);
                Stream stream = new MemoryStream();
                svgimg.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                state.buttons.Add(name, Texture2D.FromStream(state.g, stream));
                stream.Close();
            }
        }

        public void PickFonts(ContentManager content, GraphicsDevice g)
        {
            if (g.Viewport.Height <= 1440)
            {
                Settings.font = state.lowFont;
            }
            else
            {
                Settings.font = state.hiFont;
            }
        }

        public GameState Init(GraphicsDevice g, GameWindow window, ContentManager content)
        {
            state = new GameState();
            state.r = new Random();
            state.g = g;
            state.w = window;
            state.inputState = new InputState();
            
            
            Settings.selectedTexture = GraphUtils.GetTexture(g, Color.Cyan);
            Settings.panelTexture = GraphUtils.GetTexture(g, new Color(0.0f, 0.0f, 0.0f, 0.75f));

            
            state.lowFont = content.Load<SpriteFont>("equation-font");            
            state.hiFont = content.Load<SpriteFont>("equation-font-hi");
            
            PickFonts(content,g);
            LoadButtons();

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

            var btnSize = (int)(0.05f * Math.Min(winW,winH));
            state.undoButton = new Button("back-btn", FRect(winW * .01f, winH * .91f, btnSize, btnSize), state.buttons);
            state.reRollButton = new Button("reload-btn", FRect(winW * .201f, winH * .91f, btnSize, btnSize), state.buttons);
            state.evolveButton = new Button("dna-btn", FRect(winW * .401f, winH * .91f, btnSize, btnSize), state.buttons);            
            state.imageAddButton = new Button("image-btn", FRect(winW * .601f, winH * .91f, btnSize, btnSize), state.buttons);
            state.videoModeButton = new ToggleButton("movie-btn",state.buttons, FRect(winW * .801f, winH * .91f,btnSize,btnSize),Color.Gray,Color.White);


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
            foreach (var pic in state.pictures)
            {
                pic.imageCancellationSource.Cancel();
                pic.imageCancellationSource = new CancellationTokenSource();
            }            
            ResizeButtons();
            PickFonts(state.content,state.g);
            LayoutUI();
            
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            state.g.Clear(Color.Black);
            batch.Begin();
            var screen = state.screen;
            if (screen == Screen.VIDEO_GENERATING)
            {
                VideoGeneratingDraw(batch, gameTime);
                Transition.Draw(batch, state.g, gameTime);
            }
            else if (screen == Screen.IMAGE_ADDING)
            {
                ChooseDraw(batch, gameTime);
                ImageAdder.Draw(batch, state.g, state, gameTime);
            }
            else if (screen == Screen.GIF_EXPORTING)
            {
                state.zoomedPic.GifGeneratingDraw(batch, state.g, state.w, gameTime);
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
            batch.End();
        }


        public void EditDraw(SpriteBatch batch, GameTime gameTime)
        {
            state.zoomedPic.EditDraw(batch, state.g, state.w, gameTime);
        }

        public void ZoomDraw(SpriteBatch batch, GameTime gameTime)
        {
            state.zoomedPic.ZoomDraw(batch, state.g, state.w, gameTime, state.inputState);
        }

        public void VideoGeneratingDraw(SpriteBatch batch, GameTime gameTime)
        {
            state.zoomedPic.VideoGeneratingDraw(batch, state.g, state.w, gameTime, state.inputState);
        }

        public void VideoPlayingDraw(SpriteBatch batch, GameTime gameTime)
        {
            state.zoomedPic.VideoPlayingDraw(batch, state.g, gameTime, state.inputState);
        }


        public void ChooseDraw(SpriteBatch batch, GameTime gameTime)
        {
            var g = state.g;
            state.undoButton.Draw(batch, state.g, gameTime);
            state.reRollButton.Draw(batch, state.g, gameTime);
            state.evolveButton.Draw(batch, state.g, gameTime);
            state.imageAddButton.Draw(batch, state.g, gameTime);
            state.videoModeButton.Draw(batch, gameTime, state.videoMode);


            foreach (var pic in state.pictures)
            {
                pic.Draw(batch, state.g, state.w, gameTime, state.inputState);
            }


        }

        public GameState Update(GameTime gameTime)
        {
            if (state.screen == Screen.VIDEO_GENERATING)
            {
                Transition.Update(state);
                return VideoGeneratingUpdate(gameTime);
            }
            else if (state.screen == Screen.GIF_EXPORTING)
            {
                Transition.Update(state);
                return state;
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
            if (state.imageAddButton.WasLeftClicked(state.inputState))
            {
                state.screen = Screen.IMAGE_ADDING;
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

                    var child = first.BreedWith(second, state.r,state);
                    child = child.Mutate(state.r,state);
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
                    LayoutUI();
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
                    var p = lexer.ParsePic(state.g, state.w,state);
                    state.zoomedPic.Trees = p.Trees;
                    state.zoomedPic.Machines = p.Machines;
                    state.zoomedPic.type = p.type;
                    state.zoomedPic.colors = p.colors;
                    state.zoomedPic.colors = p.colors;
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


            if (state.zoomedPic.playButton.WasLeftClicked(state.inputState))
            {
                state.zoomedPic.GenVideo(state,Settings.VIDEO_WIDTH_LOW,Settings.VIDEO_HEIGHT_LOW);
            }

            if (state.zoomedPic.playHDButton.WasLeftClicked(state.inputState))
            {
                state.zoomedPic.GenVideo(state, Settings.VIDEO_WIDTH_HI, Settings.VIDEO_HEIGHT_HI);
            }

            if (state.zoomedPic.exportGIFButton.WasLeftClicked(state.inputState))
            {
                _ = state.zoomedPic.exportGIF(state);
            }
            if (state.zoomedPic.exportPNGButton.WasLeftClicked(state.inputState))
            {
                var pic = state.zoomedPic;
                var store = new FrameStore(1, pic.bigImage.Width, pic.bigImage.Height);
                store.PushFrame(pic.bigImage);



                Stream myStream;
                SaveFileDialog saveFileDialog1 = new SaveFileDialog();

                saveFileDialog1.Filter = "png images (*.png)|*.png";
                saveFileDialog1.FilterIndex = 0;
                saveFileDialog1.RestoreDirectory = false;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        store.ExportFrame(myStream, 0);
                        myStream.Close();
                    }
                }
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

        public Pic GenTree(Random r)
        {
            int chooser = r.Next(0, 3);
            PicType type = (PicType)chooser;
            return new Pic(type, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w, state.videoMode,state);
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

