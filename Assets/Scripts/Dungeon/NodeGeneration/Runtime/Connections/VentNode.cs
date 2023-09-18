using System.Collections.Generic;
using UnityEngine;

namespace DungeonGeneration
{
    public class VentNode : ConnectionNode
    {
        public HashSet<Vector2Int> parentPlatformPositions;
        public HashSet<Vector2Int> parentVentPositions;
        public HashSet<Vector2Int> childPlatformPositions;
        public HashSet<Vector2Int> childVentPositions;
    }
}
