using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] private FloatingText floatingTextPrefab;

    public void SpawnDamageNumber(int damage, Vector2 position)
    {
        if (floatingTextPrefab == null) return;

        FloatingText text = Instantiate(
            floatingTextPrefab,
            position,
            Quaternion.identity
        );

        Vector3 direction = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.8f, 1.2f),
            0f
        );

        text.Initialize(damage.ToString(), Color.red, direction);
    }
}