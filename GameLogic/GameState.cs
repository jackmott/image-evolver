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

        public void Draw(GraphicsDevice g, SpriteBatch batch,GameTime gameTime)
        {
            int winW = g.Viewport.Width;
            int winH = g.Viewport.Height;

            int hSpace = (int)(winW * GameState.HORIZONTAL_SPACING);
            int vSpace = (int)(winW * GameState.VERTICAL_SPACING);

            int numPerRow = (int)Math.Sqrt(state.populationSize);
            int spaceRemaining = winW - hSpace * (numPerRow + 1);
            int picW = spaceRemaining / numPerRow;

            spaceRemaining = winH - vSpace * (numPerRow + 1);
            int picH = spaceRemaining / numPerRow;


            g.Clear(Color.Black);
            batch.Begin();
            var pos = new Vector2(0, 0);
            int index = 0;
            for (int y = 0; y < numPerRow; y++)
            {
                pos.Y += vSpace;
                for (int x = 0; x < numPerRow; x++)
                {
                    pos.X += hSpace;
                    batch.Draw(GetTex(state.pictures[index],state.externalImages,g, picW, picH), pos, Color.White);
                    index++;
                    pos.X += picW;
                }
                pos.Y += picH;
                pos.X = 0;
            }

            batch.End();

            // TODO: Add your drawing code here

            
        }

        public GameState Update(KeyboardState keyboard, GameTime gameTime, GraphicsDevice g)
        {
            

            if (state == null)
            {                
                state = new GameState();

                DirectoryInfo d = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory+@"\Assets");
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

                state.populationSize = 16;
                state.pictures = new List<Pic>();
                Random r = new Random();               
                for (int i = 0; i < state.populationSize; i++)
                {
                    /*
                    var node = new AptNode { type = NodeType.CELL1, children = new AptNode[3] };
                    node.children[0] = new AptNode { type = NodeType.X };
                    node.children[1] = new AptNode { type = NodeType.Y };
                    node.children[2] = new AptNode { type = NodeType.CONSTANT, value = (float)r.NextDouble() * 2.0f - 1.0f };
                    while (node.AddLeaf(AptNode.GetRandomLeaf(r))) { }
                    var tree = new RGBTree(1, 1, r);
                    tree.RTree = node;
                    tree.GTree = node;
                    tree.BTree = node;
                    tree.RSM = new StackMachine(node);
                    tree.GSM = new StackMachine(node);
                    tree.BSM = new StackMachine(node);
                    state.pictures.Add(tree);
                    */
                    
                    int chooser = r.Next(0, 2);
                    if (chooser == 0)
                    {
                        var rgbTree = new RGBTree(2, 2,r,state.externalImages);
                        state.pictures.Add(rgbTree);                        
                    }
                    else
                    {
                        var hsvTree = new HSVTree(2, 2, r,state.externalImages);
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
               
            }
            
            return state;
        }
    }
}

