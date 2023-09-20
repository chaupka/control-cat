using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DungeonGeneration
{
    public class RoomFirstDungeonGenerator : AbstractDungeonGenerator
    {
        protected override void RunProceduralGeneration()
        {
            CreateRooms();
        }

        private void CreateRooms()
        {
            bool isDungeonValid = false;

            while (!isDungeonValid)
            {
                List<BoundsInt> roomBounds = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
                    r,
                    parameters,
                    tree
                );

                isDungeonValid = TryConnectRoomsByDungeonTree(roomBounds);
            }

            var rooms = tree.nodes.OfType<RoomNode>().ToHashSet();
            var corridors = tree.nodes.OfType<CorridorNode>().ToHashSet();
            var vents = tree.nodes.OfType<VentNode>().ToHashSet();
            BackgroundGenerator.Create(rooms, corridors, parameters, r);

            var backGround = rooms.SelectMany(r => r.tilePositions).ToHashSet();
            backGround.UnionWith(corridors.SelectMany(c => c.tilePositions).ToHashSet());

            var walls = TerrainGenerator.CreateWalls(backGround);
            VentGenerator.Create(rooms, vents, parameters, r);
            var ventPositions = vents
                .SelectMany(v =>
                {
                    var positions = new HashSet<Vector2Int>(v.parentVentPositions);
                    positions.UnionWith(v.childVentPositions);
                    return positions;
                })
                .ToHashSet();
            var ventPlatformPositions = vents
                .SelectMany(v =>
                {
                    var positions = new HashSet<Vector2Int>(v.parentPlatformPositions);
                    positions.UnionWith(v.childPlatformPositions);
                    return positions;
                })
                .ToHashSet();
            backGround.ExceptWith(ventPositions);
            backGround.ExceptWith(ventPlatformPositions);
            // var platforms = TerrainGenerator.CreatePlatforms(backGround, walls, parameters, r);

            originalTerrain = new HashSet<Vector2Int>(walls);
            originalTerrain.UnionWith(ventPlatformPositions);
            if (parameters.dungeon.IsFilledWithTerrain)
            {
                var notToBeFilled = new HashSet<Vector2Int>(originalTerrain);
                notToBeFilled.UnionWith(backGround);
                notToBeFilled.UnionWith(ventPositions);
                notToBeFilled.UnionWith(ventPlatformPositions);
                originalTerrain.UnionWith(
                    TerrainGenerator.CreateRestOfDungeon(notToBeFilled, parameters)
                );
            }
            PaintMaps(originalTerrain, backGround, ventPositions);
        }

        private bool TryConnectRoomsByDungeonTree(List<BoundsInt> roomBounds)
        {
            if (roomBounds.Count < tree.nodes.Count)
            {
                return false;
            }

            var remainingRoomBounds = new List<BoundsInt>(roomBounds);
            CreateConnections(remainingRoomBounds);
            return true;
        }

        private void CreateConnections(List<BoundsInt> remainingRoomBounds)
        {
            var startRoom = (StartRoom)tree.nodes.FirstOrDefault(node => node is StartRoom);
            var startBound = remainingRoomBounds[r.Next(remainingRoomBounds.Count)];
            startRoom.bounds = startBound;
            remainingRoomBounds.Remove(startBound);
            var omitPositions = new HashSet<Vector2Int>();
            DungeonTree.Traverse(
                startRoom,
                (n) =>
                {
                    RoomNode parentRoom;
                    try
                    {
                        parentRoom = (RoomNode)n;
                    }
                    catch (InvalidCastException)
                    {
                        return;
                    }
                    var parentPositions = ProceduralGenerationAlgorithms.FillRectangle(
                        parentRoom.bounds,
                        parameters.room.offset
                    );
                    omitPositions.ExceptWith(parentPositions);
                    var connections = DungeonTree.GetChildren(parentRoom).Cast<ConnectionNode>();
                    var newConnections = new List<ConnectionNode>();
                    foreach (var c in connections)
                    {
                        var childRoom = c.child;
                        childRoom.bounds =
                            childRoom.bounds == default
                                ? FindClosestPointTo(parentRoom.bounds, remainingRoomBounds)
                                : childRoom.bounds;
                        remainingRoomBounds.Remove(childRoom.bounds);
                        var childPositions = ProceduralGenerationAlgorithms.FillRectangle(
                            childRoom.bounds,
                            parameters.room.offset
                        );
                        omitPositions.ExceptWith(childPositions);
                        var connection = ConnectRooms(parentRoom, childRoom, c, omitPositions);
                        newConnections.Add(connection);
                        omitPositions.UnionWith(childPositions);
                        if (connection is CorridorNode corridor)
                        {
                            omitPositions.UnionWith(corridor.tilePositions);
                            remainingRoomBounds.RemoveAll(
                                b => corridor.tilePositions.Any(p => b.Contains((Vector3Int)p))
                            );
                        }
                    }
                    parentRoom.children.Clear();
                    parentRoom.children.AddRange(newConnections);
                    omitPositions.UnionWith(parentPositions);
                }
            );
        }

        private BoundsInt FindClosestPointTo(BoundsInt currentRoom, List<BoundsInt> otherRooms)
        {
            var closest = new BoundsInt();
            float distance = float.MaxValue;
            foreach (var room in otherRooms)
            {
                float currentDistance = Vector2.Distance(room.center, currentRoom.center);
                if (currentDistance < distance)
                {
                    distance = currentDistance;
                    closest = room;
                }
            }
            return closest;
        }

        private ConnectionNode ConnectRooms(
            RoomNode parent,
            RoomNode childRoom,
            ConnectionNode connection,
            HashSet<Vector2Int> omitPositions
        )
        {
            switch (connection)
            {
                case CorridorNode corridor:
                {
                    corridor.tilePositions = CreateCorridor(
                        Vector2Int.RoundToInt(parent.bounds.center),
                        Vector2Int.RoundToInt(childRoom.bounds.center),
                        omitPositions
                    );
                    if (corridor.tilePositions.Count == 0)
                    {
                        VentNode vent = ExchangeCorridorWithVent(corridor);
                        return ConnectRooms(parent, childRoom, vent, omitPositions);
                    }
                    return corridor;
                }
                case VentNode vent:
                {
                    return vent;
                }
            }
            return connection;
        }

        private VentNode ExchangeCorridorWithVent(CorridorNode corridor)
        {
            var vent = ScriptableObject.CreateInstance<VentNode>();
            vent.child = corridor.child;
            vent.position = corridor.position;
            tree.nodes.Remove(corridor);
            tree.nodes.Add(vent);
            return vent;
        }

        private List<Vector2Int> CreateCorridor(
            Vector2Int currentRoomCenter,
            Vector2Int destination,
            HashSet<Vector2Int> omitPositions
        )
        {
            var corridor = new List<KeyValuePair<Vector2Int, List<Vector2Int>>>();
            var position = Vector2Int.RoundToInt(currentRoomCenter);
            corridor.Add(new(position, new(Direction2D.cardinalDirectionsList)));
            var newOmitPositions = new HashSet<Vector2Int>(omitPositions);

            while (Vector2Int.Distance(position, destination) > parameters.corridor.maxWidth + 3)
            {
                if (corridor.Count > 1)
                {
                    var backDirection = corridor[^2].Key - corridor[^1].Key;
                    corridor[^1].Value.Remove(backDirection);
                }
                // random
                var directions = corridor[^1].Value
                    .OrderBy(d => Vector2Int.Distance(position + d, destination))
                    .ToList();
                corridor[^1] = new(corridor[^1].Key, directions);
                bool hasFoundPosition = false;
                var lastPosition = position;
                while (!hasFoundPosition)
                {
                    if (!corridor[^1].Value.Any())
                    {
                        if (corridor.Count == 1)
                        {
                            Debug.Log("need vent");
                            return new List<Vector2Int>();
                        }
                        corridor[^2].Value.Remove(corridor[^1].Key - corridor[^2].Key);
                        newOmitPositions.Add(position);
                        position = corridor[^2].Key;
                        corridor.RemoveAt(corridor.Count - 1);
                        break;
                    }
                    position += corridor[^1].Value[0];
                    var index = corridor.FindIndex(c => c.Key.Equals(position));
                    if (index != -1)
                    {
                        corridor[index].Value.Remove(corridor[index + 1].Key - corridor[index].Key);

                        corridor = corridor.Take(index + 1).ToList();
                        break;
                    }
                    var positionBuffer = new HashSet<Vector2Int>();
                    for (
                        int j = -1 - parameters.corridor.maxWidth;
                        j < parameters.corridor.maxWidth + 2;
                        j++
                    )
                    {
                        var newTile =
                            position
                            + ProceduralGenerationAlgorithms.RotateWalkPosition(
                                position - lastPosition,
                                new Vector2Int(j, 0)
                            );
                        positionBuffer.Add(newTile);
                    }
                    if (newOmitPositions.Overlaps(positionBuffer))
                    {
                        position = lastPosition;
                        corridor[^1].Value.RemoveAt(0);
                        continue;
                    }
                    hasFoundPosition = true;
                }
                if (!hasFoundPosition)
                {
                    continue;
                }
                corridor.Add(new(position, new(Direction2D.cardinalDirectionsList)));
            }

            return corridor.Select(c => c.Key).ToList();
        }

        private void PaintMaps(
            HashSet<Vector2Int> terrain,
            HashSet<Vector2Int> background,
            HashSet<Vector2Int> vents
        )
        {
            tilemapVisualizer.PaintTerrainTiles(terrain);
            tilemapVisualizer.PaintVentTiles(vents);
            tilemapVisualizer.PaintBackTiles(background);
            var backgroundPlane = new HashSet<Vector2Int>(terrain);
            backgroundPlane.UnionWith(background);
            backgroundPlane.UnionWith(vents);
            tilemapVisualizer.PaintBackPlaneTiles(backgroundPlane);
        }
    }
}
