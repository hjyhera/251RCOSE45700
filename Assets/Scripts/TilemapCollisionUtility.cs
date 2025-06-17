using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapCollisionUtility
{
    /// <summary>
    /// Checks if a position collides with any tile in the specified tilemaps
    /// </summary>
    /// <param name="worldPosition">World position to check</param>
    /// <param name="indestructibleTilemap">Tilemap with indestructible tiles</param>
    /// <param name="destructibleTilemap">Tilemap with destructible tiles</param>
    /// <returns>True if there's a collision with any tile</returns>
    public static bool IsPositionBlocked(Vector3 worldPosition, Tilemap indestructibleTilemap, Tilemap destructibleTilemap)
    {
        // Check indestructible tiles
        if (indestructibleTilemap != null)
        {
            Vector3Int cellPos = indestructibleTilemap.WorldToCell(worldPosition);
            if (indestructibleTilemap.GetTile(cellPos) != null)
                return true;
        }
        
        // Check destructible tiles
        if (destructibleTilemap != null)
        {
            Vector3Int cellPos = destructibleTilemap.WorldToCell(worldPosition);
            if (destructibleTilemap.GetTile(cellPos) != null)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if a position collides only with indestructible tiles
    /// </summary>
    /// <param name="worldPosition">World position to check</param>
    /// <param name="indestructibleTilemap">Tilemap with indestructible tiles</param>
    /// <returns>True if there's a collision with an indestructible tile</returns>
    public static bool IsPositionBlockedByIndestructible(Vector3 worldPosition, Tilemap indestructibleTilemap)
    {
        if (indestructibleTilemap == null) return false;
        
        Vector3Int cellPos = indestructibleTilemap.WorldToCell(worldPosition);
        return indestructibleTilemap.GetTile(cellPos) != null;
    }
    
    /// <summary>
    /// Checks if a position collides only with destructible tiles
    /// </summary>
    /// <param name="worldPosition">World position to check</param>
    /// <param name="destructibleTilemap">Tilemap with destructible tiles</param>
    /// <returns>True if there's a collision with a destructible tile</returns>
    public static bool IsPositionBlockedByDestructible(Vector3 worldPosition, Tilemap destructibleTilemap)
    {
        if (destructibleTilemap == null) return false;
        
        Vector3Int cellPos = destructibleTilemap.WorldToCell(worldPosition);
        return destructibleTilemap.GetTile(cellPos) != null;
    }
    
    /// <summary>
    /// Gets the nearest walkable position from a given position
    /// </summary>
    /// <param name="targetPosition">The desired position</param>
    /// <param name="indestructibleTilemap">Tilemap with indestructible tiles</param>
    /// <param name="destructibleTilemap">Tilemap with destructible tiles</param>
    /// <param name="searchRadius">Maximum search radius</param>
    /// <returns>Nearest walkable position, or original position if none found</returns>
    public static Vector3 GetNearestWalkablePosition(Vector3 targetPosition, Tilemap indestructibleTilemap, Tilemap destructibleTilemap, float searchRadius = 5f)
    {
        if (!IsPositionBlocked(targetPosition, indestructibleTilemap, destructibleTilemap))
            return targetPosition;
        
        // Search in expanding circles
        for (float radius = 0.5f; radius <= searchRadius; radius += 0.5f)
        {
            for (int angle = 0; angle < 360; angle += 45)
            {
                float radians = angle * Mathf.Deg2Rad;
                Vector3 testPos = targetPosition + new Vector3(
                    Mathf.Cos(radians) * radius,
                    Mathf.Sin(radians) * radius,
                    0
                );
                
                if (!IsPositionBlocked(testPos, indestructibleTilemap, destructibleTilemap))
                    return testPos;
            }
        }
        
        return targetPosition; // Return original if no walkable position found
    }
    
    /// <summary>
    /// Snaps a world position to the center of the nearest tile cell
    /// </summary>
    /// <param name="worldPosition">World position to snap</param>
    /// <param name="tilemap">Reference tilemap for cell size</param>
    /// <returns>Snapped position</returns>
    public static Vector3 SnapToTileCenter(Vector3 worldPosition, Tilemap tilemap)
    {
        if (tilemap == null) return worldPosition;
        
        Vector3Int cellPos = tilemap.WorldToCell(worldPosition);
        return tilemap.CellToWorld(cellPos) + tilemap.cellSize * 0.5f;
    }
    
    /// <summary>
    /// Checks if a rectangular area is free of obstacles
    /// </summary>
    /// <param name="center">Center position of the rectangle</param>
    /// <param name="size">Size of the rectangle</param>
    /// <param name="indestructibleTilemap">Tilemap with indestructible tiles</param>
    /// <param name="destructibleTilemap">Tilemap with destructible tiles</param>
    /// <returns>True if the area is free of obstacles</returns>
    public static bool IsAreaFree(Vector3 center, Vector2 size, Tilemap indestructibleTilemap, Tilemap destructibleTilemap)
    {
        Vector3 halfSize = size * 0.5f;
        
        // Check corners and center
        Vector3[] checkPoints = {
            center,
            center + new Vector3(-halfSize.x, -halfSize.y, 0),
            center + new Vector3(halfSize.x, -halfSize.y, 0),
            center + new Vector3(-halfSize.x, halfSize.y, 0),
            center + new Vector3(halfSize.x, halfSize.y, 0)
        };
        
        foreach (Vector3 point in checkPoints)
        {
            if (IsPositionBlocked(point, indestructibleTilemap, destructibleTilemap))
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Checks if a rectangular area (like a player) would collide with tiles
    /// </summary>
    /// <param name="center">Center position of the rectangle</param>
    /// <param name="size">Size of the rectangle (player bounds)</param>
    /// <param name="indestructibleTilemap">Tilemap with indestructible tiles</param>
    /// <param name="destructibleTilemap">Tilemap with destructible tiles</param>
    /// <returns>True if any part of the rectangle overlaps with tiles</returns>
    public static bool IsRectangleBlocked(Vector3 center, Vector2 size, Tilemap indestructibleTilemap, Tilemap destructibleTilemap)
    {
        Vector2 halfSize = size * 0.5f;
        
        // Check multiple points around the rectangle
        Vector3[] checkPoints = {
            center, // Center
            center + new Vector3(-halfSize.x, -halfSize.y, 0), // Bottom-left
            center + new Vector3(halfSize.x, -halfSize.y, 0),  // Bottom-right
            center + new Vector3(-halfSize.x, halfSize.y, 0),  // Top-left
            center + new Vector3(halfSize.x, halfSize.y, 0),   // Top-right
            center + new Vector3(0, -halfSize.y, 0),           // Bottom-center
            center + new Vector3(0, halfSize.y, 0),            // Top-center
            center + new Vector3(-halfSize.x, 0, 0),           // Left-center
            center + new Vector3(halfSize.x, 0, 0)             // Right-center
        };
        
        foreach (Vector3 point in checkPoints)
        {
            if (IsPositionBlocked(point, indestructibleTilemap, destructibleTilemap))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks collision for a circular area (better for round players)
    /// </summary>
    /// <param name="center">Center position</param>
    /// <param name="radius">Radius of the circle</param>
    /// <param name="indestructibleTilemap">Tilemap with indestructible tiles</param>
    /// <param name="destructibleTilemap">Tilemap with destructible tiles</param>
    /// <returns>True if any part of the circle overlaps with tiles</returns>
    public static bool IsCircleBlocked(Vector3 center, float radius, Tilemap indestructibleTilemap, Tilemap destructibleTilemap)
    {
        // Check center
        if (IsPositionBlocked(center, indestructibleTilemap, destructibleTilemap))
            return true;
        
        // Check points around the circle
        int numChecks = 8;
        for (int i = 0; i < numChecks; i++)
        {
            float angle = (i * 360f / numChecks) * Mathf.Deg2Rad;
            Vector3 checkPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
            
            if (IsPositionBlocked(checkPoint, indestructibleTilemap, destructibleTilemap))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Gets a safe movement position that doesn't collide with tiles
    /// </summary>
    /// <param name="currentPosition">Current position</param>
    /// <param name="targetPosition">Desired position</param>
    /// <param name="playerSize">Size of the player</param>
    /// <param name="indestructibleTilemap">Tilemap with indestructible tiles</param>
    /// <param name="destructibleTilemap">Tilemap with destructible tiles</param>
    /// <returns>Safe position to move to</returns>
    public static Vector3 GetSafeMovementPosition(Vector3 currentPosition, Vector3 targetPosition, Vector2 playerSize, 
        Tilemap indestructibleTilemap, Tilemap destructibleTilemap)
    {
        // If target position is safe, use it
        if (!IsRectangleBlocked(targetPosition, playerSize, indestructibleTilemap, destructibleTilemap))
            return targetPosition;
        
        // Try to slide along walls by checking X and Y movement separately
        Vector3 horizontalOnly = new Vector3(targetPosition.x, currentPosition.y, currentPosition.z);
        Vector3 verticalOnly = new Vector3(currentPosition.x, targetPosition.y, currentPosition.z);
        
        // Try horizontal movement only
        if (!IsRectangleBlocked(horizontalOnly, playerSize, indestructibleTilemap, destructibleTilemap))
            return horizontalOnly;
        
        // Try vertical movement only
        if (!IsRectangleBlocked(verticalOnly, playerSize, indestructibleTilemap, destructibleTilemap))
            return verticalOnly;
        
        // If neither works, stay at current position
        return currentPosition;
    }
}
