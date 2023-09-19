using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace DungeonGeneration
{
    public abstract class AbstractDungeonGenerator : MonoBehaviour
    {
        [SerializeField]
        protected TilemapVisualizer tilemapVisualizer;
        public DungeonGeneratorDataSO parameters;
        protected Random r;
        public int seed = 0;
        protected DungeonStateController dungeonStateController;
        public DungeonTree tree;
        public bool isDone;

        public void GenerateDungeon(DungeonStateController dungeonStateController)
        {
            this.dungeonStateController = dungeonStateController;
            GenerateDungeon();
            isDone = true;
        }

        public void GenerateDungeon(int seed = 0)
        {
            this.seed = seed == 0 ? Environment.TickCount : seed;
            r = new Random(this.seed);
            tilemapVisualizer.Clear();
            tree = parameters.dungeon.tree.Clone();
            RunProceduralGeneration();
        }

        protected abstract void RunProceduralGeneration();

        public void CreatePlatform(IEnumerable<Vector2Int> platform)
        {
            tilemapVisualizer.PaintTerrainTiles(platform);
            tilemapVisualizer.ClearBackTiles(platform);
        }

        public void ClearPlatform(IEnumerable<Vector2Int> platform)
        {
            tilemapVisualizer.PaintBackTiles(platform);
            tilemapVisualizer.ClearTerrainTiles(platform);
        }
    }
}
