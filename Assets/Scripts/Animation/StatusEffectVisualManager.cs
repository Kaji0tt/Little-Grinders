using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Manages visual status effects and indicators for enhanced player feedback.
/// Handles buffs, debuffs, and temporary status indicators.
/// </summary>
public class StatusEffectVisualManager : MonoBehaviour
{
    public static StatusEffectVisualManager Instance { get; private set; }

    [Header("Status Effect Settings")]
    [SerializeField] private Transform statusEffectParent;
    [SerializeField] private GameObject statusEffectIconPrefab;
    [SerializeField] private float iconSpacing = 32f;
    [SerializeField] private float maxDisplayTime = 10f;

    // Dictionary to track active status effects
    private Dictionary<string, StatusEffectIcon> activeEffects = new Dictionary<string, StatusEffectIcon>();

    // Predefined status effect types
    public enum StatusEffectType
    {
        Buff,       // Positive effects (green tint)
        Debuff,     // Negative effects (red tint)
        Neutral,    // Neutral effects (white/yellow tint)
        Special     // Special effects (custom colors)
    }

    [System.Serializable]
    public class StatusEffectData
    {
        public string effectName;
        public StatusEffectType effectType;
        public Sprite icon;
        public Color effectColor = Color.white;
        public float duration;
        public bool showTimer;
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

        // Ensure we have a parent for status effects
        if (statusEffectParent == null)
        {
            GameObject parent = new GameObject("StatusEffectContainer");
            parent.transform.SetParent(transform);
            statusEffectParent = parent.transform;
        }
    }

    /// <summary>
    /// Adds or updates a status effect visual indicator
    /// </summary>
    /// <param name="effectData">Data for the status effect</param>
    public void AddStatusEffect(StatusEffectData effectData)
    {
        // Remove existing effect if it exists
        if (activeEffects.ContainsKey(effectData.effectName))
        {
            RemoveStatusEffect(effectData.effectName);
        }

        // Create new status effect icon
        CreateStatusEffectIcon(effectData);
    }

    /// <summary>
    /// Removes a status effect visual indicator
    /// </summary>
    /// <param name="effectName">Name of the effect to remove</param>
    public void RemoveStatusEffect(string effectName)
    {
        if (activeEffects.TryGetValue(effectName, out StatusEffectIcon icon))
        {
            if (icon != null && icon.gameObject != null)
            {
                Destroy(icon.gameObject);
            }
            activeEffects.Remove(effectName);
            
            // Reposition remaining icons
            RepositionIcons();
        }
    }

    /// <summary>
    /// Updates the duration of an existing status effect
    /// </summary>
    /// <param name="effectName">Name of the effect</param>
    /// <param name="newDuration">New duration</param>
    public void UpdateStatusEffectDuration(string effectName, float newDuration)
    {
        if (activeEffects.TryGetValue(effectName, out StatusEffectIcon icon))
        {
            icon.UpdateDuration(newDuration);
        }
    }

    /// <summary>
    /// Clears all status effects
    /// </summary>
    public void ClearAllStatusEffects()
    {
        foreach (var effect in activeEffects.Values)
        {
            if (effect != null && effect.gameObject != null)
            {
                Destroy(effect.gameObject);
            }
        }
        activeEffects.Clear();
    }

    /// <summary>
    /// Creates a status effect icon
    /// </summary>
    private void CreateStatusEffectIcon(StatusEffectData effectData)
    {
        if (statusEffectIconPrefab == null)
        {
            Debug.LogWarning("StatusEffectVisualManager: No icon prefab assigned!");
            return;
        }

        GameObject iconObj = Instantiate(statusEffectIconPrefab, statusEffectParent);
        StatusEffectIcon icon = iconObj.GetComponent<StatusEffectIcon>();
        
        if (icon == null)
        {
            icon = iconObj.AddComponent<StatusEffectIcon>();
        }

        icon.Initialize(effectData);
        activeEffects[effectData.effectName] = icon;
        
        // Position the icon
        RepositionIcons();
    }

