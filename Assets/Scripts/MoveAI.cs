// using UnityEngine;
// using UnityEngine.Tilemaps;
// using System.Collections;
// using System.Collections.Generic;

// public class MoveAI : MonoBehaviour
// {
//     /* ───── Inspector 설정 ───── */
//     [Header("Movement")]
//     public float moveSpeed = 2f;
//     public float changeDirectionInterval = 2f;

//     [Header("Tilemap Collision")]
//     public Tilemap indestructibleTilemap;
//     public Tilemap destructibleTilemap;
//     public LayerMask additionalCollisionMask;

//     [Header("Spawn")]
//     public int spawnSearchAttempts = 100;

//     [Header("Detection (Overlap)")]
//     public LayerMask playerMask;
//     public LayerMask explosionMask;
//     public float detectRadius = 0.4f;

//     /* ───── 내부 필드 ───── */
//     private Rigidbody2D rb;
//     private SpriteRenderer sr;

//     private Vector2 moveDirection;
//     private float timer;
//     private bool isDead;

//     /* ───── 초기화 ───── */
//     private void Awake()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         sr = GetComponent<SpriteRenderer>();
//     }

//     private void Start()
//     {
//         SetRandomSpawnPosition();
//         ChooseNewDirection();
//     }

//     /* ───── FixedUpdate ───── */
//     private void FixedUpdate()
//     {
//         if (isDead) return;

//         timer += Time.fixedDeltaTime;
//         if (timer >= changeDirectionInterval || !CanMove(moveDirection))
//         {
//             ChooseNewDirection();
//             timer = 0f;
//         }

//         rb.linearVelocity = moveDirection * moveSpeed;
//         CheckOverlapHits();
//     }

//     /* ───── 이동 가능 여부 ───── */
//     private bool CanMove(Vector2 dir)
//     {
//         if (dir == Vector2.zero) return false;

//         // ① 이동하려는 “발끝” 위치(pivot)
//         Vector2 pivotPos = rb.position + dir;

//         // ② 타일 가운데로 0.5 ↑ 올린 체크 포인트
//         Vector2 checkPos = pivotPos + Vector2.up * 0.5f;

//         // 타일 충돌
//         if (TilemapCollisionUtility.IsPositionBlocked(checkPos,
//                                                       indestructibleTilemap,
//                                                       destructibleTilemap))
//             return false;

//         // 다른 오브젝트 충돌(발끝 기준)
//         if (Physics2D.OverlapCircle(pivotPos, 0.35f, additionalCollisionMask))
//             return false;

//         return true;
//     }

//     /* ───── 방향 선택 ───── */
//     private void ChooseNewDirection()
//     {
//         Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
//         List<Vector2> valid = new();

//         foreach (var d in dirs) if (CanMove(d)) valid.Add(d);
//         if (valid.Count == 0) { moveDirection = Vector2.zero; return; }

//         if (moveDirection != Vector2.zero && Random.value < 0.5f && CanMove(moveDirection))
//             return;                      // 50% 직진 유지

//         moveDirection = valid[Random.Range(0, valid.Count)];
//     }

//     /* ───── 스폰 위치 찾기 ───── */
//     private void SetRandomSpawnPosition()
//     {
//         indestructibleTilemap.CompressBounds();
//         BoundsInt bounds = indestructibleTilemap.cellBounds;

//         List<Vector3> candidates = new();
//         Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

//         for (int x = bounds.xMin; x < bounds.xMax; x++)
//         {
//             for (int y = bounds.yMin; y < bounds.yMax; y++)
//             {
//                 // 셀 왼쪽-아래 → Anchor(0.5,0) 더해서 “바닥 중앙” pivot 좌표
//                 Vector3Int cell = new(x, y, 0);
//                 Vector3 pivotPos = indestructibleTilemap.CellToWorld(cell) + indestructibleTilemap.tileAnchor;

//                 // 타일 가운데 위치(충돌 체크용)
//                 Vector3 checkPos = pivotPos + Vector3.up * 0.5f;

