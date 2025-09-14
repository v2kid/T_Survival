using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Setup")]
    public List<Transform> spawnPoints;       // các điểm spawn
    public PlayerStats playerStats;

    [Header("Waves")]
    public List<EnemyWaveSO> waves;           // danh sách wave
    public int currentWaveIndex = 0;

    public int aliveEnemies = 0;             // số enemy còn sống

    public static event Action<int> OnWaveStarted;   // int = wave index
    public static event Action<int> OnWaveCleared;   // int = wave index

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
        aliveEnemies = 0;

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

    private void SpawnEnemy(EnemyEntry entry)
    {
        if (spawnPoints.Count == 0) return;

        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
        EnemyBase enemyObj = Instantiate(entry.enemyData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemyObj.Initialize(entry.enemyData, playerStats);

        aliveEnemies++;
        enemyObj.OnDie += HandleEnemyDeath; 
    }

    private void HandleEnemyDeath()
    {
        aliveEnemies--;
        if (aliveEnemies <= 0 && !isSpawningWave)
        {
            OnWaveCleared?.Invoke(currentWaveIndex);
            currentWaveIndex++;
        }
    }
}
