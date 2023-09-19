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
            backgroundTile,
            ventTile;

        public void PaintTerrainTiles(IEnumerable<Vector2Int> terrainPositions)
        {
            PaintTiles(terrainPositions, terrainTilemap, terrainTile);
        }

        public void PaintBackTiles(IEnumerable<Vector2Int> backPositions)
        {
            PaintTiles(backPositions, backgroundTilemap, backgroundTile);
        }

        public void PaintVentTiles(IEnumerable<Vector2Int> ventPositions)
        {
            PaintTiles(ventPositions, backgroundTilemap, ventTile);
        }

        private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
        {
            foreach (var position in positions)
            {
                PaintSingleTile(tilemap, tile, position);
            }
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