//                 if (TilemapCollisionUtility.IsPositionBlocked(checkPos, indestructibleTilemap, destructibleTilemap) ||
//                     Physics2D.OverlapCircle(pivotPos, 0.2f, additionalCollisionMask))
//                     continue;

//                 // 네 방향 중 하나라도 이동 가능해야 후보
//                 bool freeNeighbor = false;
//                 foreach (var d in dirs)
//                 {
//                     Vector3 neighPivot = pivotPos + (Vector3)d;
//                     Vector3 neighCheck = neighPivot + Vector3.up * 0.5f;

//                     if (!TilemapCollisionUtility.IsPositionBlocked(neighCheck, indestructibleTilemap, destructibleTilemap) &&
//                         !Physics2D.OverlapCircle(neighPivot, 0.2f, additionalCollisionMask))
//                     { freeNeighbor = true; break; }
//                 }
//                 if (freeNeighbor) candidates.Add(pivotPos);
//             }
//         }

//         if (candidates.Count > 0)
//         {
//             Vector3 pos = candidates[Random.Range(0, candidates.Count)];
//             rb.position = pos;
//             transform.position = pos;
//         }
//         else
//             Debug.LogWarning("MoveAI: No valid spawn positions!");
//     }

//     /* ───── 플레이어/폭발 감지 ───── */
//     private void CheckOverlapHits()
//     {
//         if (Physics2D.OverlapCircle(rb.position, detectRadius, explosionMask))
//         {
//             StartCoroutine(FadeAndDie());
//             return;
//         }

//         Collider2D player = Physics2D.OverlapCircle(rb.position, detectRadius, playerMask);
//         if (player != null)
//             player.GetComponent<MovementController>()?.DeathSequence();
//     }

//     /* ───── 페이드 아웃 ───── */
//     private IEnumerator FadeAndDie()
//     {
//         if (isDead) yield break;
//         isDead = true;

//         rb.linearVelocity = Vector2.zero;
//         float t = 0f, fade = 0.4f;
//         Color start = sr.color;

//         while (t < fade)
//         {
//             t += Time.deltaTime;
//             sr.color = new(start.r, start.g, start.b, 1f - t / fade);
//             yield return null;
//         }

//         sr.color = new(start.r, start.g, start.b, 0f);
//         gameObject.SetActive(false);

//         FindFirstObjectByType<GameManager>()?.CheckWinState();
//     }

