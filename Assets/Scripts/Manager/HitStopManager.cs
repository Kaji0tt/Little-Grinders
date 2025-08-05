using UnityEngine;
using System.Collections;

/// <summary>
/// Manages hit stop (freeze frame) effects for enhanced combat feedback.
/// Temporarily slows down or pauses time during impactful moments.
/// </summary>
public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance { get; private set; }

    [Header("Hit Stop Settings")]
    [SerializeField] private float defaultHitStopDuration = 0.1f;
    [SerializeField] private float defaultTimeScale = 0.1f;

    private Coroutine currentHitStopCoroutine;
    private float originalTimeScale = 1f;

    // Preset hit stop types for different impact levels
    public enum HitStopType
    {
        Light,      // Small hits, light attacks
        Medium,     // Normal attacks, regular hits
        Heavy,      // Critical hits, strong attacks
        Extreme     // Special abilities, finishing moves
    }

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

        originalTimeScale = Time.timeScale;
    }

    /// <summary>
    /// Triggers a hit stop effect with predefined settings based on impact type
    /// </summary>
    /// <param name="hitStopType">Type of hit stop to perform</param>
    public void TriggerHitStop(HitStopType hitStopType)
    {
        switch (hitStopType)
        {
            case HitStopType.Light:
                TriggerHitStop(0.05f, 0.5f);
                break;
            case HitStopType.Medium:
                TriggerHitStop(0.1f, 0.2f);
                break;
            case HitStopType.Heavy:
                TriggerHitStop(0.15f, 0.1f);
                break;
            case HitStopType.Extreme:
                TriggerHitStop(0.25f, 0.05f);
                break;
        }
    }

    /// <summary>
    /// Triggers a hit stop effect with custom parameters
    /// </summary>
    /// <param name="duration">Duration of the hit stop effect in real time</param>
    /// <param name="timeScale">Time scale during the effect (0.0 = complete freeze, 1.0 = normal time)</param>
    public void TriggerHitStop(float duration, float timeScale)
    {
        // Stop any existing hit stop
        if (currentHitStopCoroutine != null)
        {
            StopCoroutine(currentHitStopCoroutine);
        }

        // Start new hit stop
        currentHitStopCoroutine = StartCoroutine(HitStopCoroutine(duration, timeScale));
    }

    /// <summary>
    /// Coroutine that performs the actual hit stop effect
    /// </summary>
    private IEnumerator HitStopCoroutine(float duration, float timeScale)
    {
        // Set the time scale
        Time.timeScale = timeScale;

        // Wait for the duration (using unscaled time)
        yield return new WaitForSecondsRealtime(duration);

        // Restore original time scale
        Time.timeScale = originalTimeScale;
        currentHitStopCoroutine = null;
    }

    /// <summary>
    /// Stops any ongoing hit stop effect
    /// </summary>
    public void StopHitStop()
    {
        if (currentHitStopCoroutine != null)
        {
            StopCoroutine(currentHitStopCoroutine);
            currentHitStopCoroutine = null;
        }

        Time.timeScale = originalTimeScale;
    }

    /// <summary>
    /// Updates the original time scale reference
    /// </summary>
    /// <param name="newTimeScale">New base time scale</param>
    public void SetOriginalTimeScale(float newTimeScale)
    {
        originalTimeScale = newTimeScale;
        
        // If no hit stop is active, update current time scale too
        if (currentHitStopCoroutine == null)
        {
            Time.timeScale = originalTimeScale;
        }
    }

    private void OnDestroy()
    {
        StopHitStop();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Ensure time scale is restored when application is paused/unpaused
        if (!pauseStatus && currentHitStopCoroutine == null)
        {
            Time.timeScale = originalTimeScale;
        }
    }
}