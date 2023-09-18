using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{
    public abstract class Node : ScriptableObject
    {
        [HideInInspector]
        public bool started = false;

        [HideInInspector]
        public string guid;

        [HideInInspector]
        public Vector2 position;

        [TextArea]
        public string description;

        public virtual Node Clone(Dictionary<Node, Node> traversedNodes = default)
        {
            return Instantiate(this);
        }
    }
}
