using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{

    public static WaveManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    [Header("Setup")]
    public List<Transform> spawnPoints;       // các điểm spawn
    public PlayerStats playerStats;

    [Header("Waves")]
    public List<EnemyWaveSO> waves;           // danh sách wave
    public int currentWaveIndex = 0;

    public ObservableValue<int> aliveEnemies = new(0); // số lượng enemy còn sống

    public static event Action<int> OnWaveStarted;   // int = wave index
    public static event Action OnWaveCleared;   // int = wave index

    private bool isSpawningWave = false;


    private void Start()
    {
        StartNextWave(); // auto start wave đầu
    }

    public void StartNextWave()
    {
        if (isSpawningWave) return;
        if (currentWaveIndex >= waves.Count) return;

        EnemyWaveSO wave = waves[currentWaveIndex];
        StartCoroutine(SpawnWave(wave));
    }

    private IEnumerator SpawnWave(EnemyWaveSO wave)
    {
        isSpawningWave = true;
        aliveEnemies.Value = 0;

        OnWaveStarted?.Invoke(currentWaveIndex);

        // spawn từng enemy entry
        foreach (var entry in wave.enemies)
        {
            yield return new WaitForSeconds(entry.startDelay);

            for (int i = 0; i < entry.enemyCount; i++)
            {
                SpawnEnemy(entry);
                yield return new WaitForSeconds(entry.spawnRate);
            }
        }
        isSpawningWave = false;
    }

    // private void SpawnEnemy(EnemyEntry entry)
    // {
    //     if (spawnPoints.Count == 0) return;

    //     Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
    //     EnemyBase enemyObj = Instantiate(entry.enemyData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    //     enemyObj.Initialize(entry.enemyData, playerStats);

    //     aliveEnemies.Value++;
    //     enemyObj.OnDie += HandleEnemyDeath;
    // }


    // private void HandleEnemyDeath()
    // {
    //     aliveEnemies.Value--;
    //     if (aliveEnemies.Value <= 0 && !isSpawningWave)
    //     {
    //         OnWaveCleared?.Invoke();
    //         currentWaveIndex++;
    //     }
    // }
    // ...existing code...
    private void SpawnEnemy(EnemyEntry entry)
    {
        if (spawnPoints.Count == 0) return;

        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
        EnemyBase enemyObj = Instantiate(entry.enemyData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemyObj.Initialize(entry.enemyData, playerStats);

        aliveEnemies.Value++;
        enemyObj.OnDie += HandleEnemyDeath;

        // Register enemy on radar
        Radar.Instance?.RegisterEnemy(enemyObj.transform);
    }

    private void HandleEnemyDeath()
    {
        aliveEnemies.Value--;
        if (aliveEnemies.Value <= 0 && !isSpawningWave)
        {
            OnWaveCleared?.Invoke();
            currentWaveIndex++;
        }
    }
    // ...existing code...
}
