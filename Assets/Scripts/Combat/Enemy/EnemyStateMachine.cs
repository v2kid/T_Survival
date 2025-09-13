using System.Collections;
using UnityEngine;

public enum EnemyState
{
    Chasing,
    Attacking,
    Dead
}

public class EnemyStateMachine : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public EnemyBase enemyBase;

    [Header("State Machine")]
    public EnemyState currentState = EnemyState.Chasing;

    [Header("Attack Settings")]
    public float attackCooldown = 1f;
    public float attackDuration = 1f; // Maximum time for an attack
    public bool canAttack = true;

    private Transform player;
    private PlayerStats playerStats;
    private EnemyStat enemyStat;
    private float lastAttackTime;
    private bool isPerformingAttack = false;
    private float attackRange;
    private float attackStartTime; // Track when attack started
    private Coroutine currentAttackCoroutine;

    // Animation parameter names
    private readonly string ANIM_SPEED = "Speed"; //float
    private readonly string ANIM_ATTACK = "Attack"; //trigger
    private readonly string ANIM_DEAD = "Dead"; //trigger

    public void Initialize(PlayerStats playerStats_)
    {
        playerStats = playerStats_;
        player = playerStats_.transform;
        if (enemyBase != null)
        {
            enemyStat = enemyBase.GetEnemyStat();
            attackRange = enemyStat.attackRange;
        }
        ChangeState(EnemyState.Chasing);
    }

    void Update()
    {
        if (player == null || enemyStat == null)
        {
            Debug.LogWarning("Player or EnemyStat is null in EnemyStateMachine.");
            return;
        }
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (isPerformingAttack && Time.time - attackStartTime > attackDuration)
        {
            Debug.LogWarning("Attack timed out, forcing completion");
            ForceAttackComplete();
        }

        EvaluateStateTransitions(distanceToPlayer);
        ExecuteCurrentState(distanceToPlayer);
    }

    #region State Evaluation

    void EvaluateStateTransitions(float distanceToPlayer)
    {
        switch (currentState)
        {
            case EnemyState.Chasing:
                // Transition to attacking if in range and can attack
                if (distanceToPlayer <= attackRange && canAttack && !isPerformingAttack)
                {
                    ChangeState(EnemyState.Attacking);
                }
                break;

            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange && !isPerformingAttack)
                {
                    ChangeState(EnemyState.Chasing);
                }
                // Transition to chasing if can't attack and not performing attack
                else if (!canAttack && !isPerformingAttack)
                {
                    ChangeState(EnemyState.Chasing);
                }
                break;

            case EnemyState.Dead:
               
                break;
        }
    }

    void ExecuteCurrentState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case EnemyState.Chasing:
                ExecuteChasingBehavior(distanceToPlayer);
                break;

            case EnemyState.Attacking:
                ExecuteAttackingBehavior(distanceToPlayer);
                break;

            case EnemyState.Dead:
                // Dead state - do nothing
                break;
        }
    }

    #endregion

    #region State Behaviors

    void ExecuteChasingBehavior(float distanceToPlayer)
    {
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += direction * enemyStat.moveSpeed * Time.deltaTime;

        // Look at player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    void ExecuteAttackingBehavior(float distanceToPlayer)
    {
        // Look at player during attack
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        // Start attack if conditions are met
        if (canAttack && !isPerformingAttack && Time.time - lastAttackTime >= attackCooldown)
        {
            StartAttack();
        }
    }

    #endregion

    #region State Management

    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        // Exit current state
        ExitState(currentState);

        // Change state
        EnemyState previousState = currentState;
        currentState = newState;

        // Enter new state
        EnterState(newState);

        Debug.Log($"Enemy state changed from {previousState} to {newState}");
    }

    void EnterState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Chasing:
                SetAnimationSpeed(1f);
                break;

            case EnemyState.Attacking:
                SetAnimationSpeed(0f);
                break;

            case EnemyState.Dead:
                SetAnimationSpeed(0f);
                if (animator != null)
                    animator.SetTrigger(ANIM_DEAD);
                // Stop any ongoing attack
                if (currentAttackCoroutine != null)
                {
                    StopCoroutine(currentAttackCoroutine);
                    currentAttackCoroutine = null;
                }
                break;
        }
    }

    void ExitState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Attacking:
                break;
        }
    }

    #endregion

    #region Attack System

    void StartAttack()
    {
        isPerformingAttack = true;
        canAttack = false;
        lastAttackTime = Time.time;
        attackStartTime = Time.time;

        if (animator != null)
        {
            animator.SetTrigger(ANIM_ATTACK);
        }

        // Start attack timeout coroutine as backup
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
        }
        currentAttackCoroutine = StartCoroutine(AttackTimeoutCoroutine());
    }

    // Called by Animation Event
    public void OnAttackHit()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange && playerStats != null)
        {
            playerStats.TakeDamage(enemyStat.attackDamage);
        }
    }

    // Called by Animation Event
    public void OnAttackComplete()
    {
        CompleteAttack();
    }

    void ForceAttackComplete()
    {
        CompleteAttack();
    }

    void CompleteAttack()
    {
        isPerformingAttack = false;
        
        // Stop timeout coroutine
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }

        // Start cooldown
        StartCoroutine(AttackCooldownRoutine());
    }

    IEnumerator AttackTimeoutCoroutine()
    {
        yield return new WaitForSeconds(attackDuration);
        if (isPerformingAttack)
        {
            Debug.LogWarning("Attack animation didn't complete in time, forcing completion");
            ForceAttackComplete();
        }
    }

    IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    #endregion

    #region Animation Helpers

    void SetAnimationSpeed(float speed)
    {
        if (animator != null)
            animator.SetFloat(ANIM_SPEED, speed);
    }

    #endregion

    #region Public API

    public void TakeDamage(float damage)
    {
        if (currentState == EnemyState.Dead) return;

        enemyStat.currentHealth -= damage;

        // Update health bar
        if (enemyBase != null)
            enemyBase.SetHealth(enemyStat.currentHealth, enemyStat.maxHealth);

        if (enemyStat.currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        ChangeState(EnemyState.Dead);
        enabled = false;
        if (enemyBase != null)
        {
            enemyBase.OnDeath();
        }
    }

    public void SetTarget(Transform newTarget)
    {
        player = newTarget;
        if (newTarget != null)
        {
            playerStats = newTarget.GetComponent<PlayerStats>();
        }
    }

    public EnemyState GetCurrentState()
    {
        return currentState;
    }

    public bool IsInAttackRange()
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= attackRange;
    }

    public void ForceAttack()
    {
        if (currentState == EnemyState.Dead || isPerformingAttack) return;

        if (IsInAttackRange())
        {
            StartAttack();
        }
    }

    #endregion

    #region Debug

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    #endregion
}