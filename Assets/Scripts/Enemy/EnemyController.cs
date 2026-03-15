using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 2f;

    [Header("순찰")]
    public float patrolDistance = 3f;
    public float idleDuration = 2f;

    [Header("감지")]
    public float detectRange = 5f;
    public LayerMask playerLayer;

    [Header("피격 판정")]
    public float topHitThreshold = 0.3f; // 머리 판정 범위 (위에서 밟힌 경우)

    private Rigidbody2D rb;
    private Transform player;

    private float idleTimer;
    private Vector2 patrolStartPos;
    private int patrolDir = 1;
    private bool patrolDone;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player")?.transform;
    }

    public void StartIdle()
    {
        idleTimer = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    public bool IdleTimeOver()
    {
        idleTimer += Time.deltaTime;
        return idleTimer >= idleDuration;
    }

    public void StartPatrol()
    {
        patrolStartPos = transform.position;
        patrolDone = false;
    }

    public bool PatrolDone()
    {
        rb.linearVelocity = new Vector2(moveSpeed * patrolDir, rb.linearVelocity.y);
        float traveled = transform.position.x - patrolStartPos.x;
        if (Mathf.Abs(traveled) >= patrolDistance)
        {
            patrolDir *= -1;
            patrolStartPos = transform.position;
            patrolDone = true;
        }
        return patrolDone;
    }

    public void StartChase() { }

    public void UpdateChase()
    {
        if (player == null) return;
        float dir = player.position.x > transform.position.x ? 1f : -1f;
        rb.linearVelocity = new Vector2(moveSpeed * 1.5f * dir, rb.linearVelocity.y);
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;
        return Vector2.Distance(transform.position, player.position) <= detectRange;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        // 충돌 지점이 적의 위쪽인지 확인
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y <= -0.7f) // 위에서 밟힌 경우
            {
                // 플레이어를 튀어오르게
                PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
                if (pc != null) pc.BounceUp();

                // 적 사망
                Die();
                return;
            }
        }

        // 옆면 충돌 → 플레이어 피격
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
        if (playerController != null) playerController.TakeHit();
    }

    void Die()
    {
        // 추후 사망 애니메이션/이펙트로 교체 가능
        Destroy(gameObject);
    }

    void Update()
    {
        if (GetComponent<EnemyFSM>().currentState == EnemyState.Chase)
            UpdateChase();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}