    /// <summary>
    /// Repositions all status effect icons
    /// </summary>
    private void RepositionIcons()
    {
        int index = 0;
        foreach (var effect in activeEffects.Values)
        {
            if (effect != null && effect.gameObject != null)
            {
                RectTransform rectTransform = effect.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(index * iconSpacing, 0);
                }
                index++;
            }
        }
    }

    /// <summary>
    /// Convenience methods for common status effects
    /// </summary>
    
    public void ShowBuffEffect(string effectName, Sprite icon, float duration)
    {
        StatusEffectData data = new StatusEffectData
        {
            effectName = effectName,
            effectType = StatusEffectType.Buff,
            icon = icon,
            effectColor = Color.green,
            duration = duration,
            showTimer = true
        };
        AddStatusEffect(data);
    }

    public void ShowDebuffEffect(string effectName, Sprite icon, float duration)
    {
        StatusEffectData data = new StatusEffectData
        {
            effectName = effectName,
            effectType = StatusEffectType.Debuff,
            icon = icon,
            effectColor = Color.red,
            duration = duration,
            showTimer = true
        };
        AddStatusEffect(data);
    }

    public void ShowCooldownEffect(string effectName, Sprite icon, float cooldownDuration)
    {
        StatusEffectData data = new StatusEffectData
        {
            effectName = effectName,
            effectType = StatusEffectType.Neutral,
            icon = icon,
            effectColor = Color.gray,
            duration = cooldownDuration,
            showTimer = true
        };
        AddStatusEffect(data);
    }
}

/// <summary>
/// Individual status effect icon component
/// </summary>
public class StatusEffectIcon : MonoBehaviour
{
    private Image iconImage;
    private Image timerFill;
    private Text timerText;
    
    private float duration;
    private float remainingTime;
    private bool showTimer;
    
    private void Awake()
    {
        // Get or create necessary components
        iconImage = GetComponent<Image>();
        if (iconImage == null)
        {
            iconImage = gameObject.AddComponent<Image>();
        }
        
        // Create timer overlay if needed
        CreateTimerOverlay();
    }
    
    public void Initialize(StatusEffectVisualManager.StatusEffectData data)
    {
        duration = data.duration;
        remainingTime = duration;
        showTimer = data.showTimer;
        
        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.color = data.effectColor;
        }
        
        if (timerFill != null)
        {
            timerFill.gameObject.SetActive(showTimer);
        }
        
        if (timerText != null)
        {
            timerText.gameObject.SetActive(showTimer && duration > 0);
        }
        
        // Start countdown if duration is set
        if (duration > 0)
        {
            StartCoroutine(CountdownCoroutine());
        }
    }
    
    public void UpdateDuration(float newDuration)
    {
        duration = newDuration;
        remainingTime = newDuration;
    }
    
    private void CreateTimerOverlay()
    {
        // Create a child object for timer fill
        GameObject timerObj = new GameObject("TimerFill");
        timerObj.transform.SetParent(transform);
        
        RectTransform timerRect = timerObj.AddComponent<RectTransform>();
        timerRect.anchorMin = Vector2.zero;
        timerRect.anchorMax = Vector2.one;
        timerRect.offsetMin = Vector2.zero;
        timerRect.offsetMax = Vector2.zero;
        
        timerFill = timerObj.AddComponent<Image>();
        timerFill.type = Image.Type.Filled;
        timerFill.fillMethod = Image.FillMethod.Radial360;
        timerFill.color = new Color(0, 0, 0, 0.5f);
        
        // Create text for timer display
        GameObject textObj = new GameObject("TimerText");
        textObj.transform.SetParent(transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        timerText = textObj.AddComponent<Text>();
        timerText.text = "";
        timerText.alignment = TextAnchor.MiddleCenter;
        timerText.fontSize = 12;
        timerText.color = Color.white;
        timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
    
    private IEnumerator CountdownCoroutine()
    {
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            
            if (showTimer)
            {
                // Update timer fill
                if (timerFill != null)
                {
                    timerFill.fillAmount = remainingTime / duration;
                }
                
                // Update timer text
                if (timerText != null && duration > 3f) // Only show text for longer durations
                {
                    timerText.text = Mathf.Ceil(remainingTime).ToString();
                }
            }
            
            yield return null;
        }
        
        // Remove effect when time runs out
        if (StatusEffectVisualManager.Instance != null)
        {
            // This will be handled by the manager's cleanup
            Destroy(gameObject);
        }
    }
}