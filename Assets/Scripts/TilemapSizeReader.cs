

using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSizeReader : MonoBehaviour
{
    public Tilemap Indestructables;

    void Start()
    {
        Indestructables.CompressBounds(); // Shrinks bounds to fit placed tiles exactly
        BoundsInt bounds = Indestructables.cellBounds;
        Vector3Int origin = bounds.position;
        Vector3Int size = bounds.size;

        Debug.Log($"Origin: {origin}, Size: {size}");
    }
}

