using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DungeonGeneration
{
    public abstract class RoomNode : Node
    {
        public List<ConnectionNode> children = new();
        public BoundsInt bounds;
        public HashSet<Vector2Int> tilePositions;

        public override Node Clone(Dictionary<Node, Node> traversedNodes = default)
        {
            traversedNodes ??= new Dictionary<Node, Node>();
            if (traversedNodes.Keys.Contains(this))
            {
                return traversedNodes[this];
            }
            RoomNode node = Instantiate(this);
            traversedNodes.Add(this, node);
            node.children = children
                .Select(c => c.Clone(traversedNodes))
                .Cast<ConnectionNode>()
                .ToList();
            return node;
        }
    }
}
