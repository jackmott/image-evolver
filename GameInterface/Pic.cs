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

        public PicType type;
        public AptNode[] Trees;
        public StackMachine[] Machines;
        public (Color?, Color?)[] gradients;
        public float[] pos;

        public Button button;
        public Button inject;

        public bool selected = false;
        public bool zoomed = false;


        public Pic()
        {
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

        public (AptNode, StackMachine) GetRandomTree(Random r)
        {
            int index = r.Next(0, Trees.Length);
            return (Trees[index], Machines[index]);
        }

    }

}

