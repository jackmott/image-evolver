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

namespace GameLogic
{

    public class GameLogic : IGameInterface
    {
        public GameState state;
        public void SetState(GameState state)
        {
            this.state = state;
        }

        private const float MOVE_SPEED = 0.05f;
        private const float JUMP_DURATION = 1000.0f;
        private const float JUMP_SPEED = 0.02f;

        private Button evolveButton;
        private Button reRollButton;

        public void Draw(GraphicsDevice g, SpriteBatch batch, GameTime gameTime)
        {
            if (state.screen == Screen.CHOOSE)
            {
                ChooseDraw(g, batch, gameTime);
            }
            else if (state.screen == Screen.ZOOM)
            {
                ZoomDraw(g, batch, gameTime);
            }
        }

        public void ZoomDraw(GraphicsDevice g, SpriteBatch batch, GameTime gameTime)
        {
            int winW = g.Viewport.Width;
            int winH = g.Viewport.Height;


            g.Clear(Color.Black);
            batch.Begin();
            var pos = new Vector2(0, 0);
            state.zoomedPic.button.bounds = new Rectangle((int)pos.X, (int)pos.Y, winW, winH);
            GetTex(state.zoomedPic, state.externalImages, g, winW, winH);
            state.zoomedPic.button.Draw(batch, gameTime);
            batch.End();
            // TODO: Add your drawing code here
        }


        public void ChooseDraw(GraphicsDevice g, SpriteBatch batch, GameTime gameTime)
        {
            int winW = g.Viewport.Width;
            int winH = g.Viewport.Height;

            int UISpace = (int)(winH * 0.1f);
            winH -= UISpace;

            int hSpace = (int)(winW * GameState.HORIZONTAL_SPACING);
            int vSpace = (int)(winH * GameState.VERTICAL_SPACING);

            int numPerRow = (int)Math.Sqrt(state.populationSize);
            int spaceRemaining = winW - hSpace * (numPerRow + 1);
            int picW = spaceRemaining / numPerRow;

            spaceRemaining = winH - vSpace * (numPerRow + 1);
            int picH = spaceRemaining / numPerRow;


            g.Clear(Color.Black);
            batch.Begin();
            evolveButton.Draw(batch, gameTime);
            reRollButton.Draw(batch, gameTime);
            var pos = new Vector2(0, 0);
            int index = 0;
            for (int y = 0; y < numPerRow; y++)
            {
                pos.Y += vSpace;
                for (int x = 0; x < numPerRow; x++)
                {
                    pos.X += hSpace;
                    if (state.pictures[index].selected)
                    {
                        var borderPos = pos;
                        borderPos.X -= hSpace / 2.0f;
                        borderPos.Y -= vSpace / 2.0f;
                        batch.Draw(GraphUtils.GetTexture(batch), new Rectangle((int)borderPos.X, (int)borderPos.Y, picW + hSpace, picH + vSpace), Color.Blue);
                    }

                    state.pictures[index].button.bounds = new Rectangle((int)pos.X, (int)pos.Y, picW, picH);
                    GetTex(state.pictures[index], state.externalImages, g, picW, picH);
                    state.pictures[index].button.Draw(batch, gameTime);
                    index++;
                    pos.X += picW;

                }
                pos.Y += picH;
                pos.X = 0;
            }

            batch.End();

            // TODO: Add your drawing code here


        }



