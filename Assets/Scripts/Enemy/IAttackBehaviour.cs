
public interface IAttackBehavior
{
    void Enter(EnemyController controller);
    void UpdateAttack(EnemyController controller);
    void Exit(EnemyController controller);
    bool IsAttackReady(EnemyController controller);
}
