using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyHealth : MonoBehaviour
{
    private Health health;
    [SerializeField] private bool dropCoinsOnHit = false;
    [SerializeField] private bool dropCoinsOnDeath = true;

    private CoinSpawner coinSpawner;
    private int lastHitDirection = 1;

    private void Awake()
    {
        health = GetComponent<Health>();
        coinSpawner = GetComponent<CoinSpawner>();
    }

    private void OnEnable()
    {
        health.OnHealthChanged += UpdateEnemyHealthUI;
        health.OnDied += Die;
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= UpdateEnemyHealthUI;
        health.OnDied -= Die;
    }

    private void Start()
    {
        UpdateEnemyHealthUI(health.CurrentHealth, health.MaxHealth);

        if (UIHandler.instance != null)
        {
            UIHandler.instance.SetEnemiesNameLabelText(name);
        }
    }

    public void TakeHit(
        AttackData attack,
        Vector2 hitPosition,
        int comboHitCount,
        int hitDirection
    )
    {
        health.TakeDamage(attack.damage);

        if (dropCoinsOnHit && coinSpawner != null)
        {
            coinSpawner.SpawnCoins(hitDirection);
        }
        lastHitDirection = hitDirection;
        Debug.Log(
            $"Enemy hit\n" +
            $"Enemy: {name}\n" +
            $"Attack: {attack.name}\n" +
            $"Damage: {attack.damage}\n" +
            $"Hit Position: {hitPosition}\n" +
            $"Combo Hits: {comboHitCount}"
        );
    }


    private void UpdateEnemyHealthUI(int current, int max)
    {
        float percentage = (float)current / max;

        if (UIHandler.instance != null)
        {
            UIHandler.instance.SetEnemyHealthValue(percentage);
        }
    }

    private void Die()
    {
        if (dropCoinsOnDeath && coinSpawner != null)
        {
            coinSpawner.SpawnCoins(lastHitDirection);
        }
        Debug.Log($"{name} died.");
        gameObject.SetActive(false);
    }
}