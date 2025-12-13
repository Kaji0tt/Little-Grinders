using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI display for active Totem challenge
/// Shows wave number, elapsed time, and countdown to next wave
/// </summary>
public class UI_TotemChallenge : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI waveNumberText;
    [SerializeField] private TextMeshProUGUI timeElapsedText;
    [SerializeField] private TextMeshProUGUI nextWaveCountdownText;
    [SerializeField] private Slider countdownSlider; // Slider instead of Image for countdown bar
    
    [Header("Visual Settings")]
    [SerializeField] private Color waveColor = new Color(0.8f, 0.2f, 0.2f, 1f); // Red
    [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0f, 1f); // Orange
    [SerializeField] private float warningPulseSpeed = 2f;
    [SerializeField] private float fadeSpeed = 2f;
    
    [Header("Animation")]
    [SerializeField] private float waveChangeScalePulse = 1.3f;
    [SerializeField] private float waveChangeAnimDuration = 0.4f;
    
    private TotemInteractable activeTotem;
    private bool isActive = false;
    private int lastWaveNumber = 0;
    
    private void Start()
    {
        // Start hidden
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
    
    private void Update()
    {
        if (!isActive || activeTotem == null)
            return;
        
        // Update displays
        UpdateWaveDisplay();
        UpdateTimeDisplay();
        UpdateCountdownDisplay();
    }
    
    /// <summary>
    /// Show the challenge UI for an active totem
    /// </summary>
    public void ShowChallengeUI(TotemInteractable totem)
    {
        activeTotem = totem;
        isActive = true;
        lastWaveNumber = 0;
        
        // Fade in
        StartCoroutine(FadeIn());
        
        Debug.Log("[UI_TotemChallenge] Challenge UI shown");
    }
    
    /// <summary>
    /// Hide the challenge UI
    /// </summary>
    public void HideChallengeUI()
    {
        isActive = false;
        activeTotem = null;
        
        // Fade out
        StartCoroutine(FadeOut());
        
        Debug.Log("[UI_TotemChallenge] Challenge UI hidden");
    }
    
    private void UpdateWaveDisplay()
    {
        if (activeTotem == null || waveNumberText == null)
            return;
        
        int currentWave = activeTotem.GetWaveNumber();
        
        // Animate on wave change
        if (currentWave > lastWaveNumber && lastWaveNumber > 0)
        {
            StartCoroutine(WaveChangeAnimation());
        }
        
        lastWaveNumber = currentWave;
        waveNumberText.text = $"Wave {currentWave}";
    }
    
    private void UpdateTimeDisplay()
    {
        if (activeTotem == null || timeElapsedText == null)
            return;
        
        float timeActive = activeTotem.GetActiveTime();
        int minutes = Mathf.FloorToInt(timeActive / 60f);
        int seconds = Mathf.FloorToInt(timeActive % 60f);
        
        timeElapsedText.text = $"{minutes:00}:{seconds:00}";
    }
    
    private void UpdateCountdownDisplay()
    {
        if (activeTotem == null)
            return;
        
        // Calculate time until next wave
        float waveInterval = 12f; // Should match TotemInteractable.waveInterval
        float timeSinceLastWave = activeTotem.GetTimeSinceLastWave();
        float timeUntilNextWave = Mathf.Max(0f, waveInterval - timeSinceLastWave);
        
        // Update countdown text
        if (nextWaveCountdownText != null)
        {
            if (timeUntilNextWave > 0f)
            {
                nextWaveCountdownText.text = $"Next Wave: {timeUntilNextWave:F1}s";
            }
            else
            {
                nextWaveCountdownText.text = "Spawning...";
            }
        }
        
        // Update slider
        if (countdownSlider != null)
        {
            float fillAmount = 1f - (timeUntilNextWave / waveInterval);
            countdownSlider.value = fillAmount;
            
            // Change color based on urgency
            Image sliderFill = countdownSlider.fillRect?.GetComponent<Image>();
            if (sliderFill != null)
            {
                if (timeUntilNextWave < 3f)
                {
                    sliderFill.color = warningColor;
                }
                else
                {
                    sliderFill.color = waveColor;
                }
            }
        }
    }
    
    private IEnumerator FadeIn()
    {
        if (canvasGroup == null)
            yield break;
        
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut()
    {
        if (canvasGroup == null)
            yield break;
        
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    
    private IEnumerator WaveChangeAnimation()
    {
        if (waveNumberText == null)
            yield break;
        
        Vector3 originalScale = waveNumberText.transform.localScale;
        Vector3 targetScale = originalScale * waveChangeScalePulse;
        
        // Scale up
        float elapsed = 0f;
        while (elapsed < waveChangeAnimDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (waveChangeAnimDuration / 2f);
            waveNumberText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        
        // Scale down
        elapsed = 0f;
        while (elapsed < waveChangeAnimDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (waveChangeAnimDuration / 2f);
            waveNumberText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        
        waveNumberText.transform.localScale = originalScale;
    }
}
