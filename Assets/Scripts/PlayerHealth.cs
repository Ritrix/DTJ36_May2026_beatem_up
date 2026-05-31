using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    private Health health;

    [SerializeField] private int playerMaxHealthBase = 100;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        health.OnHealthChanged += UpdateHealthUI;
        health.OnDied += Die;
    }

    private void OnDisable()
    {
        health.OnHealthChanged -= UpdateHealthUI;
        health.OnDied -= Die;
    }

    private void Start()
    {
        int finalMaxHealth = playerMaxHealthBase + GameManager.Instance.BonusMaxHealth;
        health.SetMaxHealth(finalMaxHealth);
        UpdateHealthUI(health.CurrentHealth, health.MaxHealth);
    }

    private void UpdateHealthUI(int current, int max)
    {
        float percentage = (float)current / max;

        if (UIHandler.instance != null)
        {
            UIHandler.instance.SetHealthValue(percentage);
        }
    }

    private void Die()
    {
        Debug.Log("Player died.");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenuAfterDeath();
        }
    }
}