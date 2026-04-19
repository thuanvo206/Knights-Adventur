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

    public override void Spawned()
    {
        if (HasStateAuthority) currentEnemyHealth = maxEnemyHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        cir2D = GetComponent<CircleCollider2D>();
        body2D = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (currentEnemyHealth <= 0 && HasStateAuthority)
        {
            if (spriteRenderer) spriteRenderer.enabled = false;
            if (cir2D) cir2D.enabled = false;
            if (body2D) body2D.constraints = RigidbodyConstraints2D.FreezePositionX;
            if (deathParticle) deathParticle.SetActive(true);
            Runner.Despawn(Object);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // BUG FIX: Phải check HasStateAuthority, không thì mỗi client đều trừ máu
        // → quái bị trừ máu nhân số lượng client trong phòng
        if (!HasStateAuthority) return;

        if (other.CompareTag("PlayerItem"))
        {
            Player player = other.GetComponentInParent<Player>();
            if (player != null && player.canDamage)
            {
                currentEnemyHealth -= playerDamageToEnemy;
            }
        }
    }
}