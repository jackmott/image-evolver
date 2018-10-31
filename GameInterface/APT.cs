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
    public enum NodeType : byte { EMPTY = 0, CONSTANT, X, Y, ADD, SUB,MUL,DIV,SIN };

    

 

    public struct AptNode
    {
        const int NUM_LEAF_TYPES = 4;        
        public NodeType type;        
        public AptNode[] children;        
        public float value;

      

        public bool IsLeaf() {
            return !IsEmpty() && (int)type < NUM_LEAF_TYPES;
        }

        public bool IsEmpty() {
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
                default:
                    throw new Exception("corrupt node type in OpString()");
            }
        }

       
        public string ToLisp()
        {
            switch (type) {
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
                    return result +")";                
                    
            }
        }

        public static AptNode GetRandomNode(Random r)
        {
            var enum_size = Enum.GetNames(typeof(NodeType)).Length;
            var typeNum = r.Next(AptNode.NUM_LEAF_TYPES, enum_size);
            var type = (NodeType)typeNum;
            switch (type) {
                case NodeType.ADD:
                case NodeType.SUB:
                case NodeType.MUL:
                case NodeType.DIV:
                    return new AptNode { type = type, children = new AptNode[2] };
                case NodeType.SIN:
                    return new AptNode { type = type, children = new AptNode[1] };
                default:
                    throw new Exception("GetRandomNode failed to match the switch");
            }

        }

        public static AptNode GetRandomLeaf(Random r)
        {
            //We start at 1 because 0 is EMPTY
            var type = (NodeType)r.Next(1, AptNode.NUM_LEAF_TYPES);
            switch (type) {
                case NodeType.CONSTANT:
                    return new AptNode { type = type, value = (float)r.NextDouble() };
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
            while (first.AddLeaf(GetRandomLeaf(r))) {
                //just keepa adding leaves until we can't 
            };
            return first;
        }

        public float Eval(float x, float y)
        {
            switch (type) {
                case NodeType.X:
                    return x;
                case NodeType.Y:
                    return y;
                case NodeType.CONSTANT:
                    return value;
                case NodeType.ADD:
                    return children[0].Eval(x, y) + children[1].Eval(x, y);
                case NodeType.SUB:
                    return children[0].Eval(x, y) - children[1].Eval(x, y);
                case NodeType.MUL:
                    return children[0].Eval(x, y) * children[1].Eval(x, y);
                case NodeType.DIV:
                    return children[0].Eval(x, y) / children[1].Eval(x, y);
                case NodeType.SIN:
                    return (float)Math.Sin(children[0].Eval(x, y));
                default:
                    throw new Exception("Eval found a bad node");
            }
        }        
    }

}
