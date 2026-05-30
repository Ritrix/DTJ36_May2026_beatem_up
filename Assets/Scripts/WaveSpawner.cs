using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private EnemyBehaviour[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Scaling")]
    [SerializeField] private int baseEnemyCount = 3;

    [Header("Spawn Rate")]
    [SerializeField] private float startingMinSpawnInterval = 1.5f;
    [SerializeField] private float startingMaxSpawnInterval = 3f;
    [SerializeField] private float minSpawnIntervalCap = 0.25f;
    [SerializeField] private float maxSpawnIntervalCap = 0.8f;
    [SerializeField] private float spawnRateIncreasePerWave = 0.12f;
    [SerializeField] private float spawnIntervalMultiplierPerWave = 0.85f;
    [SerializeField] private int maxAliveEnemies = 20;
    private int aliveEnemies;

    [Header("Timing")]
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float endWaveDelay = 2f;

    private int enemiesToSpawn;
    private int enemiesSpawned;
    private int enemiesDefeated;

    private bool isSpawning;
    private bool waveEnding;

    private void Start()
    {
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        yield return new WaitForSeconds(startDelay);

        int wave = GameManager.Instance != null
            ? GameManager.Instance.CurrentWave
            : 1;

        enemiesToSpawn = GetEnemyCountForWave(wave);
        enemiesSpawned = 0;
        enemiesDefeated = 0;
        isSpawning = true;
        waveEnding = false;

        Debug.Log($"Wave {wave} started. Enemies: {enemiesToSpawn}");

        while (enemiesSpawned < enemiesToSpawn)
        {
            if (aliveEnemies >= maxAliveEnemies)
            {
                yield return null;
                continue;
            }

            SpawnEnemy();
            enemiesSpawned++;

            float waitTime = Random.Range(
                GetMinSpawnInterval(wave),
                GetMaxSpawnInterval(wave)
            );

            yield return new WaitForSeconds(waitTime);
        }

        isSpawning = false;
    }

    private void Update()
    {
        if (waveEnding) return;
        if (isSpawning) return;
        if (enemiesDefeated < enemiesToSpawn) return;

        StartCoroutine(EndWave());
    }

    private IEnumerator EndWave()
    {
        waveEnding = true;

        Debug.Log("Wave complete.");

        yield return new WaitForSeconds(endWaveDelay);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OpenShop();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("WaveSpawner is missing enemy prefabs or spawn points.");
            return;
        }

        EnemyBehaviour prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        EnemyBehaviour enemy = Instantiate(
            prefab,
            spawnPoint.position,
            Quaternion.identity
        );
        aliveEnemies++;
        enemy.SetPlayer(player);

        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.OnEnemyDied += HandleEnemyDied;
        }
    }

    private void HandleEnemyDied(EnemyHealth enemy)
    {
        enemy.OnEnemyDied -= HandleEnemyDied;

        enemiesDefeated++;
        aliveEnemies--;

        Debug.Log($"Enemy defeated. {enemiesDefeated}/{enemiesToSpawn}");
    }

    private int GetEnemyCountForWave(int wave)
    {
        return baseEnemyCount * (int)Mathf.Pow(2, wave - 1);
    }

    private float GetMinSpawnInterval(int wave)
    {
        float interval = startingMinSpawnInterval * Mathf.Pow(spawnIntervalMultiplierPerWave, wave - 1);
        return Mathf.Max(interval, minSpawnIntervalCap);
    }

    private float GetMaxSpawnInterval(int wave)
    {
        float interval = startingMaxSpawnInterval * Mathf.Pow(spawnIntervalMultiplierPerWave, wave - 1);
        return Mathf.Max(interval, maxSpawnIntervalCap);
    }
}