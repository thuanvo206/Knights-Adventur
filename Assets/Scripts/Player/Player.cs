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
    public float stompBouncePower = 12f;

    [Networked] public int maxPlayerHealth { get; set; }
    [Networked] public int currentPlayerHealth { get; set; }
    [Networked] public int currentCoin { get; set; }
    [Networked] public NetworkBool isDead { get; set; }
    [Networked] public NetworkBool isGround { get; set; }
    [Networked] public NetworkBool canDoubleJump { get; set; }
    [Networked] public NetworkBool prevJumpPressed { get; set; }

    [Header("Combat Settings")]
    public bool canDamage = true;
    public float invincibilityDuration = 1f;

    // BUG FIX: Dùng [Networked] tick để đồng bộ thời gian bất tử qua mạng
    // Nếu dùng float lastDamageTime local + Time.time, mỗi client có giá trị riêng
    // → client có thể bị damage nhiều lần trong khi Host đang trong trạng thái bất tử
    [Networked] private float lastDamageTime { get; set; }

    public int stompDamage = 50;

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
            // BUG FIX: Networked property không có default value trong Fusion
            // phải set trong Spawned()
            maxPlayerHealth = 100;
            currentPlayerHealth = maxPlayerHealth;
            isDead = false;
            currentCoin = 0;
            lastDamageTime = -invincibilityDuration; // cho phép damage ngay từ đầu
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object == null || !Object.IsValid || (bool)isDead) return;

        isGround = Physics2D.OverlapCircle(groundCheck.position, GroundCheckRadius, groundLayer) != null;

        if (GetInput(out NetworkInputData data))
        {
            body2D.linearVelocity = new Vector2(data.move.x * playerSpeed, body2D.linearVelocity.y);

            if (data.move.x != 0) Flip(data.move.x);

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!HasStateAuthority) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    EnemyHealth enemy = collision.gameObject.GetComponent<EnemyHealth>();
                    if (enemy != null)
                    {
                        enemy.currentEnemyHealth -= stompDamage;
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
        if (transform.position.y <= -6 || currentPlayerHealth <= 0)
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
        // BUG FIX: Dùng Runner.SimulationTime thay vì Time.time
        // Time.time chạy theo real-time của từng máy, không đồng bộ với network tick
        // Runner.SimulationTime đồng bộ chính xác với Fusion simulation
        if (HasStateAuthority && Runner.SimulationTime >= lastDamageTime + invincibilityDuration)
        {
            currentPlayerHealth -= damage;
            lastDamageTime = Runner.SimulationTime;
        }
    }
}