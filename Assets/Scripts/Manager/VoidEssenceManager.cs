using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Manages Void Essence currency - a special resource earned from Totem challenges
/// Used for high-tier crafting and upgrades
/// </summary>
public class VoidEssenceManager : MonoBehaviour
{
    #region Singleton
    public static VoidEssenceManager instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    
    [Header("Void Essence")]
    [SerializeField] private int currentVoidEssence = 0;
    
    // Events
    public event Action<int> OnVoidEssenceChanged;
    
    /// <summary>
    /// Get current Void Essence amount
    /// </summary>
    public int GetVoidEssence()
    {
        return currentVoidEssence;
    }
    
    /// <summary>
    /// Add Void Essence and trigger UI update
    /// </summary>
    public void AddVoidEssence(int amount)
    {
        if (amount <= 0) return;
        
        currentVoidEssence += amount;
        OnVoidEssenceChanged?.Invoke(currentVoidEssence);
        
        Debug.Log($"[VoidEssenceManager] Added {amount} Void Essence. Total: {currentVoidEssence}");
    }
    
    /// <summary>
    /// Remove Void Essence (for purchases/crafting)
    /// </summary>
    public bool SpendVoidEssence(int amount)
    {
        if (amount <= 0 || currentVoidEssence < amount)
        {
            Debug.LogWarning($"[VoidEssenceManager] Cannot spend {amount} Void Essence. Current: {currentVoidEssence}");
            return false;
        }
        
        currentVoidEssence -= amount;
        OnVoidEssenceChanged?.Invoke(currentVoidEssence);
        
        Debug.Log($"[VoidEssenceManager] Spent {amount} Void Essence. Remaining: {currentVoidEssence}");
        return true;
    }
    
    /// <summary>
    /// Check if player has enough Void Essence
    /// </summary>
    public bool HasEnoughVoidEssence(int amount)
    {
        return currentVoidEssence >= amount;
    }
    
    /// <summary>
    /// Reset Void Essence (for new game or debug)
    /// </summary>
    public void ResetVoidEssence()
    {
        currentVoidEssence = 0;
        OnVoidEssenceChanged?.Invoke(currentVoidEssence);
        Debug.Log("[VoidEssenceManager] Void Essence reset to 0");
    }
}
