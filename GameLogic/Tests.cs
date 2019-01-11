using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLogic
{
    public static class Tests
    {
        public static void Optimizing(GraphicsDevice g, GameWindow w)
        {
            const int TEST_SIZE = 10000;
            Random r = new Random();

            Console.WriteLine("start opt test");
            for (int i = 0; i < TEST_SIZE; i++)
            {
                Console.WriteLine(i);
                var tree = AptNode.GenerateTree(r.Next(1, 20), r, true);
                var machine = new StackMachine(tree);

                var optTree = AptNode.ConstantFolding(tree);
                var optMachine = new StackMachine(optTree);
                var stack = new float[machine.nodeCount];
                var optStack = new float[optMachine.nodeCount];

                for (float y = -1.0f; y <= 1.0f; y += .01f)
                {
                    for (float x = -1.0f; x <= 1.0f; x += .01f)
                    {
                        float result = machine.Execute(x, y, stack);
                        float optResult = optMachine.Execute(x, y, optStack);

                        if (Math.Abs(optResult - result) > .01f)
                        {
                            Console.WriteLine("result:" + result);
                            Console.WriteLine("optResult:" + optResult);
                            Console.WriteLine("x:" + x + "y:" + y);
                            Console.WriteLine("--- tree --");
                            Console.WriteLine(tree.ToLisp());
                            Console.WriteLine("--- optTree --");
                            Console.WriteLine(optTree.ToLisp());
                            throw new Exception("opt fail");
                        }

                    }
                }

            }
            Console.WriteLine("done opt test");




        }

        public static void BreedingPairs(GraphicsDevice g, GameWindow w)
        {
            const int TEST_SIZE = 500;
            Random r = new Random();
            var results = new List<float>(1024);
            Console.WriteLine("start opt test");
            for (int i = 0; i < TEST_SIZE; i++)
            {
                Console.WriteLine(i);
                var treeA = AptNode.GenerateTree(r.Next(1, 20), r, true);


                var treeB = AptNode.GenerateTree(r.Next(1, 20), r, true);


                var childTree = treeA.BreedWith(treeB, r, true);
                var machine = new StackMachine(childTree);
                var stack = new float[machine.nodeCount];

                for (float y = -1.0f; y <= 1.0f; y += .005f)
                {
                    for (float x = -1.0f; x <= 1.0f; x += .005f)
                    {
                        results.Add(machine.Execute(x, y, stack));
                    }
                }
                Console.WriteLine("result[2]" + results[2]);
                results.Clear();

            }
            Console.WriteLine("done breed test");

        }

        public static void BreedingSelf(GraphicsDevice g, GameWindow w)
        {
            const int TEST_SIZE = 500;
            Random r = new Random();
            var results = new List<float>(1024);
            Console.WriteLine("start opt test");
            for (int i = 0; i < TEST_SIZE; i++)
            {
                Console.WriteLine(i);
                var treeA = AptNode.GenerateTree(r.Next(1, 20), r, true);

                var childTree = treeA.BreedWith(treeA, r, true);
                var machine = new StackMachine(childTree);
                var stack = new float[machine.nodeCount];

                for (float y = -1.0f; y <= 1.0f; y += .005f)
                {
                    for (float x = -1.0f; x <= 1.0f; x += .005f)
                    {
                        results.Add(machine.Execute(x, y, stack));
                    }
                }
                Console.WriteLine("result[2]" + results[2]);
                results.Clear();

            }
            Console.WriteLine("done breed test");

        }

        //stress test the whole system
        public static void PicGenerate(GraphicsDevice g, GameWindow w, GameState state, GameLogic logic)
        {
            const int TEST_SIZE = 5000;
            
            Random r = new Random();

            state.pictures = new Pic[state.populationSize];

            for (int i = 0; i < TEST_SIZE; i++)
            {
                for (int j = 0; j < state.populationSize; j++)
                {
                    int chooser = r.Next(0, 3);
                    PicType type = (PicType)chooser;
                    var p = new Pic(type, r, 1, 20, g, w, false);
                    p.SetNewBounds(new Rectangle(0, 0, 200, 200));
                    state.pictures[j] = p;
                    logic.LayoutUI();
                }

              
                
                //Thread.Sleep(500);
                

                for (int j = 0; j < state.pictures.Length; j++)
                {
                    state.pictures[j].Dispose();
                    state.pictures[j] = null;
                }


                Console.WriteLine(i + "/" + TEST_SIZE);
            }
        }


    

    public static void Parsing(GraphicsDevice g, GameWindow w)
    {
        const int TEST_SIZE = 1000;
        Random r = new Random();

        for (int i = 0; i < TEST_SIZE; i++)
        {
            int chooser = r.Next(0, 3);
            PicType type = (PicType)chooser;

            var p = new Pic(type, r, 1, 200, g, w, false);

            var s = p.ToLisp();

            try
            {
                var lexer = new Lexer(s);
                lexer.BeginLexing();
                var newP = lexer.ParsePic(g, w);
                var newS = newP.ToLisp();

                if (!newS.Equals(s))
                {
                    throw new Exception(newS);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(s);
                Console.WriteLine("-----------------");
                throw ex;
            }

        }



    }
}
}
