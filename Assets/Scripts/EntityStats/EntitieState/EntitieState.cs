using UnityEngine;

public abstract class EntitieState
{ 
    protected EnemyController controller;

    public EntitieState(EnemyController controller)
    {
        this.controller = controller;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}


//Enemy State Logic
#region Enemy State Logic
public class IdleState : EntitieState
{
    public IdleState(EnemyController controller) : base(controller) { }

    public override void Update()
    {
        float distance = Vector3.Distance(controller.Player.position, controller.transform.position);

        if (distance < controller.aggroRange || controller.mobStats.pulled)
        {
            controller.myTarget = controller.Player;
            controller.TransitionTo(new ChaseState(controller));
        }

        if(controller != null)
        {
            //Debug.Log("Controller of #" + controller.name + " is not null!");

            if(controller.myIsoRenderer != null)
            {
                //Debug.Log("IsoRenderer of #" + controller.name + " is not null!");
                controller.myIsoRenderer.Play(AnimationState.Idle);
            }
        }

    }
}

public class ChaseState : EntitieState
{
    public ChaseState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        controller.myIsoRenderer.Play(AnimationState.Walk);
    }

    public override void Update()
    {
        if (controller.myTarget == null)
        {
            controller.TransitionTo(new IdleState(controller));
            return;
        }

        controller.MoveToTarget();

        if (controller.IsInAttackRange())
        {
            controller.TransitionTo(new AttackState(controller));
        }

    }
}

public class AttackState : EntitieState
{
    private float attackCooldown = 1.2f;
    private float timer;

    public AttackState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        controller.StopMoving();
        controller.PerformAttack();
        timer = attackCooldown;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;

        if (controller.isDead)
        {
            controller.TransitionTo(null);
            return;
        }

        if (timer <= 0f)
        {
            // Wieder angreifen oder zurück zu Chase
            if (controller.IsInAttackRange())
                controller.TransitionTo(new AttackState(controller));
            else
                controller.TransitionTo(new ChaseState(controller));
        }
    }
}

public class DeadState : EntitieState
{
    public DeadState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        controller.myIsoRenderer.Play(AnimationState.Die);
        controller.myNavMeshAgent.isStopped = true;
        controller.enabled = false; // Stoppe weitere Logik
    }
}
#endregion