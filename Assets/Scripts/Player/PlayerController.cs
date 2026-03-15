using UnityEngine;

public enum PlayerState
{
    Idle,
    Run,
    Jump,
    Fall,
    WallJump,
    Hit
}

public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 5f;

    [Header("점프")]
    public float jumpForce = 10f;
    public float bounceForce = 8f;      // 적 밟았을 때 튀어오르는 힘

    [Header("벽 점프")]
    public float wallJumpForceX = 6f;
    public float wallJumpForceY = 10f;

    [Header("피격")]
    public float hitKnockback = 5f;     // 피격 시 넉백
    public float hitDuration = 0.5f;    // 피격 상태 지속 시간

    [Header("Ground / Wall Check")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float groundCheckRadius = 0.2f;
    public float wallCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public PlayerState currentState { get; private set; } = PlayerState.Idle;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool isTouchingWall;
    private float hitTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckGround();
        CheckWall();
        HandleInput();
        UpdateState();
    }

    void FixedUpdate()
    {
        if (currentState != PlayerState.Hit)
            Move();
    }

    // ── 입력 ──────────────────────────────
    void HandleInput()
    {
        if (currentState == PlayerState.Hit) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
                Jump();
            else if (isTouchingWall)
                WallJump();
        }
    }

    // ── 상태 업데이트 ─────────────────────
    void UpdateState()
    {
        switch (currentState)
        {
            case PlayerState.Hit:
                hitTimer += Time.deltaTime;
                if (hitTimer >= hitDuration)
                    ChangeState(isGrounded ? PlayerState.Idle : PlayerState.Fall);
                break;

            case PlayerState.Jump:
                if (rb.linearVelocity.y < 0)
                    ChangeState(PlayerState.Fall);
                break;

            case PlayerState.WallJump:
                if (rb.linearVelocity.y < 0)
                    ChangeState(PlayerState.Fall);
                break;

            case PlayerState.Fall:
                if (isGrounded)
                    ChangeState(moveInput != 0 ? PlayerState.Run : PlayerState.Idle);
                break;

            case PlayerState.Idle:
            case PlayerState.Run:
                if (!isGrounded)
                    ChangeState(PlayerState.Fall);
                else if (moveInput != 0)
                    ChangeState(PlayerState.Run);
                else
                    ChangeState(PlayerState.Idle);
                break;
        }
    }

    void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
    }

    // ── 이동 ──────────────────────────────
    void Move()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    // ── 점프 ──────────────────────────────
    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        ChangeState(PlayerState.Jump);
    }

    // ── 벽 점프 ───────────────────────────
    void WallJump()
    {
        float dir = moveInput >= 0 ? -1f : 1f; // 벽 반대 방향으로
        rb.linearVelocity = new Vector2(wallJumpForceX * dir, wallJumpForceY);
        ChangeState(PlayerState.WallJump);
    }

    // ── 적 밟기 → 튀어오름 ────────────────
    public void BounceUp()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
        ChangeState(PlayerState.Jump);
    }

    // ── 피격 ──────────────────────────────
    public void TakeHit()
    {
        if (currentState == PlayerState.Hit) return; // 무적 시간

        hitTimer = 0f;
        // 적 방향 반대로 넉백
        float knockDir = rb.linearVelocity.x >= 0 ? -1f : 1f;
        rb.linearVelocity = new Vector2(hitKnockback * knockDir, hitKnockback * 0.5f);
        ChangeState(PlayerState.Hit);
    }

    // ── 체크 ──────────────────────────────
    void CheckGround()
    {
        if (groundCheck == null) return;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void CheckWall()
    {
        if (wallCheck == null) return;
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
}