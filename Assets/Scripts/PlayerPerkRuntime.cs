using UnityEngine;

public class PlayerPerkRuntime : MonoBehaviour
{
    [SerializeField] private Health health;

    private float regenTimer;
    private float bulwarkTimer;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        HandleRegen();
        HandleBulwarkRecharge();
    }

    private void HandleRegen()
    {
        if (!GameManager.Instance.HasPerk(PerkType.Regen) &&
            !GameManager.Instance.HasPerk(PerkType.Wildcard))
            return;

        regenTimer += Time.deltaTime;

        if (regenTimer < 1f) return;

        regenTimer = 0f;

        int regenAmount = GameManager.Instance.HasPerk(PerkType.Wildcard) ? 2 : 1;
        health.Heal(regenAmount);
    }

    private void HandleBulwarkRecharge()
    {
        if (!GameManager.Instance.HasPerk(PerkType.Bulwark)) return;

        bulwarkTimer += Time.deltaTime;

        if (bulwarkTimer >= 30f)
        {
            bulwarkTimer = 0f;
            GameManager.Instance.RechargeBulwark();
        }
    }
}