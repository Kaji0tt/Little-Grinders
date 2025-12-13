using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Altar that deactivates an active Totem challenge and rewards the player
/// based on performance (waves survived, time active)
/// </summary>
public class AltarInteractable : BaseInteractable
{
    [Header("Altar Visual Effects")]
    [Tooltip("Permanentes Licht - läuft durchgehend")]
    [SerializeField] private GameObject permanentLight;
    
    [Tooltip("Special Effect - wird aktiviert während Totem Event läuft")]
    [SerializeField] private GameObject activeEventEffect;
    
    [Tooltip("Completion Effect - wird einmal abgespielt wenn Event am Altar abgegeben wird")]
    [SerializeField] private GameObject completionEffect;
    
    [Header("Reward Settings")]
    [SerializeField] private int baseVoidEssence = 10;
    [SerializeField] private int voidEssencePerWave = 5;
    [SerializeField] private int voidEssencePerMinute = 10;
    
    [Header("Sound Settings")]
    [SerializeField] private string activationSoundName = "Altar_Activate";
    [SerializeField] private string deactivationSoundName = "Altar_Deactivate";
    
    private TotemInteractable linkedTotem;
    private bool isTotemActive = false;
    
    protected override void Start()
    {
        base.Start();
        
        // Permanentes Licht immer an
        if (permanentLight != null)
        {
            permanentLight.SetActive(true);
        }
        
        // Event Effect am Anfang aus
        if (activeEventEffect != null)
        {
            activeEventEffect.SetActive(false);
        }
        
        // Completion Effect am Anfang aus
        if (completionEffect != null)
        {
            completionEffect.SetActive(false);
        }
        
        // Find linked totem
        FindLinkedTotem();
    }
    
    private void FindLinkedTotem()
    {
        // Find Totem in current scene (no longer uses Singleton)
        linkedTotem = FindFirstObjectByType<TotemInteractable>();
        
        if (linkedTotem != null)
        {
            Debug.Log($"[AltarInteractable] Linked to Totem at {linkedTotem.transform.position}");
        }
        else
        {
            Debug.LogWarning("[AltarInteractable] No Totem found in scene!");
        }
    }
    
    protected override bool CanInteract()
    {
        // Can only interact if a totem is currently active
        return linkedTotem != null && linkedTotem.IsChallengeActive();
    }
    
    protected override void OnInteract()
    {
        if (linkedTotem == null || !linkedTotem.IsChallengeActive())
        {
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("No active Totem challenge to deactivate");
            }
            return;
        }
        
        // Play sound
        if (AudioManager.instance != null && !string.IsNullOrEmpty(deactivationSoundName))
        {
            AudioManager.instance.PlaySound(deactivationSoundName);
        }
        
        // Get performance data from totem
        int wavesCompleted = linkedTotem.GetWaveNumber();
        float activeTime = linkedTotem.GetActiveTime();
        
        // Calculate rewards
        int voidEssenceReward = CalculateVoidEssenceReward(wavesCompleted, activeTime);
        
        // Stop totem challenge
        linkedTotem.DeactivateChallenge();
        
        // Deaktiviere Event Effect
        StopEventEffect();
        
        // Spiele Completion Effect ab
        PlayCompletionEffect();
        
        // Give rewards
        GiveRewards(voidEssenceReward);
        
        // Mark map as cleared ("eclipsed") and save immediately
        if (GlobalMap.instance != null && GlobalMap.instance.currentMap != null)
        {
            Vector2 mapPos = new Vector2(GlobalMap.instance.currentMap.mapIndexX, GlobalMap.instance.currentMap.mapIndexY);
            Debug.Log($"[AltarInteractable] Marking map ({mapPos.x}, {mapPos.y}) as CLEARED");
            
            GlobalMap.instance.currentMap.isCleared = true;
            Debug.Log($"[AltarInteractable] isCleared set to: {GlobalMap.instance.currentMap.isCleared}");
            
            // Save progress immediately to persist isCleared state
            PlayerSave currentSave = new PlayerSave();
            SaveSystem.SavePlayer(currentSave);
            Debug.Log("[AltarInteractable] Progress saved with cleared status");
            
            // KRITISCH: Triggere UI-Update damit Map sofort als gecleared angezeigt wird
            GlobalMap.instance.TriggerMapListChanged();
            Debug.Log("[AltarInteractable] UI update triggered - Map should now show as cleared");
        }
        
