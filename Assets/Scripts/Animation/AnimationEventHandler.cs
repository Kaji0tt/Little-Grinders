using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles animation events for enhanced feedback timing.
/// This component should be attached to GameObjects with animators to receive animation events.
/// </summary>
public class AnimationEventHandler : MonoBehaviour
{
    [Header("Combat Events")]
    [SerializeField] private UnityEvent OnAttackHit;
    [SerializeField] private UnityEvent OnAttackStart;
    [SerializeField] private UnityEvent OnAttackEnd;
    [SerializeField] private UnityEvent OnComboStep;
    
    [Header("Movement Events")]
    [SerializeField] private UnityEvent OnFootstep;
    [SerializeField] private UnityEvent OnLand;
    
    [Header("Effect Events")]
    [SerializeField] private UnityEvent OnCriticalHit;
    [SerializeField] private UnityEvent OnSpellCast;
    [SerializeField] private UnityEvent OnSpecialEffect;

    // References to feedback systems
    private ScreenShakeManager screenShake;
    private HitStopManager hitStop;
    
    private void Awake()
    {
        screenShake = ScreenShakeManager.Instance;
        hitStop = HitStopManager.Instance;
    }

    #region Animation Event Methods
    // These methods are called directly by Animation Events in the Unity Animator
    
    /// <summary>
    /// Called when an attack connects with a target
    /// </summary>
    public void AttackHit()
    {
        OnAttackHit?.Invoke();
        
        // Trigger screen shake for hit feedback
        if (screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Medium);
        }
        
        // Trigger hit stop for impact
        if (hitStop != null)
        {
            hitStop.TriggerHitStop(HitStopManager.HitStopType.Medium);
        }
    }
    
    /// <summary>
    /// Called when an attack animation starts
    /// </summary>
    public void AttackStart()
    {
        OnAttackStart?.Invoke();
    }
    
    /// <summary>
    /// Called when an attack animation ends
    /// </summary>
    public void AttackEnd()
    {
        OnAttackEnd?.Invoke();
    }
    
    /// <summary>
    /// Called when a combo step is reached
    /// </summary>
    public void ComboStep()
    {
        OnComboStep?.Invoke();
        
        // Light feedback for combo progression
        if (screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Light);
        }
    }
    
    /// <summary>
    /// Called for critical hit animations
    /// </summary>
    public void CriticalHit()
    {
        OnCriticalHit?.Invoke();
        
        // Heavy feedback for critical hits
        if (screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Heavy);
        }
        
        if (hitStop != null)
        {
            hitStop.TriggerHitStop(HitStopManager.HitStopType.Heavy);
        }
    }
    
    /// <summary>
    /// Called during footstep animations
    /// </summary>
    public void Footstep()
    {
        OnFootstep?.Invoke();
    }
    
    /// <summary>
    /// Called when character lands after jumping/falling
    /// </summary>
    public void Land()
    {
        OnLand?.Invoke();
        
        // Light shake for landing impact
        if (screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Light);
        }
    }
    
    /// <summary>
    /// Called during spell casting animations
    /// </summary>
    public void SpellCast()
    {
        OnSpellCast?.Invoke();
        
        // Medium shake for spell effects
        if (screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Medium);
        }
    }
    
    /// <summary>
    /// Called for special effect moments in animations
    /// </summary>
    public void SpecialEffect()
    {
        OnSpecialEffect?.Invoke();
        
        // Extreme shake for special moments
        if (screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Extreme);
        }
    }
    
    #endregion

    #region Public Methods for External Triggers
    
    /// <summary>
    /// Manually trigger attack hit feedback
    /// </summary>
    public void TriggerAttackHitFeedback()
    {
        AttackHit();
    }
    
    /// <summary>
    /// Manually trigger critical hit feedback
    /// </summary>
    public void TriggerCriticalHitFeedback()
    {
        CriticalHit();
    }
    
    /// <summary>
    /// Manually trigger special effect feedback
    /// </summary>
    public void TriggerSpecialEffectFeedback()
    {
        SpecialEffect();
    }
    
    #endregion
}