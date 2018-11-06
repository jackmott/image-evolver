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
        public Texture2D tex;
        public Rectangle bounds;
        public bool selected;
        public abstract string ToLisp();
    }

    public class RGBTree : Pic
    {
        public AptNode RTree;
        public AptNode GTree;
        public AptNode BTree;
        public StackMachine RSM;
        public StackMachine GSM;
        public StackMachine BSM;

        public RGBTree() { }
        public RGBTree(int min, int max, Random rand, List<ExternalImage> images)
        {

            RTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            RSM = new StackMachine(RTree, images);

            GTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            GSM = new StackMachine(GTree, images);

            BTree = AptNode.GenerateTree(rand.Next(min, max), rand);
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

            HTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            HSM = new StackMachine(HTree,images);

            STree = AptNode.GenerateTree(rand.Next(min, max), rand);
            SSM = new StackMachine(STree,images);

            VTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            VSM = new StackMachine(VTree,images);

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
