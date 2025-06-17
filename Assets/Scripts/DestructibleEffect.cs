using UnityEngine;

public class DestructibleEffect : MonoBehaviour
{
    [Header("Visual Effect")]
    public float effectDuration = 1f;
    public AnimatedSpriteRenderer effectRenderer;
    
    [Header("Item Spawning")]
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.2f;
    public GameObject[] spawnableItems;
    
    private bool hasSpawnedItem = false;

    private void Start()
    {
        // Start the destruction effect
        if (effectRenderer != null)
        {
            effectRenderer.enabled = true;
        }
        
        // Spawn item immediately
        TrySpawnItem();
        
        // Destroy this effect after duration
        Destroy(gameObject, effectDuration);
    }
    
    private void TrySpawnItem()
    {
        if (!hasSpawnedItem && spawnableItems.Length > 0 && Random.value < itemSpawnChance)
        {
            int randomIndex = Random.Range(0, spawnableItems.Length);
            Instantiate(spawnableItems[randomIndex], transform.position, Quaternion.identity);
            hasSpawnedItem = true;
        }
    }
}
