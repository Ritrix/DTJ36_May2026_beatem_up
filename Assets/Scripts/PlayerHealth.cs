using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    private Health health;

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
    }
}