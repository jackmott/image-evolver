// todo - handle typing beyond edge of text box
// todo - transition / hourglass animation while processing
// todo - test text box edge cases -- highlight to end, hit delete, causes exception
// todo - gradients don't have enough data to rebuild from the lisp output!
// todo - consider filter nodes attached to top level pic nodes (sepia, etc)



using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Content;
using System.Runtime.Serialization;
using System.Xml;
using static GameLogic.GraphUtils;


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

        public GameState Init(GraphicsDevice g, GameWindow window, ContentManager content)
        {
            var test = "(RGB 1 1 1";
            Lexer l = new Lexer(test);
            l.BeginLexing();
            Console.ReadLine();
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

            state.populationSize = Settings.POP_SIZE_COLUMNS * (Settings.POP_SIZE_COLUMNS - 1);            
            Random r = state.r;
            state.pictures = GenPics(r);
            LayoutUI();

            return state;
        }

        public void LayoutUI()
        {
            var g = state.g;
            int winW = g.Viewport.Width;
            int winH = g.Viewport.Height;


            state.undoButton = new Button(GetTexture(g, Color.White), FRect(winW * .01f, winH * .91f, winW * .1f, winH * 0.05f));
            state.reRollButton = new Button(GetTexture(g, Color.Blue), FRect(winW * .201f, winH * .91f, winW * .1f,winH * 0.05f));
            state.evolveButton = new Button(GetTexture(g, Color.Red), FRect(winW * .401f, winH * .91f, winW * .1f, winH * 0.05f));
            state.videoModeButton = new ToggleButton(GetTexture(g, Color.Green), GetTexture(g, Color.DarkGreen), FRect(winW * .601f, winH * .91f, winW * .1f, winH * 0.05f));


            int UISpace = (int)(winH * 0.1f);
            winH -= UISpace;

            int hSpace = (int)(winW * Settings.HORIZONTAL_SPACING);
            int vSpace = (int)(winH * Settings.VERTICAL_SPACING);

            int numPerRow = Settings.POP_SIZE_COLUMNS;
            int numPerColumn = Settings.POP_SIZE_COLUMNS - 1;
            int spaceRemaining = winW - hSpace * (numPerRow + 1);
            int picW = spaceRemaining / numPerRow;

            spaceRemaining = winH - vSpace * (numPerColumn + 1);
            int picH = spaceRemaining / numPerColumn;

            var pos = new Vector2(0, 0);
            int index = 0;
            for (int y = 0; y < Settings.POP_SIZE_COLUMNS - 1; y++)
            {
                pos.Y += vSpace;
                for (int x = 0; x < Settings.POP_SIZE_COLUMNS; x++)
                {
                    pos.X += hSpace;
                    if (state.pictures[index] != state.zoomedPic)
                    {
                        state.pictures[index].SetNewBounds(new Rectangle((int)pos.X, (int)pos.Y, picW, picH), g);
                    }
                    else
                    {
                        state.zoomedPic.SetNewBounds(new Rectangle(0, 0, state.g.Viewport.Width, state.g.Viewport.Height), state.g);
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

            if (state.screen == Screen.CHOOSE)
            {
                ChooseDraw(batch, gameTime);
            }
            else if (state.screen == Screen.ZOOM)
            {
                ZoomDraw(batch, gameTime);
            }
            else if (state.screen == Screen.EDIT)
            {
                EditDraw(batch, gameTime);
            }

        }


        public void EditDraw(SpriteBatch batch, GameTime gameTime)
        {
            state.g.Clear(Color.Black);
            batch.Begin();
            state.zoomedPic.EditDraw(batch, gameTime);
            batch.End();
        }

        public void ZoomDraw(SpriteBatch batch, GameTime gameTime)
        {
            state.g.Clear(Color.Black);
            batch.Begin();
            state.zoomedPic.ZoomDraw(batch, gameTime,state.inputState);
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
            state.videoModeButton.Draw(batch, gameTime,state.videoMode);
            

            foreach (var pic in state.pictures)
            {
                pic.Draw(batch, gameTime,state.inputState);
            }
            batch.End();

        }

        public GameState Update(GameTime gameTime)
        {

            if (state.screen == Screen.CHOOSE)
            {
                return ChooseUpdate(gameTime);
            }
            else if (state.screen == Screen.ZOOM)
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
                if (!state.pictures.Exists(p => p.selected))
                {
                    //no pics selected
                    return state;
                }
              
                // Build the next generation of pictures
                ClearPics(state.prevPictures);
                state.prevPictures = state.pictures; // save the current for undo
                var nextGeneration = new List<Pic>(state.pictures.Count());
                var breeders = state.pictures.Where(p => p.selected).ToArray();                    
                // Keep adding new pictures by breeding random combos of the selected ones
                while (nextGeneration.Count() < state.pictures.Count())
                {
                    var first = breeders[state.r.Next(0, breeders.Length)];
                    var second = breeders[state.r.Next(0, breeders.Length)];

                    var child = first.BreedWith(second, state.r);
                    child = child.Mutate(state.r);
                    child.Optimize();
                    nextGeneration.Add(child);

                }
                state.pictures = nextGeneration;
                LayoutUI();

            }
            for (int i = 0; i < state.pictures.Count(); i++)
            {
                var pic = state.pictures[i];

                if (pic.picButton.WasLeftClicked(state.inputState))
                {
                    pic.selected = !pic.selected;
                }

                if (pic.injectButton.WasLeftClicked(state.inputState))
                {
                    pic.picButton.tex.Dispose();
                    state.pictures[i] = GenTree(r);
                    state.pictures[i].SetNewBounds(pic.picButton.bounds, state.g);
                }

                if (pic.picButton.WasRightClicked(state.inputState))
                {
                    state.zoomedPic = pic;
                    pic.zoomed = true;
                    pic.SetNewBounds(new Rectangle(0, 0, state.g.Viewport.Width, state.g.Viewport.Height), state.g);
                    Console.WriteLine(state.zoomedPic.ToLisp());
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
                    state.zoomedPic.RegenTex(state.g);
                    state.screen = Screen.ZOOM;
                    state.zoomedPic.textBox.SetActive(false);
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
            if (state.zoomedPic.constantFoldButton.WasLeftClicked(state.inputState))
            {
                for (int i = 0; i < state.zoomedPic.Trees.Length; i++)
                {
                    state.zoomedPic.Trees[i] = AptNode.ConstantFolding(state.zoomedPic.Trees[i]);
                    state.zoomedPic.Machines[i] = new StackMachine(state.zoomedPic.Trees[i]);
                }
                state.zoomedPic.RegenTex(state.g);
                state.zoomedPic.textBox.SetText(state.zoomedPic.ToLisp());
                
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

            if (state.zoomedPic.picButton.WasRightClicked(state.inputState))
            {
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
            if (chooser == 0)
            {
                p = new Pic(PicType.RGB, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w,state.videoMode);
            }
            else if (chooser == 1)
            {
                p = new Pic(PicType.HSV, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w, state.videoMode);
            }
            else
            {
                p = new Pic(PicType.GRADIENT, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w, state.videoMode);
            }

            for (int i = 0; i < p.Trees.Count(); i++)
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
            Pic p;
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

            return p;
        }

        public void ClearPics(List<Pic> pics)
        {
            if (pics == null) return;
            foreach (var pic in pics)
            {
                if (pic.picButton.tex != null)
                {
                    pic.picButton.tex.Dispose();
                    pic.picButton.tex = null;
                }                
            }
            pics.Clear();
        }

        public List<Pic> GenPics(Random r)
        {

            List<Pic> pics = new List<Pic>(4);
            for (int i = 0; i < state.populationSize; i++)
            {
                pics.Add(GenTree(r));                
            }
            return pics;
        }

      
    }
}

