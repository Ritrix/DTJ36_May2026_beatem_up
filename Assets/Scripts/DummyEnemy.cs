using UnityEngine;
using System.Collections;

public class DummyEnemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float stunDuration = 0.4f;

    [Header("Debug Flash")]
    [SerializeField] private float flashDuration = 0.08f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private int currentHealth;
    private bool isStunned;
    private float stunTimer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (!isStunned) return;

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            isStunned = false;
            Debug.Log($"{name} is no longer stunned.");
        }
    }

    public void TakeHit(AttackData attack, Vector2 hitPosition, int comboHitCount)
    {
        currentHealth -= attack.damage;

        StartCoroutine(Flash());

        isStunned = true;
        stunTimer = stunDuration;

        Debug.Log(
            $"HIT SUCCESSFUL\n" +
            $"Target: {name}\n" +
            $"Attack: {attack.name}\n" +
            $"Damage: {attack.damage}\n" +
            $"Health: {currentHealth}/{maxHealth}\n" +
            $"Hit Position: {hitPosition}\n" +
            $"Combo Hit Count: {comboHitCount}\n" +
            $"Stunned: {isStunned}"
        );

        if (currentHealth <= 0)
        {
            Debug.Log($"{name} would be defeated. Resetting health for testing.");
            currentHealth = maxHealth;
        }
    }

    private IEnumerator Flash()
    {
        spriteRenderer.color = Color.red;

        yield return new WaitForSeconds(flashDuration);

        spriteRenderer.color = originalColor;
    }


}
