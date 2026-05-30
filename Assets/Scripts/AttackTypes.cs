using UnityEngine;

public class AttackTypes
{
    public enum AttackStrength
    {
        Light,
        Heavy
    }

    public enum AttackDirection
    {
        Neutral,
        Side,
        Up,
        Down
    }

    public struct AttackId
    {
        public AttackStrength Strength;
        public AttackDirection Direction;

        public AttackId(AttackStrength strength, AttackDirection direction)
        {
            Strength = strength;
            Direction = direction;
        }

        public override string ToString()
        {
            return $"{Strength}_{Direction}";
        }
    }
}
