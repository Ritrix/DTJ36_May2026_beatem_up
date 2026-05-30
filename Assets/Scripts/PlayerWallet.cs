using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public void AddCoins(int amount)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins(amount);
        }
    }

    public bool SpendCoins(int amount)
    {
        if (GameManager.Instance == null)
            return false;

        return GameManager.Instance.SpendCoins(amount);
    }
}