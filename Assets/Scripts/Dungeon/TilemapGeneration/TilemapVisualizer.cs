using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DungeonGeneration
{
    public class TilemapVisualizer : MonoBehaviour
    {
        [SerializeField]
        private Tilemap terrainTilemap,
            backgroundTilemap,
            backgroundPlaneTilemap;

        [SerializeField]
        private TileBase terrainTile,
            backgroundTile,
            backgroundPlaneTile,
            ventTile;

        public void PaintTerrainTiles(IEnumerable<Vector2Int> terrainPositions)
        {
            PaintTiles(terrainPositions, terrainTilemap, terrainTile);
        }

        public void ClearTerrainTiles(IEnumerable<Vector2Int> positions)
        {
            PaintTiles(positions, terrainTilemap, null);
        }

        public void PaintBackTiles(IEnumerable<Vector2Int> backPositions)
        {
            PaintTiles(backPositions, backgroundTilemap, backgroundTile);
        }

        public void ClearBackTiles(IEnumerable<Vector2Int> positions)
        {
            PaintTiles(positions, backgroundTilemap, null);
        }

        public void PaintVentTiles(IEnumerable<Vector2Int> ventPositions)
        {
            PaintTiles(ventPositions, backgroundTilemap, ventTile);
        }

        internal void PaintBackPlaneTiles(IEnumerable<Vector2Int> backgroundPlane)
        {
            PaintTiles(backgroundPlane, backgroundPlaneTilemap, backgroundPlaneTile);
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
            backgroundPlaneTilemap.ClearAllTiles();
            terrainTilemap.ClearAllTiles();
        }
    }
}
