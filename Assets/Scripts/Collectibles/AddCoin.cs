using UnityEngine;

public class AddCoin : MonoBehaviour
{
    public int coin;
    // Không cần biến Player ở đây nữa để tránh lỗi Start() chạy sớm

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            // Lấy trực tiếp script từ đối tượng va chạm
            Player p = collider.GetComponent<Player>();
            if (p != null)
            {
                p.earnCoin = true;
                Destroy(gameObject);
            }
        }
    }
}