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
        public abstract Pic Clone();
        public abstract void BreedWith(Pic partner, Random r);
        public abstract (AptNode,StackMachine) GetRandomTree(Random r);
    }

    public class RGBTree : Pic
    {
        public AptNode RTree;
        public AptNode GTree;
        public AptNode BTree;
        public StackMachine RSM;
        public StackMachine GSM;
        public StackMachine BSM;

        public RGBTree(List<ExternalImage> images)
        {
            this.images = images;
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

        public override (AptNode,StackMachine) GetRandomTree(Random r)
        {
            switch (r.Next(0, 3))
            {
                case 0:
                    return (RTree,RSM);
                case 1:
                    return (GTree,GSM);
                default:
                    return (BTree,BSM);
            }
        }
        public override void BreedWith(Pic partner, Random r)
        {
            var (ft, fs) = GetRandomTree(r);
            var (st, ss) = partner.GetRandomTree(r);
            ft.BreedWith(st, r);
            fs.RebuildInstructions(ft);
        }
        public override Pic Clone()
        {
            var pic = new RGBTree(images);
            pic.RTree = RTree.Clone();
            pic.GTree = GTree.Clone();
            pic.BTree = BTree.Clone();
            pic.RSM = new StackMachine(RTree, images);
            pic.GSM = new StackMachine(GTree, images);
            pic.BSM = new StackMachine(BTree, images);
            return pic;
        }

        public override void Mutate(Random r)
        {
            var (t, s) = GetRandomTree(r);
            t.Mutate(r);
            s.RebuildInstructions(t);
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

        public HSVTree(List<ExternalImage> images)
        {
            this.images = images;
            button = new Button(null, new Rectangle());
        }

        public HSVTree(int min, int max, Random rand, List<ExternalImage> images)
        {
            this.images = images;
            button = new Button(null, new Rectangle());
            HTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            HSM = new StackMachine(HTree, images);

            STree = AptNode.GenerateTree(rand.Next(min, max), rand);
            SSM = new StackMachine(STree, images);

            VTree = AptNode.GenerateTree(rand.Next(min, max), rand);
            VSM = new StackMachine(VTree, images);

        }

        public override (AptNode,StackMachine) GetRandomTree(Random r)
        {
            switch (r.Next(0, 3))
            {
                case 0:
                    return (HTree,HSM);
                case 1:
                    return (STree,SSM);
                default:
                    return (VTree,VSM);
            }
        }

        public override void BreedWith(Pic partner, Random r)
        {
            var (ft,fs) = GetRandomTree(r);
            var (st,ss) = partner.GetRandomTree(r);
            ft.BreedWith(st, r);
            fs.RebuildInstructions(ft);
        }

        public override Pic Clone()
        {
            var pic = new HSVTree(images);
            pic.HTree = HTree.Clone();
            pic.STree = STree.Clone();
            pic.VTree = VTree.Clone();
            pic.HSM = new StackMachine(HTree, images);
            pic.SSM = new StackMachine(STree, images);
            pic.VSM = new StackMachine(VTree, images);
            return pic;
        }
        public override void Mutate(Random r)
        {
            var (t, s) = GetRandomTree(r);
            t.Mutate(r);
            s.RebuildInstructions(t);            
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
