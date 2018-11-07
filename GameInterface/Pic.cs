using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;
using System.Diagnostics;


namespace GameInterface
{
    public abstract class Pic
    {
        public Button button;
        public bool selected;
        public List<ExternalImage> images;
        public abstract string ToLisp();
        public abstract void Mutate(Random r);
    }

    public class RGBTree : Pic
    {
        public AptNode RTree;
        public AptNode GTree;
        public AptNode BTree;
        public StackMachine RSM;
        public StackMachine GSM;
        public StackMachine BSM;

        public RGBTree() {
            button = new Button(null, new Rectangle());
        }
        public RGBTree(int min, int max, Random rand, List<ExternalImage> images)
        {
            this.images = images;
            button = new Button(null, new Rectangle());
            RTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            RSM = new StackMachine(RTree, images);

            GTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            GSM = new StackMachine(GTree, images);

            BTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            BSM = new StackMachine(BTree, images);

        }

        public override void Mutate(Random r)
        {
            Console.WriteLine("Before:" + RTree.ToLisp());
            RTree.Mutate(r);
            Console.WriteLine("After:" + RTree.ToLisp());
            GTree.Mutate(r);
            BTree.Mutate(r);
            RSM = new StackMachine(RTree, images);
            GSM = new StackMachine(GTree, images);
            BSM = new StackMachine(BTree, images);
        }

        public override string ToLisp()
        {
            string result = "( R " + RTree.ToLisp() + " )\n";
            result += "( G " + GTree.ToLisp() + " )\n";
            result += "( B " + BTree.ToLisp() + " )\n";
            return result;
        }

    }
 
     
    public class HSVTree : Pic
    {
        public AptNode HTree;
        public AptNode STree;
        public AptNode VTree;
        public StackMachine HSM;
        public StackMachine SSM;
        public StackMachine VSM;

        public HSVTree(int min, int max, Random rand,List<ExternalImage> images)
        {
            this.images = images;
            button = new Button(null, new Rectangle());
            HTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            HSM = new StackMachine(HTree,images);

            STree = AptNode.GenerateTree(rand.Next(min, max), rand);
            SSM = new StackMachine(STree,images);

            VTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            VSM = new StackMachine(VTree,images);

        }
        public override void Mutate(Random r)
        {
            HTree.Mutate(r);
            STree.Mutate(r);
            VTree.Mutate(r);
            HSM = new StackMachine(HTree, images);
            SSM = new StackMachine(STree, images);
            VSM = new StackMachine(VTree, images);
        }
        public override string ToLisp()
        {
            string result = "( H " + HTree.ToLisp() + " )\n";
            result += "( S " + STree.ToLisp() + " )\n";
            result += "( V " + VTree.ToLisp() + " )\n";
            return result;
        }
    }
}
