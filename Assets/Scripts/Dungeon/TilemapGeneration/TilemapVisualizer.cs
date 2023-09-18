using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonGeneration
{
    public class TilemapVisualizer : MonoBehaviour
    {
        [SerializeField]
        private Tilemap terrainTilemap,
            backgroundTilemap;

        [SerializeField]
        private TileBase terrainTile,
            backgroundTile;

        public void PaintBackTiles(IEnumerable<Vector2Int> backPositions)
        {
            PaintTiles(backPositions, backgroundTilemap, backgroundTile);
        }

        private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
        {
            foreach (var position in positions)
            {
                PaintSingleTile(tilemap, tile, position);
            }
        }

        public void PaintSingleTerrain(Vector2Int position)
        {
            PaintSingleTile(terrainTilemap, terrainTile, position);
        }

        private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
        {
            var tilePosition = tilemap.WorldToCell((Vector3Int)position);
            tilemap.SetTile(tilePosition, tile);
        }

        public void Clear()
        {
            backgroundTilemap.ClearAllTiles();
            terrainTilemap.ClearAllTiles();
        }
    }
}
