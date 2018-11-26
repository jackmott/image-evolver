﻿// TODO resize big buttons onresize properly

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Content;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Xml;

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
            state = new GameState();
            state.r = new Random();
            state.g = g;
            state.w = window;
            state.inputState = new InputState();
            Settings.injectTexture = GraphUtils.GetTexture(g, Color.Blue);
            Settings.selectedTexture = GraphUtils.GetTexture(g, Color.Yellow);
            Settings.equationTexture = GraphUtils.GetTexture(g, Color.White);

            Settings.equationFont = content.Load<SpriteFont>("equation-font");

            int w = g.Viewport.Width;
            int h = g.Viewport.Height;
            state.evolveButton = new Button(GraphUtils.GetTexture(g, Color.Red), new Rectangle((int)(w * .001f), (int)(h * .91f), (int)(w * .1f), (int)(h * 0.05f)));
            state.reRollButton = new Button(GraphUtils.GetTexture(g, Color.Blue), new Rectangle((int)(w * .2f), (int)(h * .91f), (int)(w * .1f), (int)(h * 0.05f)));
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
                    ExternalImage img = new ExternalImage { data = colors, w = tex.Width, h = tex.Height };
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
            state.pictures = new List<Pic>();
            Random r = state.r;
            GenTrees(r);
            LayoutUI();

            return state;
        }

        public void LayoutUI()
        {
            var g = state.g;
            int winW = g.Viewport.Width;
            int winH = g.Viewport.Height;

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
                    state.pictures[index].SetNewBounds(new Rectangle((int)pos.X, (int)pos.Y, picW, picH), g);
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
        }

        public void ZoomDraw(SpriteBatch batch, GameTime gameTime)
        {


            state.g.Clear(Color.Black);
            batch.Begin();
            state.zoomedPic.ZoomDraw(batch, gameTime);
            batch.End();
            // TODO: Add your drawing code here
        }


        public void ChooseDraw(SpriteBatch batch, GameTime gameTime)
        {
            var g = state.g;

            g.Clear(Color.Black);
            batch.Begin();
            state.evolveButton.Draw(batch, gameTime);
            state.reRollButton.Draw(batch, gameTime);

            foreach (var pic in state.pictures)
            {
                pic.Draw(batch, gameTime);
            }
            batch.End();

            // TODO: Add your drawing code here


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

            return state;
        }

        public GameState ZoomUpdate(GameTime gameTime)
        {
            if (state.zoomedPic.equation.WasLeftClicked(state.inputState))
            {
                state.zoomedPic.textBox.SetActive(true);
            }
            if (state.zoomedPic.textBox.IsActive())
            {
                state.zoomedPic.textBox.Update(state.inputState, gameTime);
            }
            if (state.zoomedPic.button.WasRightClicked(state.inputState))
            {
                state.screen = Screen.CHOOSE;
                state.zoomedPic.zoomed = false;
                state.zoomedPic = null;
                LayoutUI();
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
            while (root.AddLeaf(new AptNode { type = NodeType.CONSTANT, value = (float)r.NextDouble()*2.0f-1.0f})) { }
            //while (root.AddLeaf(new AptNode { type = NodeType.CONSTANT, value = 0.5f })) { }

            int chooser = r.Next(0, 3);
            Pic p;
            if (chooser == 0)
            {
                p = new Pic(PicType.RGB, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w);                
            }
            else if (chooser == 1)
            {
                p = new Pic(PicType.HSV, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w);
            }
            else
            {
                p = new Pic(PicType.GRADIENT, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w);
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
                p = new Pic(PicType.RGB, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE,state.g,state.w);
            }
            else if (chooser == 1)
            {
                p = new Pic(PicType.HSV, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w);
            }
            else
            {
                p = new Pic(PicType.GRADIENT, r, Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, state.g, state.w);
            }
            
            return p;
        }
        public void GenTrees(Random r)
        {
            foreach (var pic in state.pictures)
            {
                if (pic.button.tex != null)
                {
                    pic.button.tex.Dispose();
                    pic.button.tex = null;
                }
            }
            state.pictures.Clear();

            for (int i = 0; i < state.populationSize; i++)
            {
                state.pictures.Add(GenTree(r));
                //state.pictures[i].RangeTest();
            }
        }

        public GameState ChooseUpdate(GameTime gameTime)
        {
            var r = state.r;
            if (state.reRollButton.WasLeftClicked(state.inputState))
            {
                GenTrees(r);
                LayoutUI();
            }

            if (state.evolveButton.WasLeftClicked(state.inputState))
            {
                if (!state.pictures.Exists(p => p.selected))
                {
                    //no pics selected
                    return state;
                }
                // Clear out all textures as they are about to get updated
                foreach (var pic in state.pictures)
                {
                    if (pic.button.tex != null)
                        pic.button.tex.Dispose();
                    pic.button.tex = null;
                }

                // Build the next generation of pictures
                List<Pic> nextGeneration = new List<Pic>(state.pictures.Count());
                foreach (var old in state.pictures)
                {
                    //Use only the selected images to breed the new
                    if (old.selected)
                    {
                        nextGeneration.Add(old);
                        old.selected = false;
                    }
                }
                int selectedCount = nextGeneration.Count();
                if (selectedCount == 0) return state;
                // Keep adding new pictures by breeding random combos of the selected ones
                while (nextGeneration.Count() < state.pictures.Count())
                {
                    var first = nextGeneration[state.r.Next(0, selectedCount)];
                    var second = nextGeneration[state.r.Next(0, selectedCount)];

                    var child = first.BreedWith(second, state.r);
                    child = child.Mutate(state.r);
                    nextGeneration.Add(child);

                }
                state.pictures = nextGeneration;
                LayoutUI();

            }
            for (int i = 0; i < state.pictures.Count(); i++)
            {
                var pic = state.pictures[i];

                if (pic.button.WasLeftClicked(state.inputState))
                {
                    pic.selected = !pic.selected;
                }

                if (pic.inject.WasLeftClicked(state.inputState))
                {
                    pic.button.tex.Dispose();
                    state.pictures[i] = GenTree(r);
                    state.pictures[i].SetNewBounds(pic.button.bounds, state.g);
                }

                if (pic.equation.WasLeftClicked(state.inputState))
                {
                    pic.showEquation = !pic.showEquation;
                    Console.WriteLine(pic.ToLisp());
                }

                if (pic.button.WasRightClicked(state.inputState))
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
    }
}

