using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace DungeonGeneration
{
    public static class TerrainGenerator
    {
        public static HashSet<Vector2Int> CreateWalls(HashSet<Vector2Int> backPositions)
        {
            return FindWallsInDirections(backPositions);
        }

        public static HashSet<Vector2Int> CreatePlatforms(
            HashSet<Vector2Int> backPositions,
            HashSet<Vector2Int> walls,
            DungeonGeneratorDataSO parameters,
            Random r
        )
        {
            var platforms = ArrangePlatforms(walls, backPositions, parameters, r);
            backPositions.ExceptWith(platforms);
            return platforms;
        }

        public static HashSet<Vector2Int> CreateRestOfDungeon(
            HashSet<Vector2Int> notToBeFilled,
            DungeonGeneratorDataSO parameters
        )
        {
            return FillRestOfDungeon(notToBeFilled, parameters.dungeon.filledOffset);
        }

        private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> backPositions)
        {
            var wallPositions = new HashSet<Vector2Int>();
            foreach (var position in backPositions)
            {
                foreach (var direction in Direction2D.eightDirectionsList)
                {
                    var neighbourPosition = position + direction;
                    if (!backPositions.Contains(neighbourPosition))
                        wallPositions.Add(neighbourPosition);
                }
            }
            return wallPositions;
        }

        private static HashSet<Vector2Int> ArrangePlatforms(
            HashSet<Vector2Int> walls,
            HashSet<Vector2Int> backPositions,
            DungeonGeneratorDataSO parameters,
            Random r
        )
        {
            var terrain = new HashSet<Vector2Int>(walls);
            var platforms = new HashSet<Vector2Int>();
            var potPlatformPositions = new HashSet<Vector2Int>(backPositions);
            var minDist = new Vector2Int(
                parameters.platform.minDistWidth,
                parameters.platform.minDistHeight
            );
            ExcludePotentialPlatformsByBounds(parameters, potPlatformPositions, walls, r, minDist);

            while (potPlatformPositions.Count > 0)
            {
                var randomStart = new List<Vector2Int>(potPlatformPositions)[
                    r.Next(potPlatformPositions.Count)
                ];
                var randomWalk = r.Next(
                    parameters.platform.minRandomWalk,
                    parameters.platform.maxRandomWalk
                );
                var path = ProceduralGenerationAlgorithms.SimpleRandomWalkWithDynamicBoundCheck(
                    randomStart,
                    randomWalk,
                    r,
                    minDist,
                    terrain
                );
                ExcludePotentialPlatformsByBounds(parameters, potPlatformPositions, path, r);
                terrain.UnionWith(path);
                platforms.UnionWith(path);
            }
            return platforms;
        }

        private static void ExcludePotentialPlatformsByBounds(
            DungeonGeneratorDataSO parameters,
            HashSet<Vector2Int> potPlatformPositions,
            HashSet<Vector2Int> uncheckedTerrain,
            Random r,
            Vector2Int dist = default
        )
        {
            dist =
                dist == default
                    ? new Vector2Int(
                        r.Next(
                            parameters.platform.minDistWidth,
                            parameters.platform.maxDistWidth + 1
                        ),
                        r.Next(
                            parameters.platform.minDistHeight,
                            parameters.platform.maxDistHeight + 1
                        )
                    )
                    : dist;
            var exclusionBounds = ProceduralGenerationAlgorithms.GenerateBoundsWithDist(dist);
            foreach (var position in uncheckedTerrain)
            {
                potPlatformPositions.ExceptWith(exclusionBounds.Select(bound => position + bound));
            }
        }

        private static HashSet<Vector2Int> FillRestOfDungeon(
            HashSet<Vector2Int> notToBeFilled,
            int filledOffset
        )
        {
            var xOrdered = notToBeFilled.OrderBy(n => n.x);
            var yOrdered = notToBeFilled.OrderBy(n => n.y);
            var xMin = xOrdered.First().x;
            var yMin = yOrdered.First().y;
            var xSize = xOrdered.Last().x - xMin + 1;
            var ySize = yOrdered.Last().y - yMin + 1;
            var bounds = new BoundsInt(
                new Vector3Int(xMin - filledOffset, yMin - filledOffset),
                new Vector3Int(xSize + 2 * filledOffset, ySize + 2 * filledOffset)
            );
            var toBeFilled = ProceduralGenerationAlgorithms.FillRectangle(bounds);
            toBeFilled.ExceptWith(notToBeFilled);
            return toBeFilled;
        }
    }
}
