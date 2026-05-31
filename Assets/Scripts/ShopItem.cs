using UnityEngine;

public enum ShopItemType
{
    MaxHealth,
    Armour,
    Damage,
    Speed,
    EnemiesHalfHealthNextRound,
    FourthPerkSlot,
    Perk
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
}