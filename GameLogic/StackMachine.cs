using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
namespace GameLogic
{
    [DataContract]
    public struct Instruction
    {
        [DataMember]
        public NodeType type;
        [DataMember]
        public float value;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct floatint
    {
        [FieldOffset(0)]
        public float f;
        [FieldOffset(0)]
        public int i;
    }

    [DataContract]
    public class StackMachine
    {
        [DataMember]
        public Instruction[] instructions;
        [DataMember]
        public int inPtr;
        [DataMember]
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

        public void BuildInstructions(AptNode node)
        {
            if (node.children != null)
            {
                for (int i = node.children.Length - 1; i >= 0; i--)
                {
                    BuildInstructions(node.children[i]);
                }
            }
            switch (node.type)
            {
                case NodeType.EMPTY:
                    throw new Exception("can't BuildInstructions with a non finished APT");
                case NodeType.CONSTANT:
                    instructions[inPtr] = new Instruction { type = node.type, value = node.value };
                    inPtr++;
                    break;
                case NodeType.PICTURE:
                    var value = -1;
                    for (int i = 0; i < GameState.externalImages.Count; i++)
                    {
                        var filename = GameState.externalImages[i].filename;
                        if (filename.Equals(node.filename))
                        {
                            value = i;
                            break;
                        }
                    }
                    if (value == -1) throw new Exception("picture string invalid");
                    instructions[inPtr] = new Instruction { type = node.type, value = value };
                    inPtr++;
                    break;
                default:
                    instructions[inPtr] = new Instruction { type = node.type };
                    inPtr++;
                    break;
            }
        }

        public float Execute(float x, float y, float[] stack)
        {
            return Execute(x, y, 0.0f, stack);
        }

