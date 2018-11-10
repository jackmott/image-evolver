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
    public enum PicType { RGB, HSV, GRADIENT }

    public class Pic
    {
        public Rectangle bounds;
        public PicType type;
        public AptNode[] Trees;
        public StackMachine[] Machines;
        public (Color?,Color?)[] gradients;
        public float[] pos;

        public Button button;
        public Button inject;
        public bool selected;

        
        public Pic(PicType type)
        {
            this.type = type;
            button = new Button(null, new Rectangle());
            inject = new Button(null, new Rectangle());
            switch (type)
            {
                case PicType.RGB:
                case PicType.HSV:
                    Trees = new AptNode[3];
                    Machines = new StackMachine[3];
                    break;
                case PicType.GRADIENT:
                    Trees = new AptNode[1];
                    Machines = new StackMachine[1];
                    break;
            }
        }

      

       

        public string ToLisp()
        {
            switch (type)
            {
                case PicType.GRADIENT:
                    return "( Gradient " + Trees[0].ToLisp() + " )\n";
                case PicType.RGB:
                    {
                        string result = "( R " + Trees[0].ToLisp() + " )\n";
                        result += "( G " + Trees[1].ToLisp() + " )\n";
                        result += "( B " + Trees[2].ToLisp() + " )\n";
                        return result;
                    }
                case PicType.HSV:
                    {
                        string result = "( H " + Trees[0].ToLisp() + " )\n";
                        result += "( S " + Trees[1].ToLisp() + " )\n";
                        result += "( V " + Trees[2].ToLisp() + " )\n";
                        return result;
                    }
                default:
                    throw new Exception("Impossible");
            }
        }


       


        public Pic Clone()
        {
            Pic pic = new Pic(type);
            if (gradients != null)
            {
                var newGradients = new (Color?,Color?)[gradients.Length];
                var newPos = new float[pos.Length];

                for (int i = 0; i < newGradients.Length; i++)
                {
                    newGradients[i] = gradients[i];
                    newPos[i] = pos[i];
                }
                pic.gradients = newGradients;
                pic.pos = newPos;
            }
            for (int i = 0; i < Trees.Length; i++)
            {
                pic.Trees[i] = Trees[i].Clone();
                pic.Machines[i] = new StackMachine(Trees[i]);
            }
            return pic;
        }

       

        public (AptNode, StackMachine) GetRandomTree(Random r)
        {
            int index = r.Next(0, Trees.Length);
            return (Trees[index], Machines[index]);
        }

    }

}

