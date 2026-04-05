using UnityEngine;
using TMPro;
using Fusion;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : NetworkBehaviour
{
    internal Rigidbody2D body2D;
    public float knockBackForce = 15000;

    [Header("Movement Settings")]
    // Đã xóa các thẻ [Range] để bạn có thể tùy chỉnh số tự do trong Inspector
    public float playerSpeed = 25f;
    public float jumpPower = 15f;
    public float doubleJumpPower = 12f;

    [Networked] public int maxPlayerHealth { get; set; } = 100;
    [Networked] public int currentPlayerHealth { get; set; }
    [Networked] public int currentCoin { get; set; }
    [Networked] public NetworkBool isDead { get; set; }
    [Networked] public NetworkBool isGround { get; set; }
    [Networked] public NetworkBool canDoubleJump { get; set; }

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
            // DI CHUYỂN: Sử dụng vận tốc 
            body2D.linearVelocity = new Vector2(data.move.x * playerSpeed, body2D.linearVelocity.y);

            // QUAY MẶT
            if (data.move.x != 0) Flip(data.move.x);

            // NHẢY: Đã sửa thành gán vận tốc thay vì AddForce
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
            // Chú ý: Đã xóa HandleStatusEffects() vì máu và xu được xử lý trực tiếp ở các file AddCoin, GiveHealth... (như hướng dẫn trước)
            CheckDeathLimits();
        }
    }

    public override void Render()
    {
        UpdateAnimations();

        // HIỂN THỊ ĐÚNG MÁU VÀ XU CHO TỪNG NGƯỜI CHƠI
        if (HasInputAuthority)
        {
            var gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                // Cập nhật thanh máu
                if (gm.healthBar != null)
                {
                    gm.healthBar.maxValue = maxPlayerHealth;
                    gm.healthBar.value = currentPlayerHealth;
                }

                // CẬP NHẬT CHỮ SỐ XU LÊN UI
                if (gm.coinText != null)
                {
                    gm.coinText.text = currentCoin.ToString();
                }
            }
        }
    }

    public void Jump()
    {
        // Gán thẳng vận tốc trục Y
        body2D.linearVelocity = new Vector2(body2D.linearVelocity.x, jumpPower);

        if (audioSource != null && audioJump != null)
            audioSource.PlayOneShot(audioJump);
    }

    public void DoubleJump()
    {
        // Gán thẳng vận tốc trục Y
        body2D.linearVelocity = new Vector2(body2D.linearVelocity.x, doubleJumpPower);

        if (audioSource != null && audioJump != null)
            audioSource.PlayOneShot(audioJump);
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

    // ... (các code ở trên giữ nguyên)

    // THÊM 2 BIẾN NÀY ĐỂ TÍNH THỜI GIAN BẤT TỬ
    public float invincibilityDuration = 1f; // Bất tử 1 giây sau khi bị thương
    private float lastDamageTime;

    // THÊM HÀM NÀY ĐỂ XỬ LÝ TRỪ MÁU
    public void TakeDamage(int damage)
    {
        // Kiểm tra xem đã hết thời gian bất tử chưa mới cho trừ máu tiếp
        if (HasStateAuthority && Time.time >= lastDamageTime + invincibilityDuration)
        {
            currentPlayerHealth -= damage;
            lastDamageTime = Time.time; // Reset lại đồng hồ tính giờ
        }
    }
} // Chữ ngoặc nhọn kết thúc class Player
