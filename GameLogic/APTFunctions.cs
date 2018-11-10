using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameInterface;

namespace GameLogic
{
    public static class APTFunctions
    {
        public static void Mutate(AptNode node, Random r)
        {
            var nodeIndex = r.Next(0, node.Count());
            var (nodeToMutate, _) = node.GetNthNode(nodeIndex);
            var leafChance = r.Next(0, Settings.MUTATE_LEAF_CHANCE);

            AptNode newNode;
            if (leafChance == 0)
            {
                newNode = AptNode.GetRandomLeaf(r);
            }
            else
            {
                newNode = AptNode.GetRandomNode(r);
            }
            AptNode.ReplaceNode(nodeToMutate, newNode, r);
        }
    }
}
