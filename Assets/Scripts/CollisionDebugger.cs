using UnityEngine;
using UnityEngine.Tilemaps;

public class CollisionDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showTileGrid = true;
    public bool showPlayerBounds = true;
    public bool showCollisionChecks = true;
    public bool logCollisionInfo = false;
    
    [Header("References")]
    public Tilemap indestructibleTilemap;
    public Tilemap destructibleTilemap;
    public Transform player;
    public Vector2 playerSize = new Vector2(0.8f, 0.8f);
    
    [Header("Visual Settings")]
    public Color tileGridColor = Color.white;
    public Color blockedTileColor = Color.red;
    public Color freeTileColor = Color.green;
    public Color playerBoundsColor = Color.blue;
    
    private void Update()
    {
        if (logCollisionInfo && player != null)
        {
            bool blocked = TilemapCollisionUtility.IsRectangleBlocked(
                player.position, 
                playerSize, 
                indestructibleTilemap, 
                destructibleTilemap
            );
            
            if (blocked)
            {
                Debug.Log($"Player at {player.position} is blocked!");
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (showTileGrid && indestructibleTilemap != null)
        {
            DrawTileGrid();
        }
        
        if (showPlayerBounds && player != null)
        {
            DrawPlayerBounds();
        }
        
        if (showCollisionChecks && player != null)
        {
            DrawCollisionChecks();
        }
    }
    
    private void DrawTileGrid()
    {
        indestructibleTilemap.CompressBounds();
        BoundsInt bounds = indestructibleTilemap.cellBounds;
        
        // Draw grid lines
        Gizmos.color = tileGridColor;
        
        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            Vector3 start = indestructibleTilemap.CellToWorld(new Vector3Int(x, bounds.yMin, 0));
            Vector3 end = indestructibleTilemap.CellToWorld(new Vector3Int(x, bounds.yMax, 0));
            Gizmos.DrawLine(start, end);
        }
        
        for (int y = bounds.yMin; y <= bounds.yMax; y++)
        {
            Vector3 start = indestructibleTilemap.CellToWorld(new Vector3Int(bounds.xMin, y, 0));
            Vector3 end = indestructibleTilemap.CellToWorld(new Vector3Int(bounds.xMax, y, 0));
            Gizmos.DrawLine(start, end);
        }
        
        // Draw tile states
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);
                Vector3 worldPos = indestructibleTilemap.CellToWorld(cellPos);
                worldPos += indestructibleTilemap.cellSize * 0.5f; // Center of cell
                
                bool hasIndestructible = indestructibleTilemap.GetTile(cellPos) != null;
                bool hasDestructible = destructibleTilemap != null && destructibleTilemap.GetTile(cellPos) != null;
                
                if (hasIndestructible || hasDestructible)
                {
                    Gizmos.color = blockedTileColor;
                    Gizmos.DrawCube(worldPos, indestructibleTilemap.cellSize * 0.8f);
                }
                else
                {
                    Gizmos.color = freeTileColor;
                    Gizmos.DrawWireCube(worldPos, indestructibleTilemap.cellSize * 0.6f);
                }
            }
        }
    }
    
    private void DrawPlayerBounds()
    {
        Gizmos.color = playerBoundsColor;
        Gizmos.DrawWireCube(player.position, playerSize);
        
        // Draw center point
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(player.position, 0.1f);
    }
    
    private void DrawCollisionChecks()
    {
        Vector2 halfSize = playerSize * 0.5f;
        
        // Check points for collision detection
        Vector3[] checkPoints = {
            player.position, // Center
            player.position + new Vector3(-halfSize.x, -halfSize.y, 0), // Bottom-left
            player.position + new Vector3(halfSize.x, -halfSize.y, 0),  // Bottom-right
            player.position + new Vector3(-halfSize.x, halfSize.y, 0),  // Top-left
            player.position + new Vector3(halfSize.x, halfSize.y, 0),   // Top-right
        };
        
        foreach (Vector3 point in checkPoints)
        {
            bool blocked = TilemapCollisionUtility.IsPositionBlocked(point, indestructibleTilemap, destructibleTilemap);
            Gizmos.color = blocked ? Color.red : Color.green;
            Gizmos.DrawSphere(point, 0.05f);
        }
    }
    
    [ContextMenu("Log Current Collision State")]
    public void LogCurrentCollisionState()
    {
        if (player == null) return;
        
        bool blocked = TilemapCollisionUtility.IsRectangleBlocked(
            player.position, 
            playerSize, 
            indestructibleTilemap, 
            destructibleTilemap
        );
        
        Debug.Log($"=== Collision Debug Info ===");
        Debug.Log($"Player Position: {player.position}");
        Debug.Log($"Player Size: {playerSize}");
        Debug.Log($"Is Blocked: {blocked}");
        
        // Check individual tiles around player
        Vector3Int centerCell = indestructibleTilemap.WorldToCell(player.position);
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int checkCell = centerCell + new Vector3Int(x, y, 0);
                Vector3 worldPos = indestructibleTilemap.CellToWorld(checkCell);
                
                bool hasIndestructible = indestructibleTilemap.GetTile(checkCell) != null;
                bool hasDestructible = destructibleTilemap != null && destructibleTilemap.GetTile(checkCell) != null;
                
                Debug.Log($"Cell {checkCell} (world: {worldPos}): Indestructible={hasIndestructible}, Destructible={hasDestructible}");
            }
        }
    }
}
