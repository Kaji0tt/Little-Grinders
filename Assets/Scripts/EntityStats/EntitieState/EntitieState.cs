using UnityEngine;
using UnityEngine.AI;

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
    public IdleState(EnemyController controller) : base(controller) 
    {
        mySpawnPoint = controller.transform.position;

    }

    // 👇 Lokale Parameter nur für das Wandern
    private Vector3 mySpawnPoint;
    private float myIdleTimer = 0f;
    private float myNextWanderTime = 3f;
    private readonly float myWanderRadius = 2f;
    private readonly float myWaitBetweenWandersMin = 1f;
    private readonly float myWaitBetweenWandersMax = 4f;


    public override void Enter()
    {
        float distance = Vector3.Distance(controller.Player.position, controller.transform.position);

        controller.StopMoving();

        if (controller.myNavMeshAgent == null)
            controller.AddEssentialComponents();

        if (controller.myIsoRenderer != null)
            controller.myIsoRenderer.Play(AnimationState.Idle);

        myIdleTimer = 0f;
        myNextWanderTime = Random.Range(myWaitBetweenWandersMin, myWaitBetweenWandersMax);
    }

    public override void Update()
    {

        // Falls Spieler zu nahe ist -> Zustandswechsel
        if (Vector3.Distance(controller.transform.position, controller.Player.position) <= controller.aggroRange)
        {
            controller.TransitionTo(new ChaseState(controller)); // musst du ggf. anpassen
            return;
        }

        HandleWander();

    }

    public void HandleWander()
    {
        myIdleTimer += Time.deltaTime;

        if (myIdleTimer >= myNextWanderTime)
        {
            TryWander();

            myIdleTimer = 0f;
            myNextWanderTime = Random.Range(myWaitBetweenWandersMin, myWaitBetweenWandersMax);
        }

        // Wenn am Ziel angekommen → zurück zu Idle
        if (!controller.myNavMeshAgent.pathPending &&
            controller.myNavMeshAgent.remainingDistance <= controller.myNavMeshAgent.stoppingDistance)
        {
            controller.StopMoving();
            if (controller.myIsoRenderer != null)
                controller.myIsoRenderer.Play(AnimationState.Idle);
        }
    }

    private void TryWander()
    {

        Vector3 randomDirection = Random.insideUnitSphere * myWanderRadius;
        randomDirection += mySpawnPoint;
        randomDirection.y = controller.transform.position.y;

        Debug.Log("You got here and the next direction should be:" + randomDirection);

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, myWanderRadius, NavMesh.AllAreas))
        {
            controller.myNavMeshAgent.SetDestination(hit.position);

            if (controller.myIsoRenderer != null)
                controller.myIsoRenderer.Play(AnimationState.Walk);

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

        float distance = Vector3.Distance(controller.Player.position, controller.transform.position);

        controller.MoveToTarget();

        if (controller.IsInAttackRange())
        {
            controller.TransitionTo(new AttackState(controller));
        }

        if (distance > controller.aggroRange)
        {

            controller.myIsoRenderer.Play(AnimationState.Idle);

            controller.StopMoving();

            return;
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
                //Debug.Log("Ouh! Player out of range, lets Chase!");
                controller.TransitionTo(new ChaseState(controller));
            }
        }
    }
}

public class HitState : EntitieState
{
    public HitState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        controller.myIsoRenderer.Play(AnimationState.Hit);
    }

    public override void Update()
    {



    }
}

public class DeadState : EntitieState
{
    public DeadState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        controller.myIsoRenderer.Play(AnimationState.Die);
        controller.StopMoving();
        //controller.enabled = false; // Stoppe weitere Logik
    }
}
#endregion