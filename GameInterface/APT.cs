using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Concurrent;

namespace GameInterface
{
    public enum NodeType : byte { EMPTY = 0, CONSTANT, X, Y, PICTURE, ABS, WRAP, CLIP,NEGATE,ADD, SUB, MUL, DIV, SIN, COS, LOG, ATAN, ATAN2, SQRT, FLOOR, CEIL, MAX, MIN, MOD, SQUARE,FBM, BILLOW,CELL1};


    public struct AptNode
    {
        const int NUM_LEAF_TYPES = 5;
        public NodeType type;
        public AptNode[] children;
        public float value;



        public bool IsLeaf()
        {

            return !IsEmpty() && (int)type < NUM_LEAF_TYPES;
        }

        public bool IsEmpty()
        {
            return type == NodeType.EMPTY;
        }

        public int Count()
        {
            int count = 1;
            if (children != null)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    count += children[i].Count();
                }
            }
            return count;

        }
        public (AptNode, int) GetNthNode(int n)
        {
            if (n == 0) return (this, n);
            AptNode node = new AptNode { type = NodeType.EMPTY };

            if (children != null)
            {
                foreach (var child in children) {
                    (node, n) = child.GetNthNode(n - 1);
                    if (node.type != NodeType.EMPTY)
                    {
                        return (node, n);
                    }
                }             
            }
            return (new AptNode { type = NodeType.EMPTY }, n);

        }

        public static (bool, int) ReplaceNthNode(ref AptNode node, AptNode newNode, int n)
        {
            bool replaced = false;
            if (n == 0)
            {
                node = newNode;
                return (true, n);
            }
            else if (node.children != null) 
            {
                for (int i =0; i < node.children.Length;i++)  {
                    (replaced, n) = ReplaceNthNode(ref node.children[i], newNode, n - 1);
                    if (replaced)
                    {
                        return (replaced, n);
                    }
                }
            }
            return (false, n);
        }

        public void BreedWith(AptNode partner, Random r)
        {
            var (nodeToSwapIn,n) = partner.GetNthNode(r.Next(0, partner.Count()));
            ReplaceNthNode(ref this, nodeToSwapIn, r.Next(0, this.Count()));
        }

        public void Mutate(Random r)
        {
            var nodeIndex = r.Next(0, this.Count());                        
            int choose = r.Next(0, 2);
            if (choose == 0)
            {
                var newNode = GetRandomNode(r);
                ReplaceNthNode(ref this, newNode, nodeIndex);
                while (this.AddLeaf(GetRandomLeaf(r))) { };
            }
            else
            {
                var newLeaf = GetRandomLeaf(r);
                ReplaceNthNode(ref this, newLeaf, nodeIndex);
            }
        }
        
        public int LeafCount()
        {
            int count = 0;
            if (IsLeaf())
            {
                count = 1;
            }
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    count += children[i].LeafCount();
                }
            }
            return count;

        }

        // Note: assumes you are adding to a non leaf node, always
        public void AddRandom(AptNode nodeToAdd, Random r)
        {
            var addIndex = r.Next(this.children.Length);
            if (children[addIndex].type == NodeType.EMPTY)
            {
                children[addIndex] = nodeToAdd;
            }
            else
            {
                children[addIndex].AddRandom(nodeToAdd, r);
            }
        }

        public bool AddLeaf(AptNode leafToAdd)
        {
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].IsEmpty())
                {
                    children[i] = leafToAdd;
                    return true;
                }
                else if (!children[i].IsLeaf() && children[i].AddLeaf(leafToAdd))
                {
                    return true;
                }
            }
            return false;
        }

        public string OpString()
        {
            switch (type)
            {
                case NodeType.EMPTY:
                    return "EMPTY";
                case NodeType.X:
                    return "X";
                case NodeType.Y:
                    return "Y";
                case NodeType.CONSTANT:
                    return value.ToString();
                case NodeType.ABS:
                    return "Abs";
                case NodeType.CLIP:
                    return "Clip";
                case NodeType.NEGATE:
                    return "Negate";
                case NodeType.ADD:
                    return "+";
                case NodeType.SUB:
                    return "-";
                case NodeType.MUL:
                    return "*";
                case NodeType.DIV:
                    return "/";
                case NodeType.SIN:
                    return "Sin";
                case NodeType.COS:
                    return "Cos";
                case NodeType.ATAN:
                    return "Atan";
                case NodeType.ATAN2:
                    return "Atan2";
                case NodeType.CEIL:
                    return "Ceil";
                case NodeType.FLOOR:
                    return "Floor";
                case NodeType.LOG:
                    return "Log";
                case NodeType.SQRT:
                    return "Sqrt";
                case NodeType.MOD:
                    return "%";
                case NodeType.MAX:
                    return "Max";
                case NodeType.MIN:
                    return "Min";
                case NodeType.WRAP:
                    return "Wrap";
                case NodeType.SQUARE:
                    return "Square";
                case NodeType.FBM:
                    return "FBM";
                case NodeType.BILLOW:
                    return "Billow";                
                case NodeType.CELL1:
                    return "Cell1";
                case NodeType.PICTURE:
                    return "Picture-" + ((int)value).ToString();
                default:
                    throw new Exception("corrupt node type in OpString()");
            }
        }


        public string ToLisp()
        {
            switch (type)
            {
                case NodeType.EMPTY:
                case NodeType.X:
                case NodeType.Y:
                case NodeType.CONSTANT:
                    return OpString();
                default:
                    string result = "( " + OpString() + " ";
                    for (int i = 0; i < children.Length; i++)
                    {
                        result += children[i].ToLisp() + " ";
                    }
                    return result + ")";

            }
        }

        public static AptNode GetRandomNode(Random r)
        {
            var enum_size = Enum.GetNames(typeof(NodeType)).Length;
            var typeNum = r.Next(AptNode.NUM_LEAF_TYPES, enum_size);
            var type = (NodeType)typeNum;
            switch (type)
            {
                case NodeType.FBM:                
                case NodeType.BILLOW:
                case NodeType.CELL1:
                    return new AptNode { type = type, children = new AptNode[3] };                    
                case NodeType.ADD:
                case NodeType.SUB:
                case NodeType.MUL:
                case NodeType.DIV:
                case NodeType.ATAN2:
                case NodeType.MIN:
                case NodeType.MAX:
                case NodeType.MOD:
                case NodeType.CLIP:                          
                    return new AptNode { type = type, children = new AptNode[2] };                
                case NodeType.SIN:
                case NodeType.COS:
                case NodeType.ATAN:
                case NodeType.SQUARE:
                case NodeType.LOG:
                case NodeType.FLOOR:
                case NodeType.CEIL:
                case NodeType.SQRT:
                case NodeType.ABS:
                case NodeType.NEGATE:
                case NodeType.WRAP:
                    return new AptNode { type = type, children = new AptNode[1] };
                default:
                    throw new Exception("GetRandomNode failed to match the switch");
            }

        }

        public static AptNode GetRandomLeaf(Random r)
        {
            //We start at 1 because 0 is EMPTY
            var type = (NodeType)r.Next(1, AptNode.NUM_LEAF_TYPES);
            switch (type)
            {
                case NodeType.PICTURE:
                    var p = new AptNode { type = type, children = new AptNode[2], value = r.Next(0, 3) };
                    p.children[0] = new AptNode { type = NodeType.X };
                    p.children[1] = new AptNode { type = NodeType.Y };
                    return p;
                case NodeType.CONSTANT:
                    return new AptNode { type = type, value = (float)r.NextDouble()*2.0f - 1.0f };
                default:
                    return new AptNode { type = type };
            }
        }

        public static AptNode GenerateTree(int nodeCount, Random r)
        {
            AptNode first = GetRandomNode(r);
            for (int i = 1; i < nodeCount; i++)
            {
                first.AddRandom(GetRandomNode(r), r);
            }
            while (first.AddLeaf(GetRandomLeaf(r)))
            {
                //just keep adding leaves until we can't 
            };
            

            return first;
        }
              
    }

}
