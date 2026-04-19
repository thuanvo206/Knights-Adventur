using UnityEngine;

// BASE CLASS CHUNG cho tất cả enemy controllers
// BUG FIX: Logic cũ flip moveRight MỖI FRAME khi điều kiện (chạm tường/mép) đúng
// → nếu điều kiện đúng 10 frame liên tiếp thì flip 10 lần → enemy rung lắc tại chỗ
// FIX: Dùng wasAtBoundary để chỉ flip DUY NHẤT MỘT LẦN khi vừa chạm tường/mép
public abstract class EnemyControlBase : MonoBehaviour
{
    protected Rigidbody2D enemyBody2D;
    public float enemySpeed;

    protected Transform groundCheck;
    protected Transform edgeCheck;
    const float GroundCheckRadius = .1f;

    public LayerMask groundLayer;
    public bool moveRight;

    // BUG FIX: Flag để theo dõi trạng thái frame trước
    private bool wasAtBoundary = false;

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

        // BUG FIX: Chỉ flip khi VỪA chạm tường hoặc VỪA tới mép (leading edge)
        // thay vì flip liên tục mỗi frame
        bool atBoundary = isWall || !onEdge;
        if (atBoundary && !wasAtBoundary)
        {
            moveRight = !moveRight;
        }
        wasAtBoundary = atBoundary;

        enemyBody2D.linearVelocity = moveRight ? new Vector2(enemySpeed, 0) : new Vector2(-enemySpeed, 0);
        transform.localScale = moveRight ? new Vector2(-1, 1) : new Vector2(1, 1);
    }
}