using UnityEngine;

public class PlayerInvincibility : MonoBehaviour
{
    public bool IsInvincible { get; private set; }

    public void SetInvincible(bool value)
    {
        IsInvincible = value;
    }
}