using UnityEngine;
using System;

public abstract class BaseInteractable : MonoBehaviour
{
    [Header("Base Interactable Settings")]
     // Eindeutige ID für Save-System
        public string displayName = "Interactable";

        public string interactableID; // Eindeutige ID für Save-System 
        public bool isUsed = false;
        public bool canBeUsedMultipleTimes = false;
        
        [Header("VFX & Audio")]
        public string useVFXName;
        public string useSoundName;
        
        [Header("UI Feedback")]
        public string interactPrompt = "Press [E] to interact";
        
        // Hilfsfunktion für dynamischen Interact-Prompt
        protected string GetDynamicInteractPrompt()
        {
            if (KeyManager.MyInstance?.Keybinds != null && KeyManager.MyInstance.Keybinds.ContainsKey("PICK"))
            {
                var pickKey = KeyManager.MyInstance.Keybinds["PICK"];
                return interactPrompt.Replace("[E]", $"[{pickKey}]");
            }
            return interactPrompt;
        }
    protected virtual void Start()
    {
        // Auto-generiere ID falls nicht gesetzt
        if (string.IsNullOrEmpty(interactableID))
        {
            interactableID = $"{gameObject.name}_{transform.position.x}_{transform.position.z}";
        }
        
        // Lade Zustand aus SaveGame
        LoadState();
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (IsPlayer(other) && CanInteract())
        {
            // UI Prompt anzeigen
            ShowInteractPrompt(true);
            
            if (Input.GetKeyDown(KeyManager.MyInstance.Keybinds["PICK"]))
            {
                Interact();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            ShowInteractPrompt(false);
        }
    }
    
    protected bool IsPlayer(Collider other)
    {
        return other == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>();
    }
    
    protected virtual bool CanInteract()
    {
        return !isUsed || canBeUsedMultipleTimes;
    }
    
    public void Interact()
    {
        if (!CanInteract()) return;
        
        // Markiere als benutzt
        if (!canBeUsedMultipleTimes)
        {
            isUsed = true;
        }
        
        // Führe spezifische Interaktion aus
        OnInteract();
        
        // VFX & Audio
        PlayEffects();
        
        // Speichere Zustand
        SaveState();
        
        // UI Prompt verstecken
        ShowInteractPrompt(false);
    }
    
    // Abstrakte Methode - muss von Subklassen implementiert werden
    protected abstract void OnInteract();
    
    protected virtual void PlayEffects()
    {
        // VFX
        if (!string.IsNullOrEmpty(useVFXName) && VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect(useVFXName, transform.position, Quaternion.identity);
        }
        
        // Audio
        if (!string.IsNullOrEmpty(useSoundName) && AudioManager.instance != null)
        {
            AudioManager.instance.PlaySound(useSoundName);
        }
    }
    
    protected virtual void ShowInteractPrompt(bool show)
    {
        if (LogScript.instance != null && show && CanInteract())
        {
            LogScript.instance.ShowLog(interactPrompt, 0.1f);
        }
    }
    
    #region Save/Load System
    protected virtual void SaveState()
    {
        if (InteractableManager.instance != null)
        {
            InteractableManager.instance.SaveInteractableState(interactableID, GetSaveData());
        }
    }
    
    protected virtual void LoadState()
    {
        if (InteractableManager.instance != null)
        {
            var saveData = InteractableManager.instance.LoadInteractableState(interactableID);
            if (saveData != null)
            {
                ApplySaveData(saveData);
            }
        }
    }
    
    protected virtual InteractableSaveData GetSaveData()
    {
        return new InteractableSaveData
        {
            interactableID = this.interactableID,
            isUsed = this.isUsed,
            customData = GetCustomSaveData()
        };
    }
    
    protected virtual void ApplySaveData(InteractableSaveData data)
    {
        this.isUsed = data.isUsed;
        ApplyCustomSaveData(data.customData);
    }
    
    // Für spezifische Daten der Subklassen
    protected virtual string GetCustomSaveData() { return ""; }
    protected virtual void ApplyCustomSaveData(string data) { }
    #endregion
}