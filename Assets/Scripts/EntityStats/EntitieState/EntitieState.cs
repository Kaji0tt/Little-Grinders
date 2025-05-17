using UnityEngine;

/// <summary>
/// Der Vorteil einer individuellen Klassenstruktur gegenüber Enum-States ist die Flexible Anpassungsfähigkeit.
/// Bei individuellen Abläufen, Animationen, States oder Bewegungen, können diese stets als eigene Klasse geerbt von Entitie State hinzugefügt werden.
/// </summary>

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

/// <summary>
/// Die einzelnen Animationen, welche zu bestimmten Entities gehören. 
/// In der Enemy State Logic befinden sich alle Animations States die eine feindliche Entitie üblicherweise braucht.
/// </summary>

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
        //controller.myTarget = controller.Player;

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
    public AttackState(EnemyController controller) : base(controller) { }

    //private float attackCooldown;
    private float timer;



    public override void Enter()
    {
        //attackCooldown = 
        controller.StopMoving();
        controller.PerformAttack();
        timer = controller.mobStats.attackCD;
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
            {
                controller.TransitionTo(new AttackState(controller));
            }
            else
            {
                Debug.Log("Ouh! Player out of range, lets Chase!");
                controller.TransitionTo(new ChaseState(controller));
            }
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