        // Hide Void Essence UI after reward animation
        UI_VoidEssence voidEssenceUI = FindFirstObjectByType<UI_VoidEssence>();
        if (voidEssenceUI != null)
        {
            voidEssenceUI.HideForChallenge();
        }
        
        // Log
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog($"Totem deactivated! Received {voidEssenceReward} Void Essence");
        }
        
        Debug.Log($"[AltarInteractable] Totem deactivated - Waves: {wavesCompleted}, Time: {activeTime:F1}s, Reward: {voidEssenceReward} Void Essence");
    }
    
    private int CalculateVoidEssenceReward(int waves, float timeInSeconds)
    {
        int waveBonus = waves * voidEssencePerWave;
        int timeBonus = Mathf.FloorToInt((timeInSeconds / 60f) * voidEssencePerMinute);
        int total = baseVoidEssence + waveBonus + timeBonus;
        
        return total;
    }
    
    private void GiveRewards(int voidEssenceAmount)
    {
        // Give Void Essence currency
        if (VoidEssenceManager.instance != null)
        {
            VoidEssenceManager.instance.AddVoidEssence(voidEssenceAmount);
            Debug.Log($"[AltarInteractable] Granted {voidEssenceAmount} Void Essence");
        }
        else
        {
            Debug.LogError("[AltarInteractable] VoidEssenceManager not found! Cannot grant rewards.");
        }
    }
    
    /// <summary>
    /// Called by TotemInteractable when challenge starts
    /// </summary>
    public void OnTotemActivated()
    {
        isTotemActive = true;
        
        // Play sound
        if (AudioManager.instance != null && !string.IsNullOrEmpty(activationSoundName))
        {
            AudioManager.instance.PlaySound(activationSoundName);
        }
        
        // Aktiviere Event Effect
        StartEventEffect();
        
        Debug.Log("[AltarInteractable] Totem activated - Altar event effect active");
    }
    
    /// <summary>
    /// Called by TotemInteractable when challenge ends (death, map change, etc)
    /// </summary>
    public void OnTotemDeactivated()
    {
        isTotemActive = false;
        StopEventEffect();
        
        Debug.Log("[AltarInteractable] Totem deactivated - Altar event effect stopped");
    }
    
    private void StartEventEffect()
    {
        if (activeEventEffect != null)
        {
            activeEventEffect.SetActive(true);
        }
    }
    
    private void StopEventEffect()
    {
        if (activeEventEffect != null)
        {
            activeEventEffect.SetActive(false);
        }
    }
    
    private void PlayCompletionEffect()
    {
        if (completionEffect != null)
        {
            // Aktiviere kurz für einmalige Wiedergabe
            completionEffect.SetActive(false); // Reset falls vorher aktiv
            completionEffect.SetActive(true);
            
            // Deaktiviere nach 3 Sekunden automatisch (kann angepasst werden)
            StartCoroutine(DeactivateCompletionEffectAfterDelay(3f));
        }
    }
    
    private IEnumerator DeactivateCompletionEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (completionEffect != null)
        {
            completionEffect.SetActive(false);
        }
    }
    
    void OnDestroy()
    {
        StopEventEffect();
    }
    
    protected override string GetCustomSaveData()
    {
        // No save data needed for altars
        return string.Empty;
    }
    
    protected override void ApplyCustomSaveData(string data)
    {
        // No save data needed for altars
    }
}
