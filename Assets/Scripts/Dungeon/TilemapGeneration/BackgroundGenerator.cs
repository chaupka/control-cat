using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace DungeonGeneration
{
    public static class BackgroundGenerator
    {
        public static void Create(
            HashSet<RoomNode> rooms,
            HashSet<CorridorNode> corridors,
            DungeonGeneratorDataSO parameters,
            Random r
        )
        {
            if (parameters.room.isRandomWalking)
            {
                CreateRoomsRandomly(rooms, parameters, r);
            }
            else
            {
                CreateSimpleRooms(rooms, parameters.room.offset);
            }

            EmployMinimumSpaceWorker(rooms, parameters, r);
            if (rooms != null)
            {
                RemoveOutOfBoundsAndFillRoomGaps(rooms, parameters.room.offset);
            }

            IncreaseCorridorsSize(corridors, parameters, r);
        }

        public static void CreateSimpleRooms(HashSet<RoomNode> roomsList, int offset)
        {
            foreach (var room in roomsList)
            {
                room.tilePositions = ProceduralGenerationAlgorithms.FillRectangle(
                    room.bounds,
                    offset
                );
            }
        }

        public static void CreateRoomsRandomly(
            HashSet<RoomNode> roomsList,
            DungeonGeneratorDataSO parameters,
            Random r
        )
        {
            foreach (var room in roomsList)
            {
                var roomCenter = new Vector2Int(
                    Mathf.RoundToInt(room.bounds.center.x),
                    Mathf.RoundToInt(room.bounds.center.y)
                );
                room.tilePositions = RunRandomWalk(roomCenter, room.bounds, parameters, r);
            }
        }

        private static HashSet<Vector2Int> RunRandomWalk(
            Vector2Int position,
            BoundsInt roomBounds,
            DungeonGeneratorDataSO parameters,
            Random r
        )
        {
            var currentPosition = position;
            var backPositions = new HashSet<Vector2Int>();
            var iterations = r.Next(parameters.room.minIterations, parameters.room.maxIterations);
            for (int i = 0; i < iterations; i++)
            {
                var path = ProceduralGenerationAlgorithms.SimpleRandomWalkWithBoundCheck(
                    currentPosition,
                    parameters,
                    r,
                    roomBounds
                );
                backPositions.UnionWith(path);
                if (parameters.room.isStartingRandomlyEachIteration)
                    currentPosition = backPositions.ElementAt(r.Next(0, backPositions.Count));
            }
            return backPositions;
        }

        private static void RemoveOutOfBoundsAndFillRoomGaps(
            HashSet<RoomNode> roomsList,
            int offset
        )
        {
            foreach (var room in roomsList)
            {
                HashSet<Vector2Int> newBackForRoom;
                do
                {
                    var potentialRoom = ProceduralGenerationAlgorithms.FillRectangle(
                        room.bounds,
                        offset
                    );
                    newBackForRoom = new HashSet<Vector2Int>();
                    foreach (var position in potentialRoom)
                    {
                        int directions = 0;
                        foreach (var direction in Direction2D.cardinalDirectionsList)
                        {
                            if (room.tilePositions.Contains(position + direction))
                            {
                                directions += 1;
                            }
                        }
                        if (!room.tilePositions.Contains(position) && directions > 2)
                        {
                            newBackForRoom.Add(position);
                            room.tilePositions.UnionWith(newBackForRoom);
                        }
                    }
                    room.tilePositions.RemoveWhere(pos => !potentialRoom.Contains(pos));
                } while (newBackForRoom.Count > 0);
            }
        }

        #region increase corridors size
        public static void IncreaseCorridorsSize(
            HashSet<CorridorNode> corridors,
            DungeonGeneratorDataSO parameters,
            Random r
        )
        {
            foreach (var c in corridors)
            {
                c.tilePositions = IncreaseCorridorBy(c.tilePositions, parameters, r);
            }
        }

        private static List<Vector2Int> IncreaseCorridorBy(
            List<Vector2Int> corridor,
            DungeonGeneratorDataSO parameters,
            Random r
        )
        {
            var newCorridor = new List<Vector2Int>();
            var dist = r.Next(parameters.corridor.minWidth, parameters.corridor.maxWidth + 1);
            for (int i = 1; i < corridor.Count; i++)
            {
                var walkDirection = corridor[i] - corridor[i - 1];
                var width = r.Next(parameters.corridor.minWidth, parameters.corridor.maxWidth + 1);
                var startLeft = r.Next(2 - width, 0);
                for (int j = startLeft; j < startLeft + width; j++)
                {
                    var startDown = dist / -2;
                    for (int k = startDown; k < startDown + dist; k++)
                    {
                        var newTile =
                            corridor[i - 1]
                            + ProceduralGenerationAlgorithms.RotateWalkPosition(
                                walkDirection,
                                new Vector2Int(j, k)
                            );
                        newCorridor.Add(newTile);
                    }
                }
            }
            return newCorridor;
        }

        #endregion

        #region minimum space
        private static void EmployMinimumSpaceWorker(
            HashSet<RoomNode> rooms,
            DungeonGeneratorDataSO parameters,
            Random r
        )
        {
            switch (parameters.minimumSpaceWorker)
            {
                case MinimumSpaceWorker.OnlyCheckOrthogonalMinimumSpace:
                    OnlyCheckOrthogonalMinimumSpace(rooms, parameters.minSpace, r);
                    break;
                case MinimumSpaceWorker.TakeScreenShot:
                    TakeScreenShot(rooms, parameters.minSpace, r);
                    break;
                case MinimumSpaceWorker.WatchTheOther:
                default:
                    WatchTheOther(rooms, parameters.minSpace, r);
                    break;
            }
        }

        private static void OnlyCheckOrthogonalMinimumSpace(
            HashSet<RoomNode> rooms,
            int minSpace,
            Random r
        )
        {
            foreach (var room in rooms)
            {
                var oldBackPositions = new HashSet<Vector2Int>(room.tilePositions);
                var newBackPositions = new HashSet<Vector2Int>(room.tilePositions);
                do
                {
                    newBackPositions = FindMinimumOrthogonalSpace(
                        oldBackPositions,
                        newBackPositions,
                        minSpace,
                        r
                    );
                    oldBackPositions.UnionWith(newBackPositions);
                } while (newBackPositions.Count > 0);
                room.tilePositions.UnionWith(oldBackPositions);
            }
        }

        private static void TakeScreenShot(HashSet<RoomNode> rooms, int minSpace, Random r)
        {
            foreach (var room in rooms)
            {
                var oldBackPositions = new HashSet<Vector2Int>(room.tilePositions);
                var newBackPositions = new HashSet<Vector2Int>(room.tilePositions);
                do
                {
                    var newOrthogonalPositions = FindMinimumOrthogonalSpace(
                        oldBackPositions,
                        newBackPositions,
                        minSpace,
                        r
                    );
                    var newDiagonalPositions = FindMinimumDiagonalSpace(
                        oldBackPositions,
                        newBackPositions,
                        minSpace
                    );
                    newBackPositions = newOrthogonalPositions;
                    newBackPositions.UnionWith(newDiagonalPositions);
                    oldBackPositions.UnionWith(newBackPositions);
                } while (newBackPositions.Count > 0);
                room.tilePositions.UnionWith(oldBackPositions);
            }
        }

        private static void WatchTheOther(HashSet<RoomNode> rooms, int minSpace, Random r)
        {
            foreach (var room in rooms)
            {
                var oldBackPositions = new HashSet<Vector2Int>(room.tilePositions);
                var newBackPositions = new HashSet<Vector2Int>(room.tilePositions);
                var newOrthogonalPositions = new HashSet<Vector2Int>(newBackPositions);
                var newDiagonalPositions = new HashSet<Vector2Int>();
                do
                {
                    do
                    {
                        newBackPositions = FindMinimumOrthogonalSpace(
                            oldBackPositions,
                            newBackPositions,
                            minSpace,
                            r
                        );
                        newOrthogonalPositions.UnionWith(newBackPositions);
                    } while (newBackPositions.Count > 0);
                    newBackPositions.UnionWith(newOrthogonalPositions);
                    oldBackPositions.UnionWith(newBackPositions);
                    newOrthogonalPositions = new HashSet<Vector2Int>();

                    do
                    {
                        newBackPositions = FindMinimumDiagonalSpace(
                            oldBackPositions,
                            newBackPositions,
                            minSpace
                        );
                        newDiagonalPositions.UnionWith(newBackPositions);
                    } while (newBackPositions.Count > 0);
                    newBackPositions.UnionWith(newDiagonalPositions);
                    oldBackPositions.UnionWith(newBackPositions);
                    newDiagonalPositions = new HashSet<Vector2Int>();
                } while (newBackPositions.Count > 0);
                room.tilePositions.UnionWith(oldBackPositions);
            }
        }

        private static HashSet<Vector2Int> FindMinimumOrthogonalSpace(
            HashSet<Vector2Int> backPositions,
            HashSet<Vector2Int> previousPositions,
            int minSpace,
            Random r
        )
        {
            var newMinimumSpacePositions = new HashSet<Vector2Int>();
            foreach (var position in previousPositions)
            {
                foreach (var direction in new List<Vector2Int>() { new(0, 1), new(1, 0), })
                {
                    List<Vector2Int> directionHits = FindDirectionalBackHits(
                        backPositions,
                        minSpace,
                        position,
                        direction
                    );
                    FillUpOrthogonalSpace(
                        backPositions,
                        minSpace,
                        newMinimumSpacePositions,
                        direction,
                        directionHits,
                        r
                    );
                }
            }
            return newMinimumSpacePositions;
        }

        private static HashSet<Vector2Int> FindMinimumDiagonalSpace(
            HashSet<Vector2Int> backPositions,
            HashSet<Vector2Int> previousPositions,
            int minSpace
        )
        {
            var newMinimumSpacePositions = new HashSet<Vector2Int>();
            foreach (var position in previousPositions)
            {
                List<Vector2Int> upLeftHits = FindDirectionalBackHits(
                    backPositions,
                    minSpace / 2 + minSpace % 2,
                    position,
                    new Vector2Int(-1, 1)
                );
                List<Vector2Int> upRightHits = FindDirectionalBackHits(
                    backPositions,
                    minSpace / 2 + minSpace % 2,
                    position,
                    new Vector2Int(1, 1)
                );
                if (upLeftHits.Count() < minSpace && upRightHits.Count >= minSpace)
                {
                    FillUpDiagonalSpace(
                        backPositions,
                        minSpace,
                        newMinimumSpacePositions,
                        new Vector2Int(-1, 1),
                        upLeftHits,
                        position
                    );
                }
                if (upLeftHits.Count() >= minSpace && upRightHits.Count < minSpace)
                {
                    FillUpDiagonalSpace(
                        backPositions,
                        minSpace,
                        newMinimumSpacePositions,
                        new Vector2Int(1, 1),
                        upRightHits,
                        position
                    );
                }
            }
            return newMinimumSpacePositions;
        }

        private static List<Vector2Int> FindDirectionalBackHits(
            HashSet<Vector2Int> backPositions,
            int minimumSpace,
            Vector2Int position,
            Vector2Int direction
        )
        {
            var directionHits = new List<Vector2Int>() { position };
            var iterVector = new Vector2Int(1, 0);
            var signChange = -1;
            for (
                int distance = 1;
                Mathf.Abs(distance) < minimumSpace;
                distance += distance > 0 ? iterVector.x : iterVector.y
            )
            {
                if (iterVector.Equals(Vector2Int.zero))
                {
                    break;
                }
                var neighbourPosition = position + direction * distance;
                if (!backPositions.Contains(neighbourPosition))
                {
                    if (distance > 0)
                    {
                        if (signChange == 1)
                        {
                            break;
                        }
                        iterVector = new Vector2Int(0, -1);
                        if (distance == 1)
                        {
                            distance = 0;
                        }
                    }
                    else
                    {
                        iterVector = new Vector2Int(iterVector.x, 0);
                    }
                    distance *= signChange;
                    signChange = 1;
                    continue;
                }
                directionHits.Add(neighbourPosition);
                distance *= signChange;
            }
            return directionHits.OrderBy(vector => vector.y).ThenBy(vector => vector.x).ToList();
        }

        private static void FillUpOrthogonalSpace(
            HashSet<Vector2Int> backPositions,
            int minimumSpace,
            HashSet<Vector2Int> newMinimumSpacePositions,
            Vector2Int direction,
            List<Vector2Int> directionHits,
            Random r
        )
        {
            while (directionHits.Count < minimumSpace)
            {
                var otherDirection = Vector2Int.RoundToInt(
                    Quaternion.AngleAxis(90, Vector3.forward) * (Vector2)direction
                );
                var startOfHits = r.Next(2) == 0;
                Vector2Int newNeighbor;
                newNeighbor = startOfHits
                    ? directionHits.First() - direction
                    : directionHits.Last() + direction;
                if (
                    backPositions.Contains(newNeighbor + otherDirection)
                    || backPositions.Contains(newNeighbor - otherDirection)
                )
                {
                    directionHits.Insert(startOfHits ? 0 : directionHits.Count, newNeighbor);
                }
                else
                {
                    newNeighbor = startOfHits
                        ? directionHits.Last() + direction
                        : directionHits.First() - direction;
                    directionHits.Insert(startOfHits ? directionHits.Count : 0, newNeighbor);
                }
                newMinimumSpacePositions.Add(newNeighbor);
                backPositions.UnionWith(newMinimumSpacePositions);
            }
        }

        private static void FillUpDiagonalSpace(
            HashSet<Vector2Int> backPositions,
            int minimumSpace,
            HashSet<Vector2Int> newMinimumSpacePositions,
            Vector2Int direction,
            List<Vector2Int> directionHits,
            Vector2Int position
        )
        {
            while (directionHits.Count < minimumSpace)
            {
                var startOfHits = directionHits.IndexOf(position) < minimumSpace / 2;
                Vector2Int newNeighbor;
                newNeighbor = startOfHits
                    ? directionHits.First() - direction
                    : directionHits.Last() + direction;
                directionHits.Insert(startOfHits ? 0 : directionHits.Count, newNeighbor);
                newMinimumSpacePositions.Add(newNeighbor);
                backPositions.UnionWith(newMinimumSpacePositions);
            }
        }
        #endregion
    }
}
