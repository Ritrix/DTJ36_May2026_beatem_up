using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public int Coins { get; private set; }

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
}