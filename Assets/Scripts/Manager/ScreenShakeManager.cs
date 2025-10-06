using UnityEngine;
using System.Collections;

/// <summary>
/// Manages screen shake effects for enhanced player feedback.
/// Provides various shake intensities for different game events.
/// </summary>
public class ScreenShakeManager : MonoBehaviour
{
    public static ScreenShakeManager Instance { get; private set; }

    [Header("Screen Shake Settings")]
    [SerializeField] private float defaultShakeDuration = 0.2f;
    [SerializeField] private float defaultShakeIntensity = 0.1f;
    [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private Coroutine currentShakeCoroutine;

    // Preset shake types for different game events
    public enum ShakeType
    {
        Light,      // Small hits, UI interactions
        Medium,     // Normal attacks, enemy hits
        Heavy,      // Critical hits, strong attacks
        Extreme     // Level up, special abilities
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

        // Find the main camera
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
        }
        else
        {
            Debug.LogWarning("ScreenShakeManager: Main camera not found!");
        }
    }

    private void Start()
    {
        // Update camera reference in case it changed
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalCameraPosition = mainCamera.transform.localPosition;
            }
        }
    }

    /// <summary>
    /// Triggers a screen shake with predefined intensity based on shake type
    /// </summary>
    /// <param name="shakeType">Type of shake to perform</param>
    public void TriggerShake(ShakeType shakeType)
    {
        switch (shakeType)
        {
            case ShakeType.Light:
                TriggerShake(0.1f, 0.05f);
                break;
            case ShakeType.Medium:
                TriggerShake(0.2f, 0.1f);
                break;
            case ShakeType.Heavy:
                TriggerShake(0.3f, 0.2f);
                break;
            case ShakeType.Extreme:
                TriggerShake(0.5f, 0.3f);
                break;
        }
    }

    /// <summary>
    /// Triggers a screen shake with custom parameters
    /// </summary>
    /// <param name="duration">Duration of the shake effect</param>
    /// <param name="intensity">Intensity/magnitude of the shake</param>
    public void TriggerShake(float duration, float intensity)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("ScreenShakeManager: Cannot shake - no camera found!");
            return;
        }

        // Stop any existing shake
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
        }

        // Start new shake
        currentShakeCoroutine = StartCoroutine(ShakeCoroutine(duration, intensity));
    }

    /// <summary>
    /// Coroutine that performs the actual screen shake
    /// </summary>
    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Calculate shake strength using the curve
            float strength = shakeCurve.Evaluate(elapsed / duration) * intensity;

            // Generate random offset
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f) * strength,
                Random.Range(-1f, 1f) * strength,
                0f
            );

            // Apply shake to camera
            mainCamera.transform.localPosition = originalCameraPosition + offset;

            yield return null;
        }

        // Reset camera position
        mainCamera.transform.localPosition = originalCameraPosition;
        currentShakeCoroutine = null;
    }

    /// <summary>
    /// Stops any ongoing shake effect
    /// </summary>
    public void StopShake()
    {
        if (currentShakeCoroutine != null)
        {
            StopCoroutine(currentShakeCoroutine);
            currentShakeCoroutine = null;
        }

        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = originalCameraPosition;
        }
    }

    /// <summary>
    /// Updates the original camera position (call this if camera parent moves)
    /// </summary>
    public void UpdateCameraPosition()
    {
        if (mainCamera != null && currentShakeCoroutine == null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
        }
    }

    private void OnDestroy()
    {
        StopShake();
    }
}