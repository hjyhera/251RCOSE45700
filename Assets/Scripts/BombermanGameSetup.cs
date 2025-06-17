using UnityEngine;
using UnityEngine.Tilemaps;

public class BombermanGameSetup : MonoBehaviour
{
    [Header("Tilemap References")]
    public Tilemap indestructibleTilemap;
    public Tilemap destructibleTilemap;
    
    [Header("Player Setup")]
    public GameObject[] players;
    
    [Header("Tilemap Collision Setup")]
    public bool setupTilemapColliders = true;
    public bool disableTilemapColliders = false; // Option to disable physics colliders for pure tilemap collision
    public PhysicsMaterial2D bouncePhysicsMaterial;
    
    void Start()
    {
        if (setupTilemapColliders)
        {
            SetupTilemapColliders();
        }
        
        SetupPlayerReferences();
    }
    
    void SetupTilemapColliders()
    {
        if (disableTilemapColliders)
        {
            DisableTilemapColliders();
            return;
        }
        
        // Setup indestructible tilemap collider
        if (indestructibleTilemap != null)
        {
            TilemapCollider2D indestructibleCollider = indestructibleTilemap.GetComponent<TilemapCollider2D>();
            if (indestructibleCollider == null)
            {
                indestructibleCollider = indestructibleTilemap.gameObject.AddComponent<TilemapCollider2D>();
            }
            
            // Ensure it's on the correct layer
            indestructibleTilemap.gameObject.layer = LayerMask.NameToLayer("Stage");
            
            // Add composite collider for better performance
            CompositeCollider2D compositeCollider = indestructibleTilemap.GetComponent<CompositeCollider2D>();
            if (compositeCollider == null)
            {
                compositeCollider = indestructibleTilemap.gameObject.AddComponent<CompositeCollider2D>();
                compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            }
            
            indestructibleCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            
            // Add rigidbody for composite collider
            Rigidbody2D rb = indestructibleTilemap.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = indestructibleTilemap.gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Static;
            
            Debug.Log("Indestructible tilemap collider setup complete");
        }
        
        // Setup destructible tilemap collider
        if (destructibleTilemap != null)
        {
            TilemapCollider2D destructibleCollider = destructibleTilemap.GetComponent<TilemapCollider2D>();
            if (destructibleCollider == null)
            {
                destructibleCollider = destructibleTilemap.gameObject.AddComponent<TilemapCollider2D>();
            }
            
            // Ensure it's on the correct layer
            destructibleTilemap.gameObject.layer = LayerMask.NameToLayer("Stage");
            
            // Add composite collider for better performance
            CompositeCollider2D compositeCollider = destructibleTilemap.GetComponent<CompositeCollider2D>();
            if (compositeCollider == null)
            {
                compositeCollider = destructibleTilemap.gameObject.AddComponent<CompositeCollider2D>();
                compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            }
            
            destructibleCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            
            // Add rigidbody for composite collider
            Rigidbody2D rb = destructibleTilemap.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = destructibleTilemap.gameObject.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Static;
            
            Debug.Log("Destructible tilemap collider setup complete");
        }
    }
    
    void SetupPlayerReferences()
    {
        // Setup tilemap references for all players
        foreach (GameObject player in players)
        {
            if (player == null) continue;
            
            // Setup BombController references
            BombController bombController = player.GetComponent<BombController>();
            if (bombController != null)
            {
                if (bombController.indestructibleTiles == null)
                    bombController.indestructibleTiles = indestructibleTilemap;
                    
                if (bombController.destructibleTiles == null)
                    bombController.destructibleTiles = destructibleTilemap;
            }
            
            // Setup TilemapMovementController references if using new movement system
            TilemapMovementController tilemapMovement = player.GetComponent<TilemapMovementController>();
            if (tilemapMovement != null)
            {
                if (tilemapMovement.indestructibleTilemap == null)
                    tilemapMovement.indestructibleTilemap = indestructibleTilemap;
                    
                if (tilemapMovement.destructibleTilemap == null)
                    tilemapMovement.destructibleTilemap = destructibleTilemap;
            }
            
            Debug.Log($"Setup complete for player: {player.name}");
        }
    }
    
    void DisableTilemapColliders()
    {
        // Disable physics colliders on indestructible tilemap
        if (indestructibleTilemap != null)
        {
            TilemapCollider2D collider = indestructibleTilemap.GetComponent<TilemapCollider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            CompositeCollider2D composite = indestructibleTilemap.GetComponent<CompositeCollider2D>();
            if (composite != null)
            {
                composite.enabled = false;
            }
        }
        
        // Disable physics colliders on destructible tilemap
        if (destructibleTilemap != null)
        {
            TilemapCollider2D collider = destructibleTilemap.GetComponent<TilemapCollider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            CompositeCollider2D composite = destructibleTilemap.GetComponent<CompositeCollider2D>();
            if (composite != null)
            {
                composite.enabled = false;
            }
        }
        
        Debug.Log("Tilemap physics colliders disabled - using pure tilemap collision detection");
    }
    
    [ContextMenu("Force Setup Colliders")]
    public void ForceSetupColliders()
    {
        SetupTilemapColliders();
        Debug.Log("Forced tilemap collider setup");
    }
    
    [ContextMenu("Assign Player References")]
    public void ForceSetupPlayerReferences()
    {
        SetupPlayerReferences();
        Debug.Log("Forced player reference setup");
    }
}
