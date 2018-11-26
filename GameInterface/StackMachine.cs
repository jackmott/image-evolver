using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace GameLogic
{    
    public struct Instruction
    {
        public NodeType type;
        public float value;
    }

    public class StackMachine
    {
        public Instruction[] instructions;
        public int inPtr;
        public int nodeCount;
        
                
        public StackMachine(AptNode node)
        {
            nodeCount = node.Count();
            instructions = new Instruction[nodeCount];
            inPtr = 0;
            BuildInstructions(node);        
        }

        public void RebuildInstructions(AptNode node)
        {
            nodeCount = node.Count();
            instructions = new Instruction[nodeCount];
            inPtr = 0;
            BuildInstructions(node);
        }

        public void BuildInstructions(AptNode node) {            
            if (node.children != null)
            {
                for (int i = node.children.Length-1; i >= 0; i--)
                {
                    BuildInstructions(node.children[i]);
                }
            }
            switch (node.type) {
                case NodeType.EMPTY:
                    throw new Exception("can't BuildInstructions with a non finished APT");
                case NodeType.CONSTANT:
                case NodeType.PICTURE:
                    instructions[inPtr] = new Instruction { type = node.type, value = node.value };
                    inPtr++;
                    break;                
                default:                
                    instructions[inPtr] = new Instruction { type = node.type };
                    inPtr++;
                    break;
            }
        }

      

    }

}
