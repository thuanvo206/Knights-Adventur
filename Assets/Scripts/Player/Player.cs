using UnityEngine;
using TMPro;
using Fusion;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : NetworkBehaviour 
{
    internal Rigidbody2D body2D;
    public float knockBackForce = 15000;

    [Header("Movement Settings")]
    [Range(0, 20)] public float playerSpeed = 15;
    [Range(500, 1500)] public float jumpPower = 1000;
    [Range(500, 1000)] public float doubleJumpPower = 600;

    [Networked] public int maxPlayerHealth { get; set; } = 100;
    [Networked] public int currentPlayerHealth { get; set; }
    [Networked] public int currentCoin { get; set; }
    [Networked] public NetworkBool isDead { get; set; }
    [Networked] public NetworkBool isGround { get; set; }
    [Networked] public NetworkBool canDoubleJump { get; set; }

    public bool isHurt;
    public bool addHealth;
    public bool earnCoin;
    public bool canDamage = true; 

    bool facingRight = true;
    Transform groundCheck;
    const float GroundCheckRadius = .1f;
    public LayerMask groundLayer;
    Animator playerAnimController;
    AudioSource audioSource;
    AudioClip audioJump;

    public override void Spawned() 
    {
        body2D = GetComponent<Rigidbody2D>();
        groundCheck = transform.Find("GroundCheck");
        playerAnimController = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioJump = Resources.Load("Sounds/Jump") as AudioClip;

        if (HasStateAuthority)
        {
            currentPlayerHealth = maxPlayerHealth;
            isDead = false;
            currentCoin = 0;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object == null || !Object.IsValid || (bool)isDead) return;

        // Cập nhật GroundCheck liên tục trong nhịp mạng
        isGround = Physics2D.OverlapCircle(groundCheck.position, GroundCheckRadius, groundLayer) != null;

        if (GetInput(out NetworkInputData data))
        {
            // DI CHUYỂN: Sử dụng vận tốc nhưng kết hợp với Network Transform bên ngoài
            body2D.linearVelocity = new Vector2(data.move.x * playerSpeed, body2D.linearVelocity.y);

            // QUAY MẶT: Chạy trực tiếp để không bị delay 3s
            if (data.move.x != 0) Flip(data.move.x);

            // NHẢY: Sửa lỗi nhảy lúc được lúc không
            if (data.jumpPressed)
            {
                if ((bool)isGround)
                {
                    Jump();
                    canDoubleJump = true;
                }
                else if ((bool)canDoubleJump)
                {
                    DoubleJump();
                    canDoubleJump = false;
                }
            }
        }

        if (HasStateAuthority)
        {
            HandleStatusEffects();
            CheckDeathLimits();
        }
    }

public override void Render()
{
    // Giữ nguyên các code cũ của bạn về Flip và Animation...
    UpdateAnimations();

    // --- ĐOẠN CODE MỚI ĐỂ HIỂN THỊ ĐÚNG MÁU MÌNH ---
    // Nếu nhân vật này là của TÔI điều khiển
    if (HasInputAuthority) 
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm != null && gm.healthBar != null)
        {
            gm.healthBar.maxValue = maxPlayerHealth;
            gm.healthBar.value = currentPlayerHealth;
        }
    }
}

    void HandleStatusEffects()
    {
        if (!HasStateAuthority) return;
        if (isHurt) { currentPlayerHealth -= 10; isHurt = false; }
        if (addHealth) { currentPlayerHealth = Mathf.Min(currentPlayerHealth + 20, maxPlayerHealth); addHealth = false; }
        if (earnCoin) { currentCoin += 1; earnCoin = false; }
    }

    public void Jump()
    {
        body2D.linearVelocity = new Vector2(body2D.linearVelocity.x, 0);
        body2D.AddForce(new Vector2(0, jumpPower));
    }

    public void DoubleJump()
    {
        body2D.linearVelocity = new Vector2(body2D.linearVelocity.x, 0);
        body2D.AddForce(new Vector2(0, doubleJumpPower));
    }

    void Flip(float horizontal)
    {
        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
        {
            facingRight = !facingRight;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1;
            transform.localScale = theScale;
        }
    }

    void UpdateAnimations()
    {
        if (playerAnimController == null) return;
        playerAnimController.SetFloat("VelocityX", Mathf.Abs(body2D.linearVelocity.x));
        playerAnimController.SetFloat("VelocityY", body2D.linearVelocity.y);
        playerAnimController.SetBool("isGround", (bool)isGround);
        playerAnimController.SetBool("isDead", (bool)isDead);
    }

    void CheckDeathLimits()
    {
        if (HasStateAuthority && (transform.position.y <= -6 || currentPlayerHealth <= 0))
            isDead = true;
    }
}