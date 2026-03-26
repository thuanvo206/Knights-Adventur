using UnityEngine;
using TMPro;
using Fusion; // Bắt buộc phải có để dùng NetworkBehaviour và [Networked]

[RequireComponent(typeof(Rigidbody2D))]
public class Player : NetworkBehaviour 
{
    internal Rigidbody2D body2D;
    public float knockBackForce = 15000;

    [Header("Movement Settings")]
    [Range(0, 20)] public float playerSpeed = 15;
    [Range(500, 1500)] public float jumpPower = 1000;
    [Range(500, 1000)] public float doubleJumpPower = 600;

    // --- CÁC BIẾN ĐỒNG BỘ MẠNG (NETWORKED) ---
    [Networked] public int maxPlayerHealth { get; set; } = 100;
    [Networked] public int currentPlayerHealth { get; set; }
    [Networked] public int currentCoin { get; set; }
    [Networked] public NetworkBool isDead { get; set; }
    [Networked] public NetworkBool isGround { get; set; }
    [Networked] public NetworkBool canDoubleJump { get; set; }

    // --- CÁC BIẾN PUBLIC ĐỂ CÁC SCRIPT KHÁC TRUY CẬP (SỬA LỖI CS1061/CS0103) ---
    public bool isHurt;
    public bool addHealth;
    public bool earnCoin;
    public bool canDamage = true; // Cho phép gây sát thương cho quái

    // --- CÁC BIẾN NỘI BỘ ---
    bool facingRight = true;
    Transform groundCheck;
    const float GroundCheckRadius = .1f;
    public LayerMask groundLayer;
    Animator playerAnimController;
    AudioSource audioSource;
    AudioClip audioJump;

    // Tham chiếu đến các script item (nếu bạn cần lấy chỉ số từ chúng)
    GiveHealth giveHealth;
    AddCoin addCoinScript;

    public override void Spawned() 
    {
        body2D = GetComponent<Rigidbody2D>();
        groundCheck = transform.Find("GroundCheck");
        playerAnimController = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        audioJump = Resources.Load("Sounds/Jump") as AudioClip;

        // Tìm các script hỗ trợ nếu cần
        giveHealth = FindObjectOfType<GiveHealth>();
        addCoinScript = FindObjectOfType<AddCoin>();

        if (HasStateAuthority)
        {
            currentPlayerHealth = maxPlayerHealth;
            isDead = false;
            currentCoin = 0;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object == null || !Object.IsValid) return;
        if ((bool)isDead) return;

        // Kiểm tra mặt đất và ép kiểu về bool
        isGround = Physics2D.OverlapCircle(groundCheck.position, GroundCheckRadius, groundLayer) != null;

        if (GetInput(out NetworkInputData data))
        {
            body2D.linearVelocity = new Vector2(data.move.x * playerSpeed, body2D.linearVelocity.y);
            if (data.move.x != 0) Flip(data.move.x);

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

        // Xử lý các sự kiện từ va chạm (Hurt, Health, Coin)
        HandleStatusEffects();
        CheckDeathLimits();
        UpdateAnimations();
    }

    void HandleStatusEffects()
    {
        if (!HasStateAuthority) return;

        if (isHurt)
        {
            currentPlayerHealth -= 10; // Giả định mất 10 máu khi chạm quái
            isHurt = false;
            // Thêm lực đẩy lùi (Knockback) nếu cần ở đây
        }

        if (addHealth)
        {
            currentPlayerHealth = Mathf.Min(currentPlayerHealth + 20, maxPlayerHealth);
            addHealth = false;
        }

        if (earnCoin)
        {
            currentCoin += 1;
            earnCoin = false;
        }
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
        {
            isDead = true;
        }
    }
}