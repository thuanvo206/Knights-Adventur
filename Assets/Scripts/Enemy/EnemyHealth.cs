using UnityEngine;
using Fusion;

public class EnemyHealth : NetworkBehaviour
{
    public int maxEnemyHealth = 100;

    [Networked] public float currentEnemyHealth { get; set; }
    internal bool gotDamage;

    public float playerDamageToEnemy;
    public GameObject deathParticle;

    SpriteRenderer spriteRenderer;
    CircleCollider2D cir2D;
    Rigidbody2D body2D;

    // Flag tránh gọi death logic nhiều lần
    private bool isDying = false;

    public override void Spawned()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cir2D = GetComponent<CircleCollider2D>();
        body2D = GetComponent<Rigidbody2D>();

        // Chỉ Host set máu ban đầu
        // Client nhận giá trị qua [Networked] — không set ở đây tránh = 0 trước khi sync
        if (HasStateAuthority)
            currentEnemyHealth = maxEnemyHealth;
    }

    public override void FixedUpdateNetwork()
    {
        // BUG FIX: Thêm isDying để tránh freeze/despawn nhiều lần liên tiếp
        // BUG FIX: Chỉ chạy khi currentEnemyHealth đã được set (> 0 ban đầu)
        // tránh client thấy giá trị 0 trước khi Host sync xong rồi freeze luôn
        if (isDying) return;
        if (currentEnemyHealth <= 0 && HasStateAuthority)
        {
            isDying = true;
            if (spriteRenderer) spriteRenderer.enabled = false;
            if (cir2D) cir2D.enabled = false;
            if (body2D) body2D.constraints = RigidbodyConstraints2D.FreezeAll;
            if (deathParticle) deathParticle.SetActive(true);
            Runner.Despawn(Object);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!HasStateAuthority) return;
        if (isDying) return;

        if (other.CompareTag("PlayerItem"))
        {
            Player player = other.GetComponentInParent<Player>();
            if (player != null && player.canDamage)
                currentEnemyHealth -= playerDamageToEnemy;
        }
    }
}