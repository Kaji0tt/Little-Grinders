using UnityEngine;
using System.Collections.Generic;

public class EchoBarrier : Ability
{
    [Header("EchoBarrier Settings")]
    [SerializeField] private float baseArmorMultiplier = 0.5f; // Basis-Rüstungserhöhung bei Rarity 1 (50%)
    [SerializeField] private float baseReflectionMultiplier = 0.5f; // Basis-Reflektion bei Rarity 1 (0.5x)

    private float storedDamage = 0f;                        // Aktuell gespeicherter Schaden
    private bool isBarrierActive = false;                   // Ob die Barriere aktiv ist
    private StatModifier armorBoostMod;                     // Modifier für Rüstungserhöhung
    
    // Rarity Scaling Werte (werden zur Laufzeit berechnet)
    private float currentArmorMultiplier = 0.5f;            // Aktueller Rüstungs-Multiplikator
    private float currentReflectionMultiplier = 0.5f;       // Aktueller Reflektion-Multiplikator
    
    private void Awake()
    {
        // Event-Handler für eingehenden Spielerschaden registrieren
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerWasAttacked += OnPlayerTookDamage;
        }
    }
    
    private void OnDestroy()
    {
        // Event-Handler beim Zerstören des Objekts abmelden
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerWasAttacked -= OnPlayerTookDamage;
        }
        
        // Sicherstellen, dass der Rüstungsbonus-Modifier entfernt wird
        RemoveArmorBoost();
    }
    
    public override void UseBase(IEntitie entitie)
    {
        if (isBarrierActive)
        {
            // Wenn die Barriere bereits aktiv ist, reflektiere den gespeicherten Schaden
            ReflectStoredDamage();
        }
        else
        {
            // Aktiviere die Barriere
            ActivateBarrier();
        }
        
        Debug.Log($"[EchoBarrier] UseBase: Barriere aktiviert, gespeicherter Schaden: {storedDamage}");
    }
    
    public override void OnTick(IEntitie entitie)
    {
        // Bei Persistent-Fähigkeiten: Überprüfe kontinuierlich den Zustand
        if (isBarrierActive && storedDamage > 0)
        {
            // Optional: Zeige visuellen Effekt für gespeicherten Schaden
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("VFX_EchoBarrier", entitie);
            }
        }
    }
    
    public override void OnCooldown(IEntitie entitie)
    {
        // Am Ende der Ability-Dauer: Automatisch reflektieren und deaktivieren
        if (isBarrierActive)
        {
            ReflectStoredDamage();
            DeactivateBarrier();
        }
        
        Debug.Log("[EchoBarrier] OnCooldown: Barriere deaktiviert");
    }
    
    protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Mathematische Skalierung basierend auf Rarity
        // Rüstungserhöhung: Wird GRÖSSER bei höherer Rarity (50% -> 100% -> 150% -> 250%)
        currentArmorMultiplier = baseArmorMultiplier * rarityScaling;
        
        // Reflektion-Multiplikator: Wird GRÖSSER bei höherer Rarity (0.5 -> 1.0 -> 1.5 -> 2.5)
        currentReflectionMultiplier = baseReflectionMultiplier * rarityScaling;
        
        Debug.Log($"[EchoBarrier] Rarity Scaling angewendet: {rarityScaling} - Rüstungsbonus: {(currentArmorMultiplier * 100):F0}%, Reflektion: {(currentReflectionMultiplier * 100):F0}%");
    }
    
    /// <summary>
    /// Aktiviert die Echo-Barriere und erhöht die Rüstung
    /// </summary>
    private void ActivateBarrier()
    {
        isBarrierActive = true;
        storedDamage = 0f;
        
        // Erhöhe die Rüstung basierend auf aktuellem Armor-Wert und Rarity Scaling
        PlayerStats playerStats = PlayerManager.instance.playerStats;
        float currentArmor = playerStats.Armor.Value;
        float armorBoost = currentArmor * currentArmorMultiplier;
        armorBoostMod = new StatModifier(armorBoost, StatModType.Flat, this);
        playerStats.Armor.AddModifier(armorBoostMod);
        
        // Visueller Effekt für Barrieren-Aktivierung
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("VFX_EchoBarrier_Activate", playerStats);
        }
        
        Debug.Log($"[EchoBarrier] Barriere aktiviert - Rüstung erhöht um {armorBoost} ({(currentArmorMultiplier * 100):F0}%)");
    }
    
    /// <summary>
    /// Deaktiviert die Echo-Barriere
    /// </summary>
    private void DeactivateBarrier()
    {
        isBarrierActive = false;
        storedDamage = 0f;
        
        RemoveArmorBoost();
        
        Debug.Log("[EchoBarrier] Barriere deaktiviert");
    }
    
    /// <summary>
    /// Entfernt den Rüstungsbonus-Modifier
    /// </summary>
    private void RemoveArmorBoost()
    {
        if (armorBoostMod != null)
        {
            PlayerStats playerStats = PlayerManager.instance.playerStats;
            playerStats.Armor.RemoveModifier(armorBoostMod);
            armorBoostMod = null;
        }
    }
    
    /// <summary>
    /// Event-Handler für eingehenden Spielerschaden
    /// </summary>
    private void OnPlayerTookDamage(float damage)
    {
        if (!isBarrierActive) return;
        
        // Speichere einen Teil des Schadens für die Reflektion
        float damageToStore = damage;
        storedDamage += damageToStore;
        
        Debug.Log($"[EchoBarrier] Schaden gespeichert: {damageToStore}, Gesamt: {storedDamage}");
        
        // Visueller Effekt für Schadens-Absorption
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("VFX_EchoBarrier_Absorb", PlayerManager.instance.playerStats);
        }
    }
    
    /// <summary>
    /// Reflektiert den gespeicherten Schaden auf Gegner im Umkreis
    /// </summary>
    private void ReflectStoredDamage()
    {
        if (storedDamage <= 0f) return;
        
        Vector3 playerPosition = PlayerManager.instance.playerStats.GetTransform().position;
        float reflectionRadius = areaOfEffectRadius; // Nutze den AoE-Radius aus der Ability-Basis
        
        // Finde alle Gegner im Umkreis
        Collider[] hitColliders = Physics.OverlapSphere(playerPosition, reflectionRadius);
        List<EnemyController> enemiesInRange = new List<EnemyController>();
        
        foreach (var col in hitColliders)
        {
            if (col.CompareTag("Enemy"))
            {
                EnemyController enemy = col.GetComponentInParent<EnemyController>();
                if (enemy != null && !enemy.mobStats.isDead)
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }
        
        if (enemiesInRange.Count > 0)
        {
            // Verteile den Schaden gleichmäßig auf alle Gegner und wende berechneten Reflektion-Multiplikator an
            float totalReflectedDamage = storedDamage * currentReflectionMultiplier;
            float damagePerEnemy = totalReflectedDamage / enemiesInRange.Count;
            
            foreach (EnemyController enemy in enemiesInRange)
            {
                enemy.TakeDirectDamage(damagePerEnemy, (int)reflectionRadius);
                Debug.Log($"[EchoBarrier] {enemy.name} nimmt {damagePerEnemy} reflektierten Schaden");
            }
            
            // Visueller Effekt für Schaden-Reflektion
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("VFX_EchoBarrier_Reflect", PlayerManager.instance.playerStats);
            }
            
            Debug.Log($"[EchoBarrier] {storedDamage} gesammelter Schaden → {totalReflectedDamage} reflektierter Schaden auf {enemiesInRange.Count} Gegner (Multiplikator: {currentReflectionMultiplier})");
        }
        else
        {
            Debug.Log("[EchoBarrier] Keine Gegner im Umkreis für Reflektion gefunden");
        }
        
        // Zurücksetzen des gespeicherten Schadens
        storedDamage = 0f;
    }
}
