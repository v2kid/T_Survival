using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Setup")]
    public List<Transform> spawnPoints;          // các điểm spawn
    public PlayerStats playerStats;

    [Header("Waves")]
    public List<EnemyWaveSO> waves;              // danh sách wave
    private int currentWaveIndex = 0;

    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        while (currentWaveIndex < waves.Count)
        {
            EnemyWaveSO wave = waves[currentWaveIndex];

            // chờ delay trước khi bắt đầu wave
            yield return new WaitForSeconds(wave.startDelay);

            // spawn enemy trong wave
            yield return StartCoroutine(SpawnWave(wave));

            currentWaveIndex++;
        }
    }

    private IEnumerator SpawnWave(EnemyWaveSO wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave);
            yield return new WaitForSeconds(wave.spawnRate);
        }
    }

    private void SpawnEnemy(EnemyWaveSO wave)
    {
        if (spawnPoints.Count == 0) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        EnemyBase enemyObj = Instantiate(wave.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        enemyObj.Initialize(wave.enemyData, playerStats);
    }
}
