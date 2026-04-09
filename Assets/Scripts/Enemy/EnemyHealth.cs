using UnityEngine;
using Fusion;

public class EnemyHealth : NetworkBehaviour // 1. Đổi thành NetworkBehaviour
{
    public int maxEnemyHealth = 100;
    
    // 2. Đồng bộ máu quái vật qua mạng
    [Networked] public float currentEnemyHealth { get; set; } 
    internal bool gotDamage;
    
    public float playerDamageToEnemy;
    public GameObject deathParticle;
    // ... (giữ nguyên khai báo các component khác)
    SpriteRenderer spriteRenderer;
    CircleCollider2D cir2D;
    Rigidbody2D body2D;

    public override void Spawned() // Thay Start() bằng Spawned()
    {
        // Chỉ Host mới được set máu ban đầu
        if (HasStateAuthority) currentEnemyHealth = maxEnemyHealth; 
        
        // ... (giữ nguyên lệnh get component)
        spriteRenderer = GetComponent<SpriteRenderer>();
        cir2D = GetComponent<CircleCollider2D>();
        body2D = GetComponent<Rigidbody2D>();
    }

    public override void FixedUpdateNetwork() // Thay Update() bằng FixedUpdateNetwork()
    {
        if (currentEnemyHealth <= 0 && HasStateAuthority)
        {
            if(spriteRenderer) spriteRenderer.enabled = false;
            if(cir2D) cir2D.enabled = false;
            if(body2D) body2D.constraints = RigidbodyConstraints2D.FreezePositionX;
            if(deathParticle) deathParticle.SetActive(true);
            Runner.Despawn(Object);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
    if (other.CompareTag("PlayerItem"))
    {
        Player player = other.GetComponentInParent<Player>();
        
        // Thay vì trừ trực tiếp, hãy gọi TakeDamage để tận dụng logic HasStateAuthority
        if (player != null && player.canDamage) 
        {
           currentEnemyHealth -= playerDamageToEnemy; // Trừ máu quái vật
        }
    }
    }
}