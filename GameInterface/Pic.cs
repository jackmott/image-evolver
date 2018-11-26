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
        public Button equation;

        public bool selected = false;
        public bool zoomed = false;
        public bool showEquation = false;

        public TextBox textBox;
        

        public Pic()
        {
        }

       

       
    }

}

