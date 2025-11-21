using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages screen overlay effects for enhanced visual feedback.
/// Provides health-based overlays, damage indicators, and special effect overlays.
/// </summary>
public class ScreenOverlayManager : MonoBehaviour
{
    public static ScreenOverlayManager Instance { get; private set; }

    [Header("Overlay Settings")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private Canvas overlayCanvas;
    
    [Header("Low Health Effect")]
    [SerializeField] private Color lowHealthColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private float lowHealthThreshold = 0.25f;
    [SerializeField] private float pulseSpeed = 2f;
    
    [Header("Damage Flash Effect")]
    [SerializeField] private Color damageFlashColor = new Color(1f, 0f, 0f, 0.5f);
    [SerializeField] private float damageFlashDuration = 0.2f;
    
    [Header("Heal Flash Effect")]
    [SerializeField] private Color healFlashColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private float healFlashDuration = 0.3f;

    private float currentHealthPercent = 1f;
    private bool isFlashing = false;
    private Coroutine currentFlashCoroutine;

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

        SetupOverlay();
    }

    private void SetupOverlay()
    {
        // Create overlay canvas if not assigned
        if (overlayCanvas == null)
        {
            GameObject canvasObj = new GameObject("ScreenOverlayCanvas");
            canvasObj.transform.SetParent(transform);
            
            overlayCanvas = canvasObj.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 1000; // Ensure it's on top
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        // Create overlay image if not assigned
        if (overlayImage == null)
        {
            GameObject imageObj = new GameObject("OverlayImage");
            imageObj.transform.SetParent(overlayCanvas.transform);
            
            overlayImage = imageObj.AddComponent<Image>();
            overlayImage.color = Color.clear;
            
            // Make it fullscreen
            RectTransform rectTransform = overlayImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }

    private void Update()
    {
        UpdateLowHealthEffect();
    }

    /// <summary>
    /// Updates the health percentage for overlay effects
    /// </summary>
    public void UpdateHealthPercent(float healthPercent)
    {
        currentHealthPercent = Mathf.Clamp01(healthPercent);
    }

    /// <summary>
    /// Triggers a damage flash effect
    /// </summary>
    public void TriggerDamageFlash()
    {
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
        }
        currentFlashCoroutine = StartCoroutine(FlashEffect(damageFlashColor, damageFlashDuration));
    }

    /// <summary>
    /// Triggers a heal flash effect
    /// </summary>
    public void TriggerHealFlash()
    {
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
        }
        currentFlashCoroutine = StartCoroutine(FlashEffect(healFlashColor, healFlashDuration));
    }

    /// <summary>
    /// Triggers a custom flash effect
    /// </summary>
    public void TriggerCustomFlash(Color flashColor, float duration)
    {
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
        }
        currentFlashCoroutine = StartCoroutine(FlashEffect(flashColor, duration));
    }

    /// <summary>
    /// Updates the low health pulsing effect
    /// </summary>
    private void UpdateLowHealthEffect()
    {
        if (overlayImage == null || isFlashing)
            return;

        if (currentHealthPercent <= lowHealthThreshold)
        {
            // Calculate pulse intensity based on how low health is
            float healthRatio = currentHealthPercent / lowHealthThreshold;
            float pulseIntensity = 1f - healthRatio; // More intense as health gets lower
            
            // Create pulsing effect
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            pulse *= pulseIntensity;
            
            Color targetColor = lowHealthColor;
            targetColor.a *= pulse;
            
            overlayImage.color = targetColor;
        }
        else
        {
            // Fade out low health effect
            Color currentColor = overlayImage.color;
            if (currentColor.a > 0)
            {
                currentColor.a = Mathf.Lerp(currentColor.a, 0f, Time.deltaTime * 2f);
                overlayImage.color = currentColor;
            }
        }
    }

    /// <summary>
    /// Coroutine for flash effects
    /// </summary>
    private IEnumerator FlashEffect(Color flashColor, float duration)
    {
        isFlashing = true;
        
        float elapsed = 0f;
        Color startColor = overlayImage.color;
        
        // Flash in
        float flashInTime = duration * 0.2f;
        while (elapsed < flashInTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashInTime;
            overlayImage.color = Color.Lerp(startColor, flashColor, t);
            yield return null;
        }
        
        // Hold flash
        float holdTime = duration * 0.3f;
        overlayImage.color = flashColor;
        yield return new WaitForSeconds(holdTime);
        
        // Flash out
        float flashOutTime = duration * 0.5f;
        elapsed = 0f;
        while (elapsed < flashOutTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashOutTime;
            overlayImage.color = Color.Lerp(flashColor, Color.clear, t);
            yield return null;
        }
        
        overlayImage.color = Color.clear;
        isFlashing = false;
        currentFlashCoroutine = null;
    }

    /// <summary>
    /// Clears all overlay effects
    /// </summary>
    public void ClearAllEffects()
    {
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
            currentFlashCoroutine = null;
        }
        
        isFlashing = false;
        if (overlayImage != null)
        {
            overlayImage.color = Color.clear;
        }
    }

    /// <summary>
    /// Sets the overlay visibility
    /// </summary>
    public void SetOverlayEnabled(bool enabled)
    {
        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(enabled);
        }
    }
}