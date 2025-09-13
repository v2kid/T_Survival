using System.Collections;
using Maskborn;
using UnityEngine;

public class EnemyBase : MonoBehaviour, IHealthBar
{
    public EnemySO enemyData;
    private EnemyStat enemyStat;
    public PlayerStats playerStats; //track player
    private UIHealthBar _healthBar;
    private EnemyStateMachine stateMachine;
    [SerializeField] private Renderer _renderer;
    public static event System.Action<int> OnDie; //experience points

    public bool IsDead => enemyStat.currentHealth <= 0;
    public void Initialize(EnemySO data, PlayerStats playerStats_)
    {
        enemyData = data;
        playerStats = playerStats_;
        enemyStat = new EnemyStat(enemyData);
        stateMachine = GetComponent<EnemyStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.enemyBase = this;
            stateMachine.Initialize(playerStats);
        }
    }

    void Start()
    {
        if (enemyStat == null && enemyData != null)
        {
            enemyStat = new EnemyStat(enemyData);
        }

        RegisterHealthBar(transform, enemyStat.maxHealth);
        //instance new material to avoid changing original
        _renderer.material = new Material(_renderer.material);
    }


 



    public void OnDeath()
    {
        if (_healthBar != null)
            UnregisterHealthBar(_healthBar);
        OnDie?.Invoke(enemyData.experiencePoints);
        ObjectSpawner.Instance.SpawnCoin(transform.position + Vector3.up);
        StartCoroutine(DestroyAfterDelay(5f));
    }
    public void TakeDamage(DamageResult result, Vector3 hitPoint)
    {
        stateMachine.TakeDamage(result.FinalDamage);
        // VFXPoolManager.Instance.GetEffect(VisualEffectID.Hit_1).transform.position = hitPoint;
        UIDamageTextManager.Instance.ShowDamageText(transform.position, result.FinalDamage, result.IsCrit ? TextType.Critical : (result.IsMiss ? TextType.Miss : TextType.Normal));

    }
    public void PlayDissolve()
    {
        StartCoroutine(DissolveEffect());
    }

    private IEnumerator DissolveEffect()
    {
        float dissolveDuration = 2f;
        float elapsed = 0f;
        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float dissolveAmount = Mathf.Clamp01(elapsed / dissolveDuration);
            _renderer.material.SetFloat("_DissolveAmount", dissolveAmount);
            yield return null;
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public EnemyStat GetEnemyStat()
    {
        return enemyStat;
    }

    private void OnDrawGizmosSelected()
    {
        if (enemyData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
        }
    }

    public void RegisterHealthBar(Transform target, float maxHealth)
    {
        _healthBar = UIHealthBarController.Instance.RegisterHealthBar(target, maxHealth);
    }

    public void SetHealth(float health, float maxHealth)
    {
        if (_healthBar != null)
            _healthBar.SetHealth(health, maxHealth);
    }

    public void UnregisterHealthBar(UIHealthBar healthBar)
    {
        UIHealthBarController.Instance.UnregisterHealthBar(healthBar);
    }
}

// ...existing code...
public class EnemyStat
{
    public float maxHealth;
    public float currentHealth;
    public float attackDamage;
    public float moveSpeed;
    public float attackRange;
    public float attackRate;

    public EnemyStat(EnemySO enemyData)
    {
        maxHealth = enemyData.maxHealth;
        currentHealth = maxHealth;
        attackDamage = enemyData.meleeDamage;
        moveSpeed = enemyData.moveSpeed;
        attackRange = enemyData.attackRange;
        attackRate = enemyData.attackSpeed;
    }
}