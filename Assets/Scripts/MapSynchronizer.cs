using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class MapSynchronizer : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap indestructibleTilemap;
    public Tilemap destructibleTilemap;
    public TileBase destructibleTile;
    
    [Header("Map Data")]
    public MapDataSO mapData;
    
    [Header("Settings")]
    [Range(0f, 1f)]
    public float destructibleSpawnChance = 0.7f;
    public bool autoSynchronizeOnStart = false; // New option to control auto-sync
    
    void Start()
    {
        // Only auto-synchronize if explicitly enabled
        if (autoSynchronizeOnStart)
        {
            SynchronizeMapData();
            GenerateDestructibleTiles();
        }
    }
    
    [ContextMenu("Synchronize Map Data")]
    public void SynchronizeMapData()
    {
        if (indestructibleTilemap == null || mapData == null)
        {
            Debug.LogError("MapSynchronizer: Missing required references!");
            return;
        }
        
        // Compress bounds to get accurate size
        indestructibleTilemap.CompressBounds();
        BoundsInt bounds = indestructibleTilemap.cellBounds;
        Vector3Int origin = bounds.position;
        Vector3Int size = bounds.size;
        
        Debug.Log($"Indestructible Tilemap - Origin: {origin}, Size: {size}");
        
        // Update MapDataSO with tilemap dimensions
        mapData.width = size.x;
        mapData.height = size.y;
        mapData.origin = origin;
        
        // Initialize map matrix
        mapData.mapMatrix = new int[size.x * size.y];
        
        int blockedCells = 0;
        
        // Read tilemap and populate matrix
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector3Int cellPosition = new Vector3Int(origin.x + x, origin.y + y, 0);
                TileBase tile = indestructibleTilemap.GetTile(cellPosition);
                
                int index = x + y * size.x;
                
                // If there's an indestructible tile (block), mark as 0 (cannot place destructible tile)
                // If empty, mark as 1 (can place destructible tile)
                if (tile != null)
                {
                    mapData.mapMatrix[index] = 0;
                    blockedCells++;
                }
                else
                {
                    mapData.mapMatrix[index] = 1;
                }
            }
        }
        
        int emptyCells = mapData.mapMatrix.Length - blockedCells;
        Debug.Log($"Map data synchronized: {mapData.width}x{mapData.height} with {mapData.mapMatrix.Length} total cells");
        Debug.Log($"Blocked cells (indestructible tiles): {blockedCells}");
        Debug.Log($"Empty cells (available for destructible tiles): {emptyCells}");
        
        // Mark the ScriptableObject as dirty so changes are saved
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(mapData);
        #endif
    }
    
    [ContextMenu("Generate Destructible Tiles")]
    public void GenerateDestructibleTiles()
    {
        if (destructibleTilemap == null || destructibleTile == null || mapData == null)
        {
            Debug.LogError("MapSynchronizer: Missing references for destructible tile generation!");
            return;
        }
        
        if (mapData.mapMatrix == null || mapData.mapMatrix.Length == 0)
        {
            Debug.LogError("MapSynchronizer: Map data not synchronized! Run 'Synchronize Map Data' first.");
            return;
        }
        
        // Clear existing destructible tiles
        ClearDestructibleTiles();
        
        int tilesPlaced = 0;
        
        // Place destructible tiles based on map data
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                int index = x + y * mapData.width;
                
                // Only place destructible tiles where mapMatrix is 1 (empty space) and random chance passes
                if (mapData.mapMatrix[index] == 1 && Random.value < destructibleSpawnChance)
                {
                    Vector3Int cellPosition = new Vector3Int(mapData.origin.x + x, mapData.origin.y + y, 0);
                    destructibleTilemap.SetTile(cellPosition, destructibleTile);
                    tilesPlaced++;
                }
            }
        }
        
        Debug.Log($"Destructible tiles generated: {tilesPlaced} tiles placed out of {System.Array.FindAll(mapData.mapMatrix, x => x == 1).Length} available empty spaces");
    }
    
    [ContextMenu("Clear Destructible Tiles")]
    public void ClearDestructibleTiles()
    {
        if (destructibleTilemap == null || mapData == null)
        {
            Debug.LogError("MapSynchronizer: Missing references for clearing tiles!");
            return;
        }
        
        if (mapData.width > 0 && mapData.height > 0)
        {
            BoundsInt clearBounds = new BoundsInt(mapData.origin, new Vector3Int(mapData.width, mapData.height, 1));
            TileBase[] emptyTiles = new TileBase[mapData.width * mapData.height];
            destructibleTilemap.SetTilesBlock(clearBounds, emptyTiles);
            Debug.Log("All destructible tiles cleared");
        }
        else
        {
            // If no map data, clear the entire tilemap bounds
            destructibleTilemap.CompressBounds();
            BoundsInt bounds = destructibleTilemap.cellBounds;
            if (bounds.size.x > 0 && bounds.size.y > 0)
            {
                TileBase[] emptyTiles = new TileBase[bounds.size.x * bounds.size.y * bounds.size.z];
                destructibleTilemap.SetTilesBlock(bounds, emptyTiles);
                Debug.Log("All destructible tiles cleared (full tilemap)");
            }
        }
    }
    
    // Method to manually trigger synchronization from editor
    public void EditorSynchronize()
    {
        SynchronizeMapData();
        GenerateDestructibleTiles();
    }
    
    [ContextMenu("Reset and Regenerate All")]
    public void ResetAndRegenerateAll()
    {
        ClearDestructibleTiles();
        SynchronizeMapData();
        GenerateDestructibleTiles();
        Debug.Log("Map completely reset and regenerated!");
    }
}
