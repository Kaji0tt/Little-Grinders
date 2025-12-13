using UnityEngine;

/// <summary>
/// Schießt ein gezieltes Projektil auf den Spieler.
/// Verursacht doppelten AD-Schaden und verlangsamt den Spieler für 3 Sekunden (95% Slow → 0% über Zeit).
/// </summary>
public class AimedShotAbility : AbilityBehavior
{
    [Header("Aimed Shot Settings")]
    [Tooltip("Projektil-Prefab (muss _projectile Component haben)")]
    public GameObject projectilePrefab;
    
    [Tooltip("Geschwindigkeit des Projektils")]
    public float projectileSpeed = 20f;
    
    [Tooltip("Schaden-Multiplikator basierend auf AttackPower (AD)")]
    public float damageMultiplier = 2.0f;
    
    [Tooltip("Spawn-Offset vom Gegner (relativ zur Position)")]
    public Vector3 spawnOffset = new Vector3(0, 1f, 0);

    [Header("Slow Effect Settings")]
    [Tooltip("Slow-Debuff ScriptableObject (enthält alle Konfigurationen: Slow%, Dauer, Tick-Intervall)")]
    public SlowDebuff slowDebuff;

    private void Reset()
    {
        // Default-Werte für Aimed Shot
        cooldownTime = 8f;
        castTime = 1.2f; // 1.2 Sekunden Zielen
        range = 15f;
        priority = 75; // Höhere Priorität als normale Angriffe
        
        // Animation Settings
        animationType = AbilityAnimationType.Attack1; // Nutzt Attack1-Animation für "Zielen"
        executionTiming = 0.65f; // Projektil wird bei 65% der Animation abgefeuert
    }

    /// <summary>
    /// Führt den Aimed Shot aus - spawnt Projektil mit Slow-Debuff
    /// </summary>
    protected override void ExecuteAbility()
    {
        if (controller == null || controller.Player == null)
        {
            Debug.LogWarning("[AimedShotAbility] Controller oder Player ist null!");
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("[AimedShotAbility] Kein Projektil-Prefab zugewiesen!");
            return;
        }

        // Spawn-Position berechnen
        Vector3 spawnPosition = controller.transform.position + spawnOffset;

        // Richtung zum Spieler (aktuelle Position für präzisen Aimed Shot)
        Vector3 targetPosition = controller.Player.position + Vector3.up; // Leicht erhöht für besseren Treffer
        Vector3 direction = (targetPosition - spawnPosition).normalized;

        // Projektil spawnen
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        
        // Projektil konfigurieren
        _projectile projectile = projectileObj.GetComponent<_projectile>();
        if (projectile != null)
        {
            // Setze Schaden basierend auf AttackPower (AD) * 2
            float damage = controller.mobStats.AttackPower.Value * damageMultiplier;
            projectile._pDamage = damage;
            projectile._pSpeed = projectileSpeed;
            projectile.SetOrigin(controller);
            
            // Setze Flugbahn auf Direction und explizite Richtung
            projectile.SetTrajectory(_projectile.Trajectory.Direction);
            projectile.SetDirection(direction);
            
            // Konfiguriere Slow-Buff für Projektil
            if (slowDebuff != null)
            {
                projectile.buff = slowDebuff;
                projectile._pSpecialEffect = true;
                
                Debug.Log($"[AimedShotAbility] {controller.name} feuert Aimed Shot mit {damage} Schaden und '{slowDebuff.buffName}' Debuff!");
            }
            else
            {
                Debug.LogWarning("[AimedShotAbility] Kein Slow-Buff zugewiesen! Erstelle SlowDebuff ScriptableObject im Inspector.");
                projectile._pSpecialEffect = false;
            }
        }
        else
        {
            Debug.LogWarning("[AimedShotAbility] Projektil-Prefab hat keine _projectile Komponente!");
            Destroy(projectileObj);
        }

        // Sound abspielen
        if (AudioManager.instance != null)
        {
            string soundName = controller.GetBasePrefabName() + "_AimedShot";
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
        Gizmos.color = new Color(1, 1, 0, 0.3f); // Gelb
        Gizmos.DrawWireSphere(controller.transform.position, range);
        
        // Zeichne Spawn-Position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(controller.transform.position + spawnOffset, 0.2f);
        
        // Zeichne Schusslinie zum Spieler (wenn vorhanden)
        if (controller.Player != null)
        {
            Gizmos.color = Color.red;
            Vector3 spawnPos = controller.transform.position + spawnOffset;
            Gizmos.DrawLine(spawnPos, controller.Player.position);
        }
    }
}
