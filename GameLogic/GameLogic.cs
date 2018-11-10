// TODO resize big buttons onresize properly

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System;
using GameInterface;
using System.Diagnostics;
using static GameLogic.PicFunctions;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace GameLogic
{

    public class GameLogic : IGameInterface
    {
        public GameState state;
        public void SetState(GameState state)
        {
            this.state = state;
        }


        public GameState Init(GraphicsDevice g,ContentManager content)
        {
            state = new GameState();
            state.r = new Random();
            state.g = g;

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
                    var tex = Texture2D.FromStream(g, new FileStream(file.FullName, FileMode.Open));
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
                    SetNewBounds(state.pictures[index],new Rectangle((int)pos.X, (int)pos.Y, picW, picH),g);                                       
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
            state.zoomedPic.button.Draw(batch, gameTime);
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
                PicFunctions.Draw(pic, batch, gameTime);
            }
            batch.End();

            // TODO: Add your drawing code here


        }



        public GameState Update(KeyboardState keyboard, MouseState mouseState, MouseState prevMouseState, GameTime gameTime, GraphicsDevice g)
        {


            if (state.screen == Screen.CHOOSE)
            {
                return ChooseUpdate(keyboard, mouseState, prevMouseState, gameTime, g);
            }
            else if (state.screen == Screen.ZOOM)
            {
                return ZoomUpdate(keyboard, mouseState, prevMouseState, gameTime, g);
            }
            return state;
        }

        public GameState ZoomUpdate(KeyboardState keyboard, MouseState mouseState, MouseState prevMouseState, GameTime gameTime, GraphicsDevice g)
        {
            if (state.zoomedPic.button.WasRightClicked(mouseState, prevMouseState))
            {
                state.screen = Screen.CHOOSE;
                state.zoomedPic.zoomed = false;
                state.zoomedPic = null;                
                LayoutUI();
            }
            return state;
        }
        public Pic GenTree(Random r)
        {
            int chooser = r.Next(0, 3);
            if (chooser == 0)
            {
                return GenRGBPic(Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, r);                
            }
            else if (chooser == 1)
            {
                return GenHSVPic(Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, r);
                
            }
            else if (chooser == 2)
            {
                var gradients = GenGradientPic(Settings.MIN_GEN_SIZE, Settings.MAX_GEN_SIZE, r);
                return gradients;
            }
            throw new Exception("Choose");
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
            }
        }

        public GameState ChooseUpdate(KeyboardState keyboard, MouseState mouseState, MouseState prevMouseState, GameTime gameTime, GraphicsDevice g)
        {
            var r = state.r;
            if (state.reRollButton.WasLeftClicked(mouseState, prevMouseState))
            {
               
                GenTrees(r);
                LayoutUI();
            }

            if (state.evolveButton.WasLeftClicked(mouseState, prevMouseState))
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
                    
                    var child = BreedWith(first,second, state.r);
                    child = Mutate(child,state.r);                    
                    nextGeneration.Add(child);
                    
                }
                state.pictures = nextGeneration;
                LayoutUI();

            }
            for(int i = 0; i< state.pictures.Count(); i++)
            {
                var pic = state.pictures[i];
                if (pic.button.WasLeftClicked(mouseState, prevMouseState))
                {
                    pic.selected = !pic.selected;
                }

                if (pic.inject.WasLeftClicked(mouseState, prevMouseState))
                {
                    pic.button.tex.Dispose();
                    state.pictures[i] = GenTree(r);
                    SetNewBounds(state.pictures[i], pic.button.bounds, state.g);                    
                }

                if (pic.equation.WasLeftClicked(mouseState, prevMouseState))
                {
                    Console.WriteLine(pic.ToLisp());
                }

                if (pic.button.WasRightClicked(mouseState, prevMouseState))
                {
                    state.zoomedPic = pic;
                    pic.zoomed = true;
                    SetNewBounds(pic, new Rectangle(0, 0, state.g.Viewport.Width, state.g.Viewport.Height), g);
                    Console.WriteLine(state.zoomedPic.ToLisp());
                    state.screen = Screen.ZOOM;
                }
            }

            return state;
        }
    }
}

