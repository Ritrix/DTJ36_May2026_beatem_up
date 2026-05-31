using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyHealth : MonoBehaviour
{
    private Health health;
    [SerializeField] private bool dropCoinsOnHit = false;
    [SerializeField] private bool dropCoinsOnDeath = true;

    public event System.Action<EnemyHealth> OnEnemyDied;
    private bool hasDied;

    private CoinSpawner coinSpawner;
    private int lastHitDirection = 1;

    private float stunTimer;

    public bool IsStunned { get; private set; }

    private DamageNumberSpawner damageNumberSpawner;

    private void Update()
    {
        if (!IsStunned)
            return;

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            IsStunned = false;

            Debug.Log($"{name} recovered from stun.");
        }
    }

    private void Awake()
    {
        health = GetComponent<Health>();
        coinSpawner = GetComponent<CoinSpawner>();
        damageNumberSpawner = FindAnyObjectByType<DamageNumberSpawner>();
        
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
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Health playerHealth = player != null ? player.GetComponent<Health>() : null;

        int finalDamage = GameManager.Instance != null
            ? GameManager.Instance.ModifyOutgoingDamage(attack.damage, playerHealth)
            : attack.damage;

        health.TakeDamage(finalDamage);

        if (GameManager.Instance != null &&
            GameManager.Instance.HasPerk(PerkType.ExplosiveStrikes))
        {
            int explosionDamage = Mathf.CeilToInt(finalDamage * 0.3f);
            ExplosionDamage.Explode(hitPosition, explosionDamage, 1.5f, LayerMask.GetMask("Enemy"));
        }

        if (damageNumberSpawner != null)
        {
            damageNumberSpawner.SpawnDamageNumber(finalDamage, hitPosition);
        }

        if (dropCoinsOnHit && coinSpawner != null)
        {
            coinSpawner.SpawnCoins(hitDirection);
        }

        bool handledSpecialHitReaction = false;

        JumperBehaviour jumper = GetComponentInParent<JumperBehaviour>();

        if (jumper != null)
        {
            Debug.Log($"Jumper found. CanBeKnockedDown: {jumper.CanBeKnockedDown}");

            if (jumper.CanBeKnockedDown)
            {
                jumper.LaunchAwayFromPlayer();
                handledSpecialHitReaction = true;
            }
        }

        if (!handledSpecialHitReaction)
        {
            float finalStun = GameManager.Instance != null
                ? GameManager.Instance.ModifyStun(attack.stunDuration)
                : attack.stunDuration;

            ApplyStun(finalStun);
        }

        lastHitDirection = hitDirection;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMomentumDamage();
        }

        Debug.Log(
            $"Enemy hit\n" +
            $"Enemy: {name}\n" +
            $"Attack: {attack.name}\n" +
            $"Damage: {attack.damage}\n" +
            $"Hit Position: {hitPosition}\n" +
            $"Combo Hits: {comboHitCount}"
        );
    }

    private void ApplyStun(float duration)
    {
        IsStunned = true;
        stunTimer = Mathf.Max(stunTimer, duration);

        Debug.Log($"{name} stunned for {duration} seconds.");
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
        if (hasDied) return;

        if (dropCoinsOnDeath && coinSpawner != null)
        {
            coinSpawner.SpawnCoins(lastHitDirection);
        }

        hasDied = true;

        Debug.Log($"{name} died.");

        OnEnemyDied?.Invoke(this);

        gameObject.SetActive(false);
    }
}