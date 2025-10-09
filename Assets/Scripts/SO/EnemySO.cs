using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "GameData/EnemySO")]
public class EnemySO : ScriptableObject
{
    public EnemyBase enemyPrefab;
    public EnemyID enemyID;
    public string enemyName;
    public float maxHealth;
    public float moveSpeed;
    public float meleeDamage;
    public float attackRange;
    public float attackSpeed;
    public int coinDrop;
}


public enum EnemyID
{
    Zombie = 0,
    Dummy = 99
}