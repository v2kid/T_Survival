using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DealDamageArea : MonoBehaviour, IAreaEffect
{
    [Header("Damage Settings")]
    public LayerMask enemyLayer;
    private Collider hitboxCollider;

    private float timer = 0f;
    private float activationDelay;
    private bool isActive = false;
    private bool isEffectRunning = true;
    private float damageInterval;
    private float existingDuration;

    public bool IsRunning => isEffectRunning;

    public void Initialize(AreaEffectConfig config)
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        activationDelay = config.activationDelay;
        damageInterval = config.interval;
        existingDuration = config.duration;
        isActive = false;
        isEffectRunning = true;
        timer = 0f;
    }

    public void RestartEffect(AreaEffectConfig config)
    {
        Initialize(config);
    }

    public void Stop()
    {
        isEffectRunning = false;
        isActive = false;
    }

    private void Update()
    {
        if (!isEffectRunning) return;

        if (!isActive)
        {
            activationDelay -= Time.deltaTime;
            if (activationDelay <= 0f)
            {
                isActive = true;
            }
            return;
        }

        timer += Time.deltaTime;
        existingDuration -= Time.deltaTime;

        if (timer >= damageInterval)
        {
            timer = 0f;
            DealAreaDamage();
        }

        if (existingDuration <= 0f)
        {
            Stop();
        }
    }

    private void DealAreaDamage()
    {
        Vector3 center = hitboxCollider.bounds.center;
        float radius = GetEffectiveRadius();

        Collider[] hitTargets = Physics.OverlapSphere(center, radius, enemyLayer);

        foreach (var targetCollider in hitTargets)
        {
            if (targetCollider.TryGetComponent<IDamageable>(out var damageable))
            {
                DamageResult damageResult = new DamageResult { FinalDamage = 10 };
                Vector3 hitPoint = targetCollider.ClosestPoint(center);
                damageable.TakeDamage(damageResult, hitPoint);
            }
        }
    }

    private float GetEffectiveRadius()
    {
        return Mathf.Max(hitboxCollider.bounds.extents.x,
                        hitboxCollider.bounds.extents.y,
                        hitboxCollider.bounds.extents.z);
    }
}