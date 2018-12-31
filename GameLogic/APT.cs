using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GameLogic
{
    public enum NodeType : byte { EMPTY = 0,T, CONSTANT, X, Y,PICTURE, IF, ABS,WRAP,CLAMP, NEGATE, ADD, SUB, MUL, DIV, SIN, COS, LOG, ATAN, ATAN2, SQRT, FLOOR, CEIL, MAX, MIN, MOD, SQUARE, FBM, BILLOW, CELL1, WARP1 };

    [DataContract(IsReference = true)]
    public class AptNode
    {        
        [DataMember]
        const int NUM_LEAF_TYPES = 6;
        [DataMember]
        public NodeType type;
        [DataMember]
        public AptNode parent;
        [DataMember]
        public AptNode[] children;
        [DataMember]
        public float value;
        [DataMember]
        public string filename; // for pictures only

        public AptNode()
        {
        }

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
                foreach (var child in children)
                {
                    (node, n) = child.GetNthNode(n - 1);
                    if (node.type != NodeType.EMPTY)
                    {
                        return (node, n);
                    }
                }
            }
            return (new AptNode { type = NodeType.EMPTY }, n);

        }

        public static float Evaluate(AptNode node, float x, float y, float t) {
            StackMachine m = new StackMachine(node);
            var stack = new float[m.nodeCount];
            return m.Execute(x, y, t,stack);
        }

        public static AptNode ConstantFolding(AptNode node)
        {
            var clone = new AptNode { type = node.type, children = new AptNode[node.children.Length]};
            switch (node.type) {
                case NodeType.X:
                case NodeType.T:
                case NodeType.Y:
                case NodeType.CONSTANT:
                    return clone;
                default:
                    bool allConstants = true;
                    
                    for (int i = 0; i < node.children.Length;i++)
                    {
                        clone.children[i] = ConstantFolding(node.children[i]);
                        if (clone.children[i].type != NodeType.CONSTANT) allConstants = false;
                    }

                    if (allConstants)
                    {
                        var v = Evaluate(clone,0,0,0);
                        return new AptNode { type = NodeType.CONSTANT, value = v };
                    }
            return clone;
            }
        }

        public static void ReplaceNode(AptNode nodeToMutate, AptNode newNode,Random r, bool video)
        {
            nodeToMutate.type = newNode.type;
            nodeToMutate.value = newNode.value;
            if (nodeToMutate.children != null && newNode.children != null)
            {
                for (int i = 0; i < nodeToMutate.children.Length; i++)
                {
                    if (i == newNode.children.Length) break;
                    newNode.children[i] = nodeToMutate.children[i];
                }
            }
            while (newNode.AddLeaf(GetRandomLeaf(r,video))) { }
            nodeToMutate.children = newNode.children;
        }

        public void BreedWith(AptNode partner, Random r, bool video)
        {
            var (newNode, _) = partner.GetNthNode(r.Next(0, partner.Count()));
            var (nodeToMutate, _) = this.GetNthNode(r.Next(0, this.Count()));
            ReplaceNode(nodeToMutate, newNode,r,video);
        }

        public AptNode Clone()
        {
            AptNode result = new AptNode { };
            result.type = type;
            result.value = value;
            result.filename = filename;
            if (children != null)
            {
                result.children = new AptNode[children.Length];
                for (int i = 0; i < children.Length;i++)
                {
                    result.children[i] = children[i].Clone();
                }
            }
            return result;
        }

        public void InsertWarp(Random r, bool video)
        {
            var node = this;
            if (node.children == null) return;

            //1 in 3 chance of applying warp to a node that has an X and a Y as children
            if (node.children.Length >= 2 && r.Next(0, 3) == 0 &&
                ((node.children[0].type == NodeType.X && node.children[1].type == NodeType.Y) ||
                (node.children[1].type == NodeType.X && node.children[0].type == NodeType.Y)))
            {

                Console.WriteLine("WARP");
                var newChildren = new AptNode[node.children.Length - 1];
                var warp = new AptNode { type = NodeType.WARP1, children = new AptNode[5] };
                warp.children[0] = new AptNode { type = NodeType.X };
                warp.children[0].parent = warp;
                warp.children[1] = new AptNode { type = NodeType.Y };
                warp.children[1].parent = warp;
                warp.children[4] = new AptNode { type = NodeType.CONSTANT, value = (float)r.NextDouble() * 2.0f - 1.0f };
                warp.children[4].parent = warp;

                //fill in the stuff the warp node needs
                while (warp.AddLeaf(GetRandomLeaf(r,video)))
                {
                }
                newChildren[0] = warp;
                warp.parent = node;
                for (int i = 1; i < newChildren.Length; i++)
                {
                    newChildren[i] = node.children[i + 1];
                }
                node.children = newChildren;

            }
            else
            {
                foreach (var child in node.children)
                {
                    child.InsertWarp(r,video);
                }
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
            if (children[addIndex] == null || children[addIndex].type == NodeType.EMPTY)
            {
                children[addIndex] = nodeToAdd;
                nodeToAdd.parent = this;
            }
            else
            {
                children[addIndex].AddRandom(nodeToAdd, r);
            }
        }

        public bool AddLeaf(AptNode leafToAdd)
        {
            if (children == null) return false;
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] == null || children[i].IsEmpty())
                {
                    children[i] = leafToAdd;
                    leafToAdd.parent = this;
                    return true;
                }
                else if (!children[i].IsLeaf() && children[i].AddLeaf(leafToAdd))
                {
                    return true;
                }
            }
            return false;
        }

        public static string OpString(NodeType type)
        {
            return new AptNode { type = type }.OpString();
        }

        public string OpString()
        {
            switch (type)
            {
                case NodeType.EMPTY:
                    return "EMPTY";
                case NodeType.T:
                    return "T";
                case NodeType.X:
                    return "X";
                case NodeType.Y:
                    return "Y";
                case NodeType.CONSTANT:
                    return value.ToString("0.000");
                case NodeType.ABS:
                    return "Abs";                
                case NodeType.CLAMP:
                    return "Clamp";
                case NodeType.WRAP:
                    return "Wrap";
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
                case NodeType.IF:
                    return "If";
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
                case NodeType.SQUARE:
                    return "Square";
                case NodeType.FBM:
                    return "FBM";
                case NodeType.BILLOW:
                    return "Billow";
                case NodeType.CELL1:
                    return "Cell1";
                case NodeType.WARP1:
                    return "Warp1";
                case NodeType.PICTURE:
                    return "Picture";
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
                case NodeType.T:
                case NodeType.CONSTANT:
                    return OpString();
                case NodeType.PICTURE:

                default:
                    string result = "( " + OpString() + " ";
                    if (type == NodeType.PICTURE)
                    {
                       result += "\"" + filename + "\" ";
                    }
                    for (int i = 0; i < children.Length; i++)
                    {
                        result += children[i].ToLisp() + " ";
                    }
                    return result + ")";
            }
        }

        public static AptNode MakeNode(NodeType type)
        {
            AptNode result;
            switch (type)
            {
                case NodeType.FBM:
                case NodeType.BILLOW:                    
                case NodeType.CELL1:
                case NodeType.IF:
                    result = new AptNode { type = type, children = new AptNode[3] };
                    break;
                case NodeType.ADD:
                case NodeType.SUB:
                case NodeType.MUL:
                case NodeType.DIV:
                case NodeType.ATAN2:
                case NodeType.MIN:
                case NodeType.MAX:
                case NodeType.MOD:                
                    result = new AptNode { type = type, children = new AptNode[2] };
                    break;
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
                case NodeType.CLAMP:
                case NodeType.WRAP:                
                    result = new AptNode { type = type, children = new AptNode[1] };
                    break;
                case NodeType.X:
                case NodeType.Y:
                case NodeType.T:
                    result = new AptNode { type = type };
                    break;
                case NodeType.PICTURE:
                    result = new AptNode { type = type, children = new AptNode[2]};
                    break;                                
                case NodeType.WARP1:
                    result =new AptNode { type = type, children = new AptNode[5] };
                    break;
                default:
                    throw new Exception("MakeNode failed to match the switch");
            }
            return result;
        }

        public static AptNode GetRandomNode(Random r)
        {
            var enum_size = Enum.GetNames(typeof(NodeType)).Length;
            //-1 because we don't include warp
            var typeNum = r.Next(AptNode.NUM_LEAF_TYPES, enum_size - 1);
            var type = (NodeType)typeNum;
            return MakeNode(type);

        }

        public static AptNode GetRandomLeaf(Random r, bool videoMode)
        {            
            var picChance = r.Next(0, 3);
            NodeType type;
            //We start at 2 because 0 is EMPTY  and 1 is Time for videos only          
            int start = 2;
            if (videoMode) start = 1;            
            if (picChance == 0)
            {
                type = (NodeType)r.Next(start, NUM_LEAF_TYPES);
            }
            else
            {
                type = (NodeType)r.Next(start, NUM_LEAF_TYPES - 1);
            }

            switch (type)
            {
                case NodeType.PICTURE:
                    {
                        var pic = GameState.externalImages[r.Next(0, GameState.externalImages.Count)];
                        var result = new AptNode { type = type, children = new AptNode[2], filename = pic.filename };
                        result.children[0] = new AptNode { type = NodeType.X };
                        result.children[1] = new AptNode { type = NodeType.Y };
                        return result;
                    }
                case NodeType.CONSTANT:
                    return new AptNode { type = type, value = (float)r.NextDouble() * 2.0f - 1.0f };
                default:
                    return new AptNode { type = type };
            }
        }

        public static AptNode GenerateTree(int nodeCount, Random r,bool video)
        {
            AptNode first = GetRandomNode(r);
            for (int i = 1; i < nodeCount; i++)
            {
                first.AddRandom(GetRandomNode(r), r);
            }
            while (first.AddLeaf(GetRandomLeaf(r,video)))
            {
                //just keep adding leaves until we can't 
            };
            first.InsertWarp(r,video);

            return first;
        }

        public void Mutate(Random r, bool video)
        {
            var nodeIndex = r.Next(0, Count());
            var (nodeToMutate, _) = GetNthNode(nodeIndex);
            var leafChance = r.Next(0, Settings.MUTATE_LEAF_CHANCE);

            AptNode newNode;
            if (leafChance == 0)
            {
                newNode = GetRandomLeaf(r,video);
            }
            else
            {
                newNode = GetRandomNode(r);
            }
            ReplaceNode(nodeToMutate, newNode, r,video);
        }
    }

}
