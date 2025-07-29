using UnityEngine;
using System.Collections.Generic;

public class Bruteforce : Ability
{
    [Header("Bruteforce Settings")]
    [SerializeField] private float critChanceBonus = 0.1f;       // 10% zusätzliche Crit-Chance
    [SerializeField] private float healPercentage = 0.8f;        // 80% des Attack Damage als Heilung
    [SerializeField] private float teleportDistance = 2f;        // Abstand hinter dem Gegner für Teleportation
    
    private StatModifier critChanceMod;                          // Modifier für kritische Trefferchance
    private bool isBruteforceActive = false;                     // Ob Bruteforce aktiv ist
    
    private void Awake()
    {
        // Event-Handler für kritische Treffer registrieren
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnEnemyTookDamage += OnEnemyTookDamage;
        }
    }
    
    private void OnDestroy()
    {
        // Event-Handler beim Zerstören des Objekts abmelden
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnEnemyTookDamage -= OnEnemyTookDamage;
        }
        
        // Sicherstellen, dass der Crit-Chance-Modifier entfernt wird
        RemoveCritChanceBonus();
    }
    
    public override void UseBase(IEntitie entitie)
    {
        // Aktiviere Bruteforce - erhöhe kritische Trefferchance
        ActivateBruteforce();
        
        Debug.Log($"[Bruteforce] Aktiviert - Kritische Trefferchance um {(critChanceBonus * 100):F0}% erhöht für {activeTime}s");
        
        // Visueller Effekt für Bruteforce-Aktivierung
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("VFX_Bruteforce_Activate", entitie);
        }
    }
    
    public override void OnTick(IEntitie entitie)
    {
        // Optional: Kontinuierlicher visueller Effekt während der aktiven Zeit
        if (isBruteforceActive)
        {
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("VFX_Bruteforce_Active", entitie);
            }
        }
    }
    
    public override void OnCooldown(IEntitie entitie)
    {
        // Deaktiviere Bruteforce - entferne kritische Trefferchance-Bonus
        DeactivateBruteforce();
        
        Debug.Log("[Bruteforce] Deaktiviert - Kritische Trefferchance auf Normalwert zurückgesetzt");
        
        // Visueller Effekt für Bruteforce-Deaktivierung
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("VFX_Bruteforce_Deactivate", entitie);
        }
    }
    
    protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Skaliere die kritische Trefferchance mit Rarity
        critChanceBonus = 0.1f + rarityScaling / 100;  // Rarity 2 = 20% Crit-Chance, etc.
        //healPercentage = 0.8f * rarityScaling;   // Rarity 2 = 160% Attack Damage Heilung, etc.
        
        Debug.Log($"[Bruteforce] Rarity Scaling angewendet: {rarityScaling} - Crit-Bonus: {(critChanceBonus * 100):F0}%, Heilung bleibt konstant bei {(healPercentage * 100):F0}% AD");
    }
    
    /// <summary>
    /// Aktiviert Bruteforce und erhöht die kritische Trefferchance
    /// </summary>
    private void ActivateBruteforce()
    {
        isBruteforceActive = true;
        
        // Erhöhe die kritische Trefferchance
        PlayerStats playerStats = PlayerManager.instance.playerStats;
        critChanceMod = new StatModifier(critChanceBonus, StatModType.Flat, this);
        playerStats.CriticalChance.AddModifier(critChanceMod);
        
        Debug.Log($"[Bruteforce] Kritische Trefferchance erhöht von {(playerStats.CriticalChance.Value - critChanceBonus):F1}% auf {playerStats.CriticalChance.Value:F1}%");
    }
    
    /// <summary>
    /// Deaktiviert Bruteforce und entfernt den kritischen Trefferchance-Bonus
    /// </summary>
    private void DeactivateBruteforce()
    {
        isBruteforceActive = false;
        RemoveCritChanceBonus();
    }
    
    /// <summary>
    /// Entfernt den kritischen Trefferchance-Modifier
    /// </summary>
    private void RemoveCritChanceBonus()
    {
        if (critChanceMod != null)
        {
            PlayerStats playerStats = PlayerManager.instance.playerStats;
            playerStats.CriticalChance.RemoveModifier(critChanceMod);
            critChanceMod = null;
            
            Debug.Log($"[Bruteforce] Kritische Trefferchance zurückgesetzt auf {playerStats.CriticalChance.Value:F1}%");
        }
    }
    
    /// <summary>
    /// Event-Handler für kritische Treffer - löst Teleportation und Heilung aus
    /// </summary>
    private void OnEnemyTookDamage(EnemyController enemy, bool isCrit)
    {
        // Nur reagieren, wenn Bruteforce aktiv ist und es ein kritischer Treffer war
        if (!isBruteforceActive || !isCrit) return;
        
        // Prüfe, ob der kritische Treffer vom Spieler kam (Distanz-Check)
        PlayerStats playerStats = PlayerManager.instance.playerStats;
        Vector3 playerPosition = playerStats.GetTransform().position;
        float distance = Vector3.Distance(enemy.transform.position, playerPosition);
        
        // Annahme: Kritischer Treffer kam vom Spieler, wenn Enemy in Angriffsreichweite ist
        if (distance <= playerStats.Range + 2f) // Etwas Toleranz für Bewegung
        {
            // Teleportiere den Spieler hinter den Gegner
            TeleportBehindEnemy(enemy, playerStats);
            
            // Heile den Spieler basierend auf Attack Damage
            HealPlayer(playerStats);
            
            Debug.Log($"[Bruteforce] Kritischer Treffer auf {enemy.name}! Teleportiert und geheilt.");
            
            // Visueller Effekt für Teleportation + Heilung
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("VFX_Bruteforce_Teleport", playerStats);
                VFX_Manager.instance.PlayEffect("VFX_Bruteforce_Heal", playerStats);
            }
        }
    }
    
    /// <summary>
    /// Teleportiert den Spieler hinter den angegebenen Gegner
    /// </summary>
    private void TeleportBehindEnemy(EnemyController enemy, PlayerStats player)
    {
        Vector3 enemyPosition = enemy.transform.position;
        Vector3 playerPosition = player.GetTransform().position;
        
        // Berechne die Richtung vom Spieler zum Gegner
        Vector3 directionToEnemy = (enemyPosition - playerPosition).normalized;
        
        // Berechne die Position hinter dem Gegner (entgegengesetzte Richtung)
        Vector3 teleportPosition = enemyPosition - directionToEnemy * teleportDistance;
        
        // Setze die Y-Koordinate auf die des Gegners (um auf dem Boden zu bleiben)
        teleportPosition.y = enemyPosition.y;
        
        // Teleportiere den Spieler
        player.transform.position = teleportPosition;
        
        Debug.Log($"[Bruteforce] Spieler teleportiert hinter {enemy.name} zu Position {teleportPosition}");
    }
    
    /// <summary>
    /// Heilt den Spieler basierend auf seinem Attack Damage
    /// </summary>
    private void HealPlayer(PlayerStats player)
    {
        float attackDamage = player.AttackPower.Value;
        int healAmount = Mathf.RoundToInt(attackDamage * healPercentage);
        
        // Heile den Spieler
        player.Heal(healAmount);
        
        Debug.Log($"[Bruteforce] Spieler geheilt um {healAmount} HP ({(healPercentage * 100):F0}% von {attackDamage} AD)");
    }
    
    protected override void OnUpdateAbility()
    {
        // Optional: Zusätzliche Update-Logik für Bruteforce
        base.OnUpdateAbility();
    }
}
