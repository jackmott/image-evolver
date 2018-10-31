using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameInterface
{    
    public struct Instruction
    {
        public NodeType type;
        public float value;
    }

    public class StackMachine
    {
        List<Instruction> instructions;
        public int nodeCount;
        
        public StackMachine(AptNode node)
        {
            nodeCount = node.Count();
            instructions = new List<Instruction>(nodeCount);            
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
                    instructions.Add(new Instruction { type = node.type, value = node.value });
                    break;
                default:                
                    instructions.Add(new Instruction { type = node.type });
                    break;
            }
        }

        public float Execute(float x, float y, float[] stack)
        {
            unsafe
            {
                fixed (float* stackPointer = stack) //this fails with an implicit cast error
                {
                    int sp = 0;
                    for (int i = 0; i < instructions.Count; i++)
                    {
                        var ins = instructions[i];
                        switch (ins.type)
                        {
                            case NodeType.X:
                                sp++;
                                stackPointer[sp] = x;
                                break;
                            case NodeType.Y:
                                sp++;
                                stackPointer[sp] = y;
                                break;
                            case NodeType.CONSTANT:
                                sp++;
                                stackPointer[sp] = ins.value;
                                break;
                            case NodeType.ADD:
                                stackPointer[sp - 1] = stackPointer[sp] + stackPointer[sp - 1];
                                sp--;
                                break;
                            case NodeType.SUB:
                                stackPointer[sp - 1] = stackPointer[sp] - stackPointer[sp - 1];
                                sp--;
                                break;
                            case NodeType.MUL:
                                stackPointer[sp - 1] = stackPointer[sp] * stackPointer[sp - 1];
                                sp--;
                                break;
                            case NodeType.DIV:
                                stackPointer[sp - 1] = stackPointer[sp] / stackPointer[sp - 1];
                                sp--;
                                break;
                            case NodeType.SIN:
                                stackPointer[sp] = (float)Math.Sin(stackPointer[sp]);                                
                                break;
                        }
                    }
                    return stackPointer[sp];
                }
            }
        }

    }

}
