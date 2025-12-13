using UnityEngine;

/// <summary>
/// Animation-Typen für Enemy Abilities
/// Entspricht den vorhandenen Animation-Clips in Enemy-Animatoren
/// </summary>
public enum AbilityAnimationType
{
    None,       // Keine Animation (default)
    Idle,       // Idle Animation
    Walk,       // Walk Animation
    Attack1,    // Attack1 Animation
    Attack2,    // Attack2 Animation
    Casting,    // Casting Animation (Standard für Casts)
    Die1,       // Die1 Animation
    Die2,       // Die2 Animation
    Hit1,       // Hit1 Animation
    Hit2,       // Hit2 Animation
    Open1,      // Open1 Animation
    Open2       // Open2 Animation
}

public interface IAbilityBehavior
{
    void Enter(EnemyController controller);
    void OnUpdateAbility(EnemyController controller);
    void Execute(EnemyController controller);
    void Exit(EnemyController controller);
    bool IsAbilityReady(EnemyController controller);
    float GetCooldownRemaining();
    int GetPriority();
    AbilityAnimationType GetAnimationType();
    float GetExecutionTiming();
}

/// <summary>
/// Basis-Klasse für alle Enemy-Fähigkeiten.
/// Handhabt Cooldown-Management, Cast-Zeit und Prioritäten.
/// </summary>
public abstract class AbilityBehavior : MonoBehaviour, IAbilityBehavior
{
    [Header("Ability Settings")]
    [Tooltip("Cooldown in Sekunden (wird von MobStats.castCD multipliziert)")]
    public float cooldownTime = 5f;
    
    [Tooltip("Cast-Zeit in Sekunden (0 = instant cast)")]
    public float castTime = 0f;
    
    [Tooltip("Maximale Reichweite der Fähigkeit")]
    public float range = 10f;
    
    [Tooltip("Priorität dieser Fähigkeit (höher = wird bevorzugt ausgeführt)")]
    [Range(0, 100)]
    public int priority = 75;

    [Header("Animation Settings")]
    [Tooltip("Welche Animation soll während der Ability abgespielt werden?")]
    public AbilityAnimationType animationType = AbilityAnimationType.Casting;
    
    [Tooltip("An welcher Stelle der Animation (0.0 - 1.0) wird ExecuteAbility() aufgerufen?")]
    [Range(0f, 1f)]
    public float executionTiming = 0.5f;

    protected EnemyController controller;
    protected float currentCooldown = 0f;
    protected bool isCasting = false;

    public virtual void Enter(EnemyController controller)
    {
        this.controller = controller;
    }

    /// <summary>
    /// Wird kontinuierlich aufgerufen, um Cooldowns zu aktualisieren
    /// </summary>
    public virtual void OnUpdateAbility(EnemyController controller)
    {
        this.controller = controller;
        
        // Cooldown herunterzählen
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown < 0f)
                currentCooldown = 0f;
        }
        
        // Spezifische Update-Logik
        UpdateAbility();
    }

    /// <summary>
    /// Überschreibe diese Methode für zusätzliche Update-Logik
    /// </summary>
    protected virtual void UpdateAbility() { }

    /// <summary>
    /// Führt die Fähigkeit aus - wird von CastState aufgerufen
    /// </summary>
    public virtual void Execute(EnemyController controller)
    {
        this.controller = controller;
        isCasting = true;
        
        // Cooldown starten mit MobStats.castCD als Multiplier
        float effectiveCooldown = cooldownTime;
        if (controller.mobStats != null && controller.mobStats.castCD > 0f)
        {
            effectiveCooldown *= controller.mobStats.castCD;
        }
        
        currentCooldown = effectiveCooldown;
        
        // Spezifische Ability-Logik
        ExecuteAbility();
    }

    /// <summary>
    /// Überschreibe diese Methode für die eigentliche Fähigkeits-Logik
    /// </summary>
    protected abstract void ExecuteAbility();

    /// <summary>
    /// Prüft ob die Fähigkeit bereit ist
    /// </summary>
    public virtual bool IsAbilityReady(EnemyController controller)
    {
        if (controller == null || isCasting || currentCooldown > 0f)
            return false;
        
        // Prüfe Reichweite
        if (controller.Player == null)
            return false;
        
        float distanceToPlayer = Vector3.Distance(controller.transform.position, controller.Player.position);
        return distanceToPlayer <= range;
    }

    /// <summary>
    /// Gibt verbleibende Cooldown-Zeit zurück
    /// </summary>
    public float GetCooldownRemaining()
    {
        return currentCooldown;
    }

    /// <summary>
    /// Gibt Priorität der Fähigkeit zurück
    /// </summary>
    public int GetPriority()
    {
        return priority;
    }

    public virtual void Exit(EnemyController controller)
    {
        isCasting = false;
    }

    /// <summary>
    /// Wird nach Abschluss des Casts aufgerufen
    /// </summary>
    public virtual void OnCastComplete()
    {
        isCasting = false;
    }

    /// <summary>
    /// Gibt den Animation-Typ zurück
    /// </summary>
    public AbilityAnimationType GetAnimationType()
    {
        return animationType;
    }

    /// <summary>
    /// Gibt das Execution-Timing zurück (0.0 - 1.0)
    /// </summary>
    public float GetExecutionTiming()
    {
        return executionTiming;
    }
}
