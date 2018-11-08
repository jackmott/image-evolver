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
        public AptNode[] Trees;
        public StackMachine[] Machines;

        public Button button;
        public bool selected;        

        public abstract string ToLisp();
        public abstract void Mutate(Random r);
        public abstract Pic Clone();
        public abstract void BreedWith(Pic partner, Random r);

        public (AptNode, StackMachine) GetRandomTree(Random r)
        {
            int index = r.Next(0, Trees.Length);
            return (Trees[index], Machines[index]);
        }

    }

   

    public class GradientTree : Pic
    {
        public Color[] gradients;
        public float[] pos;

        public GradientTree()
        {
            button = new Button(null, new Rectangle());
            Trees = new AptNode[1];
            Machines = new StackMachine[1];
        }

        public static Color RandomColor(Random r)
        {
            return new Color((byte)r.Next(0, 256), (byte)r.Next(0, 256), (byte)r.Next(0, 256), (byte)255);
        }

        public GradientTree(int min, int max, Random rand)
        {

            button = new Button(null, new Rectangle());
            Trees = new AptNode[1];
            Machines = new StackMachine[1];
            
            Trees[0] = AptNode.GenerateTree(rand.Next(min, max), rand);
            Machines[0] = new StackMachine(Trees[0]);

            int numGradients = rand.Next(2,20);
            gradients = new Color[numGradients];
            pos = new float[numGradients];
            for (int i = 0; i < gradients.Length; i++)
            {
                gradients[i] = RandomColor(rand);
                pos[i] = (float)(rand.NextDouble()*2.0-1.0);
                Array.Sort(pos);
            }
            pos[0] = -1.0f;
            pos[pos.Length - 1] = 1.0f;
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
            var pic = new GradientTree();
            for (int i = 0; i < Trees.Length; i++)
            {
                pic.Trees[i] = Trees[i].Clone();
                pic.Machines[i] = new StackMachine(Trees[i]);
            }
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
            return "( Gradient " + Trees[0].ToLisp() + " )\n";                        
        }
    }

    public class RGBTree : Pic
    {        

        public RGBTree()
        {            
            button = new Button(null, new Rectangle());
            Trees = new AptNode[3];
            Machines = new StackMachine[3];
        }
        public RGBTree(int min, int max, Random rand)
        {
        
            button = new Button(null, new Rectangle());
            Trees = new AptNode[3];
            Machines = new StackMachine[3];

            for (int i = 0; i < 3; i++)
            {
                Trees[i] = AptNode.GenerateTree(rand.Next(min, max), rand);
                Machines[i] = new StackMachine(Trees[i]);
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
            var pic = new RGBTree();
            for (int i = 0; i < Trees.Length; i++)
            {
                pic.Trees[i] = Trees[i].Clone();
                pic.Machines[i] = new StackMachine(Trees[i]);
            }
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
            string result = "( R " + Trees[0].ToLisp() + " )\n";
            result += "( G " + Trees[1].ToLisp() + " )\n";
            result += "( B " + Trees[2].ToLisp() + " )\n";
            return result;
        }

    }


    public class HSVTree : Pic
    {
        
        public HSVTree()
        {            
            button = new Button(null, new Rectangle());
            Trees = new AptNode[3];
            Machines = new StackMachine[3];
        }

        public HSVTree(int min, int max, Random rand)
        {
                        
            button = new Button(null, new Rectangle());
            Trees = new AptNode[3];
            Machines = new StackMachine[3];

            for (int i = 0; i < 3; i++)
            {
                Trees[i] = AptNode.GenerateTree(rand.Next(min, max), rand);
                Machines[i] = new StackMachine(Trees[i]);
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
            var pic = new HSVTree();
            for (int i = 0; i < Trees.Length; i++)
            {
                pic.Trees[i] = Trees[i].Clone();
                pic.Machines[i] = new StackMachine(Trees[i]);
            }
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
            string result = "( H " + Trees[0].ToLisp() + " )\n";
            result += "( S " + Trees[1].ToLisp() + " )\n";
            result += "( V " + Trees[2].ToLisp() + " )\n";
            return result;
        }
    }
}
