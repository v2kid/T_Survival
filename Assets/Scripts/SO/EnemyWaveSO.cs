using UnityEngine;

[CreateAssetMenu(menuName = "Game/WaveConfig", fileName = "NewWave")]
public class EnemyWaveSO : ScriptableObject
{
    [Header("Enemy Settings")]
    public EnemyBase enemyPrefab;
    public EnemySO enemyData;

    [Header("Wave Settings")]
    public int enemyCount = 10;           // số lượng quái trong wave
    public float spawnRate = 1f;          // thời gian giữa mỗi quái
    public float startDelay = 0f;         // delay trước khi wave này bắt đầu
}
