using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Testing and validation script for the Chaos Portal system
/// This can be used to verify that all components work correctly together
/// </summary>
public class ChaosPortalSystemValidator : MonoBehaviour
{
    [Header("Validation Settings")]
    [SerializeField] private bool runValidationOnStart = true;
    [SerializeField] private bool verboseLogging = true;
    
    [Header("System References")]
    private ChaosZoneManager chaosZoneManager;
    private ChaosZoneRewardSystem rewardSystem;
    private ChaosPortalPlacer portalPlacer;

    private void Start()
    {
        if (runValidationOnStart)
        {
            StartCoroutine(ValidateSystemAfterDelay(1f));
        }
    }

    private IEnumerator ValidateSystemAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ValidateChaosPortalSystem();
    }

    public void ValidateChaosPortalSystem()
    {
        Debug.Log("=== Chaos Portal System Validation ===");
        
        bool allSystemsValid = true;
        
        // Test 1: Check Manager Systems
        allSystemsValid &= ValidateManagerSystems();
        
        // Test 2: Check Scene Management
        allSystemsValid &= ValidateSceneManagement();
        
        // Test 3: Check Integration with Existing Systems
        allSystemsValid &= ValidateExistingSystemIntegration();
        
        // Test 4: Check Portal Creation
        allSystemsValid &= ValidatePortalCreation();
        
        // Final result
        string result = allSystemsValid ? "PASSED" : "FAILED";
        Debug.Log($"=== Chaos Portal System Validation {result} ===");
        
        if (!allSystemsValid)
        {
            Debug.LogWarning("Some validation tests failed. Check individual test results above.");
        }
    }

    private bool ValidateManagerSystems()
    {
        if (verboseLogging) Debug.Log("Testing Manager Systems...");
        
        bool success = true;
        
        // Find or create ChaosZoneManager
        chaosZoneManager = FindObjectOfType<ChaosZoneManager>();
        if (chaosZoneManager == null)
        {
            if (verboseLogging) Debug.Log("ChaosZoneManager not found, creating new instance");
            GameObject managerObj = new GameObject("ChaosZoneManager");
            chaosZoneManager = managerObj.AddComponent<ChaosZoneManager>();
        }
        
        if (chaosZoneManager != null)
        {
            if (verboseLogging) Debug.Log("✓ ChaosZoneManager available");
        }
        else
        {
            Debug.LogError("✗ Failed to create ChaosZoneManager");
            success = false;
        }
        
        // Find or create RewardSystem
        rewardSystem = FindObjectOfType<ChaosZoneRewardSystem>();
        if (rewardSystem == null)
        {
            if (verboseLogging) Debug.Log("ChaosZoneRewardSystem not found, creating new instance");
            GameObject rewardObj = new GameObject("ChaosZoneRewardSystem");
            rewardSystem = rewardObj.AddComponent<ChaosZoneRewardSystem>();
        }
        
        if (rewardSystem != null)
        {
            if (verboseLogging) Debug.Log("✓ ChaosZoneRewardSystem available");
        }
        else
        {
            Debug.LogError("✗ Failed to create ChaosZoneRewardSystem");
            success = false;
        }
        
        return success;
    }

    private bool ValidateSceneManagement()
    {
        if (verboseLogging) Debug.Log("Testing Scene Management...");
        
        bool success = true;
        
        // Test SceneLoader enum expansion
        try
        {
            var chaosZoneEnum = SceneLoader.Scene.ChaosZone;
            if (verboseLogging) Debug.Log("✓ SceneLoader.Scene.ChaosZone enum available");
        }
        catch (System.Exception e)
        {
            Debug.LogError("✗ SceneLoader chaos zone enum not available: " + e.Message);
            success = false;
        }
        
        // Test chaos zone detection
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isChaosScene = SceneLoader.IsChaosZoneScene(currentScene);
        if (verboseLogging) Debug.Log($"✓ Scene detection working. Current scene '{currentScene}' is chaos zone: {isChaosScene}");
        
        return success;
    }

    private bool ValidateExistingSystemIntegration()
    {
        if (verboseLogging) Debug.Log("Testing Integration with Existing Systems...");
        
        bool success = true;
        
        // Test PlayerManager integration
        if (PlayerManager.instance != null)
        {
            if (verboseLogging) Debug.Log("✓ PlayerManager.instance available");
            
            if (PlayerManager.instance.player != null)
            {
                if (verboseLogging) Debug.Log("✓ Player reference available");
            }
            else
            {
                Debug.LogWarning("✗ Player reference not available in PlayerManager");
                success = false;
            }
        }
        else
        {
            Debug.LogWarning("✗ PlayerManager.instance not available");
            success = false;
        }
        
        // Test ItemDatabase integration
        if (ItemDatabase.instance != null)
        {
            if (verboseLogging) Debug.Log("✓ ItemDatabase.instance available");
        }
        else
        {
            Debug.LogWarning("✗ ItemDatabase.instance not available (affects rewards)");
            // Not a critical failure for portal functionality
        }
        
        // Test AudioManager integration
        if (AudioManager.instance != null)
        {
            if (verboseLogging) Debug.Log("✓ AudioManager.instance available");
        }
        else
        {
            Debug.LogWarning("✗ AudioManager.instance not available (affects sound effects)");
            // Not a critical failure for portal functionality
        }
        
        return success;
    }

    private bool ValidatePortalCreation()
    {
        if (verboseLogging) Debug.Log("Testing Portal Creation...");
        
        bool success = true;
        
        // Find or create portal placer
        portalPlacer = FindObjectOfType<ChaosPortalPlacer>();
        if (portalPlacer == null)
        {
            if (verboseLogging) Debug.Log("ChaosPortalPlacer not found, creating test instance");
            GameObject placerObj = new GameObject("TestChaosPortalPlacer");
            portalPlacer = placerObj.AddComponent<ChaosPortalPlacer>();
        }
        
        if (portalPlacer != null)
        {
            // Test portal creation
            Vector3 testPosition = Vector3.zero + Vector3.right * 10f;
            int initialPortalCount = portalPlacer.GetPortalCount();
            
            portalPlacer.CreatePortalAtPosition(testPosition, "TestChaosZone");
            
            int finalPortalCount = portalPlacer.GetPortalCount();
            
            if (finalPortalCount > initialPortalCount)
            {
                if (verboseLogging) Debug.Log("✓ Portal creation successful");
                
                // Clean up test portal
                portalPlacer.RemoveAllPortals();
                if (verboseLogging) Debug.Log("✓ Portal cleanup successful");
            }
            else
            {
                Debug.LogError("✗ Portal creation failed");
                success = false;
            }
        }
        else
        {
            Debug.LogError("✗ Failed to create ChaosPortalPlacer");
            success = false;
        }
        
        return success;
    }

    // Test method for validating chaos portal interactable base class integration
    public void TestInteractableIntegration()
    {
        Debug.Log("Testing Interactable Integration...");
        
        // Create a temporary test portal
        GameObject testPortalObj = new GameObject("TestChaosPortal");
        ChaosPortal testPortal = testPortalObj.AddComponent<ChaosPortal>();
        
        // Test if it properly inherits from Interactable
        Interactable interactableComponent = testPortal.GetComponent<Interactable>();
        
        if (interactableComponent != null)
        {
            Debug.Log("✓ ChaosPortal properly inherits from Interactable");
        }
        else
        {
            Debug.LogError("✗ ChaosPortal does not inherit from Interactable properly");
        }
        
        // Clean up
        DestroyImmediate(testPortalObj);
    }

    // Public method to run specific tests
    public void RunQuickValidation()
    {
        Debug.Log("Running Quick Validation...");
        bool managersOk = ValidateManagerSystems();
        bool scenesOk = ValidateSceneManagement();
        
        string result = (managersOk && scenesOk) ? "BASIC SYSTEMS OK" : "ISSUES DETECTED";
        Debug.Log($"Quick Validation Result: {result}");
    }

    // Editor buttons for testing
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Little Grinders/Validate Chaos Portal System")]
    public static void ValidateFromMenu()
    {
        ChaosPortalSystemValidator validator = FindObjectOfType<ChaosPortalSystemValidator>();
        if (validator == null)
        {
            GameObject validatorObj = new GameObject("ChaosPortalValidator");
            validator = validatorObj.AddComponent<ChaosPortalSystemValidator>();
        }
        
        validator.ValidateChaosPortalSystem();
    }
    
    [UnityEditor.MenuItem("Little Grinders/Quick Chaos Portal Test")]
    public static void QuickTestFromMenu()
    {
        ChaosPortalSystemValidator validator = FindObjectOfType<ChaosPortalSystemValidator>();
        if (validator == null)
        {
            GameObject validatorObj = new GameObject("ChaosPortalValidator");
            validator = validatorObj.AddComponent<ChaosPortalSystemValidator>();
        }
        
        validator.RunQuickValidation();
    }
#endif
}