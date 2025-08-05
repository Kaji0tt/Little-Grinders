using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Specifically handles adding chaos portal to the Intro level as requested in the requirements.
/// This introduces the "chaos zone" concept early in the player experience.
/// </summary>
public class IntroLevelChaosSetup : MonoBehaviour
{
    [Header("Intro Level Chaos Portal")]
    [SerializeField] private Vector3 chaosPortalPosition = new Vector3(10f, 0f, 10f);
    [SerializeField] private bool createOnStart = true;
    [SerializeField] private float introDelay = 5f; // Delay to let player explore first
    
    [Header("Tutorial Integration")]
    [SerializeField] private bool showTutorialMessage = true;
    [SerializeField] private string tutorialMessage = "A strange portal has appeared! Press Q to investigate when near.";
    [SerializeField] private float tutorialMessageDuration = 4f;

    private ChaosPortalPlacer portalPlacer;
    private bool portalCreated = false;

    private void Start()
    {
        // Verify we're in the intro scene
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (!currentScene.Contains("Intro"))
        {
            Debug.Log("IntroLevelChaosSetup: Not in intro level, disabling.");
            gameObject.SetActive(false);
            return;
        }

        if (createOnStart)
        {
            StartCoroutine(CreateIntroPortalWithDelay());
        }
    }

    private IEnumerator CreateIntroPortalWithDelay()
    {
        // Wait for intro delay to let player get oriented
        yield return new WaitForSeconds(introDelay);

        CreateIntroPortal();
    }

    public void CreateIntroPortal()
    {
        if (portalCreated) return;

        // Get or create portal placer
        portalPlacer = FindObjectOfType<ChaosPortalPlacer>();
        if (portalPlacer == null)
        {
            GameObject placerObject = new GameObject("ChaosPortalPlacer");
            portalPlacer = placerObject.AddComponent<ChaosPortalPlacer>();
        }

        // Create the portal at specified position
        portalPlacer.CreatePortalAtPosition(chaosPortalPosition, "ChaosZone");
        
        portalCreated = true;

        // Show tutorial message if enabled
        if (showTutorialMessage)
        {
            ShowTutorialMessage();
        }

        // Play discovery sound
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySound("PortalDiscover");
        }

        Debug.Log("Intro level chaos portal created! Position: " + chaosPortalPosition);
    }

    private void ShowTutorialMessage()
    {
        // Try to use existing log system
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog(tutorialMessage, tutorialMessageDuration);
        }
        else
        {
            // Fallback to debug and console
            Debug.Log("TUTORIAL: " + tutorialMessage);
            
            // Could also create a simple UI message here if needed
            StartCoroutine(ShowSimpleUIMessage());
        }
    }

    private IEnumerator ShowSimpleUIMessage()
    {
        // This is a placeholder for a simple UI message system
        // In a real implementation, this would create a temporary UI element
        yield return new WaitForSeconds(tutorialMessageDuration);
        // Message would disappear here
    }

    // Method to manually trigger portal creation (for testing or special events)
    public void TriggerPortalCreation()
    {
        if (!portalCreated)
        {
            StopAllCoroutines(); // Stop any delayed creation
            CreateIntroPortal();
        }
    }

    // Method to position the portal at player's current location (for easy setup)
    public void SetPortalAtPlayerPosition()
    {
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            Vector3 playerPos = PlayerManager.instance.player.transform.position;
            chaosPortalPosition = playerPos + Vector3.forward * 5f; // Place 5 units in front of player
            Debug.Log("Chaos portal position set to: " + chaosPortalPosition);
        }
    }

    // Validate portal position (ensure it's on ground, not in walls, etc.)
    private Vector3 ValidatePortalPosition(Vector3 desiredPosition)
    {
        // Raycast down to find ground level
        RaycastHit hit;
        Vector3 rayStart = desiredPosition + Vector3.up * 10f;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, 20f))
        {
            if (hit.collider.CompareTag("Floor") || hit.collider.CompareTag("Ground"))
            {
                return hit.point + Vector3.up * 0.1f; // Slightly above ground
            }
        }
        
        // Fallback to original position if no suitable ground found
        return desiredPosition;
    }

    // Public getter for other systems
    public bool IsPortalCreated()
    {
        return portalCreated;
    }

    public Vector3 GetPortalPosition()
    {
        return chaosPortalPosition;
    }

    // Editor visualization
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw portal position in editor
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(chaosPortalPosition, 1.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(chaosPortalPosition, Vector3.one * 0.5f);
        
        // Draw line to player if available
        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 playerPos = PlayerManager.instance.player.transform.position;
            Gizmos.DrawLine(playerPos, chaosPortalPosition);
        }
    }
#endif
}