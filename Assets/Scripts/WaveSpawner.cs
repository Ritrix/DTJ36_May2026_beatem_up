using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject[] enemyPrefabs;
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

    [Header("Jumper Spawn Area")]
    [SerializeField] private Vector2 jumperSpawnCenter;
    [SerializeField] private Vector2 jumperSpawnSize;

    [Header("Timing")]
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float endWaveDelay = 2f;

    private int enemiesToSpawn;
    private int enemiesSpawned;
    private int enemiesDefeated;

    private bool isSpawning;
    private bool waveEnding;

    private bool waveStarted;

    private void Start()
    {
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        waveStarted = false;

        yield return new WaitForSeconds(startDelay);

        waveStarted = true;

        int wave = GameManager.Instance != null
            ? GameManager.Instance.CurrentWave
            : 1;
        GameManager.Instance.BeginRoundEffects();

        enemiesToSpawn = GetEnemyCountForWave(wave);
        enemiesSpawned = 0;
        enemiesDefeated = 0;
        isSpawning = true;
        waveEnding = false;

        bool halfHealthEnemies =
            GameManager.Instance != null &&
            GameManager.Instance.EnemiesStartHalfHealthNextRound;

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

        if (halfHealthEnemies && GameManager.Instance != null)
        {
            GameManager.Instance.ConsumeEnemiesHalfHealthEffect();
        }

        isSpawning = false;
    }

    private void Update()
    {
        if (!waveStarted) return;
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
            GameManager.Instance.CompleteRoundEffects();
            GameManager.Instance.OpenShop();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("WaveSpawner is missing enemy prefabs.");
            return;
        }

        GameObject prefab = GetRandomAllowedEnemyPrefab();

        Vector3 spawnPosition;

        if (prefab.GetComponent<JumperBehaviour>() != null)
        {
            spawnPosition = GetRandomJumperPosition();
        }
        else
        {
            if (spawnPoints.Length == 0)
            {
                Debug.LogWarning("WaveSpawner is missing spawn points.");
                return;
            }

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            spawnPosition = spawnPoint.position;
        }

        GameObject enemyObject = Instantiate(
            prefab,
            spawnPosition,
            Quaternion.identity
        );

        aliveEnemies++;

        EnemyHealth enemyHealth = enemyObject.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnEnemyDied += HandleEnemyDied;
        }

        if (GameManager.Instance != null &&
            GameManager.Instance.EnemiesStartHalfHealthNextRound)
        {
            Health enemyRawHealth = enemyObject.GetComponent<Health>();

            if (enemyRawHealth != null)
            {
                enemyRawHealth.SetCurrentHealth(enemyRawHealth.MaxHealth / 2);
            }
        }

        EnemyBehaviour enemyBehaviour = enemyObject.GetComponent<EnemyBehaviour>();
        if (enemyBehaviour != null)
        {
            enemyBehaviour.SetPlayer(player);
        }

        JumperBehaviour jumperBehaviour = enemyObject.GetComponent<JumperBehaviour>();
        if (jumperBehaviour != null)
        {
            jumperBehaviour.SetPlayer(player);
        }
    }

    private GameObject GetRandomAllowedEnemyPrefab()
    {
        int wave = GameManager.Instance != null
            ? GameManager.Instance.CurrentWave
            : 1;

        for (int attempts = 0; attempts < 20; attempts++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            bool isJumper = prefab.GetComponent<JumperBehaviour>() != null;

            if (isJumper && wave < 4)
                continue;

            return prefab;
        }

        return enemyPrefabs[0];
    }

    private Vector2 GetRandomJumperPosition()
    {
        float x = Random.Range(
            jumperSpawnCenter.x - jumperSpawnSize.x * 0.5f,
            jumperSpawnCenter.x + jumperSpawnSize.x * 0.5f
        );

        float y = Random.Range(
            jumperSpawnCenter.y - jumperSpawnSize.y * 0.5f,
            jumperSpawnCenter.y + jumperSpawnSize.y * 0.5f
        );

        return new Vector2(x, y);
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
        int count = baseEnemyCount * (int)Mathf.Pow(2, wave - 1);

        if (GameManager.Instance != null &&
            GameManager.Instance.ChallengeModeActiveThisRound)
        {
            count *= 3;
        }

        return count;
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