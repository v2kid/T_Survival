using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DealDamageArea : MonoBehaviour
{


    [Header("Damage Settings")]
    public LayerMask enemyLayer;
    public float damage = 10f;
    public float damageInterval = 0.4f;

    [Header("Timing Settings")]
    public float activationDelay = 0.2f;
    private float duration = 1.2f;

    private Collider hitboxCollider;
    private float damageTimer;
    private float lifetimeTimer;
    private bool isActive;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
    }

    private void Update()
    {
        lifetimeTimer += Time.deltaTime;

        // Handle activation delay
        if (!isActive && lifetimeTimer >= activationDelay)
        {
            isActive = true;
        }

        // Handle damage dealing
        if (isActive)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                DealAreaDamage();
                damageTimer = 0f;
            }
        }

        // Handle lifetime expiration
        if (lifetimeTimer >= duration)
        {
            DeactivateHitbox();
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
                DamageResult damageResult = new DamageResult { FinalDamage = damage };
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

    private void DeactivateHitbox()
    {
        isActive = false;
    }


}