using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private string combatSceneName = "SampleScene";
    [SerializeField] private string shopSceneName = "ShopScene";

    [Header("Wave")]
    public int CurrentWave { get; private set; } = 1;

    [Header("Player Currency")]
    public int Coins { get; private set; }

    [Header("Player Upgrades")]
    public int BonusMaxHealth { get; private set; }
    public float BonusMoveSpeed { get; private set; }
    public int BonusDamage { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ReturnToMainMenuAfterDeath()
    {
        ResetRunDataOnly();
        SceneManager.LoadScene("MainMenu");
    }

    public void ResetRunDataOnly()
    {
        CurrentWave = 1;
        Coins = 0;
        BonusMaxHealth = 0;
        BonusMoveSpeed = 0f;
        BonusDamage = 0;
    }

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

    public void OpenShop()
    {
        SceneManager.LoadScene(shopSceneName);
    }

    public void StartNextWave()
    {
        CurrentWave++;
        SceneManager.LoadScene(combatSceneName);
    }

    public void ResetRun()
    {
        CurrentWave = 1;
        Coins = 0;
        BonusMaxHealth = 0;
        BonusMoveSpeed = 0f;
        BonusDamage = 0;

        SceneManager.LoadScene(combatSceneName);
    }

    public void IncreaseMaxHealth(int amount)
    {
        BonusMaxHealth += amount;
    }

    public void IncreaseMoveSpeed(float amount)
    {
        BonusMoveSpeed += amount;
    }

    public void IncreaseDamage(int amount)
    {
        BonusDamage += amount;
    }
}