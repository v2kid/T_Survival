using UnityEngine;

public class Zombie : EnemyBase
{
    public override void Initialize(EnemySO data, PlayerStats playerStats_)
    {
        base.Initialize(data, playerStats_);
        AddState(new MoveState());
        AddState(new AttackState());
        ChangeState(EnemyStateID.Move);
    }

    
    

    public override void OnDeath()
    {
        // Implement zombie-specific death behavior
    }
}