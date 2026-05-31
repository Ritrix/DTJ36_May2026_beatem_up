using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Pools")]
    [SerializeField] private ShopItem[] basicItems;
    [SerializeField] private PerkData[] perks;


    [Header("Buttons")]
    [SerializeField] private Button[] shopButtons;
    [SerializeField] private TMP_Text[] shopButtonTexts;
    [SerializeField] private TMP_Text[] shopTextInButtons;

    private ShopItem[] currentOffers = new ShopItem[3];

    [Header("UI")]
    [SerializeField] private PerkPopupUI perkPopupUI;

    private void Start()
    {
        GenerateShop();
    }

    private void GenerateShop()
    {
        currentOffers[0] = GetWeightedBasicItem();
        currentOffers[1] = GetWeightedBasicItem();
        currentOffers[2] = CreatePerkOffer();

        Debug.Log($"Generated shop offers: {currentOffers[0].itemName}, {currentOffers[1].itemName}, {currentOffers[2].itemName}");

        for (int i = 0; i < shopButtons.Length; i++)
        {
            int index = i;

            shopButtons[i].onClick.RemoveAllListeners();
            shopButtons[i].onClick.AddListener(() => BuyItem(index));

            UpdateButtonText(i);
        }
    }

    private ShopItem GetWeightedBasicItem()
    {
        int totalWeight = 0;

        foreach (ShopItem item in basicItems)
        {
            totalWeight += item.weight;
        }

        int roll = Random.Range(0, totalWeight);
        int current = 0;

        foreach (ShopItem item in basicItems)
        {
            current += item.weight;

            if (roll < current)
                return item;
        }

        return basicItems[0];
    }

    private PerkData GetWeightedPerk()
    {
        int totalWeight = 0;

        foreach (PerkData perk in perks)
        {
            totalWeight += perk.weight;
        }

        int roll = Random.Range(0, totalWeight);
        int current = 0;

        foreach (PerkData perk in perks)
        {
            current += perk.weight;

            if (roll < current)
                return perk;
        }

        return perks[0];
    }

    private ShopItem CreatePerkOffer()
    {
        PerkData perk = GetWeightedPerk();

        ShopItem offer = ScriptableObject.CreateInstance<ShopItem>();

        offer.itemName = perk.perkName;
        offer.description = perk.description;
        offer.itemType = ShopItemType.Perk;

        float rarityMultiplier = Mathf.Pow(1.6f, 7 - perk.weight);
        offer.baseCost = Mathf.RoundToInt(600 * rarityMultiplier);

        offer.perk = perk;

        return offer;
    }

    private void BuyItem(int index)
    {
        ShopItem item = currentOffers[index];

        if (item == null) return;

        int cost = GetCurrentCost(item);

        if (GameManager.Instance == null) return;

        if (!GameManager.Instance.SpendCoins(cost))
        {
            Debug.Log("Not enough coins.");
            return;
        }

        ApplyItem(item);

        shopButtons[index].interactable = false;
        shopButtonTexts[index].text = "SOLD";
    }

    private int GetCurrentCost(ShopItem item)
    {
        int purchaseCount = GameManager.Instance.GetPurchaseCount(item.itemName);

        return Mathf.RoundToInt(item.baseCost * Mathf.Pow(1.5f, purchaseCount));
    }

    private void ApplyItem(ShopItem item)
    {
        switch (item.itemType)
        {
            case ShopItemType.MaxHealth:
                GameManager.Instance.IncreaseMaxHealth(item.value);
                break;

            case ShopItemType.Armour:
                GameManager.Instance.IncreaseArmour(item.value);
                break;

            case ShopItemType.Damage:
                GameManager.Instance.IncreaseDamage(item.value);
                break;

            case ShopItemType.Speed:
                GameManager.Instance.IncreaseMoveSpeed(item.value);
                break;

            case ShopItemType.EnemiesHalfHealthNextRound:
                GameManager.Instance.SetEnemiesHalfHealthNextRound();
                break;

            case ShopItemType.FourthPerkSlot:
                GameManager.Instance.UnlockFourthPerkSlot();
                break;

            case ShopItemType.CoinMagnetism:
                GameManager.Instance.UnlockCoinMagnetism();
                break;

            case ShopItemType.OneUseItem:
                GameManager.Instance.TrySetHeldItem(item.oneUseItemType);
                break;

            case ShopItemType.ChallengeMode:
                GameManager.Instance.SetChallengeModeNextRound();
                break;

            case ShopItemType.Perk:
                perkPopupUI.OpenPopup(item.perk);
                break;
        }

        GameManager.Instance.RegisterPurchase(item.itemName);
    }


    private void UpdateButtonText(int index)
    {
        ShopItem item = currentOffers[index];

        if (item == null)
        {
            shopButtonTexts[index].text = "EMPTY";
            return;
        }

        int cost = GetCurrentCost(item);

        shopButtonTexts[index].text =
            $"{item.description}\n" +
            $"Cost: {cost}";

        shopTextInButtons[index].text =
            $"{item.itemName}\n";
    }
}