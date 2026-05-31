using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Wave Scaling")]
    [SerializeField] private int baseEnemyCount = 4;
    [SerializeField] private int enemiesAddedPerWave = 2;

    [Header("Max Alive Scaling")]
    [SerializeField] private int startingMaxAliveEnemies = 2;
    [SerializeField] private int maxAliveEnemiesAddedPerWave = 1;
    [SerializeField] private int maxAliveEnemiesCap = 12;

    [Header("Spawn Rate")]
    [SerializeField] private float startingMinSpawnInterval = 1.5f;
    [SerializeField] private float startingMaxSpawnInterval = 3f;
    [SerializeField] private float minSpawnIntervalCap = 0.25f;
    [SerializeField] private float maxSpawnIntervalCap = 0.8f;
    [SerializeField] private float spawnIntervalMultiplierPerWave = 0.85f;

    [Header("Jumper Scaling")]
    [SerializeField] private int jumperFirstWave = 4;
    [SerializeField] private int jumperMaxStartingCount = 1;
    [SerializeField] private int jumperMaxAddedPerWave = 1;

    [Header("Jumper Spawn Area")]
    [SerializeField] private Vector2 jumperSpawnCenter;
    [SerializeField] private Vector2 jumperSpawnSize;

    [Header("Timing")]
    [SerializeField] private float startDelay = 2f;
    [SerializeField] private float endWaveDelay = 10f;

    [Header("Survival Scaling")]
    [SerializeField] private bool survivalMode = true;
    [SerializeField] private int healthIncreaseEveryXRounds = 3;
    [SerializeField] private int healthIncreaseAmount = 25;

    private int enemiesToSpawn;
    private int enemiesSpawned;
    private int enemiesDefeated;
    private int aliveEnemies;
    private int jumpersSpawnedThisWave;

    private bool isSpawning;
    private bool waveEnding;
    private bool waveStarted;
    private bool halfHealthThisWave;

    private void Start()
    {
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        waveStarted = false;
        isSpawning = true;
        waveEnding = false;

        yield return new WaitForSeconds(startDelay);

        int wave = GameManager.Instance != null
            ? GameManager.Instance.CurrentWave
            : 1;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.BeginRoundEffects();
        }

        enemiesToSpawn = GetEnemyCountForWave(wave);
        enemiesSpawned = 0;
        enemiesDefeated = 0;
        aliveEnemies = 0;
        jumpersSpawnedThisWave = 0;

        halfHealthThisWave =
            GameManager.Instance != null &&
            GameManager.Instance.EnemiesStartHalfHealthNextRound;

        if (halfHealthThisWave && GameManager.Instance != null)
        {
            GameManager.Instance.ConsumeEnemiesHalfHealthEffect();
        }

        int currentMaxAliveEnemies = GetMaxAliveEnemiesForWave(wave);

        Debug.Log(
            $"Wave {wave} started. " +
            $"Total enemies: {enemiesToSpawn}, " +
            $"Max alive: {currentMaxAliveEnemies}, " +
            $"Max jumpers: {GetMaxJumpersForWave(wave)}"
        );

        waveStarted = true;

        while (enemiesSpawned < enemiesToSpawn)
        {
            while (aliveEnemies >= currentMaxAliveEnemies)
            {
                yield return null;
            }

            SpawnEnemy(wave);
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

    private void SpawnEnemy(int wave)
    {
        if (enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("WaveSpawner is missing enemy prefabs.");
            return;
        }

        GameObject prefab = GetRandomAllowedEnemyPrefab(wave);

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

        Health enemyRawHealth = enemyObject.GetComponent<Health>();
        if (enemyRawHealth != null)
        {
            int bonusHealth = GetEnemyBonusHealthForCurrentWave();

            if (bonusHealth > 0)
            {
                enemyRawHealth.SetMaxHealth(enemyRawHealth.MaxHealth + bonusHealth, true);
            }

            if (halfHealthThisWave)
            {
                enemyRawHealth.SetCurrentHealth(
                    Mathf.CeilToInt(enemyRawHealth.MaxHealth * 0.5f)
                );
            }

            Debug.Log(
                $"{enemyObject.name} spawned with health " +
                $"{enemyRawHealth.CurrentHealth}/{enemyRawHealth.MaxHealth}"
            );
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

    private void HandleEnemyDied(EnemyHealth enemy)
    {
        enemy.OnEnemyDied -= HandleEnemyDied;

        enemiesDefeated++;
        aliveEnemies--;

        aliveEnemies = Mathf.Max(0, aliveEnemies);

        Debug.Log($"Enemy defeated. {enemiesDefeated}/{enemiesToSpawn}");
    }

    private int GetEnemyCountForWave(int wave)
    {
        int count = baseEnemyCount + ((wave - 1) * enemiesAddedPerWave);

        if (GameManager.Instance != null &&
            GameManager.Instance.ChallengeModeActiveThisRound)
        {
            count *= 3;
        }

        return count;
    }

    private int GetMaxAliveEnemiesForWave(int wave)
    {
        int increaseSteps = (wave - 1) / 2;

        int maxAlive = startingMaxAliveEnemies +
                       (increaseSteps * maxAliveEnemiesAddedPerWave);

        return Mathf.Min(maxAlive, maxAliveEnemiesCap);
    }

    private int GetMaxJumpersForWave(int wave)
    {
        if (wave < jumperFirstWave)
            return 0;

        int jumperCount = jumperMaxStartingCount +
                          ((wave - jumperFirstWave) * jumperMaxAddedPerWave);

        return jumperCount;
    }

    private GameObject GetRandomAllowedEnemyPrefab(int wave)
    {
        int maxJumpersThisWave = GetMaxJumpersForWave(wave);

        for (int attempts = 0; attempts < 30; attempts++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            bool isJumper = prefab.GetComponent<JumperBehaviour>() != null;

            if (isJumper)
            {
                if (wave < jumperFirstWave)
                    continue;

                if (jumpersSpawnedThisWave >= maxJumpersThisWave)
                    continue;

                jumpersSpawnedThisWave++;
                return prefab;
            }

            return prefab;
        }

        return GetFallbackNonJumperPrefab();
    }

    private GameObject GetFallbackNonJumperPrefab()
    {
        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab.GetComponent<JumperBehaviour>() == null)
                return prefab;
        }

        return enemyPrefabs[0];
    }

    private int GetEnemyBonusHealthForCurrentWave()
    {
        if (!survivalMode) return 0;
        if (GameManager.Instance == null) return 0;

        int wave = GameManager.Instance.CurrentWave;

        int increaseSteps = (wave - 1) / healthIncreaseEveryXRounds;

        return increaseSteps * healthIncreaseAmount;
    }

    private float GetMinSpawnInterval(int wave)
    {
        float interval = startingMinSpawnInterval *
                         Mathf.Pow(spawnIntervalMultiplierPerWave, wave - 1);

        return Mathf.Max(interval, minSpawnIntervalCap);
    }

    private float GetMaxSpawnInterval(int wave)
    {
        float interval = startingMaxSpawnInterval *
                         Mathf.Pow(spawnIntervalMultiplierPerWave, wave - 1);

        return Mathf.Max(interval, maxSpawnIntervalCap);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(jumperSpawnCenter, jumperSpawnSize);
    }
}