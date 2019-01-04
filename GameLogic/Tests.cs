using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameLogic
{
    public static class Tests
    {
        public static void Optimizing(GraphicsDevice g, GameWindow w)
        {
            const int TEST_SIZE = 1000;
            Random r = new Random();

            Console.WriteLine("start opt test");
            for (int i = 0; i < TEST_SIZE; i++)
            {
                Console.WriteLine(i);
                var tree = AptNode.GenerateTree(8, r, false);
                var machine = new StackMachine(tree);

                var optTree = AptNode.ConstantFolding(tree);
                var optMachine = new StackMachine(optTree);
                var stack = new float[machine.nodeCount];
                var optStack = new float[optMachine.nodeCount];

                for (float y = -1.0f; y <= 1.0f; y += .01f) {
                    for (float x = -1.0f; x <= 1.0f; x += .01f)
                    {
                        float result = machine.Execute(x, y, stack);
                        float optResult = optMachine.Execute(x, y, stack);

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

        public static void Parsing(GraphicsDevice g, GameWindow w)
        {
            const int TEST_SIZE = 10000;
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
