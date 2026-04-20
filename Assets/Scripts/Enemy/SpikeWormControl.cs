using UnityEngine;

public class SpikeWormControl : EnemyControlBase
{
    // Override scale gốc của SpikeWorm — giữ đúng như code cũ
    protected override Vector2 ScaleRight => new Vector2(-0.32929f, 0.32929f);
    protected override Vector2 ScaleLeft  => new Vector2(0.32929f, 0.32929f);
}