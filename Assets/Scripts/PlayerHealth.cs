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
        int finalMaxHealth = GameManager.Instance != null
            ? GameManager.Instance.GetMaxHealthModifier(playerMaxHealthBase)
            : playerMaxHealthBase;

        health.SetMaxHealth(finalMaxHealth, true);
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
        if (GameManager.Instance != null &&
            GameManager.Instance.TryUseSecondWind())
        {
            health.ReviveToPercent(0.5f);
            Debug.Log("Second Wind revived player.");
            return;
        }

        Debug.Log("Player died.");

        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenuAfterDeath();
    }
}