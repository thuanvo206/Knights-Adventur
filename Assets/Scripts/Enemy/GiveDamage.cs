using UnityEngine;
using Fusion;

public class GiveDamage : MonoBehaviour
{
    public int damage = 10;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            Player p = collider.GetComponent<Player>();
            if (p != null && p.HasStateAuthority)
            {
                // Gọi hàm TakeDamage để nó tự kiểm tra thời gian bất tử
                p.TakeDamage(damage);
            } 
        }
    }
}