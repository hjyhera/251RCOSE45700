using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapMovementController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public bool useGridMovement = false;
    
    [Header("Input")]
    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;
    
    [Header("Tilemap Collision")]
    public Tilemap indestructibleTilemap;
    public Tilemap destructibleTilemap;
    public LayerMask additionalCollisionMask;
    
    [Header("Sprites")]
    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    public AnimatedSpriteRenderer spriteRendererDeath;
    
    private new Rigidbody2D rigidbody;
    private Vector2 direction = Vector2.down;
    private AnimatedSpriteRenderer activeSpriteRenderer;
    private Vector2 gridPosition;
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        activeSpriteRenderer = spriteRendererDown;
        
        if (useGridMovement)
        {
            // Snap to grid on start
            Vector3 pos = transform.position;
            gridPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
            transform.position = gridPosition;
        }
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void FixedUpdate()
    {
        if (useGridMovement)
        {
            HandleGridMovement();
        }
        else
        {
            HandleSmoothMovement();
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
    
    private void HandleSmoothMovement()
    {
        if (direction == Vector2.zero) return;
        
        Vector2 currentPosition = rigidbody.position;
        Vector2 targetPosition = currentPosition + direction * speed * Time.fixedDeltaTime;
        
        // Check if the target position is blocked
        if (!TilemapCollisionUtility.IsPositionBlocked(targetPosition, indestructibleTilemap, destructibleTilemap))
        {
            // Check for additional collisions
            if (!Physics2D.OverlapCircle(targetPosition, 0.4f, additionalCollisionMask))
            {
                rigidbody.MovePosition(targetPosition);
            }
        }
    }
    
    private void HandleGridMovement()
    {
        if (direction == Vector2.zero) return;
        
        Vector2 targetGridPosition = gridPosition + direction;
        Vector3 targetWorldPosition = new Vector3(targetGridPosition.x, targetGridPosition.y, transform.position.z);
        
        // Check if the target grid position is blocked
        if (!TilemapCollisionUtility.IsPositionBlocked(targetWorldPosition, indestructibleTilemap, destructibleTilemap))
        {
            // Check for additional collisions
            if (!Physics2D.OverlapCircle(targetWorldPosition, 0.4f, additionalCollisionMask))
            {
                gridPosition = targetGridPosition;
                rigidbody.MovePosition(gridPosition);
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
    
    private void DeathSequence()
    {
        enabled = false;
        
        // Disable bomb controller if it exists
        BombController bombController = GetComponent<BombController>();
        if (bombController != null)
        {
            bombController.enabled = false;
        }
        
        // Switch to death sprite
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
    
    /// <summary>
    /// Snaps the player to the nearest grid position
    /// </summary>
    public void SnapToGrid()
    {
        Vector3 pos = transform.position;
        Vector2 snappedPos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        transform.position = snappedPos;
        
        if (useGridMovement)
        {
            gridPosition = snappedPos;
        }
    }
}
