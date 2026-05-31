using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private string combatSceneName = "SampleScene";
    [SerializeField] private string shopSceneName = "ShopScene";
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    [Header("Wave")]
    public int CurrentWave { get; private set; } = 1;

    [Header("Player Currency")]
    public int Coins { get; private set; }

    [Header("Permanent Player Upgrades")]
    public int BonusMaxHealth { get; private set; }
    public int BonusArmour { get; private set; }
    public int BonusDamage { get; private set; }
    public float BonusMoveSpeed { get; private set; }
    public bool HasCoinMagnetism { get; private set; }

    [Header("Temporary Round Effects")]
    public bool EnemiesStartHalfHealthNextRound { get; private set; }
    public bool ChallengeModeNextRound { get; private set; }
    public bool ChallengeModeActiveThisRound { get; private set; }

    [Header("One Use Item")]
    public OneUseItemType HeldItem { get; private set; } = OneUseItemType.None;

    [Header("Perks")]
    [SerializeField] private int perkSlotCount = 3;
    private readonly PerkData[] equippedPerks = new PerkData[4];

    public int PerkSlotCount => perkSlotCount;

    private readonly Dictionary<string, int> purchaseCounts = new();

    private int momentumBonusDamage;
    private bool secondWindUsedThisRound;
    private bool bulwarkAvailable = true;

    private bool currentHitIsRepeated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------------- SCENES ----------------

    public void OpenShop()
    {
        SceneManager.LoadScene(shopSceneName);
    }

    public void StartNextWave()
    {
        CurrentWave++;
        SceneManager.LoadScene(combatSceneName);
    }

    public void ReturnToMainMenuAfterDeath()
    {
        ResetRunDataOnly();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ResetRun()
    {
        ResetRunDataOnly();
        SceneManager.LoadScene(combatSceneName);
    }

    public void ResetRunDataOnly()
    {
        CurrentWave = 1;
        Coins = 0;

        BonusMaxHealth = 0;
        BonusArmour = 0;
        BonusDamage = 0;
        BonusMoveSpeed = 0f;
        HasCoinMagnetism = false;

        EnemiesStartHalfHealthNextRound = false;
        ChallengeModeNextRound = false;
        ChallengeModeActiveThisRound = false;

        HeldItem = OneUseItemType.None;

        perkSlotCount = 3;

        for (int i = 0; i < equippedPerks.Length; i++)
        {
            equippedPerks[i] = null;
        }

        purchaseCounts.Clear();

        momentumBonusDamage = 0;
        secondWindUsedThisRound = false;
        bulwarkAvailable = true;
        currentHitIsRepeated = false;
    }

    // ---------------- CURRENCY ----------------

    public void AddCoins(int amount)
    {
        Coins += amount;
        Debug.Log($"Coins: {Coins}");
    }

    public bool SpendCoins(int amount)
    {
        if (Coins < amount)
        {
            Debug.Log($"Not enough coins. Needed {amount}, had {Coins}.");
            return false;
        }

        Coins -= amount;
        Debug.Log($"Coins: {Coins}");
        return true;
    }

    public void DoubleWallet()
    {
        Coins *= 2;
        Debug.Log($"Wallet doubled. Coins: {Coins}");
    }

    // ---------------- PURCHASE SCALING ----------------

    public int GetPurchaseCount(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return 0;

        return purchaseCounts.TryGetValue(itemId, out int count)
            ? count
            : 0;
    }

    public void RegisterPurchase(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return;

        if (!purchaseCounts.ContainsKey(itemId))
            purchaseCounts[itemId] = 0;

        purchaseCounts[itemId]++;
    }

    // ---------------- PERMANENT UPGRADES ----------------

    public void IncreaseMaxHealth(int amount)
    {
        BonusMaxHealth += amount;
        Debug.Log($"Bonus Max Health: {BonusMaxHealth}");
    }

    public void IncreaseArmour(int amount)
    {
        BonusArmour += amount;
        Debug.Log($"Bonus Armour: {BonusArmour}");
    }

    public void IncreaseDamage(int amount)
    {
        BonusDamage += amount;
        Debug.Log($"Bonus Damage: {BonusDamage}");
    }

    public void IncreaseMoveSpeed(float amount)
    {
        BonusMoveSpeed += amount;
        Debug.Log($"Bonus Move Speed: {BonusMoveSpeed}");
    }

    public void UnlockCoinMagnetism()
    {
        HasCoinMagnetism = true;
        Debug.Log("Coin magnetism unlocked.");
    }

    // ---------------- ROUND EFFECTS ----------------

    public void SetEnemiesHalfHealthNextRound()
    {
        EnemiesStartHalfHealthNextRound = true;
        Debug.Log("Enemies will start next round at half health.");
    }

    public void ConsumeEnemiesHalfHealthEffect()
    {
        EnemiesStartHalfHealthNextRound = false;
    }

    public void SetChallengeModeNextRound()
    {
        ChallengeModeNextRound = true;
        Debug.Log("Challenge mode enabled for next round.");
    }

    public void BeginRoundEffects()
    {
        ChallengeModeActiveThisRound = ChallengeModeNextRound;
        ChallengeModeNextRound = false;

        ResetRoundPerkUses();

        Debug.Log($"Round effects started. Challenge active: {ChallengeModeActiveThisRound}");
    }

    public void CompleteRoundEffects()
    {
        if (ChallengeModeActiveThisRound)
        {
            DoubleWallet();
            Debug.Log("Challenge mode survived. Wallet doubled.");
        }

        ChallengeModeActiveThisRound = false;
    }

    public void ResetRoundPerkUses()
    {
        secondWindUsedThisRound = false;
        bulwarkAvailable = true;
        momentumBonusDamage = 0;
        currentHitIsRepeated = false;
    }

    // ---------------- ONE USE ITEMS ----------------

    public bool TrySetHeldItem(OneUseItemType item)
    {
        if (item == OneUseItemType.None)
            return false;

        if (HeldItem != OneUseItemType.None)
        {
            Debug.Log($"Replaced held item: {HeldItem} with {item}");
        }
        else
        {
            Debug.Log($"Held item: {item}");
        }

        HeldItem = item;
        return true;
    }

    public OneUseItemType ConsumeHeldItem()
    {
        OneUseItemType item = HeldItem;
        HeldItem = OneUseItemType.None;

        Debug.Log($"Consumed held item: {item}");
        return item;
    }

    // ---------------- PERKS ----------------

    public void UnlockFourthPerkSlot()
    {
        perkSlotCount = 4;
        Debug.Log("Fourth perk slot unlocked.");
    }

    public void EquipPerk(PerkData perk, int slotIndex)
    {
        if (perk == null) return;

        if (slotIndex < 0 || slotIndex >= perkSlotCount)
        {
            Debug.LogWarning($"Invalid perk slot: {slotIndex}");
            return;
        }

        equippedPerks[slotIndex] = perk;

        Debug.Log($"Equipped perk {perk.perkName} in slot {slotIndex + 1}");
    }

    public PerkData GetEquippedPerkInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedPerks.Length)
            return null;

        return equippedPerks[slotIndex];
    }

    public bool HasPerk(PerkType perkType)
    {
        for (int i = 0; i < perkSlotCount; i++)
        {
            if (equippedPerks[i] != null &&
                equippedPerks[i].perkType == perkType)
            {
                return true;
            }
        }

        return false;
    }

    // ---------------- STAT MODIFIERS ----------------

    public int GetMaxHealthModifier(int baseHealth)
    {
        int finalHealth = baseHealth + BonusMaxHealth;

        if (HasPerk(PerkType.Juggernaut))
            finalHealth *= 2;

        if (HasPerk(PerkType.Wildcard))
            finalHealth *= 2;

        if (HasPerk(PerkType.GlassCannon))
            finalHealth = 1;

        return Mathf.Max(1, finalHealth);
    }

    public float ModifyMoveSpeed(float baseSpeed)
    {
        float finalSpeed = baseSpeed + BonusMoveSpeed;

        if (HasPerk(PerkType.Wildcard))
            finalSpeed *= 2f;

        return finalSpeed;
    }

    public float ModifyComboTimer(float baseTimer)
    {
        float finalTimer = baseTimer;

        if (HasPerk(PerkType.DoubleComboTimer))
            finalTimer *= 2f;

        if (HasPerk(PerkType.Wildcard))
            finalTimer *= 2f;

        return finalTimer;
    }

    public float ModifyStun(float baseStun)
    {
        float finalStun = baseStun;

        if (HasPerk(PerkType.DoubleStun))
            finalStun *= 2f;

        if (HasPerk(PerkType.Wildcard))
            finalStun *= 2f;

        return finalStun;
    }

    public int ModifyCoinValue(int baseValue)
    {
        int finalValue = baseValue;

        if (HasPerk(PerkType.DoubleCoins))
            finalValue *= 2;

        if (HasPerk(PerkType.Wildcard))
            finalValue *= 2;

        return finalValue;
    }

    public int ModifyCoinDropCount(int baseCount)
    {
        int finalCount = baseCount;

        if (HasPerk(PerkType.Wildcard))
            finalCount *= 2;

        return Mathf.Max(1, finalCount);
    }

    public int ModifyOutgoingDamage(int baseDamage, Health playerHealth = null)
    {
        int finalDamage = baseDamage + BonusDamage + momentumBonusDamage;

        if (HasPerk(PerkType.GlassCannon))
            finalDamage *= 4;

        if (HasPerk(PerkType.LastStand) && playerHealth != null)
        {
            if (playerHealth.CurrentHealth <= playerHealth.MaxHealth * 0.25f)
                finalDamage *= 2;
        }

        if (HasPerk(PerkType.Freestyle) && currentHitIsRepeated)
        {
            finalDamage = Mathf.CeilToInt(finalDamage * 0.5f);
        }

        if (HasPerk(PerkType.Wildcard))
            finalDamage *= 2;

        return Mathf.Max(1, finalDamage);
    }

    public int ModifyIncomingDamage(int incomingDamage)
    {
        if (incomingDamage <= 0)
            return 0;

        if (TryConsumeBulwark())
            return 0;

        float armourPercent = BonusArmour / 100f;
        armourPercent = Mathf.Clamp(armourPercent, 0f, 0.4f);

        if (HasPerk(PerkType.Wildcard))
            armourPercent *= 2f;

        armourPercent = Mathf.Clamp(armourPercent, 0f, 0.8f);

        int finalDamage = Mathf.CeilToInt(incomingDamage * (1f - armourPercent));

        return Mathf.Max(1, finalDamage);
    }

    // ---------------- MOMENTUM / REPEAT HIT ----------------

    public void AddMomentumDamage()
    {
        if (!HasPerk(PerkType.Momentum))
            return;

        momentumBonusDamage++;

        Debug.Log($"Momentum bonus damage: {momentumBonusDamage}");
    }

    public void ResetMomentumDamage()
    {
        momentumBonusDamage = 0;
    }

    public void SetCurrentHitRepeated(bool repeated)
    {
        currentHitIsRepeated = repeated;
    }

    public bool CurrentHitIsRepeated => currentHitIsRepeated;

    // ---------------- SECOND WIND / BULWARK ----------------

    public bool TryUseSecondWind()
    {
        if (!HasPerk(PerkType.SecondWind))
            return false;

        if (secondWindUsedThisRound)
            return false;

        secondWindUsedThisRound = true;

        Debug.Log("Second Wind used.");
        return true;
    }

    private bool TryConsumeBulwark()
    {
        if (!HasPerk(PerkType.Bulwark))
            return false;

        if (!bulwarkAvailable)
            return false;

        bulwarkAvailable = false;

        Debug.Log("Bulwark blocked incoming damage.");
        return true;
    }

    public void RechargeBulwark()
    {
        bulwarkAvailable = true;
        Debug.Log("Bulwark recharged.");
    }
}