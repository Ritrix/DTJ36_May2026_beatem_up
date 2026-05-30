using UnityEngine;
using static AttackTypes;

[CreateAssetMenu(fileName = "AttackData", menuName = "Scriptable Objects/AttackData")]
public class AttackData : ScriptableObject
{
    public AttackStrength strength;
    public AttackDirection direction;

    [Header("Damage")]
    public int damage = 10;

    [Header("Timing")]
    public float attackDuration = 0.45f;
    public float hitboxStartTime = 0.15f;
    public float hitboxEndTime = 0.28f;

    [Header("Hitbox")]
    public Vector2 hitboxOffset = new Vector2(1f, 0f);
    public Vector2 hitboxSize = new Vector2(1f, 1f);

    [Header("Stun")]
    public float stunDuration = 0.3f;

    [Header("Animation")]
    public string animationName;
}
