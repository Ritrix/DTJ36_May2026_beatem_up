using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerHealth : MonoBehaviour
{
    private Health health;

    [SerializeField] private int playerMaxHealthBase = 100;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string hurtAnimationState = "Player_Hurt";
    [SerializeField] private float hurtFrameTime = 0.15f;

    private int previousHealth;
    private Coroutine hurtRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();

        if (animator == null)
            animator = GetComponent<Animator>();
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

        previousHealth = health.CurrentHealth;

        UpdateHealthUI(health.CurrentHealth, health.MaxHealth);
    }

    private void UpdateHealthUI(int current, int max)
    {
        if (current < previousHealth)
        {
            PlayHurtAnimation();
        }

        previousHealth = current;

        float percentage = (float)current / max;

        if (UIHandler.instance != null)
        {
            UIHandler.instance.SetHealthValue(percentage);
        }
    }

    private void PlayHurtAnimation()
    {
        if (hurtRoutine != null)
            StopCoroutine(hurtRoutine);

        hurtRoutine = StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        PlayAnimationState(hurtAnimationState);

        yield return new WaitForSeconds(hurtFrameTime);

        PlayerMovement movement = GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.RefreshMovementAnimation();
        }
    }

    private void PlayAnimationState(string stateName)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(stateName)) return;

        animator.Play(stateName, 0, 0f);
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