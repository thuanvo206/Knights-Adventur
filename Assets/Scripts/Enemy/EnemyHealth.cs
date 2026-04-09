using UnityEngine;
using Fusion;

public class EnemyHealth : NetworkBehaviour // 1. Đổi thành NetworkBehaviour
{
    public int maxEnemyHealth = 100;
    
    // 2. Đồng bộ máu quái vật qua mạng
    [Networked] public float currentEnemyHealth { get; set; } 
    
    public float playerDamageToEnemy;
    public GameObject deathParticle;
    // ... (giữ nguyên khai báo các component khác)

    public override void Spawned() // Thay Start() bằng Spawned()
    {
        // Chỉ Host mới được set máu ban đầu
        if (HasStateAuthority) currentEnemyHealth = maxEnemyHealth; 
        
        // ... (giữ nguyên lệnh get component)
    }

    public override void FixedUpdateNetwork() // Thay Update() bằng FixedUpdateNetwork()
    {
        if (currentEnemyHealth <= 0 && HasStateAuthority)
        {
            // Bật Particle (nên dùng RPC nếu muốn đẹp đồng bộ, nhưng tạm thời bật local cũng được)
            if(deathParticle) deathParticle.SetActive(true);
            
            // Host ra lệnh tiêu diệt quái trên toàn mạng
            Runner.Despawn(Object); 
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerItem"))
        {
            Player player = other.GetComponentInParent<Player>();
            
            // Chỉ Host mới có quyền trừ máu quái
            if (player != null && player.canDamage && HasStateAuthority)
            {
                currentEnemyHealth -= playerDamageToEnemy;
            }
        }
    }
}