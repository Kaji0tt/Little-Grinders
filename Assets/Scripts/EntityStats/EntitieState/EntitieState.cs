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
    private readonly float myWaitBetweenWandersMin = 2f;
    private readonly float myWaitBetweenWandersMax = 5f;


    public override void Enter()
    {
        //Debug.Log("Idle Enter is called");
        controller.mobStats.isRegenerating = true;

        controller.StopMoving();

        if (controller.myNavMeshAgent == null)
            controller.AddEssentialComponents();

        // Animation wird automatisch durch IsometricRenderer Movement-Detection gesteuert
        // GameEvents.Instance?.EnemyStartIdle(controller);

        myIdleTimer = 0f;
        myNextWanderTime = Random.Range(myWaitBetweenWandersMin, myWaitBetweenWandersMax);
    }

    public override void Update()
    {
        // Debug: Zeige pulled Status
        if (controller.pulled)
        {
            Debug.Log($"[IdleState] Enemy {controller.name} is pulled, transitioning to Chase!");
            controller.TransitionTo(new ChaseState(controller));
            return;
        }
        
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
            // Animation wird automatisch durch IsometricRenderer Movement-Detection gesteuert
            // GameEvents.Instance?.EnemyStartIdle(controller);
        }
    }

    private void TryWander()
    {
        // Sound nur mit 1/10 Wahrscheinlichkeit abspielen
        if (AudioManager.instance != null && Random.value < 0.1f)
        {
            string soundName = controller.GetBasePrefabName() + "_Wander";
            AudioManager.instance.PlayEntitySound(soundName, controller.gameObject);
        }

        Vector3 randomDirection = Random.insideUnitSphere * myWanderRadius;
        randomDirection += mySpawnPoint;
        randomDirection.y = controller.transform.position.y;

        //Debug.Log("You got here and the next direction should be:" + randomDirection);

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, myWanderRadius, NavMesh.AllAreas))
        {
            controller.myNavMeshAgent.SetDestination(hit.position);
            // Animation wird automatisch durch IsometricRenderer Movement-Detection gesteuert
            // GameEvents.Instance?.EnemyStartWalk(controller);
        }
    }

    public override void Exit()
    {
        //Debug.Log("Idle exit was called");
        controller.mobStats.isRegenerating = false;
    }
}

public class ChaseState : EntitieState
{
    public ChaseState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        // Animation wird automatisch durch IsometricRenderer Movement-Detection gesteuert
        // GameEvents.Instance?.EnemyStartWalk(controller);
    }

    public override void Update()
    {
        //float distance = Vector3.Distance(controller.Player.position, controller.transform.position);

        if (controller.IsPlayerInAttackRange())
        {
            controller.TransitionTo(new AttackState(controller));
            return;
        }

        // Nur verfolgen, wenn nicht in Angriffsreichweite
        controller.MoveToPlayer();

        // Wenn Ziel wieder außerhalb der Aggro-Reichweite ist UND nicht gepullt
        if (controller.TargetDistance() > controller.aggroRange && !controller.pulled)
        {
            // Animation wird automatisch durch IsometricRenderer Movement-Detection gesteuert
            // GameEvents.Instance?.EnemyStartIdle(controller);
            controller.StopMoving();
            controller.TransitionTo(new IdleState(controller));
        }
    }
}

public class AttackState : EntitieState
{
    public AttackState(EnemyController controller) : base(controller) { }

    //private float attackCooldown;
    private float attackTime;



    public override void Enter()
    {
        //Blickrichtung zum Spieler vor Angriff
        controller.myIsoRenderer.SetFacingDirection();

        //Setze das Angriffsverhalten
        controller.attackBehavior?.Enter(controller);
        
        // Berechne gewünschte Angriffsdauer basierend auf AttackSpeed
        attackTime = 1f / controller.mobStats.AttackSpeed.Value;
    }

    public override void Update()
    {
        // Hier wird das Angriffsverhalten aktualisiert
        //controller.myIsoRenderer.ToggleActionState(true);
        //controller.myIsoRenderer.Play(AnimationState.Attack);
        //controller.myIsoRenderer.UpdateAnimation();
        controller.attackBehavior?.OnUpdateAttack(controller);

        // NEU: Sofort prüfen, ob Spieler noch in Reichweite ist
        if (!controller.IsPlayerInAttackRange())
        {
            controller.TransitionTo(new IdleState(controller));
            return;
        }

        attackTime -= Time.deltaTime;

        if (controller.isDead)
        {
            controller.TransitionTo(null);
            return;
        }

        // Wenn die Angriffsanimation vorbei ist, zurück zu Idle oder erneut angreifen
        // Wird derzeit ausgeklammert, da dies über AttackBehavior gesteuert wird
        /*
        if (attackTime <= 0f)
        {
            // Wieder angreifen oder zurück zu Chase
            if (controller.IsPlayerInAttackRange())
            {
                // Debug.Log("Create new Attack from Update of AttackState");
                controller.TransitionTo(new AttackState(controller));
            }
            else
            {
                controller.TransitionTo(new IdleState(controller));
            }
        }
        */
    }
}

public class HitState : EntitieState
{
    private float hitTimer;

    public HitState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        //controller.myIsoRenderer.ToggleActionState(true);
        GameEvents.Instance?.EnemyStartHit(controller);

        if (AudioManager.instance != null)
        {
            string soundName = controller.GetBasePrefabName() + "_Hitted";
            AudioManager.instance.PlayEntitySound(soundName, controller.gameObject);
        }

        // Dynamisch: echte Animationslänge holen
        hitTimer = controller.myIsoRenderer.GetCurrentAnimationLength();
    }

    public override void Update()
    {
        hitTimer -= Time.deltaTime;

        if (hitTimer <= 0f)
        {
            //controller.myIsoRenderer.ToggleActionState(false);

            if (controller.IsPlayerInAttackRange())
            {
                //Debug.Log("Ich bin hier, im Update von HitState.");
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

public class DeadState : EntitieState
{
    public DeadState(EnemyController controller) : base(controller) { }

    public override void Enter()
    {
        controller.StopMoving();
        controller.hpBar.SetActive(false);
        controller.myIsoRenderer.Play(AnimationState.Die);

        if (AudioManager.instance != null)
        {
            string soundName = controller.GetBasePrefabName() + "_Die";
            AudioManager.instance.PlayEntitySound(soundName, controller.gameObject);
        }

            // XP Orbs spawnen
        int xpToGive = controller.mobStats.Get_xp();
        if (xpToGive > 0)
        {
            XP_OrbSpawner.SpawnXPOrbs(controller.transform.position, xpToGive);
        }

        controller.mobStats.Die();
    }
}
#endregion