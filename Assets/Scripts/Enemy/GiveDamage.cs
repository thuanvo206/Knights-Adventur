using UnityEngine;

public class GiveDamage : MonoBehaviour
{
    public int damage;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            Player p = collider.GetComponent<Player>();
            if (p != null)
            {
                p.isHurt = true;
            } 
        }
    }
}