        public float Execute(float x, float y, float t, float[] stack)
        {
            unsafe
            {
                fixed (float* stackPointer = stack) //this fails with an implicit cast error
                {
                          int sp = -1;
                    for (int i = 0; i < instructions.Length;i++) 
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
                            case NodeType.T:
                                sp++;
                                stackPointer[sp] = t;
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
                                stackPointer[sp] = (float)Math.Sin(Math.PI * stackPointer[sp]);
                                break;
                            case NodeType.COS:
                                stackPointer[sp] = (float)Math.Cos(Math.PI * stackPointer[sp]);
                                break;
                            case NodeType.ATAN:
                                stackPointer[sp] = 0.6366f * (float)Math.Atan(5.0f * stackPointer[sp]);
                                break;
                            case NodeType.ATAN2:
                                stackPointer[sp - 1] = (float)(Math.Atan2(stackPointer[sp], stackPointer[sp - 1]) / Math.PI);
                                sp--;
                                break;
                            case NodeType.LOG:
                                stackPointer[sp] = ((float)Math.Log(stackPointer[sp] * 7.0f)) / 2.0f;
                                break;
                            case NodeType.SQUARE:
                                stackPointer[sp] = stackPointer[sp] * stackPointer[sp];
                                break;
                            case NodeType.SQRT:
                                {
                                    // Modify sqrt to work on negatives for art's sake
                                    var n = stackPointer[sp];
                                    if (n >= 0)
                                    {
                                        stackPointer[sp] = (float)Math.Sqrt(n);
                                    }
                                    else
                                    {
                                        n = n * -1;
                                        n = (float)Math.Sqrt(n);
                                        stackPointer[sp] = n * -1;
                                    }
                                }
                                break;
                            case NodeType.CEIL:
                                stackPointer[sp] = (float)Math.Ceiling(stackPointer[sp]);
                                break;
                            case NodeType.FLOOR:
                                stackPointer[sp] = MathUtils.FastFloor(stackPointer[sp]);
                                break;
                            case NodeType.MIN:
                                stackPointer[sp - 1] = Math.Min(stackPointer[sp], stackPointer[sp - 1]);
                                sp--;
                                break;
                            case NodeType.MAX:
                                stackPointer[sp - 1] = Math.Max(stackPointer[sp], stackPointer[sp - 1]);
                                sp--;
                                break;
                            case NodeType.CLAMP:
                                var v = stackPointer[sp];
                                if (v > 1.0f) v = 1.0f;
                                else if (v < -1.0f) v = -1.0f;
                                stackPointer[sp] = v;
                                break;
                            case NodeType.WRAP:
                                stackPointer[sp] = MathUtils.WrapMinMax(stackPointer[sp], -1.0f, 1.001f);
                                break;
                            case NodeType.NEGATE:
                                stackPointer[sp] = -1.0f * stackPointer[sp];
                                break;
                            case NodeType.ABS:
                                stackPointer[sp] = (float)Math.Abs(stackPointer[sp]);
                                break;
                            case NodeType.MOD:
                                stackPointer[sp - 1] = stackPointer[sp] % stackPointer[sp - 1];
                                sp--;
                                break;
                            case NodeType.IF:
                                if (stackPointer[sp] < 0)
                                {
                                    stackPointer[sp - 2] = stackPointer[sp - 1];
                                } // implicit else stackPointer[sp-2] = stackPointer[sp-2]
                                sp -= 2;
                                break;
                            case NodeType.FBM:
                                {
                                    const float lac = 2.0f;
                                    const float gain = 0.5f;
                                    stackPointer[sp - 2] = 2.0f * 0.49f * FastNoise.SingleSimplexFractalFBM(stackPointer[sp], stackPointer[sp - 1], stackPointer[sp - 2], 1337, 3, lac, gain);
                                    sp -= 2;
                                    break;
                                }
                            case NodeType.BILLOW:
                                {
                                    const float lac = 2.0f;
                                    const float gain = 0.5f;
                                    stackPointer[sp - 2] = 2.0f * ((0.422f * FastNoise.SingleSimplexFractalBillow(stackPointer[sp], stackPointer[sp - 1], stackPointer[sp - 2], 1337, 3, lac, gain)) + 0.739f) - 1.0f;
                                    sp -= 2;
                                    break;
                                }
                            case NodeType.CELL1:
                                var jitter = stackPointer[sp - 2] / 2.0f;
                                stackPointer[sp - 2] = 2.0f * 0.274f * FastNoise.SingleCellular2Edge(2.5f * stackPointer[sp], 2.5f * stackPointer[sp - 1], jitter, 1337, FastNoise.CellularDistanceFunction.Euclidean, FastNoise.CellularReturnType.Distance2Add) - 1.0f;
                                stackPointer[sp - 2] *= -1;
                                sp -= 2;
                                break;
                            case NodeType.WARP1:
                                {
                                    var octaves = (int)(Math.Abs(stackPointer[sp - 4]) * 2.0f + 1.0f);
                                    var (xf, yf) = FastNoise.GradientPerturbFractal(stackPointer[sp], stackPointer[sp - 1], 2f * stackPointer[sp - 2], stackPointer[sp - 3] / 3.0f, 1337, octaves, 2.0f, 0.5f, FastNoise.Interp.Quintic);
                                    stackPointer[sp - 3] = xf;
                                    stackPointer[sp - 4] = MathUtils.WrapMinMax(yf, -1.0f, 1.0f);
                                    sp -= 3;
                                    break;
                                }
                            case NodeType.PICTURE:
                                {
                                    var image = GameState.externalImages[(int)ins.value];
                                    var xf = (stackPointer[sp] + 1.0f) / 2.0f;
                                    var yf = (stackPointer[sp - 1] + 1.0f) / 2.0f;
                                    var xi = (int)(xf * image.w);
                                    var yi = (int)(yf * image.h);
                                    var index = yi * image.w + xi;
                                    if (index > image.data.Length - 1) index = image.data.Length - 1;
                                    if (index < 0) index = 0;
                                    var c = image.data[index];
                                    var fc = (float)(c.R + c.G + c.B) / (255.0f * 3.0f);
                                    sp--;
                                    stackPointer[sp] = fc;
                                    break;
                                }
                            default:
                                throw new Exception("Evexecute found a bad node");
                        }
                        stackPointer[sp] = MathUtils.FixNan(stackPointer[sp]);
                    }
                    return stackPointer[sp];
                }
            }
        }



    }

}
