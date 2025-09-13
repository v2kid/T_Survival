using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "GameData/EnemySO")]
public class EnemySO : ScriptableObject
{
    public EnemyID enemyID;
    public string enemyName;
    public float maxHealth;
    public float moveSpeed;
    public float meleeDamage;
    public float attackRange;
    public float attackSpeed;
    public int experiencePoints;

}


public enum EnemyID
{
    Zombie = 0,
    Skeleton = 1,
    Goblin = 2
}