using UnityEngine;

/// <summary>
/// Schießt ein Feuerball-Projektil auf den Spieler.
/// Demonstriert Projektil-Abilities mit Cast-Animation und Execution-Timing.
/// </summary>
public class FireballAbility : AbilityBehavior
{
    [Header("Fireball Settings")]
    [Tooltip("Projektil-Prefab (z.B. Feuerball)")]
    public GameObject projectilePrefab;
    
    [Tooltip("Geschwindigkeit des Projektils")]
    public float projectileSpeed = 15f;
    
    [Tooltip("Schaden-Multiplikator basierend auf AbilityPower")]
    public float damageMultiplier = 2.0f;
    
    [Tooltip("Spawn-Offset vom Gegner (relativ zur Position)")]
    public Vector3 spawnOffset = new Vector3(0, 1f, 0);

    private void Reset()
    {
        // Default-Werte für Fireball
        cooldownTime = 6f;
        castTime = 1.5f; // 1.5 Sekunden Cast-Zeit
        range = 12f;
        priority = 70; // Mittlere Priorität
        
        // Animation Settings
        animationType = AbilityAnimationType.Casting; // Nutzt Casting-Animation
        executionTiming = 0.7f; // Feuerball wird bei 70% der Animation abgefeuert
    }

    /// <summary>
    /// Führt den Fireball-Cast aus
    /// </summary>
    protected override void ExecuteAbility()
    {
        if (controller == null || controller.Player == null)
        {
            Debug.LogWarning("[FireballAbility] Controller oder Player ist null!");
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("[FireballAbility] Kein Projektil-Prefab zugewiesen!");
            return;
        }

        // Spawn-Position berechnen
        Vector3 spawnPosition = controller.transform.position + spawnOffset;

        // Richtung zum Spieler
        Vector3 direction = (controller.Player.position - spawnPosition).normalized;

        // Projektil spawnen
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
        
        // Projektil konfigurieren
        _projectile projectile = projectileObj.GetComponent<_projectile>();
        if (projectile != null)
        {
            // Setze Schaden basierend auf AbilityPower
            float damage = controller.mobStats.AbilityPower.Value * damageMultiplier;
            projectile._pDamage = damage;
            projectile._pSpeed = projectileSpeed;
            projectile.SetOrigin(controller);
            projectile.SetTrajectory(_projectile.Trajectory.Direction);
            
            Debug.Log($"[FireballAbility] {controller.name} feuert Fireball mit {damage} Schaden!");
        }
        else
        {
            Debug.LogWarning("[FireballAbility] Projektil-Prefab hat keine _projectile Komponente!");
        }

        // Sound abspielen
        if (AudioManager.instance != null)
        {
            string soundName = controller.GetBasePrefabName() + "_Fireball";
            AudioManager.instance.PlayEntitySound(soundName, controller.gameObject);
        }
    }

    /// <summary>
    /// Visualisiere Ability-Range im Editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (controller == null)
            return;

        // Zeichne Reichweite
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f); // Orange
        Gizmos.DrawWireSphere(controller.transform.position, range);
        
        // Zeichne Spawn-Position
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(controller.transform.position + spawnOffset, 0.2f);
    }
}
