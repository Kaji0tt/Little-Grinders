using UnityEngine;
using System.Collections;

public class PendingAttack : MonoBehaviour, IAttackBehavior
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
    private bool hasTriggeredWalkAnimation = false; // Verhindert mehrfaches Walk-Event triggern

    public void Enter(EnemyController controller)
    {
        // Berechne Angriffs-Cooldown basierend auf AttackSpeed (1 / AttackSpeed = Sekunden pro Angriff)
        attackCooldown = 1f / controller.mobStats.AttackSpeed.Value;
        timer = 0f; // Starte sofort mit Angriff
        isPausing = false;
        hasTriggeredWalkAnimation = false; // Reset Walk-Animation Trigger
        Debug.Log($"[PendingAttack] Enter: Bereit zum Angriff (AttackSpeed: {controller.mobStats.AttackSpeed.Value})");
    }

    public void UpdateAttack(EnemyController controller)
    {
        timer -= Time.deltaTime;

        if (isPausing)
        {
            // Phase 2: Rückzug und Warten
            HandleFallbackBehavior(controller);
        }
        else if (timer <= 0 && !isAttacking)
        {
            // Phase 1: Angriff
            Debug.Log("[PendingAttack] Angriff bereit - gehe zum Spieler");
            
            // Trigger Walk Animation nur einmal
            if (!hasTriggeredWalkAnimation)
            {
                GameEvents.Instance?.EnemyStartWalk(controller);
                hasTriggeredWalkAnimation = true;
            }
            
            controller.MoveToPlayer();

            if (controller.IsPlayerInAttackRange())
            {
                Debug.Log("[PendingAttack] Spieler in Angriffsreichweite, Angriff wird gestartet.");
                StartCoroutine(ExecuteDelayedAttack(controller));
            }
            else
            {
                Debug.Log("[PendingAttack] Bewege zum Spieler...");
            }
        }
    }

    /// <summary>
    /// Handhabt das Rückzugs- und Warteverhalten nach einem Angriff
    /// </summary>
    private void HandleFallbackBehavior(EnemyController controller)
    {
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.Player.position);
        
        // Prüfe ob wir zu nah am Spieler sind
        if (distanceToPlayer < holdDistance)
        {
            // Bewege weg vom Spieler
            Vector3 directionAwayFromPlayer = (controller.transform.position - controller.Player.position).normalized;
            Vector3 targetPosition = controller.Player.position + directionAwayFromPlayer * holdDistance;
            
            // Füge etwas Variation hinzu für ausweichende Bewegungen
            Vector3 sideMovement = Vector3.Cross(directionAwayFromPlayer, Vector3.up) * Random.Range(-1f, 1f);
            targetPosition += sideMovement * 1.5f;
            
            controller.myNavMeshAgent.SetDestination(targetPosition);
            Debug.Log($"[PendingAttack] Weiche aus - Zieldistanz: {holdDistance}m");
        }
        else
        {
            // Halte Position und mache kleine ausweichende Bewegungen
            if (!controller.myNavMeshAgent.pathPending && controller.myNavMeshAgent.remainingDistance < 0.5f)
            {
                Vector3 randomMovement = Random.insideUnitSphere * 1f;
                randomMovement.y = 0;
                Vector3 newPosition = controller.transform.position + randomMovement;
                controller.myNavMeshAgent.SetDestination(newPosition);
            }
        }

        // Prüfe ob Pause-Zeit abgelaufen ist
        if (timer <= 0)
        {
            Debug.Log("[PendingAttack] Pause beendet - bereit für nächsten Angriff");
            isPausing = false;
            hasTriggeredWalkAnimation = false; // Reset für nächsten Angriffszyklus
            timer = 0f; // Sofort zum nächsten Angriff bereit
        }
    }

    /// <summary>
    /// Führt den Angriff mit verzögertem Schaden aus - Schaden erfolgt in der Mitte der Animation
    /// </summary>
    private IEnumerator ExecuteDelayedAttack(EnemyController controller)
    {
        isAttacking = true;
        
        // Berechne die halbe Animationsdauer basierend auf AttackSpeed
        float attackDuration = 1f / controller.mobStats.AttackSpeed.Value;
        float damageDelay = attackDuration * 0.5f; // Schaden bei 50% der Animation
        
        Debug.Log($"[PendingAttack] Angriff gestartet, Schaden in {damageDelay:F2} Sekunden");
        
        // Event: Angriff gestartet - IsometricRenderer reagiert darauf
        GameEvents.Instance?.EnemyStartAttack(controller, attackDuration);
        
        // Warte bis zur Mitte der Animation
        yield return new WaitForSeconds(damageDelay);
        
        // Event: Angriff-Impact - für Sound/Effects
        GameEvents.Instance?.EnemyAttackHit(controller);
        
        // Prüfe erneut, ob Spieler noch in Reichweite ist
        if (controller.IsPlayerInAttackRange())
        {
            Debug.Log("[PendingAttack] Schaden wird ausgeführt (Mitte der Animation)");
            controller.PerformAttack();
        }
        else
        {
            Debug.Log("[PendingAttack] Spieler nicht mehr in Reichweite - kein Schaden");
        }
        
        // Warte bis zum Ende der Animation
        yield return new WaitForSeconds(damageDelay);
        
        // Event: Angriff beendet
        GameEvents.Instance?.EnemyEndAttack(controller);
        
        isAttacking = false;
        
        // Nach dem Angriff: Starte Rückzugs-Phase
        isPausing = true;
        timer = pauseTime;
        
        // Trigger Idle Animation nach dem Angriff
        //GameEvents.Instance?.EnemyStartIdle(controller);
        
        Debug.Log($"[PendingAttack] Angriff beendet - starte Rückzug für {pauseTime} Sekunden");
    }

    public void Exit(EnemyController controller)
    {
        Debug.Log("[PendingAttack] Exit aufgerufen.");
    }

    public bool IsAttackReady(EnemyController controller)
    {
        bool ready = timer <= 0 && controller.IsPlayerInAttackRange() && !isAttacking && !isPausing;
        Debug.Log($"[PendingAttack] IsAttackReady: {ready} (timer: {timer:F2}, inRange: {controller.IsPlayerInAttackRange()}, isAttacking: {isAttacking}, isPausing: {isPausing})");
        return ready;
    }
}