using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerOneUseItem : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Vector2 playAreaMin = new Vector2(-8, -4);
    [SerializeField] private Vector2 playAreaMax = new Vector2(8, 4);

    private Health health;
    private PlayerCombat combat;

    private void Awake()
    {
        health = GetComponent<Health>();
        combat = GetComponent<PlayerCombat>();
    }

    public void OnUseItem(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        UseHeldItem();
    }

    private void UseHeldItem()
    {
        if (GameManager.Instance == null) return;

        OneUseItemType item = GameManager.Instance.ConsumeHeldItem();

        switch (item)
        {
            case OneUseItemType.Nuke:
                UseNuke();
                break;

            case OneUseItemType.HealthPotion:
                health.Heal(Mathf.CeilToInt(health.MaxHealth * 0.5f));
                break;

            case OneUseItemType.InvincibilityInjection:
                StartCoroutine(InvincibilityRoutine());
                break;

            case OneUseItemType.BrokenTeleport:
                UseBrokenTeleport();
                break;

            case OneUseItemType.AdrenalineInjection:
                Debug.Log("Super meter not implemented yet.");
                break;

            default:
                Debug.Log("No item held.");
                break;
        }
    }

    private void UseNuke()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 30f, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Health enemyHealth = hit.GetComponentInParent<Health>();
            if (enemyHealth != null)
                enemyHealth.TakeDamage(999999);
        }
    }

    private System.Collections.IEnumerator InvincibilityRoutine()
    {
        PlayerInvincibility inv = GetComponent<PlayerInvincibility>();
        if (inv != null)
            inv.SetInvincible(true);

        yield return new WaitForSeconds(10f);

        if (inv != null)
            inv.SetInvincible(false);
    }

    private void UseBrokenTeleport()
    {
        transform.position = new Vector3(
            Random.Range(playAreaMin.x, playAreaMax.x),
            Random.Range(playAreaMin.y, playAreaMax.y),
            transform.position.z
        );
    }
}