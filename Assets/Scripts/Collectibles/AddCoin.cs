using UnityEngine;
using Fusion;

public class AddCoin : MonoBehaviour
{
    public int coin = 1;
    bool isCollected = false; // Thêm biến khóa

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (isCollected) return; // Nếu đã bị ăn rồi thì thoát luôn

        if (collider.CompareTag("Player"))
        {
            Player p = collider.GetComponent<Player>();
            if (p != null && p.HasStateAuthority)
            {
                isCollected = true; // Khóa lại ngay lập tức
                p.currentCoin += coin;
                Destroy(gameObject);
            }
        }
    }
}