using UnityEngine;

public enum ShopItemType
{
    MaxHealth,
    Armour,
    Damage,
    Speed,
    EnemiesHalfHealthNextRound,
    FourthPerkSlot,

    OneUseItem,

    CoinMagnetism,
    ChallengeMode,

    Perk
}

public enum OneUseItemType
{
    None,
    Nuke,
    HealthPotion,
    InvincibilityInjection,
    AdrenalineInjection,
    BrokenTeleport
}


[CreateAssetMenu(menuName = "Shop/Shop Item")]
public class ShopItem : ScriptableObject
{
    public string itemName;
    public string description;

    public ShopItemType itemType;

    public int baseCost = 50;
    public int value = 1;

    [Header("Random Shop Weight")]
    public int weight = 10;

    [Header("Perk Only")]
    public PerkData perk;

    [Header("One Use Item Only")]
    public OneUseItemType oneUseItemType;
}