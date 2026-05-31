using UnityEngine;

public enum PerkType
{
    DoubleCoins,
    SecondWind,
    Regen,
    Bulwark,
    Juggernaut,
    Momentum,
    DoubleComboTimer,
    DoubleStun,
    GlassCannon,
    LastStand,
    ExplosiveStrikes,
    Freestyle,
    Wildcard
}

[CreateAssetMenu(menuName = "Shop/Perk Data")]
public class PerkData : ScriptableObject
{
    public string perkName;
    public string description;
    public PerkType perkType;
    public int weight = 10;
}