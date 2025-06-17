using UnityEngine;
using UnityEngine.Tilemaps;

public class MovementCollisionFixer : MonoBehaviour
{
    [Header("Auto-Fix Settings")]
    [Tooltip("Automatically fix common collision issues on start")]
    public bool autoFixOnStart = true;
    
    [Header("Player Settings")]
    [Tooltip("Reduce player collider size to prevent getting stuck")]
    public bool adjustPlayerColliders = true;
    public float colliderSizeMultiplier = 0.8f;
    
    [Header("Tilemap Settings")]
    [Tooltip("Check and log tilemap cell sizes for debugging")]
    public bool checkTilemapCellSize = true;
    
    [Header("References")]
    public Tilemap indestructibleTilemap;
    public Tilemap destructibleTilemap;
    public GameObject[] players;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixCollisionIssues();
        }
    }
    
    [ContextMenu("Fix Collision Issues")]
    public void FixCollisionIssues()
    {
        Debug.Log("=== Starting Collision Fix ===");
        
        if (checkTilemapCellSize)
        {
            FixTilemapCellSizes();
        }
        
        if (adjustPlayerColliders)
        {
            AdjustPlayerColliders();
        }
        
        FixPlayerPositions();
        
        Debug.Log("=== Collision Fix Complete ===");
    }
    
    void FixTilemapCellSizes()
    {
        // Note: cellSize is read-only in modern Unity versions
        // Cell size is set in the Grid component, not individual tilemaps
        GameObject gridObject = null;
        
        if (indestructibleTilemap != null)
        {
            gridObject = indestructibleTilemap.transform.parent?.gameObject;
            Debug.Log($"Indestructible tilemap cell size: {indestructibleTilemap.cellSize}");
        }
        
        if (destructibleTilemap != null)
        {
            if (gridObject == null)
                gridObject = destructibleTilemap.transform.parent?.gameObject;
            Debug.Log($"Destructible tilemap cell size: {destructibleTilemap.cellSize}");
        }
        
        if (gridObject != null)
        {
            Grid grid = gridObject.GetComponent<Grid>();
            if (grid != null)
            {
                Debug.Log($"Grid cell size: {grid.cellSize}");
                // Cell size should be set manually in the Grid component if needed
            }
        }
    }
    
    void AdjustPlayerColliders()
    {
        foreach (GameObject player in players)
        {
            if (player == null) continue;
            
            CircleCollider2D circleCollider = player.GetComponent<CircleCollider2D>();
            if (circleCollider != null)
            {
                float originalRadius = circleCollider.radius;
                circleCollider.radius = originalRadius * colliderSizeMultiplier;
                Debug.Log($"Adjusted {player.name} collider radius from {originalRadius} to {circleCollider.radius}");
            }
            
            BoxCollider2D boxCollider = player.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                Vector2 originalSize = boxCollider.size;
                boxCollider.size = originalSize * colliderSizeMultiplier;
                Debug.Log($"Adjusted {player.name} collider size from {originalSize} to {boxCollider.size}");
            }
        }
    }
    
    void FixPlayerPositions()
    {
        foreach (GameObject player in players)
        {
            if (player == null) continue;
            
            Vector3 currentPos = player.transform.position;
            Vector3 snappedPos = SnapToGrid(currentPos);
            
            // Check if snapped position is safe
            if (!TilemapCollisionUtility.IsPositionBlocked(snappedPos, indestructibleTilemap, destructibleTilemap))
            {
                player.transform.position = snappedPos;
                Debug.Log($"Snapped {player.name} from {currentPos} to {snappedPos}");
            }
            else
            {
                // Find nearest safe position
                Vector3 safePos = FindNearestSafePosition(currentPos);
                player.transform.position = safePos;
                Debug.Log($"Moved {player.name} from {currentPos} to safe position {safePos}");
            }
        }
    }
    
    Vector3 SnapToGrid(Vector3 position)
    {
        return new Vector3(
            Mathf.Round(position.x),
            Mathf.Round(position.y),
            position.z
        );
    }
    
    Vector3 FindNearestSafePosition(Vector3 position)
    {
        // Try positions in expanding circles
        for (float radius = 0.5f; radius <= 3f; radius += 0.5f)
        {
            for (int angle = 0; angle < 360; angle += 45)
            {
                float radians = angle * Mathf.Deg2Rad;
                Vector3 testPos = position + new Vector3(
                    Mathf.Cos(radians) * radius,
                    Mathf.Sin(radians) * radius,
                    0
                );
                
                testPos = SnapToGrid(testPos);
                
                if (!TilemapCollisionUtility.IsPositionBlocked(testPos, indestructibleTilemap, destructibleTilemap))
                {
                    return testPos;
                }
            }
        }
        
        return position; // Return original if no safe position found
    }
    
    [ContextMenu("Test Player Positions")]
    public void TestPlayerPositions()
    {
        Debug.Log("=== Testing Player Positions ===");
        
        foreach (GameObject player in players)
        {
            if (player == null) continue;
            
            Vector3 pos = player.transform.position;
            bool blocked = TilemapCollisionUtility.IsPositionBlocked(pos, indestructibleTilemap, destructibleTilemap);
            
            Debug.Log($"{player.name} at {pos}: {(blocked ? "BLOCKED" : "FREE")}");
            
            if (blocked)
            {
                Vector3 safePos = FindNearestSafePosition(pos);
                Debug.Log($"  Nearest safe position: {safePos}");
            }
        }
    }
    
    [ContextMenu("Reset All Players to Safe Positions")]
    public void ResetPlayersToSafePositions()
    {
        // Common safe starting positions
        Vector3[] safePositions = {
            new Vector3(1, 1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, -1, 0),
            new Vector3(-1, -1, 0)
        };
        
        for (int i = 0; i < players.Length && i < safePositions.Length; i++)
        {
            if (players[i] != null)
            {
                Vector3 targetPos = safePositions[i];
                
                // Make sure the position is actually safe
                if (!TilemapCollisionUtility.IsPositionBlocked(targetPos, indestructibleTilemap, destructibleTilemap))
                {
                    players[i].transform.position = targetPos;
                    Debug.Log($"Reset {players[i].name} to safe position {targetPos}");
                }
                else
                {
                    Vector3 safePos = FindNearestSafePosition(targetPos);
                    players[i].transform.position = safePos;
                    Debug.Log($"Reset {players[i].name} to alternative safe position {safePos}");
                }
            }
        }
    }
}