//     /* ───── Gizmos ───── */
//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.magenta;
//         Gizmos.DrawWireSphere(transform.position, detectRadius);
//     }
// }

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class MoveAI : MonoBehaviour
{
    /* ───────── Inspector 설정 ───────── */
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float changeDirectionInterval = 2f;

    [Header("Tilemap Collision")]
    public Tilemap indestructibleTilemap;    // 벽 맵
    public Tilemap destructibleTilemap;      // 파괴 가능한 타일 맵
    public Tilemap groundTilemap;            // 이동 가능 바닥(초록) 맵
    public LayerMask additionalCollisionMask; // 폭탄·플레이어 등

    [Header("Spawn")]
    public int spawnSearchAttempts = 100;

    [Header("Detection (Overlap)")]
    public LayerMask playerMask;
    public LayerMask explosionMask;
    public float detectRadius = 0.4f;

    /* ───────── 내부 필드 ───────── */
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Vector2 moveDirection;
    private float timer;
    private bool isDead;
    private BoundsInt mapBounds;

    /* ───────── 초기화 ───────── */
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // 맵 경계 계산
        indestructibleTilemap.CompressBounds();
        mapBounds = indestructibleTilemap.cellBounds;

        // 슬라임 랜덤 스폰
        SetRandomSpawnPosition();
        ChooseNewDirection();
        timer = 0f;
    }

    /* ───────── FixedUpdate ───────── */
    private void FixedUpdate()
    {
        if (isDead) return;

        timer += Time.fixedDeltaTime;
        if (timer >= changeDirectionInterval || !CanMove(moveDirection))
        {
            ChooseNewDirection();
            timer = 0f;
        }

        rb.linearVelocity = moveDirection * moveSpeed;
        SnapToTileBottom();
        CheckOverlapHits();
    }

    /* ───────── 이동 가능 여부 ───────── */
    private bool CanMove(Vector2 dir)
    {
        if (dir == Vector2.zero) return false;

        Vector2 nextCenter = rb.position + dir;
        Vector3Int cell = groundTilemap.WorldToCell(nextCenter);

        // 1) 바닥 타일이 아니면 이동 불가
        if (!groundTilemap.HasTile(cell))
            return false;

        // 2) 맵 경계 & 벽/파괴 타일 체크
        if (!mapBounds.Contains(cell) ||
            TilemapCollisionUtility.IsPositionBlocked(nextCenter,
                                                     indestructibleTilemap,
                                                     destructibleTilemap))
            return false;

        // 3) 추가 오브젝트 충돌 체크
        if (Physics2D.OverlapCircle(nextCenter, 0.35f, additionalCollisionMask))
            return false;

        return true;
    }

    /* ───────── 방향 선택 ───────── */
    private void ChooseNewDirection()
    {
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        List<Vector2> valid = new List<Vector2>();
        foreach (var d in dirs)
            if (CanMove(d)) valid.Add(d);

        if (valid.Count == 0)
        {
            moveDirection = Vector2.zero;
            return;
        }

        if (moveDirection != Vector2.zero && Random.value < 0.5f && CanMove(moveDirection))
            return;

        moveDirection = valid[Random.Range(0, valid.Count)];
    }

    /* ───────── 스폰 위치 찾기 ───────── */
    private void SetRandomSpawnPosition()
    {
        indestructibleTilemap.CompressBounds();
        BoundsInt bounds = indestructibleTilemap.cellBounds;

        List<Vector3> candidates = new List<Vector3>();
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        for (int i = 0; i < spawnSearchAttempts; i++)
        {
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int cell = new Vector3Int(x, y, 0);
            // 타일 중심
            Vector3 center = groundTilemap.CellToWorld(cell) + groundTilemap.cellSize * 0.5f;

            // 초록 바닥 확인
            if (!groundTilemap.HasTile(cell))
                continue;
            // 벽/파괴 타일 위인지 체크
            if (TilemapCollisionUtility.IsPositionBlocked(center,
                                                         indestructibleTilemap,
                                                         destructibleTilemap))
                continue;
            // 오브젝트 차지 영역인지 체크
            if (Physics2D.OverlapCircle(center, 0.2f, additionalCollisionMask))
                continue;

            // 이웃 이동 가능 확인
            bool hasExit = false;
            foreach (var d in dirs)
            {
                Vector3 nCenter = center + (Vector3)d;
                Vector3Int nCell = groundTilemap.WorldToCell(nCenter);
                if (!groundTilemap.HasTile(nCell)) continue;
                if (!mapBounds.Contains(nCell)) continue;
                if (TilemapCollisionUtility.IsPositionBlocked(nCenter,
                                                             indestructibleTilemap,
                                                             destructibleTilemap))
                    continue;
                if (Physics2D.OverlapCircle(nCenter, 0.2f, additionalCollisionMask))
                    continue;
                hasExit = true;
                break;
            }
            if (hasExit)
                candidates.Add(center);
        }

        if (candidates.Count > 0)
        {
            Vector3 pos = candidates[Random.Range(0, candidates.Count)];
            rb.position = pos;
            transform.position = pos;
        }
        else
        {
            Debug.LogWarning("MoveAI: No valid spawn positions in green area");
        }
    }

    /* ───────── 타일 중앙 스냅 ───────── */
    private void SnapToTileBottom()
    {
        Vector2 p = rb.position;
        // pivot.x 는 여전히 센터, pivot.y 는 정수(바닥)
        p.x = Mathf.Round(p.x - 0.5f) + 0.5f;
        p.y = Mathf.Round(p.y - 0f) + 0f;
        rb.position = p;
        transform.position = p;
    }
    /* ───────── 플레이어/폭발 감지 ───────── */
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

    /* ───────── 페이드 아웃 ───────── */
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

    /* ───────── Gizmos ───────── */
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
