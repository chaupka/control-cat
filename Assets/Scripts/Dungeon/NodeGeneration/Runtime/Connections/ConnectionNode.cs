using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DungeonGeneration
{
    public abstract class ConnectionNode : Node
    {
        public RoomNode child;

        public override Node Clone(Dictionary<Node, Node> traversedNodes = default)
        {
            traversedNodes ??= new Dictionary<Node, Node>();
            if (traversedNodes.Keys.Contains(this))
            {
                return this;
            }
            ConnectionNode node = Instantiate(this);
            traversedNodes.Add(this, node);
            node.child = (RoomNode)child.Clone(traversedNodes);
            return node;
        }
    }
}
