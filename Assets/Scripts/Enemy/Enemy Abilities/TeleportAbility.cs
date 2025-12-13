using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Teleportiert den Gegner zu einer strategischen Position relativ zum Spieler.
/// Instant-Cast (castTime = 0) für schnelle Repositionierung.
/// </summary>
public class TeleportAbility : AbilityBehavior
{
    [Header("Teleport Settings")]
    [Tooltip("Minimale Distanz zum Spieler nach Teleport")]
    public float minDistanceFromPlayer = 3f;
    
    [Tooltip("Maximale Distanz zum Spieler nach Teleport")]
    public float maxDistanceFromPlayer = 8f;
    
    [Tooltip("Versuche einen Teleport hinter den Spieler")]
    public bool preferBehindPlayer = true;
    
    [Tooltip("Spawn-Effekt Prefab (optional)")]
    public GameObject teleportEffectPrefab;

    [Header("Teleport Behavior")]
    [Tooltip("Nur teleportieren wenn Spieler weiter als diese Distanz entfernt ist")]
    public float activateIfPlayerFartherThan = 10f;
    
    [Tooltip("Nur teleportieren wenn Spieler näher als diese Distanz ist")]
    public float activateIfPlayerCloserThan = 2f;

    private void Reset()
    {
        // Default-Werte für Teleport
        cooldownTime = 8f;
        castTime = 0f; // Instant cast
        range = 15f;
        priority = 80; // Höhere Priorität als normale Angriffe
        
        // Animation Settings
        animationType = AbilityAnimationType.Casting; // Nutzt Casting-Animation
        executionTiming = 0.0f; // Sofort am Anfang (instant cast)
    }

    /// <summary>
    /// Prüft ob Teleport bereit ist - erweitert Base-Logik um Distanz-Check
    /// </summary>
    public override bool IsAbilityReady(EnemyController controller)
    {
        if (!base.IsAbilityReady(controller))
            return false;

        // Nur teleportieren wenn strategisch sinnvoll
        float distance = Vector3.Distance(controller.transform.position, controller.Player.position);
        
        // Teleportiere wenn zu weit weg ODER zu nah dran
        bool shouldTeleport = (distance > activateIfPlayerFartherThan) || 
                             (distance < activateIfPlayerCloserThan);

        return shouldTeleport;
    }

    /// <summary>
    /// Führt den Teleport aus
    /// </summary>
    protected override void ExecuteAbility()
    {
        if (controller == null || controller.Player == null)
        {
            Debug.LogWarning("[TeleportAbility] Controller oder Player ist null!");
            return;
        }

        Vector3 teleportPosition = CalculateTeleportPosition();

        // Prüfe ob Position gültig ist (auf NavMesh)
        if (NavMesh.SamplePosition(teleportPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            // Spawn-Effekt an alter Position (optional)
            if (teleportEffectPrefab != null)
            {
                Instantiate(teleportEffectPrefab, controller.transform.position, Quaternion.identity);
            }

            // Teleportiere den Gegner
            controller.transform.position = hit.position;
            
            // NavMeshAgent Position aktualisieren
            if (controller.myNavMeshAgent != null && controller.myNavMeshAgent.isOnNavMesh)
            {
                controller.myNavMeshAgent.Warp(hit.position);
            }

            // Spawn-Effekt an neuer Position (optional)
            if (teleportEffectPrefab != null)
            {
                Instantiate(teleportEffectPrefab, hit.position, Quaternion.identity);
            }

            // Sound abspielen
            if (AudioManager.instance != null)
            {
                string soundName = controller.GetBasePrefabName() + "_Teleport";
                AudioManager.instance.PlayEntitySound(soundName, controller.gameObject);
            }

            Debug.Log($"[TeleportAbility] {controller.name} teleportiert zu {hit.position}");
        }
        else
        {
            Debug.LogWarning($"[TeleportAbility] Keine gültige NavMesh-Position gefunden für {teleportPosition}");
        }
    }

    /// <summary>
    /// Berechnet eine strategische Teleport-Position
    /// </summary>
    private Vector3 CalculateTeleportPosition()
    {
        Vector3 playerPos = controller.Player.position;
        Vector3 randomDirection;

        if (preferBehindPlayer)
        {
            // Versuche hinter den Spieler zu teleportieren
            Vector3 playerForward = controller.Player.forward;
            
            // 180° hinter dem Spieler + zufällige Variation
            float angleVariation = Random.Range(-45f, 45f);
            Quaternion rotation = Quaternion.Euler(0, 180 + angleVariation, 0);
            randomDirection = rotation * playerForward;
        }
        else
        {
            // Zufällige Richtung
            randomDirection = Random.insideUnitSphere;
            randomDirection.y = 0; // Halte Y konstant
            randomDirection.Normalize();
        }

        // Zufällige Distanz innerhalb des Bereichs
        float distance = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
        
        Vector3 targetPosition = playerPos + randomDirection * distance;
        targetPosition.y = controller.transform.position.y; // Behalte Original-Höhe

        return targetPosition;
    }

    /// <summary>
    /// Visualisiere Teleport-Range im Editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (controller == null || controller.Player == null)
            return;

        Vector3 playerPos = controller.Player.position;

        // Zeichne minimale und maximale Teleport-Distanz
        Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan
        Gizmos.DrawWireSphere(playerPos, minDistanceFromPlayer);
        
        Gizmos.color = new Color(0, 0.5f, 1, 0.3f); // Blau
        Gizmos.DrawWireSphere(playerPos, maxDistanceFromPlayer);

        // Zeichne Aktivierungsbereiche
        Gizmos.color = new Color(1, 1, 0, 0.2f); // Gelb
        Gizmos.DrawWireSphere(playerPos, activateIfPlayerCloserThan);
        Gizmos.DrawWireSphere(playerPos, activateIfPlayerFartherThan);
    }
}
