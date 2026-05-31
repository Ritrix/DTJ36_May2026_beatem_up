using UnityEngine;

public enum PerkType
{
    DoubleCoins,
    HealthRegen,
    ReviveOncePerRound,
    DoubleHealth,
    ExplosionsOnHit,
    QuadDamageOneHP,
    RechargeShield,
    DamageRamp
}

[CreateAssetMenu(menuName = "Shop/Perk Data")]
public class PerkData : ScriptableObject
{
    public string perkName;
    public string description;
    public PerkType perkType;
    public int weight = 10;
}