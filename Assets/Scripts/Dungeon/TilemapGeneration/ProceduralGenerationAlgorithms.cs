using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace DungeonGeneration
{
    public static class ProceduralGenerationAlgorithms
    {
        public static HashSet<Vector2Int> SimpleRandomWalk(
            Vector2Int startPosition,
            int walkLength,
            Random r
        )
        {
            var path = new HashSet<Vector2Int> { startPosition };
            var previousPosition = startPosition;

            for (int i = 0; i < walkLength; i++)
            {
                var newPosition = previousPosition + Direction2D.GetRandomCardinalDirection(r);
                path.Add(newPosition);
                previousPosition = newPosition;
            }
            return path;
        }

        public static HashSet<Vector2Int> SimpleRandomWalkWithBoundCheck(
            Vector2Int startPosition,
            DungeonGeneratorDataSO parameters,
            Random r,
            BoundsInt roomBounds
        )
        {
            var path = new HashSet<Vector2Int> { startPosition };
            var previousPosition = startPosition;
            var walkLength = r.Next(parameters.room.minWalkLength, parameters.room.maxWalkLength);
            for (int i = 0; i < walkLength; i++)
            {
                Vector2Int newPosition;
                var excludedDirections = new List<Vector2Int>();
                do
                {
                    var direction = Direction2D.GetRandomCardinalDirection(r, excludedDirections);
                    excludedDirections.Add(direction);
                    newPosition = previousPosition + direction;
                } while (
                    newPosition.x < (roomBounds.xMin + parameters.room.offset)
                    || newPosition.x > (roomBounds.xMax - parameters.room.offset)
                        && newPosition.y < (roomBounds.yMin + parameters.room.offset)
                        && newPosition.y > (roomBounds.yMax - parameters.room.offset)
                );
                path.Add(newPosition);
                previousPosition = newPosition;
            }
            return path;
        }

        public static HashSet<Vector2Int> SimpleRandomWalkWithDynamicBoundCheck(
            Vector2Int startPosition,
            int walkLength,
            Random r,
            Vector2Int dist,
            HashSet<Vector2Int> terrain
        )
        {
            var path = new HashSet<Vector2Int> { startPosition };
            var previousPosition = startPosition;

            for (int i = 0; i < walkLength; i++)
            {
                var exclusionBounds = GenerateBoundsWithDist(dist);
                Vector2Int newPosition;
                var excludedDirections = new List<Vector2Int>();
                do
                {
                    if (excludedDirections.Count == 4)
                    {
                        return path;
                    }
                    var direction = Direction2D.GetRandomCardinalDirection(r, excludedDirections);
                    excludedDirections.Add(direction);
                    newPosition = previousPosition + direction;
                } while (
                    terrain.Intersect(exclusionBounds.Select(bound => newPosition + bound)).Any()
                );
                path.Add(newPosition);
                previousPosition = newPosition;
            }
            return path;
        }

        public static HashSet<Vector2Int> GenerateBoundsWithDist(Vector2Int dist)
        {
            var exclusionBounds = new HashSet<Vector2Int>();
            for (int col = -dist.x; col <= dist.x; col++)
            {
                for (int row = -dist.y; row <= dist.y; row++)
                {
                    var position = new Vector2Int(col, row);
                    exclusionBounds.Add(position);
                }
            }
            return exclusionBounds;
        }

        public static HashSet<Vector2Int> FillRectangle(BoundsInt room, int offset = 0)
        {
            var roomBack = new HashSet<Vector2Int>();
            for (int col = offset; col < room.size.x - offset; col++)
            {
                for (int row = offset; row < room.size.y - offset; row++)
                {
                    var position = (Vector2Int)room.min + new Vector2Int(col, row);
                    roomBack.Add(position);
                }
            }
            return roomBack;
        }

        public static List<BoundsInt> BinarySpacePartitioning(
            Random r,
            DungeonGeneratorDataSO parameters,
            DungeonTree tree
        )
        {
            var (minWidth, minHeight, number) = parameters.room;
            var mainRoomNumber = tree.nodes.Where(node => node is RoomNode).ToList().Count;
            var roomsQueue = new Queue<BoundsInt>();
            roomsQueue.Enqueue(parameters.dungeon.bounds);
            var roomsList = new List<BoundsInt>();
            while (roomsQueue.Count > 0)
            {
                var room = roomsQueue.Dequeue();
                if (room.size.y >= minHeight && room.size.x >= minWidth)
                {
                    bool split;
                    if (r.Next(2) == 0)
                    {
                        split =
                            SplitHorizontally(minHeight, roomsQueue, room, r)
                            || SplitVertically(minWidth, roomsQueue, room, r);
                    }
                    else
                    {
                        split =
                            SplitVertically(minWidth, roomsQueue, room, r)
                            || SplitHorizontally(minHeight, roomsQueue, room, r);
                    }
                    if (!split)
                    {
                        roomsList.Add(room);
                    }
                }
            }
            return roomsList;
        }

        public static Vector2Int RotateWalkPosition(Vector2 direction, Vector2 walkPosition)
        {
            var rotation = Quaternion.LookRotation(Vector3.forward, direction);
            return Vector2Int.RoundToInt(rotation * walkPosition);
        }

        private static bool SplitVertically(
            int minWidth,
            Queue<BoundsInt> roomsQueue,
            BoundsInt room,
            Random r
        )
        {
            if (room.size.x < minWidth * 2)
            {
                return false;
            }
            var xSplit = r.Next(1, room.size.x);
            var room1 = new BoundsInt(room.min, new Vector3Int(xSplit, room.size.y, room.size.z));
            var room2 = new BoundsInt(
                new Vector3Int(room.min.x + xSplit, room.min.y, room.min.z),
                new Vector3Int(room.size.x - xSplit, room.size.y, room.size.z)
            );
            roomsQueue.Enqueue(room1);
            roomsQueue.Enqueue(room2);
            return true;
        }

        private static bool SplitHorizontally(
            int minHeight,
            Queue<BoundsInt> roomsQueue,
            BoundsInt room,
            Random r
        )
        {
            if (room.size.y < minHeight * 2)
            {
                return false;
            }
            var ySplit = r.Next(1, room.size.y);
            var room1 = new BoundsInt(room.min, new Vector3Int(room.size.x, ySplit, room.size.z));
            var room2 = new BoundsInt(
                new Vector3Int(room.min.x, room.min.y + ySplit, room.min.z),
                new Vector3Int(room.size.x, room.size.y - ySplit, room.size.z)
            );
            roomsQueue.Enqueue(room1);
            roomsQueue.Enqueue(room2);
            return true;
        }
    }

    public static class Direction2D
    {
        public static List<Vector2Int> cardinalDirectionsList =
            new()
            {
                new Vector2Int(0, 1), //UP
                new Vector2Int(1, 0), //RIGHT
                new Vector2Int(0, -1), // DOWN
                new Vector2Int(-1, 0) //LEFT
            };

        public static List<Vector2Int> diagonalDirectionsList =
            new()
            {
                new Vector2Int(1, 1), //UP-RIGHT
                new Vector2Int(1, -1), //RIGHT-DOWN
                new Vector2Int(-1, -1), // DOWN-LEFT
                new Vector2Int(-1, 1) //LEFT-UP
            };

        public static List<Vector2Int> eightDirectionsList =
            new()
            {
                new Vector2Int(0, 1), //UP
                new Vector2Int(1, 1), //UP-RIGHT
                new Vector2Int(1, 0), //RIGHT
                new Vector2Int(1, -1), //RIGHT-DOWN
                new Vector2Int(0, -1), // DOWN
                new Vector2Int(-1, -1), // DOWN-LEFT
                new Vector2Int(-1, 0), //LEFT
                new Vector2Int(-1, 1) //LEFT-UP
            };

        public static Vector2Int GetRandomCardinalDirection(
            Random r,
            List<Vector2Int> excludedDirections = default
        )
        {
            if (excludedDirections == null)
            {
                return cardinalDirectionsList[r.Next(0, cardinalDirectionsList.Count)];
            }
            var filteredList = cardinalDirectionsList.FindAll(
                dir => !excludedDirections.Contains(dir)
            );
            return filteredList[r.Next(0, filteredList.Count)];
        }
    }
}
