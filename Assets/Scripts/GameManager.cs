using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private string combatSceneName = "SampleScene";
    [SerializeField] private string shopSceneName = "ShopScene";

    [Header("Wave")]
    public int CurrentWave { get; private set; } = 1;

    [Header("Player Currency")]
    public int Coins { get; private set; }
    

    [Header("Player Stats")]
    public int BonusArmour { get; private set; }
    public int BonusMaxHealth { get; private set; }
    public float BonusMoveSpeed { get; private set; }
    public int BonusDamage { get; private set; }

    [Header("Temporary Round Effects")]
    public bool EnemiesStartHalfHealthNextRound { get; private set; }

    [Header("Perks")]
    [SerializeField] private int perkSlotCount = 3;
    private PerkData[] equippedPerks = new PerkData[4];

    public int PerkSlotCount => perkSlotCount;

    private Dictionary<string, int> purchaseCounts = new();

    public int GetPurchaseCount(string itemId)
    {
        if (!purchaseCounts.ContainsKey(itemId))
            return 0;

        return purchaseCounts[itemId];
    }

    public void RegisterPurchase(string itemId)
    {
        if (!purchaseCounts.ContainsKey(itemId))
            purchaseCounts[itemId] = 0;

        purchaseCounts[itemId]++;
    }

    public void UnlockFourthPerkSlot()
    {
        perkSlotCount = 4;
    }

    public void EquipPerk(PerkData perk, int slotIndex)
    {
        if (perk == null) return;
        if (slotIndex < 0 || slotIndex >= perkSlotCount) return;

        equippedPerks[slotIndex] = perk;

        Debug.Log($"Equipped perk {perk.perkName} in slot {slotIndex + 1}");
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

    public PerkData GetEquippedPerkInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= perkSlotCount) return null;
        return equippedPerks[slotIndex];
    }

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

    public void ReturnToMainMenuAfterDeath()
    {
        ResetRunDataOnly();
        SceneManager.LoadScene("MainMenuScene");
    }

    public void ResetRunDataOnly()
    {
        CurrentWave = 1;
        Coins = 0;
        BonusMaxHealth = 0;
        BonusMoveSpeed = 0f;
        BonusDamage = 0;
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        Debug.Log($"Coins: {Coins}");
    }

    public bool SpendCoins(int amount)
    {
        if (Coins < amount)
            return false;

        Coins -= amount;
        Debug.Log($"Coins: {Coins}");
        return true;
    }

    public void OpenShop()
    {
        SceneManager.LoadScene(shopSceneName);
    }

    public void StartNextWave()
    {
        CurrentWave++;
        SceneManager.LoadScene(combatSceneName);
    }

    public void ResetRun()
    {
        CurrentWave = 1;
        Coins = 0;
        BonusMaxHealth = 0;
        BonusMoveSpeed = 0f;
        BonusDamage = 0;

        SceneManager.LoadScene(combatSceneName);
    }

    public void IncreaseMaxHealth(int amount)
    {
        BonusMaxHealth += amount;
    }

    public void IncreaseArmour(int amount)
    {
        BonusArmour += amount;
    }

    public void IncreaseDamage(int amount)
    {
        BonusDamage += amount;
    }

    public void IncreaseMoveSpeed(float amount)
    {
        BonusMoveSpeed += amount;
    }

    public void SetEnemiesHalfHealthNextRound()
    {
        EnemiesStartHalfHealthNextRound = true;
    }

    public void ConsumeEnemiesHalfHealthEffect()
    {
        EnemiesStartHalfHealthNextRound = false;
    }
}