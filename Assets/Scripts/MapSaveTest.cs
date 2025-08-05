using System;
using UnityEngine;

/// <summary>
/// Simple test script to verify the Map Save enhancements work correctly
/// This script can be attached to a GameObject in the Unity scene for testing
/// 
/// USAGE:
/// 1. Attach this script to any GameObject in the map scene
/// 2. Generate a map with interactables
/// 3. Use some interactables (open chests, activate totems, etc.)
/// 4. Press F1 to manually save current interactables
/// 5. Press F2 to manually restore interactables (will clear current and restore saved ones)
/// 6. Or save/load the game normally - interactables should persist automatically
/// 
/// AUTOMATIC INTEGRATION:
/// - Interactables are automatically saved when creating new maps
/// - Interactables are automatically saved when player saves their game
/// - Interactables are automatically restored when loading existing maps
/// - Interactable states (used/unused) persist across save/load cycles
/// </summary>
public class MapSaveTest : MonoBehaviour
{
    [Header("Test Controls")]
    public KeyCode testSaveKey = KeyCode.F1;
    public KeyCode testLoadKey = KeyCode.F2;
    public KeyCode clearInteractablesKey = KeyCode.F3;
    
    void Update()
    {
        if (Input.GetKeyDown(testSaveKey))
        {
            TestSaveInteractables();
        }
        
        if (Input.GetKeyDown(testLoadKey))
        {
            TestLoadInteractables();
        }
        
        if (Input.GetKeyDown(clearInteractablesKey))
        {
            ClearAllInteractables();
        }
    }
    
    void TestSaveInteractables()
    {
        Debug.Log("=== TESTING INTERACTABLE SAVE ===");
        
        if (GlobalMap.instance?.currentMap != null)
        {
            GlobalMap.instance.currentMap.SaveInteractables();
            Debug.Log("✓ Interactables saved to current map");
            
            // Show what was saved
            var map = GlobalMap.instance.currentMap;
            Debug.Log($"Saved {map.interactables.Count} interactables:");
            foreach (var interactable in map.interactables)
            {
                Debug.Log($"  - {interactable.interactableType} at {interactable.GetPosition()} (Used: {interactable.isUsed})");
            }
        }
        else
        {
            Debug.LogWarning("No current map found");
        }
    }
    
    void TestLoadInteractables()
    {
        Debug.Log("=== TESTING INTERACTABLE LOAD ===");
        
        if (GlobalMap.instance?.currentMap != null)
        {
            // Clear existing interactables first for cleaner test
            ClearAllInteractables();
            
            GlobalMap.instance.currentMap.RestoreInteractables();
            Debug.Log("✓ Interactables restored from current map");
        }
        else
        {
            Debug.LogWarning("No current map found");
        }
    }
    
    void ClearAllInteractables()
    {
        Debug.Log("=== CLEARING ALL INTERACTABLES ===");
        
        Interactable[] interactables = FindObjectsOfType<Interactable>();
        foreach (Interactable interactable in interactables)
        {
            DestroyImmediate(interactable.gameObject);
        }
        
        Debug.Log($"Cleared {interactables.Length} interactables");
    }
    
    void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 300, 100), "Map Save Interactable Test");
        GUI.Label(new Rect(20, 35, 280, 20), $"F1: Save Interactables");
        GUI.Label(new Rect(20, 55, 280, 20), $"F2: Restore Interactables");
        GUI.Label(new Rect(20, 75, 280, 20), $"F3: Clear All Interactables");
        
        if (GlobalMap.instance?.currentMap != null)
        {
            int savedCount = GlobalMap.instance.currentMap.interactables?.Count ?? 0;
            GUI.Label(new Rect(20, 95, 280, 20), $"Saved: {savedCount} interactables");
        }
    }
}