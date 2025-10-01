
public interface IAttackBehavior
{
    void Enter(EnemyController controller);
    void Update(EnemyController controller);
    void Exit(EnemyController controller);
    bool IsAttackReady(EnemyController controller);
}
