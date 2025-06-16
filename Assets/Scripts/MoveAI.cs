using UnityEngine;

public class RandomMoveAI : MonoBehaviour
{
    public float moveSpeed = 2f;                // 이동 속도
    public float changeDirectionInterval = 2f;  // 방향을 바꿀 시간 간격(초)
    public LayerMask Stage;                     // 장애물 레이어(Inspector에서 설정)

    private Vector2 moveDirection;
    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private float timer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        ChangeDirection(); // 시작하자마자 한 번 방향 지정
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 일정 시간마다 방향 변경 또는 이동 불가 시 방향 변경
        if (timer >= changeDirectionInterval || !CanMove(moveDirection))
        {
            ChangeDirection();
            timer = 0f;
        }

        // 이동
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    // 벽 체크: moveDirection 방향으로 앞으로 벽이 있으면 false
    bool CanMove(Vector2 direction)
    {
        if (direction == Vector2.zero)
            return false;

        float radius = circleCollider != null ? circleCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y) : 0.16f;
        float distance = radius + 0.1f; // 콜라이더 반지름 + 약간의 여유 (0.1~0.2)

        // Raycast 시작점: 현재 위치에서 "반지름만큼" 앞쪽으로 이동
        Vector2 start = rb.position + direction * radius;
        RaycastHit2D hit = Physics2D.Raycast(start, direction, distance, Stage);

        // 디버깅용 시각화 (Scene 뷰에서만)
        Debug.DrawRay(start, direction * distance, Color.red, 0.2f);

        return hit.collider == null;
    }

    void ChangeDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        System.Collections.Generic.List<Vector2> validDirections = new System.Collections.Generic.List<Vector2>();

        foreach (var dir in directions)
        {
            if (CanMove(dir))
                validDirections.Add(dir);
        }

        if (validDirections.Count > 0)
        {
            moveDirection = validDirections[Random.Range(0, validDirections.Count)];
        }
        else
        {
            moveDirection = Vector2.zero; // 이동 불가(모두 막혀 있으면 멈춤)
        }
    }
}
