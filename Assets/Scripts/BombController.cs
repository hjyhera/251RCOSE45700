using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviour
{   
    [Header("Bomb")]
    public GameObject bombPrefab;
    public KeyCode inputKey = KeyCode.Space;
    public float bombFuseTime = 3f;
    public int bombAmount = 1;
    private int bombsRemaining;

    [Header("Explosion")]
    public Explosion explosionPrefab;
    public LayerMask explosionLayerMask;
    public float explosionDuration = 1f;
    public int explosionRadius = 1;

    [Header("Tilemaps")]
    public Tilemap indestructibleTiles;
    public Tilemap destructibleTiles;    [Header("Destructible Items")]
    public Destructible destructiblePrefab;
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.2f;
    public GameObject[] spawnableItems;



    private void OnEnable()
    {
        bombsRemaining = bombAmount;
    }
    private void Update()
    {
        if (bombsRemaining > 0 && Input.GetKeyDown(inputKey) ){
            StartCoroutine(PlaceBomb());
        }
    }

    private IEnumerator PlaceBomb()
    {
        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bombsRemaining--;

        yield return new WaitForSeconds(bombFuseTime);

        position = bomb.transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(explosion.start);
        explosion.DestroyAfter(explosionDuration);

        Explode(position, Vector2.up, explosionRadius);
        Explode(position, Vector2.down, explosionRadius);
        Explode(position, Vector2.left, explosionRadius);
        Explode(position, Vector2.right, explosionRadius);        Destroy(bomb);
        bombsRemaining++;
    }
    
    private void Explode(Vector2 position, Vector2 direction, int length)
    {
        if (length <= 0){
            return;
        }

        position += direction;

        // Check if position is blocked by indestructible tiles
        if (IsPositionBlockedByIndestructible(position))
        {
            return; // Stop explosion at indestructible tiles
        }

        // Check if position is blocked by destructible tiles
        if (IsPositionBlockedByDestructible(position))
        {
            ClearDestructible(position);
            return; // Stop explosion after destroying destructible tile
        }

        // Check for other collisions (players, items, etc.)
        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask))
        {
            return;
        }

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(length > 1 ? explosion.middle : explosion.end);
        explosion.SetDirection(direction);
        explosion.DestroyAfter(explosionDuration);

        Explode(position, direction, length -1);
    }
    
    private bool IsPositionBlockedByIndestructible(Vector2 position)
    {
        if (indestructibleTiles == null) return false;
        
        Vector3Int cell = indestructibleTiles.WorldToCell(position);
        return indestructibleTiles.GetTile(cell) != null;
    }
    
    private bool IsPositionBlockedByDestructible(Vector2 position)
    {
        if (destructibleTiles == null) return false;
        
        Vector3Int cell = destructibleTiles.WorldToCell(position);        return destructibleTiles.GetTile(cell) != null;
    }
    
    private void ClearDestructible(Vector2 position)
    {
        Vector3Int cell = destructibleTiles.WorldToCell(position);
        TileBase tile = destructibleTiles.GetTile(cell);

        if (tile != null)
        {
            // Remove the tile from the tilemap
            destructibleTiles.SetTile(cell, null);
            
            // Spawn destructible effect (optional)
            if (destructiblePrefab != null)
            {
                Instantiate(destructiblePrefab, position, Quaternion.identity);
            }
              // Try to spawn items
            TrySpawnItem(position);
        }
    }
    
    private void TrySpawnItem(Vector3 position)
    {
        if (spawnableItems.Length > 0 && UnityEngine.Random.value < itemSpawnChance)
        {
            int randomIndex = UnityEngine.Random.Range(0, spawnableItems.Length);
            Instantiate(spawnableItems[randomIndex], position, Quaternion.identity);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bomb")){
            other.isTrigger = false;
        }
    }

    public void AddBomb()
    {
        bombAmount++;
        bombsRemaining++;
    }
}
