using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI display for Void Essence currency counter
/// Shows current amount with visual polish (glow, animation on change)
/// </summary>
public class UI_VoidEssence : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI essenceText;
    [SerializeField] private Image essenceIcon;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = new Color(0.6f, 0.2f, 0.8f, 1f); // Purple
    [SerializeField] private Color highlightColor = new Color(1f, 0.5f, 1f, 1f); // Bright purple
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseDuration = 0.5f;
    
    [Header("Animation")]
    [SerializeField] private float scaleUpAmount = 1.2f;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private float fadeSpeed = 5f; // Speed of fade in/out
    
    private int currentDisplayedAmount = 0;
    private Coroutine pulseCoroutine;
    private Coroutine scaleCoroutine;
    private Coroutine fadeCoroutine;
    private bool isChallengeActive = false;
    private bool isInventoryOpen = false;
    
    private void Start()
    {
        // Subscribe to Void Essence changes
        if (VoidEssenceManager.instance != null)
        {
            VoidEssenceManager.instance.OnVoidEssenceChanged += OnVoidEssenceChanged;
            currentDisplayedAmount = VoidEssenceManager.instance.GetVoidEssence();
        }
        
        // Initialize display
        UpdateDisplay(currentDisplayedAmount, false);
        
        // Set default color
        if (essenceIcon != null)
        {
            essenceIcon.color = normalColor;
        }
        
        // Start hidden
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
    
    private void Update()
    {
        // Check inventory state
        bool inventoryCurrentlyOpen = CheckInventoryOpen();
        
        if (inventoryCurrentlyOpen != isInventoryOpen)
        {
            isInventoryOpen = inventoryCurrentlyOpen;
            UpdateVisibility();
        }
    }
    
    private bool CheckInventoryOpen()
    {
        // Find inventory canvas by searching for InterfaceElement with Inventar type
        InterfaceElement[] elements = FindObjectsByType<InterfaceElement>(FindObjectsSortMode.None);
        foreach (var element in elements)
        {
            if (element.interfaceElementEnum == InterfaceElementDeclaration.Inventar)
            {
                CanvasGroup cg = element.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    return cg.alpha > 0.5f; // Consider visible if more than 50% opacity
                }
            }
        }
        return false;
    }
    
    /// <summary>
    /// Called by TotemInteractable when challenge starts
    /// </summary>
    public void ShowForChallenge()
    {
        isChallengeActive = true;
        UpdateVisibility();
    }
    
    /// <summary>
    /// Called by TotemInteractable/AltarInteractable when challenge ends
    /// </summary>
    public void HideForChallenge()
    {
        isChallengeActive = false;
        UpdateVisibility();
    }
    
    private void UpdateVisibility()
    {
        bool shouldBeVisible = isChallengeActive || isInventoryOpen;
        
        if (canvasGroup != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(shouldBeVisible ? 1f : 0f));
            canvasGroup.blocksRaycasts = shouldBeVisible;
        }
    }
    
    private IEnumerator FadeCanvasGroup(float targetAlpha)
    {
        if (canvasGroup == null) yield break;
        
        while (Mathf.Abs(canvasGroup.alpha - targetAlpha) > 0.01f)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe
        if (VoidEssenceManager.instance != null)
        {
            VoidEssenceManager.instance.OnVoidEssenceChanged -= OnVoidEssenceChanged;
        }
    }
    
    private void OnVoidEssenceChanged(int newAmount)
    {
        UpdateDisplay(newAmount, true);
    }
    
    private void UpdateDisplay(int amount, bool animate)
    {
        currentDisplayedAmount = amount;
        
        if (essenceText != null)
        {
            essenceText.text = amount.ToString();
        }
        
        if (animate)
        {
            // Play pulse effect
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine);
            }
            pulseCoroutine = StartCoroutine(PulseEffect());
            
            // Play scale animation
            if (scaleCoroutine != null)
            {
                StopCoroutine(scaleCoroutine);
            }
            scaleCoroutine = StartCoroutine(ScaleAnimation());
        }
    }
    
    private IEnumerator PulseEffect()
    {
        float elapsed = 0f;
        
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pulseDuration;
            
            // Pulse color
            Color targetColor = Color.Lerp(highlightColor, normalColor, t);
            
            if (essenceIcon != null)
            {
                essenceIcon.color = targetColor;
            }
            
            if (essenceText != null)
            {
                essenceText.color = targetColor;
            }
            
            yield return null;
        }
        
        // Ensure we end at normal color
        if (essenceIcon != null)
        {
            essenceIcon.color = normalColor;
        }
        
        if (essenceText != null)
        {
            essenceText.color = normalColor;
        }
    }
    
    private IEnumerator ScaleAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleUpAmount;
        
        // Scale up
        float elapsed = 0f;
        while (elapsed < animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (animationDuration / 2f);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        // Scale down
        elapsed = 0f;
        while (elapsed < animationDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (animationDuration / 2f);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        // Ensure we end at original scale
        transform.localScale = originalScale;
    }
}
