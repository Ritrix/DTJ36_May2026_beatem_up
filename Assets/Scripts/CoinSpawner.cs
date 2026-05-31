using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class CoinSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private CoinPickup coinPrefab;

    [Header("Drop Count")]
    [SerializeField] private int minCoins = 5;
    [SerializeField] private int maxCoins = 15;

    [Header("Coin Chances")]
    [SerializeField, Range(0f, 1f)] private float bronzeChance = 0.70f;
    [SerializeField, Range(0f, 1f)] private float silverChance = 0.22f;
    [SerializeField, Range(0f, 1f)] private float goldChance = 0.07f;
    [SerializeField, Range(0f, 1f)] private float redChance = 0.01f;

    [Header("Spawn")]
    [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 0.5f);

    private int GetRandomCoinValue()
    {
        float roll = Random.value;

        if (roll < bronzeChance)
            return 1;

        if (roll < bronzeChance + silverChance)
            return 10;

        if (roll < bronzeChance + silverChance + goldChance)
            return 25;

        return 100;
    }

    public void SpawnCoins(int horizontalDirection)
    {
        int count = Random.Range(minCoins, maxCoins + 1);

        if (GameManager.Instance.HasPerk(PerkType.DoubleCoins))
        {
            count *= 2;
        }

        Vector2 spawnPosition = (Vector2)transform.position + spawnOffset;

        for (int i = 0; i < count; i++)
        {
            CoinPickup coin = Instantiate(
                coinPrefab,
                spawnPosition,
                Quaternion.identity
            );

            int value = GetRandomCoinValue();

            coin.SetValue(value);
            coin.Launch(spawnPosition, horizontalDirection);
        }
    }
}