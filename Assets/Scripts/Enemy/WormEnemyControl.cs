using UnityEngine;

public class WormEnemyControl : EnemyControlBase
{
    // Override scale gốc của Worm — giữ đúng như code cũ
    protected override Vector2 ScaleRight => new Vector2(-0.63693f, 0.63693f);
    protected override Vector2 ScaleLeft  => new Vector2(0.63693f, 0.63693f);
}