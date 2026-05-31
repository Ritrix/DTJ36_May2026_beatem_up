using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    public static void Explode(Vector2 position, int damage, float radius, LayerMask enemyLayer)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy == null) continue;

            Health rawHealth = enemy.GetComponent<Health>();
            if (rawHealth == null) continue;

            rawHealth.TakeDamage(damage);
        }

        Debug.Log($"Explosion dealt {damage} damage in radius {radius}");
    }
}