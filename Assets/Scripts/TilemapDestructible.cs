using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapDestructible : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap destructibleTilemap;
    
    [Header("Destruction Settings")]
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.2f;
    public GameObject[] spawnableItems;
    
    private void Start()
    {
        if (destructibleTilemap == null)
        {
            Debug.LogError("TilemapDestructible: Destructible tilemap not assigned!");
        }
    }
    
    /// <summary>
    /// Destroys a destructible tile at the specified world position
    /// </summary>
    /// <param name="worldPosition">World position where the tile should be destroyed</param>
    /// <returns>True if a tile was destroyed, false otherwise</returns>
    public bool DestroyTileAt(Vector3 worldPosition)
    {
        if (destructibleTilemap == null) return false;
        
        // Convert world position to cell position
        Vector3Int cellPosition = destructibleTilemap.WorldToCell(worldPosition);
        
        // Check if there's a tile at this position
        TileBase tile = destructibleTilemap.GetTile(cellPosition);
        if (tile == null) return false;
        
        // Remove the tile
        destructibleTilemap.SetTile(cellPosition, null);
        
        // Try to spawn an item
        TrySpawnItem(destructibleTilemap.CellToWorld(cellPosition));
        
        Debug.Log($"Destroyed tile at {cellPosition} (world: {worldPosition})");
        return true;
    }
    
    /// <summary>
    /// Checks if there's a destructible tile at the specified world position
    /// </summary>
    /// <param name="worldPosition">World position to check</param>
    /// <returns>True if there's a destructible tile at this position</returns>
    public bool HasTileAt(Vector3 worldPosition)
    {
        if (destructibleTilemap == null) return false;
        
        Vector3Int cellPosition = destructibleTilemap.WorldToCell(worldPosition);
        return destructibleTilemap.GetTile(cellPosition) != null;
    }
    
    /// <summary>
    /// Gets the cell position for a world position
    /// </summary>
    /// <param name="worldPosition">World position</param>
    /// <returns>Cell position in the tilemap</returns>
    public Vector3Int GetCellPosition(Vector3 worldPosition)
    {
        if (destructibleTilemap == null) return Vector3Int.zero;
        return destructibleTilemap.WorldToCell(worldPosition);
    }
    
    /// <summary>
    /// Gets the world position for a cell position
    /// </summary>
    /// <param name="cellPosition">Cell position in the tilemap</param>
    /// <returns>World position</returns>
    public Vector3 GetWorldPosition(Vector3Int cellPosition)
    {
        if (destructibleTilemap == null) return Vector3.zero;
        return destructibleTilemap.CellToWorld(cellPosition);
    }
    
    /// <summary>
    /// Destroys all destructible tiles within a radius from the center position
    /// </summary>
    /// <param name="centerPosition">Center world position</param>
    /// <param name="radius">Radius in world units</param>
    /// <returns>Number of tiles destroyed</returns>
    public int DestroyTilesInRadius(Vector3 centerPosition, float radius)
    {
        if (destructibleTilemap == null) return 0;
        
        int tilesDestroyed = 0;
        Vector3Int centerCell = destructibleTilemap.WorldToCell(centerPosition);
        int cellRadius = Mathf.CeilToInt(radius);
        
        // Check all cells within the radius
        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int y = -cellRadius; y <= cellRadius; y++)
            {
                Vector3Int cellPos = centerCell + new Vector3Int(x, y, 0);
                Vector3 worldPos = destructibleTilemap.CellToWorld(cellPos);
                
                // Check if this cell is within the actual radius
                if (Vector3.Distance(centerPosition, worldPos) <= radius)
                {
                    if (DestroyTileAt(worldPos))
                    {
                        tilesDestroyed++;
                    }
                }
            }
        }
        
        return tilesDestroyed;
    }
    
    private void TrySpawnItem(Vector3 position)
    {
        if (spawnableItems.Length > 0 && Random.value < itemSpawnChance)
        {
            int randomIndex = Random.Range(0, spawnableItems.Length);
            Instantiate(spawnableItems[randomIndex], position, Quaternion.identity);
        }
    }
    
    /// <summary>
    /// Get all destructible tile positions in the tilemap
    /// </summary>
    /// <returns>Array of world positions where destructible tiles exist</returns>
    public Vector3[] GetAllDestructiblePositions()
    {
        if (destructibleTilemap == null) return new Vector3[0];
        
        destructibleTilemap.CompressBounds();
        BoundsInt bounds = destructibleTilemap.cellBounds;
        
        System.Collections.Generic.List<Vector3> positions = new System.Collections.Generic.List<Vector3>();
        
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                if (destructibleTilemap.GetTile(cellPos) != null)
                {
                    positions.Add(destructibleTilemap.CellToWorld(cellPos));
                }
            }
        }
        
        return positions.ToArray();
    }
}