        public GameState Init(GraphicsDevice g, SpriteBatch batch)
        {
            state = new GameState();
            state.r = new Random();
            int w = g.Viewport.Width;
            int h = g.Viewport.Height;
            evolveButton = new Button(GraphUtils.GetTexture(batch), new Rectangle((int)(w * .001f), (int)(h * .91f), (int)(w * .1f), (int)(h * 0.05f)));
            reRollButton = new Button(GraphUtils.GetTexture(batch), new Rectangle((int)(w * .2f), (int)(h * .91f), (int)(w * .1f), (int)(h * 0.05f)));
            DirectoryInfo d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Assets");
            var files = d.GetFiles("*.jpg").AsEnumerable().Concat(d.GetFiles("*.png"));
            state.externalImages = new List<ExternalImage>();
            foreach (var file in files)
            {
                try
                {
                    var tex = Texture2D.FromStream(g, new FileStream(file.FullName, FileMode.Open));
                    Color[] colors = new Color[tex.Width * tex.Height];
                    tex.GetData(colors);
                    ExternalImage img = new ExternalImage { data = colors, w = tex.Width, h = tex.Height };
                    state.externalImages.Add(img);
                    tex.Dispose();
                }
                catch (Exception e)
                {
                    //do something
                    throw e;
                }
            }

            state.populationSize = 36;
            state.pictures = new List<Pic>();
            Random r = state.r;
            for (int i = 0; i < state.populationSize; i++)
            {

                /*
                var node = new AptNode { type = NodeType.PICTURE, value=2,children = new AptNode[2] };
                node.children[0] = new AptNode { type = NodeType.X };
                node.children[1] = new AptNode { type = NodeType.Y };
                node.InsertWarp(r);
                                                
                    var tree = new RGBTree();
                    tree.RTree = node;
                    tree.GTree = node;
                    tree.BTree = node;
                    tree.RSM = new StackMachine(node,state.externalImages);
                    tree.GSM = new StackMachine(node, state.externalImages);
                    tree.BSM = new StackMachine(node, state.externalImages);
                    state.pictures.Add(tree);
                Console.WriteLine(tree.ToLisp());
                    
                */

                int chooser = r.Next(0, 2);
                if (chooser == 0)
                {
                    var rgbTree = new RGBTree(1, 8, r, state.externalImages);
                    state.pictures.Add(rgbTree);
                }
                else
                {
                    var hsvTree = new HSVTree(1, 8, r, state.externalImages);
                    state.pictures.Add(hsvTree);
                }

            }

            /*
            float min = float.MaxValue;
            float max = float.MinValue;
            float[] stack = new float[10];
            for (float y =  -1.0f; y <= 1.0f; y = y + 0.001f)
            {
                for (float x = -1.0f; x <= 1.0f; x+= 0.001f)
                {
                    var pic = (RGBTree)state.pictures[0];
                    var f = pic.BSM.Execute(x, y, stack);
                    if (f < min) min = f;
                    if (f > max) max = f;

                    pic = (RGBTree)state.pictures[2];
                    f = pic.BSM.Execute(x, y, stack);
                    if (f < min) min = f;
                    if (f > max) max = f;

                    pic = (RGBTree)state.pictures[3];
                    f = pic.BSM.Execute(x, y, stack);
                    if (f < min) min = f;
                    if (f > max) max = f;

                    pic = (RGBTree)state.pictures[4];
                    f = pic.BSM.Execute(x, y, stack);
                    if (f < min) min = f;
                    if (f > max) max = f;

                    pic = (RGBTree)state.pictures[5];
                    f = pic.BSM.Execute(x, y, stack);
                    if (f < min) min = f;
                    if (f > max) max = f;
                }
            }
            Console.WriteLine("min:" + min + " max:" + max + " range:"+ (max - min));
       */

            return state;
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
                state.zoomedPic = null;
            }
            return state;
        }

        public GameState ChooseUpdate(KeyboardState keyboard, MouseState mouseState, MouseState prevMouseState, GameTime gameTime, GraphicsDevice g)
        {
            var r = state.r;
            if (reRollButton.WasLeftClicked(mouseState, prevMouseState))
            {
                for (int i = 0; i < state.pictures.Count(); i++)
                {
                    state.pictures[i].button.tex.Dispose();
                    state.pictures[i].button.tex = null;

                    int chooser = r.Next(0, 2);
                    if (chooser == 0)
                    {
                        var rgbTree = new RGBTree(1, 8, r, state.externalImages);
                        state.pictures[i] = rgbTree;
                    }
                    else
                    {
                        var hsvTree = new HSVTree(1, 8, r, state.externalImages);
                        state.pictures[i] = hsvTree;
                    }
                }
            }

            if (evolveButton.WasLeftClicked(mouseState, prevMouseState))
            {

                foreach (var pic in state.pictures)
                {
                    pic.button.tex.Dispose();
                    pic.button.tex = null;
                }

                List<Pic> nextGeneration = new List<Pic>(state.pictures.Count());
                foreach (var old in state.pictures)
                {
                    if (old.selected)
                    {
                        nextGeneration.Add(old);
                        old.selected = false;
                    }
                }
                int selectedCount = nextGeneration.Count();
                if (selectedCount == 0) return state;
                while (nextGeneration.Count() < state.pictures.Count())
                {
                    var first = nextGeneration[state.r.Next(0, selectedCount)];
                    var second = nextGeneration[state.r.Next(0, selectedCount)];
                    var chooser = state.r.Next(0, 3);
                    if (chooser == 0)
                    {
                        var clone = first.Clone();
                        clone.Mutate(state.r);
                        nextGeneration.Add(clone);
                    }
                    else if (chooser == 1)
                    {
                        var clone = first.Clone();
                        clone.BreedWith(second, state.r);                        
                        clone.Mutate(state.r);                        
                        nextGeneration.Add(clone);
                    }
                    else if (chooser == 2)
                    {
                        var clone = first.Clone();
                        clone.BreedWith(second, state.r);                        
                        nextGeneration.Add(clone);
                    }
                }
                state.pictures = nextGeneration;


            }
            foreach (var pic in state.pictures)
            {
                if (pic.button.WasLeftClicked(mouseState, prevMouseState))
                {
                    pic.selected = !pic.selected;
                }

                if (pic.button.WasRightClicked(mouseState, prevMouseState))
                {
                    state.zoomedPic = pic;
                    Console.WriteLine(state.zoomedPic.ToLisp());
                    state.screen = Screen.ZOOM;
                }
            }

            return state;
        }
    }
}

