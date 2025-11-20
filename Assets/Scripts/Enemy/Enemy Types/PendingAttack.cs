using UnityEngine;
using System.Collections;

public class PendingAttack : AttackBehavior
{
    [Header("Pending Attack Behavior")]
    [Tooltip("Zeit in Sekunden, die der Gegner nach einem Angriff zurückfällt und wartet")]
    public float pauseTime = 2f;
    [Tooltip("Distanz zum Spieler, auf die der Gegner nach einem Angriff zurückfällt")]
    public float holdDistance = 3f;
    
    private float timer;
    private float attackCooldown;
    private bool isAttacking = false;
    private bool isPausing = false;
    private Vector3 fallbackPosition;
    private bool hasTriggeredWalkAnimation = false;

    public override void Enter(EnemyController controller)
    {
        // Berechne Angriffs-Cooldown basierend auf AttackSpeed (1 / AttackSpeed = Sekunden pro Angriff)
        attackCooldown = 1f / controller.mobStats.AttackSpeed.Value;
        timer = 0f;
        isPausing = false;
        hasTriggeredWalkAnimation = false;
    }

    /// <summary>
    /// Überschreibt UpdateAttack - wird von Base-Klasse aufgerufen
    /// FacingDirection wird automatisch in OnUpdateAttack gesteuert
    /// </summary>
    protected override void UpdateAttack()
    {
        timer -= Time.deltaTime;

        if (isPausing)
        {
            HandleFallbackBehavior(controller);
        }
        else if (timer <= 0 && !isAttacking)
        {
            if (!hasTriggeredWalkAnimation)
            {
                GameEvents.Instance?.EnemyStartWalk(controller);
                hasTriggeredWalkAnimation = true;
            }
            
            controller.MoveToPlayer();

            if (controller.IsPlayerInAttackRange())
            {
                StartCoroutine(ExecuteDelayedAttack(controller));
            }
        }
    }

    /// <summary>
    /// Handhabt das Rückzugs- und Warteverhalten nach einem Angriff
    /// </summary>
    private void HandleFallbackBehavior(EnemyController controller)
    {
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.Player.position);
        
        if (distanceToPlayer < holdDistance)
        {
            Vector3 directionAwayFromPlayer = (controller.transform.position - controller.Player.position).normalized;
            Vector3 targetPosition = controller.Player.position + directionAwayFromPlayer * holdDistance;
            
            Vector3 sideMovement = Vector3.Cross(directionAwayFromPlayer, Vector3.up) * Random.Range(-1f, 1f);
            targetPosition += sideMovement * 1.5f;
            
            controller.myNavMeshAgent.SetDestination(targetPosition);
        }
        else
        {
            if (!controller.myNavMeshAgent.pathPending && controller.myNavMeshAgent.remainingDistance < 0.5f)
            {
                Vector3 randomMovement = Random.insideUnitSphere * 1f;
                randomMovement.y = 0;
                Vector3 newPosition = controller.transform.position + randomMovement;
                controller.myNavMeshAgent.SetDestination(newPosition);
            }
        }

        if (timer <= 0)
        {
            isPausing = false;
            hasTriggeredWalkAnimation = false;
            timer = 0f;
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
        
        isPausing = true;
        timer = pauseTime;
    }

    public override void Exit(EnemyController controller)
    {
        // Cleanup falls nötig
    }

    public override bool IsAttackReady(EnemyController controller)
    {
        bool ready = timer <= 0 && controller.IsPlayerInAttackRange() && !isAttacking && !isPausing;
        return ready;
    }
}
