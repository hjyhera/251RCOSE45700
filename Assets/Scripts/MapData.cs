

using UnityEngine;

[CreateAssetMenu(fileName = "MapData", menuName = "ScriptableObjects/MapData")]
public class MapDataSO : ScriptableObject
{
    public int width;
    public int height;
    public Vector3Int origin; // Origin position of the tilemap bounds
    public int[] mapMatrix; // Flattened 2D array: row-major order
    
    // Helper method to get matrix value at x, y coordinates
    public int GetMatrixValue(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return 0;
        
        return mapMatrix[x + y * width];
    }
    
    // Helper method to set matrix value at x, y coordinates
    public void SetMatrixValue(int x, int y, int value)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return;
        
        mapMatrix[x + y * width] = value;
    }
}
