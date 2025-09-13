using System.Collections;
using UnityEngine;
using Maskborn;
using System.Collections.Generic;

public abstract class EnemyBase : MonoBehaviour, IHealthBar
{
    public EnemySO enemyData;
    protected EnemyStat enemyStat;
    public PlayerStats playerStats; // track player
    private UIHealthBar _healthBar;
    [SerializeField] private Renderer _renderer;
    public static event System.Action<int> OnDie; // experience points


    [Header("State Machine")]
    private Dictionary<EnemyStateID, EnemyState> states = new Dictionary<EnemyStateID, EnemyState>();
    protected EnemyState currentState;
    public Animator animator;

    public bool IsDead => enemyStat?.currentHealth <= 0;

    public virtual void Initialize(EnemySO data, PlayerStats playerStats_)
    {
        enemyData = data;
        playerStats = playerStats_;
        enemyStat = new EnemyStat(enemyData);

    }
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        if (enemyStat == null && enemyData != null)
        {
            enemyStat = new EnemyStat(enemyData);
        }
        _healthBar = UIHealthBarController.Instance.RegisterHealthBar(transform, enemyStat.maxHealth);
        if (_renderer != null)
            _renderer.material = new Material(_renderer.material);
    }
    public void AddState(EnemyState state)
    {
        states[state.GetID()] = state;
    }

    public void ChangeState(EnemyStateID newStateID)
    {
        if (currentState != null)
            currentState.Exit(this);

        if (states.TryGetValue(newStateID, out EnemyState newState))
        {
            currentState = newState;
            currentState.Enter(this);
        }
        else
        {
            Debug.LogWarning($"State {newStateID} not found in the state machine.");
        }

    }

    private void Update()
    {
        if (currentState != null)
            currentState.Update(this);
    }

    public abstract void OnDeath();

    public virtual void TakeDamage(DamageResult result, Vector3 hitPoint)
    {
        enemyStat.currentHealth -= result.FinalDamage;
        UIDamageTextManager.Instance.ShowDamageText(
            transform.position,
            result.FinalDamage,
            result.IsCrit ? TextType.Critical :
            (result.IsMiss ? TextType.Miss : TextType.Normal)
        );
        SetHealth(enemyStat.currentHealth, enemyStat.maxHealth);
    }

    public virtual void Attack() //use in animation event
    {
        playerStats.TakeDamage(enemyStat.attackDamage);
    }
    public void PlayeAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    protected void PlayDissolve()
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
            if (_renderer != null)
                _renderer.material.SetFloat("_DissolveAmount", dissolveAmount);
            yield return null;
        }
    }

    protected IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    public EnemyStat GetEnemyStat() => enemyStat;

    private void OnDrawGizmosSelected()
    {
        if (enemyData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
        }
    }



    public void SetHealth(float health, float maxHealth)
    {
        if (_healthBar != null)
            _healthBar.SetHealth(health, maxHealth);
    }

    public bool IsInAttackRange()
    {
        float distance = Vector3.Distance(playerStats.transform.position, transform.position);
        return distance <= enemyStat.attackRange;
    }



    void IHealthBar.RegisterHealthBar(Transform target, float maxHealth)
    {
        _healthBar = UIHealthBarController.Instance.RegisterHealthBar(target, maxHealth);
    }

    void IHealthBar.UnregisterHealthBar(UIHealthBar healthBar)
    {
        UIHealthBarController.Instance.UnregisterHealthBar(healthBar);
    }
}

// --- Stats data class ---
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
public enum EnemyStateID
{
    Idle,
    Move,
    Attack,
    Hitted,
    Die
}

