using UnityEngine;
using Fusion;

// QUAN TRỌNG: Phải gắn thêm component "NetworkObject" vào GameObject
// chứa script này trong Unity Inspector thì mới hoạt động được
public class AddCoin : NetworkBehaviour
{
    public int coin = 1;

    // Dùng [Networked] để trạng thái đã nhặt được sync cho tất cả client
    [Networked] private NetworkBool isCollected { get; set; }

    void OnTriggerEnter2D(Collider2D collider)
    {
        // Chỉ Host mới xử lý logic
        if (!HasStateAuthority) return;
        if (isCollected) return;

        if (collider.CompareTag("Player"))
        {
            Player p = collider.GetComponent<Player>();
            if (p != null)
            {
                isCollected = true;
                p.currentCoin += coin;
                // Despawn đồng bộ cho tất cả client
                Runner.Despawn(Object);
            }
        }
    }
}