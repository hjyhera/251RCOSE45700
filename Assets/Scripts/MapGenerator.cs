using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    public Tilemap destructableTilemap;
    public TileBase destructableTile;
    public MapDataSO mapData;

    void Start()
    {
        GenerateDestructibleTiles();
    }
    
    public void GenerateDestructibleTiles()
    {
        if (mapData == null || destructableTilemap == null || destructableTile == null)
        {
            Debug.LogError("MapGenerator: Missing required references!");
            return;
        }
        
        // Clear existing tiles first
        destructableTilemap.SetTilesBlock(
            new BoundsInt(mapData.origin, new Vector3Int(mapData.width, mapData.height, 1)), 
            new TileBase[mapData.width * mapData.height]
        );
        
        // Generate tiles based on map data with origin offset
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                int index = x + y * mapData.width;
                if (mapData.mapMatrix[index] == 1)
                {
                    Vector3Int worldPosition = new Vector3Int(
                        mapData.origin.x + x, 
                        mapData.origin.y + y, 
                        0
                    );
                    destructableTilemap.SetTile(worldPosition, destructableTile);
                }
            }
        }
        
        Debug.Log($"Generated destructible tiles using origin {mapData.origin}");
    }
}
