using UnityEngine;

public interface IAttackBehavior
{
    void Enter(EnemyController controller);
    void OnUpdateAttack(EnemyController controller);
    void Exit(EnemyController controller);
    bool IsAttackReady(EnemyController controller);
    int GetPriority();
}

/// <summary>
/// Basis-Klasse für alle Angriffsverhalten.
/// Handhabt automatisch die FacingDirection zwischen Angriffen.
/// </summary>
public abstract class AttackBehavior : MonoBehaviour, IAttackBehavior
{
    [Header("Attack Behavior Settings")]
    [Tooltip("Priorität dieses Angriffs (niedriger als Abilities = wird überschrieben)")]
    [Range(0, 100)]
    public int priority = 50;
    
    protected EnemyController controller;
    
    public virtual void Enter(EnemyController controller)
    {
        this.controller = controller;
    }

    /// <summary>
    /// Template Method: Steuert FacingDirection automatisch + ruft UpdateAttack auf
    /// Ruft auch das OnEnemyStartAttack Event auf, wenn ein Angriff startet
    /// Prüft ob eine Ability Priorität hat und löst ggf. Cast-Anfrage aus
    /// </summary>
    public void OnUpdateAttack(EnemyController controller)
    {
        this.controller = controller;
        
        // 👉 Prüfe zuerst ob IRGENDEINE Ability bereit ist und höhere Priorität hat
        IAbilityBehavior readyAbility = controller.GetReadyAbility();
        if (readyAbility != null)
        {
            int abilityPriority = readyAbility.GetPriority();
            int attackPriority = GetPriority();
            
            if (abilityPriority > attackPriority)
            {
                // Ability hat Priorität - Request Cast State
                GameEvents.Instance?.EnemyRequestCast(controller);
                return; // Kein Angriff in diesem Frame
            }
        }
        
        // 👉 FacingDirection IMMER aktualisieren (zwischen Angriffen)
        UpdateFacingDirection();
        
        // 👉 Event: Angriff startet (wird aufgerufen wenn IsAttackReady() wahr ist)
        bool isReady = IsAttackReady(controller);
        
        if (isReady)
        {
            float attackDuration = 1f / controller.mobStats.AttackSpeed.Value;
        
            GameEvents.Instance?.EnemyStartAttack(controller, attackDuration);
        }
        
        // 👉 Spezifische Logik des Verhaltens
        UpdateAttack();
    }

    /// <summary>
    /// Überschreibe DIESE Methode statt UpdateAttack!
    /// </summary>
    protected abstract void UpdateAttack();

    public abstract void Exit(EnemyController controller);
    public abstract bool IsAttackReady(EnemyController controller);

    /// <summary>
    /// Gibt die Priorität dieses Angriffsverhaltens zurück
    /// </summary>
    public virtual int GetPriority()
    {
        return priority;
    }

    /// <summary>
    /// Aktualisiert automatisch die Blickrichtung zum Spieler
    /// </summary>
    protected void UpdateFacingDirection()
    {
        if (controller?.myIsoRenderer != null)
        {
            controller.myIsoRenderer.SetFacingDirection();
        }
    }
}
