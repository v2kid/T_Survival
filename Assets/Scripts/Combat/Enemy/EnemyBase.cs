using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public abstract class EnemyBase : MonoBehaviour, IHealthBar
{
    public EnemySO enemyData;
    protected EnemyStat enemyStat;
    public PlayerStats playerStats; // track player
    private UIHealthBar _healthBar;
    [SerializeField] protected Renderer _renderer;
    public event System.Action OnDie; // experience points


    [Header("State Machine")]
    private Dictionary<EnemyStateID, EnemyState> states = new Dictionary<EnemyStateID, EnemyState>();
    private List<GlobalTransition> globalTransitions = new List<GlobalTransition>();
    protected EnemyState currentState;
    public Animator animator;

    public bool IsDead => enemyStat?.currentHealth <= 0;

    public virtual void Initialize(EnemySO data, PlayerStats playerStats_)
    {
        enemyData = data;
        playerStats = playerStats_;
        enemyStat = new EnemyStat(enemyData);
        AddGlobalTransition(() => IsDead, EnemyStateID.Die);

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
    public void AddGlobalTransition(System.Func<bool> condition, EnemyStateID targetState)
    {
        globalTransitions.Add(new GlobalTransition(condition, targetState));
    }
    public void ChangeState(EnemyStateID newStateID)
    {
        if (currentState != null && currentState.GetID() == newStateID)
            return;

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
        foreach (var transition in globalTransitions)
        {
            if (transition.Condition())
            {
                ChangeState(transition.TargetState);
                return;
            }
        }
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
        var vfx = VFXPoolManager.Instance.GetEffect(VisualEffectID.Blood);
        vfx.transform.position = playerStats.transform.position + Vector3.up * 1.0f;
        vfx.Play();

    }
    public void PlayeAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    public virtual void PlayDieEffect()
    {
        OnDie?.Invoke();
        StartCoroutine(DestroyDelay(2.0f));
    }
    public IEnumerator DestroyDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }


    public EnemyStat GetEnemyStat() => enemyStat;




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

    public void RegisterHealthBar(Transform target, float maxHealth)
    {
        _healthBar = UIHealthBarController.Instance.RegisterHealthBar(target, maxHealth);
    }

    public void UnregisterHealthBar()
    {
        UIHealthBarController.Instance.UnregisterHealthBar(_healthBar);
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

