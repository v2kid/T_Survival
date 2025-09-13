public class DieState : EnemyState
{
    public override EnemyStateID GetID() => EnemyStateID.Die;

    public override void Enter(EnemyBase enemy)
    {
       
    }

    public override void Update(EnemyBase enemy) { }
    public override void Exit(EnemyBase enemy) { }
}