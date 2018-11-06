﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameLogic.PicFunctions;
using GameInterface;

namespace GameLogic
{
    public static class StackMachineFunctions
    {
        public static float Execute(StackMachine sm, float x, float y, float[] stack, List<ExternalImage> images)
        {
            unsafe
            {
                fixed (float* stackPointer = stack) //this fails with an implicit cast error
                {
                    int sp = 0;
                    foreach (var ins in sm.instructions)
                    {
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
                            case NodeType.COS:
                                stackPointer[sp] = (float)Math.Cos(stackPointer[sp]);
                                break;
                            case NodeType.ATAN:
                                stackPointer[sp] = (float)Math.Atan(stackPointer[sp]);
                                break;
                            case NodeType.ATAN2:
                                stackPointer[sp - 1] = (float)Math.Atan2(stackPointer[sp], stackPointer[sp - 1]);
                                sp--;
                                break;
                            case NodeType.LOG:
                                stackPointer[sp] = (float)Math.Log(stackPointer[sp]);
                                break;
                            case NodeType.SQUARE:
                                stackPointer[sp] = stackPointer[sp] * stackPointer[sp];
                                break;
                            case NodeType.SQRT:
                                stackPointer[sp] = (float)Math.Sqrt(stackPointer[sp]);
                                break;
                            case NodeType.CEIL:
                                stackPointer[sp] = (float)Math.Ceiling(stackPointer[sp]);
                                break;
                            case NodeType.FLOOR:
                                stackPointer[sp] = (float)Math.Floor(stackPointer[sp]);
                                break;
                            case NodeType.MIN:
                                stackPointer[sp - 1] = Math.Min(stackPointer[sp], stackPointer[sp - 1]);
                                sp--;
                                break;
                            case NodeType.MAX:
                                stackPointer[sp - 1] = Math.Max(stackPointer[sp], stackPointer[sp - 1]);
                                sp--;
                                break;
                            case NodeType.CLIP:
                                var v = stackPointer[sp];
                                var max = Math.Abs(stackPointer[sp - 1]);
                                if (v > max) v = max;
                                if (v < -max) v = -max;
                                stackPointer[sp - 1] = v;
                                sp--;
                                break;
                            case NodeType.NEGATE:
                                stackPointer[sp] = -1.0f * stackPointer[sp];
                                break;
                            case NodeType.WRAP:
                                var f = stackPointer[sp];
                                var temp = (f - -1.0f) / (2.0f);
                                f = -1.0f + 2.0f * (temp - (float)Math.Floor(temp));
                                stackPointer[sp] = f;
                                break;
                            case NodeType.ABS:
                                stackPointer[sp] = (float)Math.Abs(stackPointer[sp]);
                                break;
                            case NodeType.MOD:
                                stackPointer[sp - 1] = stackPointer[sp] % stackPointer[sp - 1];
                                sp--;
                                break;
                            case NodeType.FBM:
                                {                                    
                                    stackPointer[sp - 2] = 2.0f * 0.3584f * FastNoise.SingleSimplexFractalFBM(stackPointer[sp], stackPointer[sp - 1], 2.0f*stackPointer[sp-2], 1337, 3, 2.0f, 0.5f);
                                    sp -= 2;
                                    break;
                                }
                            case NodeType.BILLOW:
                                {                                 
                                    stackPointer[sp - 2] = 2.0f * ((0.3496f * FastNoise.SingleSimplexFractalBillow(stackPointer[sp], stackPointer[sp - 1], 2.0f*stackPointer[sp-2], 1337, 3, 2.0f, 0.5f)) + 0.6118f) - 1.0f;
                                    sp -= 2;
                                    break;
                                }
                            case NodeType.CELL1:
                                var jitter = stackPointer[sp - 2];
                                if (jitter < -0.4) jitter = -0.4f;
                                if (jitter > 0.4) jitter = 0.4f;
                                stackPointer[sp - 2] = 2.0f * 0.274f * FastNoise.SingleCellular2Edge(2.5f * stackPointer[sp], 2.5f * stackPointer[sp - 1],jitter, 1337, FastNoise.CellularDistanceFunction.Euclidean, FastNoise.CellularReturnType.Distance2Add) - 1.0f;
                                sp -= 2;
                                break;
                            case NodeType.WARP1:
                                {
                                    var octaves = Math.Abs((int)stackPointer[sp - 4]);
                                    if (octaves < 1) octaves = 1;
                                    if (octaves > 3) octaves = 3;
                                   var (xf, yf) = FastNoise.GradientPerturbFractal(stackPointer[sp], stackPointer[sp - 1], 2f * stackPointer[sp - 2], stackPointer[sp - 3] / 3.0f, 1337,octaves,2.0f,0.5f, FastNoise.Interp.Quintic);
                                    stackPointer[sp - 3] = xf;
                                    stackPointer[sp - 4] = yf;
                                    sp -= 3;
                                    break;
                                }
                            case NodeType.PICTURE:
                                {
                                    var image = images[(int)ins.value];
                                    var xf = (stackPointer[sp] + 1.0f) / 2.0f;
                                    //xf = PicFunctions.Wrap0To1(xf);
                                    if (xf >= 1) xf = x/2.0f + 0.5f;
                                    if (xf < 0) xf = x / 2.0f + 0.5f;
                                    var yf = (stackPointer[sp-1] + 1.0f) / 2.0f;
                                    //yf = PicFunctions.Wrap0To1(yf);
                                    if (yf >= 1) yf = y / 2.0f + 0.5f;
                                    if (yf < 0) yf = y / 2.0f + 0.5f;
                                    var xi = (int)(xf * image.w);
                                    var yi = (int)(yf * image.h);
                                    var index = yi * image.w + xi;                                    
                                    var c = image.data[index];
                                    var fc = (float)(c.R + c.G + c.B) / (255.0f * 3.0f);                                    
                                    stackPointer[sp-1] = fc;
                                    sp--;
                                    break;
                                }                            
                            default:
                                throw new Exception("Evexecute found a bad node");


                        }
                    }
                    return stackPointer[sp];
                }
            }
        }
    }
}