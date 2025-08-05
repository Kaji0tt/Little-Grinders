using UnityEngine;

/// <summary>
/// Centralized manager for all player feedback systems.
/// Coordinates screen shake, hit stops, animations, and visual effects.
/// </summary>
public class FeedbackSystemManager : MonoBehaviour
{
    public static FeedbackSystemManager Instance { get; private set; }

    [Header("Feedback Systems")]
    [SerializeField] private bool enableScreenShake = true;
    [SerializeField] private bool enableHitStop = true;
    [SerializeField] private bool enableEnhancedPopups = true;
    [SerializeField] private bool enableStatusEffects = true;
    [SerializeField] private bool enableScreenOverlays = true;

    [Header("Feedback Intensity Multipliers")]
    [Range(0f, 2f)]
    [SerializeField] private float screenShakeMultiplier = 1f;
    [Range(0f, 2f)]
    [SerializeField] private float hitStopMultiplier = 1f;

    // References to feedback systems
    private ScreenShakeManager screenShake;
    private HitStopManager hitStop;
    private StatusEffectVisualManager statusEffects;
    private ScreenOverlayManager screenOverlay;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeFeedbackSystems();
    }

    private void InitializeFeedbackSystems()
    {
        // Find or create screen shake manager
        screenShake = FindObjectOfType<ScreenShakeManager>();
        if (screenShake == null && enableScreenShake)
        {
            GameObject shakeManager = new GameObject("ScreenShakeManager");
            shakeManager.transform.SetParent(transform);
            screenShake = shakeManager.AddComponent<ScreenShakeManager>();
        }

        // Find or create hit stop manager
        hitStop = FindObjectOfType<HitStopManager>();
        if (hitStop == null && enableHitStop)
        {
            GameObject hitStopManager = new GameObject("HitStopManager");
            hitStopManager.transform.SetParent(transform);
            hitStop = hitStopManager.AddComponent<HitStopManager>();
        }

        // Find or create status effect manager
        statusEffects = FindObjectOfType<StatusEffectVisualManager>();
        if (statusEffects == null && enableStatusEffects)
        {
            GameObject statusManager = new GameObject("StatusEffectVisualManager");
            statusManager.transform.SetParent(transform);
            statusEffects = statusManager.AddComponent<StatusEffectVisualManager>();
        }

        // Find or create screen overlay manager
        screenOverlay = FindObjectOfType<ScreenOverlayManager>();
        if (screenOverlay == null && enableScreenOverlays)
        {
            GameObject overlayManager = new GameObject("ScreenOverlayManager");
            overlayManager.transform.SetParent(transform);
            screenOverlay = overlayManager.AddComponent<ScreenOverlayManager>();
        }
    }

    #region Combat Feedback Methods

    /// <summary>
    /// Triggers feedback for a normal attack hit
    /// </summary>
    public void TriggerAttackHitFeedback()
    {
        if (enableScreenShake && screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Medium);
        }

        if (enableHitStop && hitStop != null)
        {
            hitStop.TriggerHitStop(HitStopManager.HitStopType.Medium);
        }
    }

    /// <summary>
    /// Triggers feedback for a critical hit
    /// </summary>
    public void TriggerCriticalHitFeedback()
    {
        if (enableScreenShake && screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Heavy);
        }

        if (enableHitStop && hitStop != null)
        {
            hitStop.TriggerHitStop(HitStopManager.HitStopType.Heavy);
        }
    }

    /// <summary>
    /// Triggers feedback for a combo step
    /// </summary>
    public void TriggerComboStepFeedback()
    {
        if (enableScreenShake && screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Light);
        }
    }

    /// <summary>
    /// Triggers feedback for level up
    /// </summary>
    public void TriggerLevelUpFeedback()
    {
        if (enableScreenShake && screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Extreme);
        }
    }

    /// <summary>
    /// Triggers feedback for spell casting
    /// </summary>
    public void TriggerSpellCastFeedback()
    {
        if (enableScreenShake && screenShake != null)
        {
            screenShake.TriggerShake(ScreenShakeManager.ShakeType.Medium);
        }

        if (enableHitStop && hitStop != null)
        {
            hitStop.TriggerHitStop(HitStopManager.HitStopType.Light);
        }
    }

    /// <summary>
    /// Triggers feedback for taking damage
    /// </summary>
    public void TriggerDamageTakenFeedback()
    {
        if (enableScreenOverlays && screenOverlay != null)
        {
            screenOverlay.TriggerDamageFlash();
        }
    }

    /// <summary>
    /// Triggers feedback for healing
    /// </summary>
    public void TriggerHealingFeedback()
    {
        if (enableScreenOverlays && screenOverlay != null)
        {
            screenOverlay.TriggerHealFlash();
        }
    }

    /// <summary>
    /// Updates health percentage for overlay effects
    /// </summary>
    public void UpdateHealthPercent(float healthPercent)
    {
        if (enableScreenOverlays && screenOverlay != null)
        {
            screenOverlay.UpdateHealthPercent(healthPercent);
        }
    }

    #endregion

    #region Status Effect Methods

    /// <summary>
    /// Shows a buff status effect
    /// </summary>
    public void ShowBuffEffect(string effectName, Sprite icon, float duration)
    {
        if (enableStatusEffects && statusEffects != null)
        {
            statusEffects.ShowBuffEffect(effectName, icon, duration);
        }
    }

    /// <summary>
    /// Shows a debuff status effect
    /// </summary>
    public void ShowDebuffEffect(string effectName, Sprite icon, float duration)
    {
        if (enableStatusEffects && statusEffects != null)
        {
            statusEffects.ShowDebuffEffect(effectName, icon, duration);
        }
    }

    /// <summary>
    /// Shows a cooldown effect
    /// </summary>
    public void ShowCooldownEffect(string effectName, Sprite icon, float cooldownDuration)
    {
        if (enableStatusEffects && statusEffects != null)
        {
            statusEffects.ShowCooldownEffect(effectName, icon, cooldownDuration);
        }
    }

    /// <summary>
    /// Removes a status effect
    /// </summary>
    public void RemoveStatusEffect(string effectName)
    {
        if (statusEffects != null)
        {
            statusEffects.RemoveStatusEffect(effectName);
        }
    }

    #endregion

    #region Custom Feedback Methods

    /// <summary>
    /// Triggers custom screen shake
    /// </summary>
    public void TriggerCustomScreenShake(float duration, float intensity)
    {
        if (enableScreenShake && screenShake != null)
        {
            float adjustedDuration = duration * screenShakeMultiplier;
            float adjustedIntensity = intensity * screenShakeMultiplier;
            screenShake.TriggerShake(adjustedDuration, adjustedIntensity);
        }
    }

    /// <summary>
    /// Triggers custom hit stop
    /// </summary>
    public void TriggerCustomHitStop(float duration, float timeScale)
    {
        if (enableHitStop && hitStop != null)
        {
            float adjustedDuration = duration * hitStopMultiplier;
            hitStop.TriggerHitStop(adjustedDuration, timeScale);
        }
    }

    #endregion

    #region Settings and Configuration

    /// <summary>
    /// Enables or disables screen shake
    /// </summary>
    public void SetScreenShakeEnabled(bool enabled)
    {
        enableScreenShake = enabled;
    }

    /// <summary>
    /// Enables or disables hit stop effects
    /// </summary>
    public void SetHitStopEnabled(bool enabled)
    {
        enableHitStop = enabled;
    }

    /// <summary>
    /// Sets the screen shake intensity multiplier
    /// </summary>
    public void SetScreenShakeMultiplier(float multiplier)
    {
        screenShakeMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
    }

    /// <summary>
    /// Sets the hit stop intensity multiplier
    /// </summary>
    public void SetHitStopMultiplier(float multiplier)
    {
        hitStopMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Stops all ongoing feedback effects
    /// </summary>
    public void StopAllFeedback()
    {
        if (screenShake != null)
        {
            screenShake.StopShake();
        }

        if (hitStop != null)
        {
            hitStop.StopHitStop();
        }
    }

    /// <summary>
    /// Clears all status effects
    /// </summary>
    public void ClearAllStatusEffects()
    {
        if (statusEffects != null)
        {
            statusEffects.ClearAllStatusEffects();
        }
    }

    #endregion

    private void OnDestroy()
    {
        StopAllFeedback();
    }
}