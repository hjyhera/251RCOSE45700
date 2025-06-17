using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float changeDirectionInterval = 2f;
    
    [Header("Tilemap Collision")]
    public Tilemap indestructibleTilemap;
    public Tilemap destructibleTilemap;
    public LayerMask additionalCollisionMask;
    
    [Header("AI Behavior")]
    public bool avoidExplosions = true;
    public float explosionAvoidanceRadius = 2f;
    
    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private float timer;
    private float colliderRadius;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        
        // Calculate collider radius with scale
        colliderRadius = circleCollider != null ? 
            circleCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y) : 
            0.16f;
            
        ChangeDirection();
    }
    
    void Update()
    {
        timer += Time.deltaTime;
        
        // Change direction at intervals or when can't move
        if (timer >= changeDirectionInterval || !CanMove(moveDirection))
        {
            ChangeDirection();
            timer = 0f;
        }
        
        // Apply movement
        rb.linearVelocity = moveDirection * moveSpeed;
    }
    
    bool CanMove(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return false;
        
        float checkDistance = colliderRadius + 0.1f;
        Vector2 startPos = rb.position + direction * colliderRadius;
        Vector2 targetPos = startPos + direction * checkDistance;
        
        // Check tilemap collision
        if (TilemapCollisionUtility.IsPositionBlocked(targetPos, indestructibleTilemap, destructibleTilemap))
        {
            return false;
        }
        
        // Check additional collisions (other objects)
        if (Physics2D.OverlapCircle(targetPos, colliderRadius, additionalCollisionMask))
        {
            return false;
        }
        
        // Check for explosions if avoidance is enabled
        if (avoidExplosions && IsExplosionNearby(targetPos))
        {
            return false;
        }
        
        return true;
    }
    
    bool IsExplosionNearby(Vector2 position)
    {
        Collider2D[] explosions = Physics2D.OverlapCircleAll(position, explosionAvoidanceRadius, 
            LayerMask.GetMask("Explosion"));
        return explosions.Length > 0;
    }
    
    void ChangeDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        System.Collections.Generic.List<Vector2> validDirections = new System.Collections.Generic.List<Vector2>();
        
        // Find all valid directions
        foreach (var dir in directions)
        {
            if (CanMove(dir))
                validDirections.Add(dir);
        }
        
        if (validDirections.Count > 0)
        {
            // Choose random valid direction
            moveDirection = validDirections[Random.Range(0, validDirections.Count)];
        }
        else
        {
            // If no valid directions, try to find escape route
            moveDirection = FindEscapeDirection();
        }
    }
    
    Vector2 FindEscapeDirection()
    {
        // Try to find a direction that leads away from obstacles
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 bestDirection = Vector2.zero;
        float bestScore = float.MinValue;
        
        foreach (var dir in directions)
        {
            float score = EvaluateDirection(dir);
            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = dir;
            }
        }
        
        return bestDirection;
    }
    
    float EvaluateDirection(Vector2 direction)
    {
        float score = 0f;
        Vector2 checkPos = rb.position;
        
        // Check multiple steps in this direction
        for (int i = 1; i <= 3; i++)
        {
            checkPos += direction;
            
            if (!TilemapCollisionUtility.IsPositionBlocked(checkPos, indestructibleTilemap, destructibleTilemap))
            {
                score += 1f; // Bonus for each free step
            }
            else
            {
                break; // Stop checking if blocked
            }
        }
        
        // Penalty for explosions
        if (avoidExplosions && IsExplosionNearby(checkPos))
        {
            score -= 10f;
        }
        
        return score;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            DeathSequence();
        }
    }
    
    private void DeathSequence()
    {
        enabled = false;
        
        // Disable bomb controller if it exists
        BombController bombController = GetComponent<BombController>();
        if (bombController != null)
        {
            bombController.enabled = false;
        }
        
        // You can add death animation here
        
        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }
    
    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
        
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.CheckWinState();
        }
    }
    
    // Debug visualization
    void OnDrawGizmos()
    {
        if (rb != null)
        {
            // Draw movement direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(rb.position, moveDirection * 0.5f);
            
            // Draw collision check area
            Gizmos.color = Color.red;
            Vector2 checkPos = rb.position + moveDirection * (colliderRadius + 0.1f);
            Gizmos.DrawWireSphere(checkPos, colliderRadius);
            
            // Draw explosion avoidance radius
            if (avoidExplosions)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(rb.position, explosionAvoidanceRadius);
            }
        }
    }
}
