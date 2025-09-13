using UnityEngine;

public class Dummy : EnemyBase
{
    public override void Initialize(EnemySO data, PlayerStats playerStats_)
    {
        base.Initialize(data, playerStats_);
    }
    public override void OnDeath()
    {
    }
    public override void TakeDamage(DamageResult result, Vector3 hitPoint)
    {
        Debug.Log("Dummy took damage: " + result.FinalDamage);
        base.TakeDamage(result, hitPoint);
        animator.SetTrigger("Hit");
    }
}   