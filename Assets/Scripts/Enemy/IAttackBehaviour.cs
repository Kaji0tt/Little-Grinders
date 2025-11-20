using UnityEngine;

public interface IAttackBehavior
{
    void Enter(EnemyController controller);
    void OnUpdateAttack(EnemyController controller);
    void Exit(EnemyController controller);
    bool IsAttackReady(EnemyController controller);
}

/// <summary>
/// Basis-Klasse für alle Angriffsverhalten.
/// Handhabt automatisch die FacingDirection zwischen Angriffen.
/// </summary>
public abstract class AttackBehavior : MonoBehaviour, IAttackBehavior
{
    protected EnemyController controller;
    
    public virtual void Enter(EnemyController controller)
    {
        this.controller = controller;
    }

    /// <summary>
    /// Template Method: Steuert FacingDirection automatisch + ruft UpdateAttack auf
    /// Ruft auch das OnEnemyStartAttack Event auf, wenn ein Angriff startet
    /// </summary>
    public void OnUpdateAttack(EnemyController controller)
    {
        this.controller = controller;
        
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
