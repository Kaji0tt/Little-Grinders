using UnityEngine;
using System.Collections.Generic;

public class Bladevortex : Ability
{
    [Header("Bladevortex Settings")]
    [SerializeField] private float baseDamageMultiplier = 0.33f; // 1/3 AD pro Tick
    [SerializeField] private float critTimeExtension = 2f;       // Zeit, die bei Crit hinzugefügt wird
    
    private int activeBlades = 0;                                // Anzahl aktiver Blades
    private bool hasResetThisActivation = false;                 // Ob bereits ein Reset in dieser Aktivierung stattgefunden hat
    private float originalActiveTime;                            // Ursprüngliche Aktivierungszeit für Reset
    private float customActiveTimer;                             // Eigener Timer für Zeitkontrolle
    
    private void Awake()
    {
        // Event-Handler für Schadens- und Todesevents registrieren
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnEnemyTookDamage += OnEnemyTookDamage;
            GameEvents.Instance.OnEnemyDied += OnEnemyDied;
        }
    }
    
    private void OnDestroy()
    {
        // Event-Handler beim Zerstören des Objekts abmelden
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnEnemyTookDamage -= OnEnemyTookDamage;
            GameEvents.Instance.OnEnemyDied -= OnEnemyDied;
        }
    }
    
    public override void UseBase(IEntitie entitie)
    {
        // Aktiviere eine Blade (verbrauche eine Ladung)
        if (activeBlades < GetMaxCharges())
        {
            activeBlades++;
            hasResetThisActivation = false; // Reset für jede neue Blade
            originalActiveTime = activeTime; // Speichere ursprüngliche Zeit
            customActiveTimer = activeTime;  // Initialisiere eigenen Timer
            
            Debug.Log($"[Bladevortex] Blade aktiviert - Aktive Blades: {activeBlades}/{GetMaxCharges()}");
            
            // Visueller Effekt für Blade-Aktivierung
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("VFX_Bladevortex_Activate", entitie);
            }
        }
    }
    
    public override void OnTick(IEntitie entitie)
    {
        if (activeBlades <= 0) return;
        
        Vector3 playerPosition = entitie.GetTransform().position;
        float damageRadius = areaOfEffectRadius;
        
        // Finde alle Gegner im Umkreis
        Collider[] hitColliders = Physics.OverlapSphere(playerPosition, damageRadius);
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
            // Berechne Schaden basierend auf Spieler Attack Power
            PlayerStats playerStats = PlayerManager.instance.playerStats;
            float baseAttackPower = playerStats.AttackPower.Value;
            float damagePerBlade = baseAttackPower * baseDamageMultiplier;
            float totalDamage = damagePerBlade * activeBlades;
            
            foreach (EnemyController enemy in enemiesInRange)
            {
                // Berechne kritischen Treffer
                bool isCrit = Random.value < playerStats.CriticalChance.Value;
                float finalDamage = totalDamage;
                if (isCrit)
                    finalDamage *= 2f; // Annahme: Crit = 2x Schaden
                
                // Verwende TakeDamage für kritische Treffer-Möglichkeit
                enemy.TakeDamage(finalDamage, (int)damageRadius, isCrit);
                
                Debug.Log($"[Bladevortex] {enemy.name} nimmt {finalDamage} Schaden ({activeBlades} Blades) - Crit: {isCrit}");
            }
            
            // Visueller Effekt für Blade-Schaden
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("VFX_Bladevortex_Damage", entitie);
            }
        }
    }
    
    public override void OnCooldown(IEntitie entitie)
    {
        // Deaktiviere alle Blades
        activeBlades = 0;
        hasResetThisActivation = false;
        
        Debug.Log("[Bladevortex] Alle Blades deaktiviert");
        
        // Visueller Effekt für Blade-Deaktivierung
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("VFX_Bladevortex_Deactivate", entitie);
        }
    }
    
    protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Erhöhe die maximalen Ladungen basierend auf Rarity
        // Basis: 3 Ladungen + rarityScaling
        int bonusCharges = Mathf.RoundToInt(rarityScaling);
        maxCharges = 3 + bonusCharges;
        
        Debug.Log($"[Bladevortex] Rarity Scaling angewendet: {rarityScaling} - Maximale Ladungen: {maxCharges}");
    }
    
    /// <summary>
    /// Event-Handler für kritische Treffer - verlängert die aktive Zeit
    /// </summary>
    private void OnEnemyTookDamage(EnemyController enemy, bool isCrit)
    {
        // Nur reagieren, wenn diese Fähigkeit aktiv ist und es ein kritischer Treffer war
        if (state != AbilityState.Active || !isCrit || activeBlades <= 0) return;
        
        // Prüfe, ob der Schaden von uns kam (Gegner im Umkreis)
        Vector3 playerPosition = PlayerManager.instance.playerStats.GetTransform().position;
        float distance = Vector3.Distance(enemy.transform.position, playerPosition);
        
        if (distance <= areaOfEffectRadius)
        {
            // Verlängere die aktive Zeit um 2 Sekunden
            customActiveTimer += critTimeExtension;
            
            Debug.Log($"[Bladevortex] Kritischer Treffer! Aktive Zeit um {critTimeExtension}s verlängert");
            
            // Visueller Effekt für Crit-Verlängerung
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("VFX_Bladevortex_CritExtend", PlayerManager.instance.playerStats);
            }
        }
    }
    
    /// <summary>
    /// Event-Handler für Todesfälle - setzt die aktive Zeit zurück (einmal pro Aktivierung)
    /// </summary>
    private void OnEnemyDied(EnemyController enemy)
    {
        // Nur reagieren, wenn diese Fähigkeit aktiv ist und noch kein Reset stattgefunden hat
        if (state != AbilityState.Active || hasResetThisActivation || activeBlades <= 0) return;
        
        // Prüfe, ob der Tod von uns verursacht wurde (Gegner im Umkreis)
        Vector3 playerPosition = PlayerManager.instance.playerStats.GetTransform().position;
        float distance = Vector3.Distance(enemy.transform.position, playerPosition);
        
        if (distance <= areaOfEffectRadius)
        {
            // Setze die aktive Zeit auf die ursprüngliche Dauer zurück
            customActiveTimer = originalActiveTime;
            hasResetThisActivation = true; // Markiere, dass Reset verwendet wurde
            
            Debug.Log($"[Bladevortex] Gegner getötet! Aktive Zeit zurückgesetzt auf {originalActiveTime}s");
            
            // Visueller Effekt für Zeit-Reset
            if (VFX_Manager.instance != null)
            {
                VFX_Manager.instance.PlayEffect("VFX_Bladevortex_TimeReset", PlayerManager.instance.playerStats);
            }
        }
    }
    
    protected override void OnUpdateAbility()
    {
        // Verwalte den eigenen aktiven Timer nur wenn Blades aktiv sind
        if (state == AbilityState.Active && activeBlades > 0)
        {
            customActiveTimer -= Time.deltaTime;
            
            // Wenn die Zeit abgelaufen ist, deaktiviere alle Blades
            if (customActiveTimer <= 0)
            {
                OnCooldown(PlayerManager.instance.playerStats);
            }
        }
    }
}
