using UnityEngine;

public class GiveHealth : MonoBehaviour
{
    public int health;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            Player p = collider.GetComponent<Player>();
            if (p != null)
            {
                p.addHealth = true; 
                Destroy(gameObject);
            }
        }
    }
}