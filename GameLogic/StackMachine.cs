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

        private void BuildInstructions(AptNode node)
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

        // So you can test without passing a pointer
        public float Execute(float x, float y, float[] stack)
        {
            unsafe
            {
                fixed (float* s = stack)
                {
                    return Execute(x, y, s);
                }
            }
        }

        // So you can test without passing a pointer
        public float Execute(float x, float y, float t, float[] stack)
        {
            unsafe
            {
                fixed (float* s = stack)
                {
                    return Execute(x, y, t, s);
                }
            }
        }

        unsafe public float Execute(float x, float y, float* stack)
        {
            return Execute(x, y, 0.0f, stack);
        }

        unsafe public float Execute(float x, float y, float t, float* stack)
        {
            int sp = -1;
            foreach (var ins in instructions)
            {
                switch (ins.type)
                {
                    case NodeType.X:
                        sp++;
                        stack[sp] = x;
                        break;
                    case NodeType.Y:
                        sp++;
                        stack[sp] = y;
                        break;
                    case NodeType.T:
                        sp++;
                        stack[sp] = t;
                        break;
                    case NodeType.CONSTANT:
                        sp++;
                        stack[sp] = ins.value;
                        break;
                    case NodeType.ADD:
                        stack[sp - 1] = stack[sp] + stack[sp - 1];
                        sp--;
                        break;
                    case NodeType.SUB:
                        stack[sp - 1] = stack[sp] - stack[sp - 1];
                        sp--;
                        break;
                    case NodeType.MUL:
                        stack[sp - 1] = stack[sp] * stack[sp - 1];
                        sp--;
                        break;
                    case NodeType.DIV:
                        stack[sp - 1] = stack[sp] / stack[sp - 1];
                        sp--;
                        break;
                    case NodeType.SIN:
                        stack[sp] = (float)Math.Sin(Math.PI * stack[sp]);
                        break;
                    case NodeType.COS:
                        stack[sp] = (float)Math.Cos(Math.PI * stack[sp]);
                        break;
                    case NodeType.ATAN:
                        stack[sp] = 0.6366f * (float)Math.Atan(5.0f * stack[sp]);
                        break;
                    case NodeType.ATAN2:
                        stack[sp - 1] = (float)(Math.Atan2(stack[sp], stack[sp - 1]) / Math.PI);
                        sp--;
                        break;
                    case NodeType.LOG:
                        stack[sp] = ((float)Math.Log(stack[sp] * 7.0f)) / 2.0f;
                        break;
                    case NodeType.SQUARE:
                        stack[sp] = stack[sp] * stack[sp];
                        break;
                    case NodeType.SQRT:
                        {
                            // Modify sqrt to work on negatives for art's sake
                            var n = stack[sp];
                            if (n >= 0)
                            {
                                stack[sp] = (float)Math.Sqrt(n);
                            }
                            else
                            {
                                n = n * -1;
                                n = (float)Math.Sqrt(n);
                                stack[sp] = n * -1;
                            }
                        }
                        break;
                    case NodeType.CEIL:
                        stack[sp] = (float)Math.Ceiling(stack[sp]);
                        break;
                    case NodeType.FLOOR:
                        stack[sp] = MathUtils.FastFloor(stack[sp]);
                        break;
                    case NodeType.MIN:
                        stack[sp - 1] = Math.Min(stack[sp], stack[sp - 1]);
                        sp--;
                        break;
                    case NodeType.MAX:
                        stack[sp - 1] = Math.Max(stack[sp], stack[sp - 1]);
                        sp--;
                        break;
                    case NodeType.CLAMP:
                        var v = stack[sp];
                        if (v > 1.0f) v = 1.0f;
                        else if (v < -1.0f) v = -1.0f;
                        stack[sp] = v;
                        break;
                    case NodeType.WRAP:
                        stack[sp] = MathUtils.WrapMinMax(stack[sp], -1.0f, 1.001f);
                        break;
                    case NodeType.NEGATE:
                        stack[sp] = -1.0f * stack[sp];
                        break;
                    case NodeType.ABS:
                        stack[sp] = (float)Math.Abs(stack[sp]);
                        break;
                    case NodeType.MOD:
                        stack[sp - 1] = stack[sp] % stack[sp - 1];
                        sp--;
                        break;
                    case NodeType.IF:
                        if (stack[sp] < 0)
                        {
                            stack[sp - 2] = stack[sp - 1];
                        } // implicit else stackPointer[sp-2] = stackPointer[sp-2]
                        sp -= 2;
                        break;
                    case NodeType.FBM:
                        {
                            const float lac = 2.0f;
                            const float gain = 0.5f;
                            stack[sp - 2] = 2.0f * 0.49f * FastNoise.SingleSimplexFractalFBM(stack[sp], stack[sp - 1], stack[sp - 2], 1337, 3, lac, gain);
                            sp -= 2;
                        }
                        break;
                    case NodeType.BILLOW:
                        {
                            const float lac = 2.0f;
                            const float gain = 0.5f;
                            stack[sp - 2] = 2.0f * ((0.422f * FastNoise.SingleSimplexFractalBillow(stack[sp], stack[sp - 1], stack[sp - 2], 1337, 3, lac, gain)) + 0.739f) - 1.0f;
                            sp -= 2;
                        }
                        break;
                    case NodeType.CELL1:
                        {
                            var jitter = stack[sp - 2] / 2.0f;
                            stack[sp - 2] = 2.0f * 0.274f * FastNoise.SingleCellular2Edge(2.5f * stack[sp], 2.5f * stack[sp - 1], jitter, 1337, FastNoise.CellularDistanceFunction.Euclidean, FastNoise.CellularReturnType.Distance2Add) - 1.0f;
                            stack[sp - 2] *= -1;
                            sp -= 2;
                        }
                        break;
                    case NodeType.WARP1:
                        {
                            var octaves = (int)(Math.Abs(stack[sp - 4]) * 2.0f + 1.0f);
                            var (xf, yf) = FastNoise.GradientPerturbFractal(stack[sp], stack[sp - 1], 2f * stack[sp - 2], stack[sp - 3] / 3.0f, 1337, octaves, 2.0f, 0.5f, FastNoise.Interp.Quintic);
                            stack[sp - 3] = xf;                            
                            stack[sp - 4] = yf;
                            sp -= 3;
                        }
                        break;
                    case NodeType.PICTURE:
                        {
                            var image = GameState.externalImages[(int)ins.value];
                            var xf = (stack[sp] + 1.0f) / 2.0f;
                            var yf = (stack[sp - 1] + 1.0f) / 2.0f;
                            var xi = (int)(xf * image.w);
                            var yi = (int)(yf * image.h);
                            var index = yi * image.w + xi;
                            if (index > image.data.Length - 1) index = image.data.Length - 1;
                            if (index < 0) index = 0;
                            var c = image.data[index];
                            var fc = (float)(c.R + c.G + c.B) / (255.0f * 3.0f);
                            sp--;
                            stack[sp] = fc;
                        }
                        break;
                    default:
                        throw new Exception("Evexecute found a bad node");
                }
                stack[sp] = MathUtils.FixNan(stack[sp]);
            }

            return stack[sp];

        }

    }

}
