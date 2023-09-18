using System;
using UnityEngine;

namespace DungeonGeneration
{
    [CreateAssetMenu(
        fileName = "DungeonGeneratorParameters_",
        menuName = "Carsten/DungeonGeneratorData"
    )]
    public class DungeonGeneratorDataSO : ScriptableObject
    {
        public Dungeon dungeon;
        public Room room;
        public Corridor corridor;
        public Vent vent;
        public Platform platform;

        [Min(3)]
        public int minSpace;
        public MinimumSpaceWorker minimumSpaceWorker;

        [Serializable]
        public record Dungeon
        {
            public DungeonTree tree;
            public BoundsInt bounds;
            public bool IsFilledWithTerrain;
            public int filledOffset;
        }

        [Serializable]
        public record Room
        {
            public bool isStartingRandomlyEachIteration,
                isRandomWalking;
            public int minIterations,
                maxIterations,
                minWalkLength,
                maxWalkLength,
                minWidth,
                minHeight,
                number;

            [Range(0, 10)]
            public int offset;

            internal void Deconstruct(out int minWidth, out int minHeight, out int number)
            {
                minWidth = this.minWidth;
                minHeight = this.minHeight;
                number = this.number;
            }
        }

        [Serializable]
        public record Corridor
        {
            [Min(3)]
            public int minWidth,
                maxWidth;
        }

        [Serializable]
        public record Vent
        {
            [Range(1, 10)]
            public int minDistWidth,
                maxDistWidth,
                minDistHeight,
                maxDistHeight;
        }

        [Serializable]
        public record Platform
        {
            [Range(1, 50)]
            public int minDistWidth,
                maxDistWidth,
                minDistHeight,
                maxDistHeight,
                minRandomWalk,
                maxRandomWalk;
        }
    }

    public enum MinimumSpaceWorker
    {
        OnlyCheckOrthogonalMinimumSpace, // only checks orthogonal space (not diagonal)
        TakeScreenShot, // only changes take into account that happened in last iteration
        WatchTheOther // also check what the other has done
    }
}
