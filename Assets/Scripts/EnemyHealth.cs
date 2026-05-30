using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyHealth : MonoBehaviour
{
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
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

    public void TakeHit(AttackData attack, Vector2 hitPosition, int comboHitCount)
    {
        health.TakeDamage(attack.damage);

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
        Debug.Log($"{name} died.");
        gameObject.SetActive(false);
    }
}