using UnityEngine;
using System.Collections;

public class SteadyCloseAttack : AttackBehavior
{
    [Header("Steady Close Attack Behavior")]
    [Tooltip("Zeit in Sekunden zwischen den Angriffen")]
    public float pauseTime = 1.5f;
    [Tooltip("Zusätzliche Distanz zum Spieler - wird zur attackRange hinzugefügt")]
    public float additionalDistance = 0.5f;
    
    private float timer;
    private float attackCooldown;
    private bool isAttacking = false;
    private bool hasReachedPlayer = false;
    private bool isInPosition = false;

    public override void Enter(EnemyController controller)
    {
        base.Enter(controller);
        
        attackCooldown = 1f / controller.mobStats.AttackSpeed.Value;
        timer = 0f;
        isAttacking = false;
        hasReachedPlayer = false;
        isInPosition = false;
    }

    /// <summary>
    /// Überschreibt UpdateAttack - wird von Base-Klasse aufgerufen
    /// FacingDirection wird automatisch in OnUpdateAttack gesteuert
    /// </summary>
    protected override void UpdateAttack()
    {
        timer -= Time.deltaTime;

        if (!isInPosition)
        {
            MoveToOptimalPosition(controller);
        }
        else if (!isAttacking && timer <= 0)
        {
            if (controller.IsPlayerInAttackRange())
            {
                StartCoroutine(ExecuteDelayedAttack(controller));
            }
            else
            {
                isInPosition = false;
                hasReachedPlayer = false;
                timer = 0f;
            }
        }
        else if (isInPosition && !isAttacking)
        {
            MaintainPosition(controller);
        }
    }

    /// <summary>
    /// Bewegt sich zur optimalen Angriffsposition beim Spieler
    /// </summary>
    private void MoveToOptimalPosition(EnemyController controller)
    {
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.Player.position);
        float optimalDistance = controller.attackRange - additionalDistance;

        if (distanceToPlayer > optimalDistance)
        {
            controller.MoveToPlayer();
        }
        else
        {
            controller.StopMoving();
            isInPosition = true;
            hasReachedPlayer = true;
            timer = 0f;
        }
    }

    /// <summary>
    /// Hält die Position beim Spieler und macht minimale Korrektionen
    /// </summary>
    private void MaintainPosition(EnemyController controller)
    {
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.Player.position);
        float optimalDistance = controller.attackRange - additionalDistance;
        float toleranceRange = 0.5f;

        if (distanceToPlayer > optimalDistance + toleranceRange)
        {
            controller.MoveToPlayer();
            isInPosition = false;
        }
        else if (distanceToPlayer < optimalDistance - toleranceRange)
        {
            Vector3 directionAwayFromPlayer = (controller.transform.position - controller.Player.position).normalized;
            Vector3 targetPosition = controller.Player.position + directionAwayFromPlayer * optimalDistance;
            controller.myNavMeshAgent.SetDestination(targetPosition);
        }
        else
        {
            controller.StopMoving();
        }
    }

    /// <summary>
    /// Führt den Angriff mit verzögertem Schaden aus
    /// </summary>
    private IEnumerator ExecuteDelayedAttack(EnemyController controller)
    {
        isAttacking = true;
        
        float attackDuration = 1f / controller.mobStats.AttackSpeed.Value;
        float damageDelay = attackDuration * 0.5f;
        
        GameEvents.Instance?.EnemyStartAttack(controller, attackDuration);
        
        yield return new WaitForSeconds(damageDelay);
        
        GameEvents.Instance?.EnemyAttackHit(controller);
        
        if (controller.IsPlayerInAttackRange())
        {
            controller.PerformAttack();
        }
        
        yield return new WaitForSeconds(damageDelay);
        
        GameEvents.Instance?.EnemyEndAttack(controller);
        
        isAttacking = false;
        
        timer = pauseTime;
    }

    public override void Exit(EnemyController controller)
    {
        isInPosition = false;
        hasReachedPlayer = false;
        isAttacking = false;
    }

    public override bool IsAttackReady(EnemyController controller)
    {
        bool ready = isInPosition && timer <= 0 && controller.IsPlayerInAttackRange() && !isAttacking;
        return ready;
    }
}
