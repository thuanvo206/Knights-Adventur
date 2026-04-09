using UnityEngine;
using TMPro;
using Fusion;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : NetworkBehaviour
{
    internal Rigidbody2D body2D;
    public float knockBackForce = 15000;

    [Header("Movement Settings")]
    public float playerSpeed = 10f; // Đã chỉnh nhỏ lại cho hợp với Fusion
    public float jumpPower = 15f;
    public float doubleJumpPower = 12f;

    [Networked] public int maxPlayerHealth { get; set; } = 100;
    [Networked] public int currentPlayerHealth { get; set; }
    [Networked] public int currentCoin { get; set; }
    [Networked] public NetworkBool isDead { get; set; }
    [Networked] public NetworkBool isGround { get; set; }
    [Networked] public NetworkBool canDoubleJump { get; set; }
    
    // Biến để sửa triệt để lỗi nhảy kép
    [Networked] public NetworkBool prevJumpPressed { get; set; }

    [Header("Combat Settings")]
    public bool canDamage = true;
    public float invincibilityDuration = 1f; // Bất tử 1 giây sau khi bị thương
    private float lastDamageTime;

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

        // Chỉ Host (StateAuthority) mới được set các giá trị khởi tạo
        if (HasStateAuthority)
        {
            currentPlayerHealth = maxPlayerHealth;
            isDead = false;
            currentCoin = 0;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Dừng xử lý nếu object lỗi hoặc nhân vật đã chết
        if (Object == null || !Object.IsValid || (bool)isDead) return;

        // Cập nhật chạm đất liên tục
        isGround = Physics2D.OverlapCircle(groundCheck.position, GroundCheckRadius, groundLayer) != null;

        // Lấy Input từ mạng
        if (GetInput(out NetworkInputData data))
        {
            // --- 1. DI CHUYỂN ---
            body2D.linearVelocity = new Vector2(data.move.x * playerSpeed, body2D.linearVelocity.y);

            // --- 2. QUAY MẶT ---
            if (data.move.x != 0) Flip(data.move.x);

            // --- 3. NHẢY ---
            // Chỉ nhảy khi lúc này bấm, nhưng frame trước chưa bấm (Just Pressed)
            bool jumpJustPressed = data.jumpPressed && !prevJumpPressed;

            if (jumpJustPressed)
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
            
            // Lưu lại trạng thái nút bấm cho frame tiếp theo
            prevJumpPressed = data.jumpPressed; 
        }
        else
        {
            // Tránh lỗi trượt vô tận khi mất kết nối/thả nút
            body2D.linearVelocity = new Vector2(0, body2D.linearVelocity.y);
        }

        // --- 4. KIỂM TRA CHẾT (CHỈ HOST QUYẾT ĐỊNH) ---
        if (HasStateAuthority)
        {
            CheckDeathLimits();
        }
    }

    public override void Render()
    {
        UpdateAnimations();

        // HIỂN THỊ UI DÀNH RIÊNG CHO MÁY CỦA NGƯỜI CHƠI NÀY (Local Player)
        if (HasInputAuthority)
        {
            var gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                if (gm.healthBar != null)
                {
                    gm.healthBar.maxValue = maxPlayerHealth;
                    gm.healthBar.value = currentPlayerHealth;
                }

                if (gm.coinText != null)
                {
                    gm.coinText.text = currentCoin.ToString();
                }
            }
        }
    }

    public void Jump()
    {
        body2D.linearVelocity = new Vector2(body2D.linearVelocity.x, jumpPower);
        if (audioSource != null && audioJump != null) audioSource.PlayOneShot(audioJump);
    }

    public void DoubleJump()
    {
        body2D.linearVelocity = new Vector2(body2D.linearVelocity.x, doubleJumpPower);
        if (audioSource != null && audioJump != null) audioSource.PlayOneShot(audioJump);
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
            if (!(bool)isDead)
            {
                isDead = true;
                Debug.Log("Player chết! Đang Load lại mạng...");
                
                // Load lại cảnh qua đường truyền mạng của Fusion
                int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                Runner.LoadScene(SceneRef.FromIndex(currentSceneIndex));
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // Kiểm tra thời gian bất tử (chỉ Host mới được tính)
        if (HasStateAuthority && Time.time >= lastDamageTime + invincibilityDuration)
        {
            currentPlayerHealth -= damage;
            lastDamageTime = Time.time; // Reset đồng hồ bất tử
        }
    }
}