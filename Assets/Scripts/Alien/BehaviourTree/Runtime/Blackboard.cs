using UnityEngine;

namespace TheKiwiCoder
{

    // This is the blackboard container shared between all nodes.
    // Use this to store temporary data that multiple nodes need read and write access to.
    // Add other properties here that make sense for your specific use case.
    [System.Serializable]
    public class Blackboard {
        public Vector2 moveToPosition;
        public float moveToHeadAngle;
        public bool isAgitated;
        public bool isChasing;
        public float agitateTimer;
        public Vector2 searchPosition;
    }
}