using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxEnemyHealth = 100;
    public float currentEnemyHealth;
    internal bool gotDamage;
    public float playerDamageToEnemy;
    public GameObject deathParticle;
    SpriteRenderer spriteRenderer;
    CircleCollider2D cir2D;
    Rigidbody2D body2D;

    void Start()
    {
        currentEnemyHealth = maxEnemyHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        cir2D = GetComponent<CircleCollider2D>();
        body2D = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (currentEnemyHealth <= 0)
        {
            if(spriteRenderer) spriteRenderer.enabled = false;
            if(cir2D) cir2D.enabled = false;
            if(body2D) body2D.constraints = RigidbodyConstraints2D.FreezePositionX;
            if(deathParticle) deathParticle.SetActive(true);
            Destroy(gameObject, 1);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerItem"))
        {
            // Tìm player trong cảnh ngay lúc va chạm để đảm bảo không bị Null
            Player player = FindObjectOfType<Player>();
            if (player != null && player.canDamage)
            {
                currentEnemyHealth -= playerDamageToEnemy;
            }
        }
    }
}