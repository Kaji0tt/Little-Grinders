using System;
using UnityEngine;

/// <summary>
/// Simple test script to verify the Map Save enhancements work correctly
/// This script can be attached to a GameObject in the Unity scene for testing
/// </summary>
public class MapSaveTest : MonoBehaviour
{
    [Header("Test Controls")]
    public KeyCode testSaveKey = KeyCode.F1;
    public KeyCode testLoadKey = KeyCode.F2;
    
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
    }
    
    void TestSaveInteractables()
    {
        Debug.Log("=== TESTING INTERACTABLE SAVE ===");
        
        if (GlobalMap.instance?.currentMap != null)
        {
            GlobalMap.instance.currentMap.SaveInteractables();
            Debug.Log("✓ Interactables saved to current map");
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
            GlobalMap.instance.currentMap.RestoreInteractables();
            Debug.Log("✓ Interactables restored from current map");
        }
        else
        {
            Debug.LogWarning("No current map found");
        }
    }
}