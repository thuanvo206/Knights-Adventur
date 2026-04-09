using UnityEngine;
using TMPro;
using Fusion;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : NetworkBehaviour
{
    internal Rigidbody2D body2D;
    public float knockBackForce = 15000;

    [Header("Movement Settings")]
    public float playerSpeed = 10f;
    public float jumpPower = 15f;
    public float doubleJumpPower = 12f;
    public float stompBouncePower = 12f; // Lực nảy lên khi giẫm trúng quái

    [Networked] public int maxPlayerHealth { get; set; } = 100;
    [Networked] public int currentPlayerHealth { get; set; }
    [Networked] public int currentCoin { get; set; }
    [Networked] public NetworkBool isDead { get; set; }
    [Networked] public NetworkBool isGround { get; set; }
    [Networked] public NetworkBool canDoubleJump { get; set; }
    
    [Networked] public NetworkBool prevJumpPressed { get; set; }

    [Header("Combat Settings")]
    public bool canDamage = true;
    public float invincibilityDuration = 1f;
    private float lastDamageTime;
    public int stompDamage = 50; // Sát thương khi nhảy lên đầu

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

        isGround = Physics2D.OverlapCircle(groundCheck.position, GroundCheckRadius, groundLayer) != null;

        if (GetInput(out NetworkInputData data))
        {
            // --- 1. DI CHUYỂN ---
            body2D.linearVelocity = new Vector2(data.move.x * playerSpeed, body2D.linearVelocity.y);

            // --- 2. QUAY MẶT ---
            if (data.move.x != 0) Flip(data.move.x);

            // --- 3. NHẢY ---
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
            
            prevJumpPressed = data.jumpPressed; 
        }
        else
        {
            body2D.linearVelocity = new Vector2(0, body2D.linearVelocity.y);
        }

        if (HasStateAuthority)
        {
            CheckDeathLimits();
        }
    }

    // --- XỬ LÝ GIẪM LÊN ĐẦU QUÁI ---
    private void OnCollisionEnter2D(Collision2D collision)
{
    // Chỉ thực hiện xử lý trên Host (StateAuthority) để đảm bảo đồng bộ
    if (!HasStateAuthority) return;

    if (collision.gameObject.CompareTag("Enemy"))
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Kiểm tra nếu điểm va chạm nằm ở phía dưới chân Player (normal.y > 0.5)
            if (contact.normal.y > 0.5f)
            {
                EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    // Gây sát thương
                    enemy.currentEnemyHealth -= stompDamage;
                    
                    // Đẩy Player nảy lên
                    body2D.linearVelocity = new Vector2(body2D.linearVelocity.x, stompBouncePower);
                    canDoubleJump = true;
                    break;
                }
            }
        }
    }
}

    public override void Render()
    {
        UpdateAnimations();

        if (HasInputAuthority)
        {
            // Tìm GameManager để cập nhật UI (Giữ nguyên logic cũ của bạn)
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
                int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
                Runner.LoadScene(SceneRef.FromIndex(currentSceneIndex));
            }
        }
    }
    public void TakeDamage(int damage)
    {
        // Kiểm tra xem đã hết thời gian bất tử chưa mới cho trừ máu tiếp
        if (HasStateAuthority && Time.time >= lastDamageTime + invincibilityDuration)
        {
            currentPlayerHealth -= damage;
            lastDamageTime = Time.time; // Reset lại đồng hồ tính giờ
        }
    }
}