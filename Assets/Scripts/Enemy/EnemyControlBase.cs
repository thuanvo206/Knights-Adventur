using UnityEngine;

public abstract class EnemyControlBase : MonoBehaviour
{
    protected Rigidbody2D enemyBody2D;
    public float enemySpeed;

    protected Transform groundCheck;
    protected Transform edgeCheck;
    const float GroundCheckRadius = .1f;

    public LayerMask groundLayer;
    public bool moveRight;

    private bool wasAtBoundary = false;

    // Cho phép từng loại quái override scale riêng — giữ đúng kích thước gốc
    protected virtual Vector2 ScaleRight => new Vector2(-1, 1);
    protected virtual Vector2 ScaleLeft  => new Vector2(1, 1);

    void Start()
    {
        enemyBody2D = GetComponent<Rigidbody2D>();
        groundCheck = transform.Find("GroundCheck");
        edgeCheck = transform.Find("EdgeCheck");
    }

    void Update()
    {
        if (groundCheck == null || edgeCheck == null) return;

        bool isWall = Physics2D.OverlapCircle(groundCheck.position, GroundCheckRadius, groundLayer);
        bool onEdge = Physics2D.OverlapCircle(edgeCheck.position, GroundCheckRadius, groundLayer);

        bool atBoundary = isWall || !onEdge;
        if (atBoundary && !wasAtBoundary)
            moveRight = !moveRight;
        wasAtBoundary = atBoundary;

        enemyBody2D.linearVelocity = moveRight
            ? new Vector2(enemySpeed, 0)
            : new Vector2(-enemySpeed, 0);

        // Dùng scale riêng của từng loại quái thay vì hardcode (-1,1)/(1,1)
        transform.localScale = moveRight ? ScaleRight : ScaleLeft;
    }
}