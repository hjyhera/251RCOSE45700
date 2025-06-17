using UnityEngine;
using UnityEngine.Tilemaps;

public class ImprovedMovementController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public bool usePhysicsMovement = false; // Toggle between physics and direct movement
    
    [Header("Input")]
    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;
    
    [Header("Collision")]
    public Tilemap indestructibleTilemap;
    public Tilemap destructibleTilemap;
    public Vector2 playerSize = new Vector2(0.8f, 0.8f); // Slightly smaller than tile size
    public LayerMask physicsCollisionMask = -1; // For physics-based collision
    
    [Header("Sprites")]
    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    public AnimatedSpriteRenderer spriteRendererDeath;
    
    private new Rigidbody2D rigidbody;
    private Vector2 direction = Vector2.down;
    private AnimatedSpriteRenderer activeSpriteRenderer;
    private CircleCollider2D playerCollider;
    private float playerRadius;
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CircleCollider2D>();
        activeSpriteRenderer = spriteRendererDown;
        
        // Calculate player radius
        playerRadius = playerCollider != null ? 
            playerCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y) : 
            0.4f;
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void FixedUpdate()
    {
        if (usePhysicsMovement)
        {
            HandlePhysicsMovement();
        }
        else
        {
            HandleDirectMovement();
        }
    }
    
    private void HandleInput()
    {
        Vector2 newDirection = Vector2.zero;
        AnimatedSpriteRenderer newSpriteRenderer = activeSpriteRenderer;
        
        if (Input.GetKey(inputUp))
        {
            newDirection = Vector2.up;
            newSpriteRenderer = spriteRendererUp;
        }
        else if (Input.GetKey(inputDown))
        {
            newDirection = Vector2.down;
            newSpriteRenderer = spriteRendererDown;
        }
        else if (Input.GetKey(inputLeft))
        {
            newDirection = Vector2.left;
            newSpriteRenderer = spriteRendererLeft;
        }
        else if (Input.GetKey(inputRight))
        {
            newDirection = Vector2.right;
            newSpriteRenderer = spriteRendererRight;
        }
        
        SetDirection(newDirection, newSpriteRenderer);
    }
    
    private void HandlePhysicsMovement()
    {
        // Use physics-based movement (original system)
        Vector2 currentPosition = rigidbody.position;
        Vector2 targetPosition = currentPosition + direction * speed * Time.fixedDeltaTime;
        
        // Check for physics collisions
        if (!Physics2D.OverlapCircle(targetPosition, playerRadius, physicsCollisionMask))
        {
            rigidbody.MovePosition(targetPosition);
        }
    }
    
    private void HandleDirectMovement()
    {
        if (direction == Vector2.zero) return;
        
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = currentPosition + (Vector3)direction * speed * Time.fixedDeltaTime;
        
        // Use improved collision detection
        Vector3 safePosition = TilemapCollisionUtility.GetSafeMovementPosition(
            currentPosition, 
            targetPosition, 
            playerSize, 
            indestructibleTilemap, 
            destructibleTilemap
        );
        
        // Move to the safe position
        if (safePosition != currentPosition)
        {
            if (rigidbody != null)
            {
                rigidbody.MovePosition(safePosition);
            }
            else
            {
                transform.position = safePosition;
            }
        }
    }
    
    private void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
    {
        direction = newDirection;
        
        if (spriteRendererUp != null) spriteRendererUp.enabled = spriteRenderer == spriteRendererUp;
        if (spriteRendererDown != null) spriteRendererDown.enabled = spriteRenderer == spriteRendererDown;
        if (spriteRendererLeft != null) spriteRendererLeft.enabled = spriteRenderer == spriteRendererLeft;
        if (spriteRendererRight != null) spriteRendererRight.enabled = spriteRenderer == spriteRendererRight;
        
        activeSpriteRenderer = spriteRenderer;
        if (activeSpriteRenderer != null)
        {
            activeSpriteRenderer.idle = direction == Vector2.zero;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            DeathSequence();
        }
    }
    
    public void DeathSequence()
    {
        enabled = false;
        
        BombController bombController = GetComponent<BombController>();
        if (bombController != null)
        {
            bombController.enabled = false;
        }
        
        if (spriteRendererUp != null) spriteRendererUp.enabled = false;
        if (spriteRendererDown != null) spriteRendererDown.enabled = false;
        if (spriteRendererLeft != null) spriteRendererLeft.enabled = false;
        if (spriteRendererRight != null) spriteRendererRight.enabled = false;
        if (spriteRendererDeath != null) spriteRendererDeath.enabled = true;
        
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
    private void OnDrawGizmos()
    {
        // Draw player bounds
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, playerSize);
        
        // Draw movement direction
        if (direction != Vector2.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, (Vector3)direction * 0.5f);
        }
        
        // Draw collision radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerRadius);
    }
    
    [ContextMenu("Test Collision")]
    public void TestCollision()
    {
        bool blocked = TilemapCollisionUtility.IsRectangleBlocked(
            transform.position, 
            playerSize, 
            indestructibleTilemap, 
            destructibleTilemap
        );
        
        Debug.Log($"Position {transform.position} is blocked: {blocked}");
    }
}
