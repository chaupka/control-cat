using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using System.Linq;
using System;

namespace DungeonGeneration
{
    public static class VentGenerator
    {
        public static void Create(
            in HashSet<RoomNode> rooms,
            HashSet<VentNode> vents,
            DungeonGeneratorDataSO parameters,
            Random r
        )
        {
            foreach (var v in vents)
            {
                var parent = rooms.FirstOrDefault(r => r.children.Contains(v));
                (v.parentVentPositions, v.parentPlatformPositions) = ChooseVentPositions(
                    parent,
                    parameters,
                    r
                );
                (v.childVentPositions, v.childPlatformPositions) = ChooseVentPositions(
                    v.child,
                    parameters,
                    r
                );
            }
        }

        private static Tuple<HashSet<Vector2Int>, HashSet<Vector2Int>> ChooseVentPositions(
            RoomNode parent,
            DungeonGeneratorDataSO parameters,
            Random r
        )
        {
            var dist = new Vector2Int(
                r.Next(parameters.vent.minDistWidth, parameters.vent.maxDistWidth + 1),
                r.Next(parameters.vent.minDistHeight, parameters.vent.maxDistHeight + 1)
            );
            var positions = ProceduralGenerationAlgorithms.GenerateBoundsWithDist(dist);
            Vector2Int startPosition = Vector2Int.zero;
            bool foundPositions = false;
            while (!foundPositions)
            {
                startPosition = parent.tilePositions.ElementAt(r.Next(parent.tilePositions.Count));
                if (!positions.Any(p => !parent.tilePositions.Contains(startPosition + p)))
                {
                    foundPositions = true;
                }
            }
            var vent = new HashSet<Vector2Int>
            {
                startPosition,
                startPosition + Vector2Int.left,
                startPosition + Vector2Int.up,
                startPosition + new Vector2Int(-1, 1)
            };
            var platform = positions.Select(p => startPosition + p).ToHashSet();
            platform.ExceptWith(vent);
            platform.RemoveWhere(p => vent.Any(v => p.y > v.y));

            return new(vent, platform);
        }
    }
}
