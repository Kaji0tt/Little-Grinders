using UnityEngine;
using System.Collections;

public class SteadyCloseAttack : MonoBehaviour, IAttackBehavior
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

    public void Enter(EnemyController controller)
    {
        // Berechne Angriffs-Cooldown basierend auf AttackSpeed
        attackCooldown = 1f / controller.mobStats.AttackSpeed.Value;
        timer = 0f; // Kein initialer Timer - sofortiger Angriff wenn in Position
        isAttacking = false;
        hasReachedPlayer = false;
        isInPosition = false;
        Debug.Log($"[SteadyCloseAttack] Enter: Bewege zum Spieler (AttackSpeed: {controller.mobStats.AttackSpeed.Value})");
    }

    public void UpdateAttack(EnemyController controller)
    {
        timer -= Time.deltaTime;

        if (!isInPosition)
        {
            // Phase 1: Bewege zum Spieler
            MoveToOptimalPosition(controller);
        }
        else if (!isAttacking && timer <= 0)
        {
            // Phase 2: Angriff ausführen
            if (controller.IsPlayerInAttackRange())
            {
                Debug.Log("[SteadyCloseAttack] Spieler in Reichweite - starte Angriff");
                StartCoroutine(ExecuteDelayedAttack(controller));
            }
            else
            {
                // Spieler ist weggelaufen - verfolge erneut
                Debug.Log("[SteadyCloseAttack] Spieler außerhalb der Reichweite - verfolge erneut");
                isInPosition = false;
                hasReachedPlayer = false;
                timer = 0f; // Reset timer for immediate attack when repositioned
            }
        }
        else if (isInPosition && !isAttacking)
        {
            // Phase 3: In Position bleiben und auf nächsten Angriff warten
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

        // Bewege zum Spieler bis zur optimalen Distanz
        if (distanceToPlayer > optimalDistance)
        {
            controller.MoveToPlayer();
            Debug.Log($"[SteadyCloseAttack] Bewege zum Spieler (Distanz: {distanceToPlayer:F2}m, Ziel: {optimalDistance:F2}m)");
        }
        else
        {
            // Optimale Position erreicht
            controller.StopMoving();
            isInPosition = true;
            hasReachedPlayer = true;
            timer = 0f; // Sofortiger Angriff wenn Position erreicht
            Debug.Log($"[SteadyCloseAttack] Optimale Position erreicht - bereit zum sofortigen Angriff");
        }
    }

    /// <summary>
    /// Hält die Position beim Spieler und macht minimale Korrekturen
    /// </summary>
    private void MaintainPosition(EnemyController controller)
    {
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.Player.position);
        float optimalDistance = controller.attackRange - additionalDistance;
        float toleranceRange = 0.5f; // Toleranzbereich für Position

        // Nur bewegen wenn Spieler zu weit weg ist
        if (distanceToPlayer > optimalDistance + toleranceRange)
        {
            Debug.Log($"[SteadyCloseAttack] Spieler zu weit entfernt ({distanceToPlayer:F2}m) - Position korrigieren");
            controller.MoveToPlayer();
            isInPosition = false; // Muss Position neu finden
        }
        else if (distanceToPlayer < optimalDistance - toleranceRange)
        {
            // Spieler ist zu nah - kleine Rückwärtsbewegung
            Vector3 directionAwayFromPlayer = (controller.transform.position - controller.Player.position).normalized;
            Vector3 targetPosition = controller.Player.position + directionAwayFromPlayer * optimalDistance;
            controller.myNavMeshAgent.SetDestination(targetPosition);
            Debug.Log($"[SteadyCloseAttack] Spieler zu nah ({distanceToPlayer:F2}m) - weiche etwas zurück");
        }
        else
        {
            // Position ist optimal - nicht bewegen
            controller.StopMoving();
        }
    }

    /// <summary>
    /// Führt den Angriff mit verzögertem Schaden aus
    /// </summary>
    private IEnumerator ExecuteDelayedAttack(EnemyController controller)
    {
        isAttacking = true;
        
        // Berechne Animationsdauer basierend auf AttackSpeed
        float attackDuration = 1f / controller.mobStats.AttackSpeed.Value;
        float damageDelay = attackDuration * 0.5f; // Schaden bei 50% der Animation
        
        Debug.Log($"[SteadyCloseAttack] Angriff gestartet, Schaden in {damageDelay:F2} Sekunden");
        
        // Event: Angriff gestartet
        GameEvents.Instance?.EnemyStartAttack(controller, attackDuration);
        
        // Warte bis zur Mitte der Animation
        yield return new WaitForSeconds(damageDelay);
        
        // Event: Angriff-Impact
        GameEvents.Instance?.EnemyAttackHit(controller);
        
        // Schaden ausführen wenn Spieler noch in Reichweite
        if (controller.IsPlayerInAttackRange())
        {
            Debug.Log("[SteadyCloseAttack] Schaden wird ausgeführt (Mitte der Animation)");
            controller.PerformAttack();
        }
        else
        {
            Debug.Log("[SteadyCloseAttack] Spieler nicht mehr in Reichweite - kein Schaden");
        }
        
        // Warte bis zum Ende der Animation
        yield return new WaitForSeconds(damageDelay);
        
        // Event: Angriff beendet
        GameEvents.Instance?.EnemyEndAttack(controller);
        
        isAttacking = false;
        
        // NACH dem Angriff: Timer für Pause bis zum nächsten Angriff setzen
        timer = pauseTime;
        
        Debug.Log($"[SteadyCloseAttack] Angriff beendet - Pause von {pauseTime} Sekunden bis zum nächsten Angriff");
    }

    public void Exit(EnemyController controller)
    {
        Debug.Log("[SteadyCloseAttack] Exit aufgerufen.");
        isInPosition = false;
        hasReachedPlayer = false;
        isAttacking = false;
    }

    public bool IsAttackReady(EnemyController controller)
    {
        bool ready = isInPosition && timer <= 0 && controller.IsPlayerInAttackRange() && !isAttacking;
        Debug.Log($"[SteadyCloseAttack] IsAttackReady: {ready} (isInPosition: {isInPosition}, timer: {timer:F2}, inRange: {controller.IsPlayerInAttackRange()}, isAttacking: {isAttacking})");
        return ready;
    }
}
