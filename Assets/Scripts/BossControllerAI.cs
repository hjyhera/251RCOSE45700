using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class MoveAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float changeDirectionInterval = 2f;

    [Header("Tilemap Collision")]
    public Tilemap indestructibleTilemap;    
    public Tilemap destructibleTilemap;      
    public Tilemap groundTilemap;            
    public LayerMask additionalCollisionMask; 

    [Header("Spawn")]
    public int spawnSearchAttempts = 100;

    [Header("Detection (Overlap)")]
    public LayerMask playerMask;
    public LayerMask explosionMask;
    public float detectRadius = 0.4f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 moveDirection;
    private float timer;
    private bool isDead;
    private BoundsInt mapBounds;

    private Vector3Int moveCellDir;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        indestructibleTilemap.CompressBounds();
        mapBounds = indestructibleTilemap.cellBounds;

        SetRandomSpawnPosition();
        ChooseNewDirection();
        timer = 0f;
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        timer += Time.fixedDeltaTime;
        if (timer >= changeDirectionInterval || !CanMove(moveCellDir))
        {
            ChooseNewDirection();
            timer = 0f;
        }

        Vector3Int currentCell = groundTilemap.WorldToCell(rb.position);
        Vector3Int nextCell    = currentCell + moveCellDir;
        Vector3   targetWorld  = groundTilemap.GetCellCenterWorld(nextCell);

        Vector2 newPos = Vector2.MoveTowards(
            rb.position,
            (Vector2)targetWorld,
            moveSpeed * Time.fixedDeltaTime
        );
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, (Vector2)targetWorld) < 0.01f)
            SnapToTileBottom();

        CheckOverlapHits();
    }


    private bool CanMove(Vector3Int cellDir)
    {
        Vector3Int currentCell = groundTilemap.WorldToCell(rb.position);
        Vector3Int nextCell = currentCell + cellDir;

        if (!groundTilemap.HasTile(nextCell))
            return false;

        Vector3 nextCenter = groundTilemap.GetCellCenterWorld(nextCell);

        if (!mapBounds.Contains(nextCell) ||
            TilemapCollisionUtility.IsPositionBlocked(nextCenter, indestructibleTilemap, destructibleTilemap))
            return false;

        if (Physics2D.OverlapCircle(nextCenter, 0.35f, additionalCollisionMask))
            return false;

        return true;
    }

    private void ChooseNewDirection()
    {
        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        var valid = new List<Vector3Int>();
        foreach (var d in dirs)
            if (CanMove(d)) valid.Add(d);

        if (valid.Count == 0)
        {
            moveCellDir = Vector3Int.zero;
            return;
        }

        if (valid.Contains(moveCellDir) && Random.value < 0.5f)
            return;

        moveCellDir = valid[Random.Range(0, valid.Count)];
    }
    private void SetRandomSpawnPosition()
    {
        indestructibleTilemap.CompressBounds();
        BoundsInt bounds = indestructibleTilemap.cellBounds;

        List<Vector3Int> groundCells = new List<Vector3Int>();
        for (int x = bounds.xMin; x < bounds.xMax; x++)
            for (int y = bounds.yMin; y < bounds.yMax; y++)
                if (groundTilemap.HasTile(new Vector3Int(x, y, 0)))
                    groundCells.Add(new Vector3Int(x, y, 0));

        for (int i = 0; i < spawnSearchAttempts; i++)
        {
            var cell = groundCells[Random.Range(0, groundCells.Count)];
            Vector3 center = groundTilemap.GetCellCenterWorld(cell);

            if (TilemapCollisionUtility.IsPositionBlocked(center, indestructibleTilemap, destructibleTilemap)) continue;
            if (Physics2D.OverlapCircle(center, 0.2f, additionalCollisionMask)) continue;

            bool hasExit = false;
            foreach (var d in new Vector3Int[]{Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right})
                if (CanMove(d))
                {
                    hasExit = true;
                    break;
                }
            if (!hasExit) continue;

            // 유효 위치 발견!
            rb.position = center;
            transform.position = center;
            return;
        }

        Debug.LogWarning("MoveAI: No valid spawn positions in green area");
    }

    private void SnapToTileBottom()
    {
        Vector2 p = rb.position;
        p.x = Mathf.Round(p.x - 0.5f) + 0.5f;
        p.y = Mathf.Round(p.y);
        rb.position = p;
        transform.position = p;
    }
    private void CheckOverlapHits()
    {
        if (Physics2D.OverlapCircle(rb.position, detectRadius, explosionMask))
        {
            StartCoroutine(FadeAndDie());
            return;
        }

        Collider2D player = Physics2D.OverlapCircle(rb.position, detectRadius, playerMask);
        if (player != null)
            player.GetComponent<MovementController>()?.DeathSequence();
    }

    private IEnumerator FadeAndDie()
    {
        if (isDead) yield break;
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        float t = 0f, fade = 0.4f;
        Color start = sr.color;

        while (t < fade)
        {
            t += Time.deltaTime;
            sr.color = new Color(start.r, start.g, start.b, 1f - t / fade);
            yield return null;
        }

        sr.color = new Color(start.r, start.g, start.b, 0f);
        gameObject.SetActive(false);

        FindFirstObjectByType<GameManager>()?.CheckWinState();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
