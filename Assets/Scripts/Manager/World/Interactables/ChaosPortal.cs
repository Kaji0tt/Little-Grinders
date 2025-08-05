using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosPortal : Interactable
{
    [Header("Chaos Portal Settings")]
    [SerializeField] private string chaosZoneSceneName = "ChaosZone";
    [SerializeField] private float activationRadius = 2f;
    [SerializeField] private ParticleSystem portalEffect;
    [SerializeField] private Light portalGlow;
    [SerializeField] private bool isActive = true;
    
    [Header("Portal Visual Effects")]
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private float pulseSpeed = 2f;
    
    private bool playerNearby = false;
    private ChaosZoneManager chaosZoneManager;

    private void Start()
    {
        // Find or create the chaos zone manager
        chaosZoneManager = FindObjectOfType<ChaosZoneManager>();
        if (chaosZoneManager == null)
        {
            GameObject managerObject = new GameObject("ChaosZoneManager");
            chaosZoneManager = managerObject.AddComponent<ChaosZoneManager>();
        }

        SetupPortalVisuals();
    }

    private void Update()
    {
        if (isActive && portalGlow != null)
        {
            // Create pulsing effect for active portals
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.3f + 0.7f;
            portalGlow.intensity = pulse;
        }
    }

    public override void Use()
    {
        if (!isActive)
        {
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySound("ErrorSound");
            return;
        }

        // Play portal activation sound
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySound("PortalActivate");

        // Store the current scene information for return
        chaosZoneManager.SetReturnPoint(transform.position);
        chaosZoneManager.SetReturnScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        // Start portal teleportation effect
        StartCoroutine(TeleportToChaosZone());
    }

    private IEnumerator TeleportToChaosZone()
    {
        // Visual effect before teleportation
        if (portalEffect != null)
            portalEffect.Play();

        // Brief delay for effect
        yield return new WaitForSeconds(0.5f);

        // Load the chaos zone scene
        try
        {
            SceneLoader.LoadChaosZone(chaosZoneSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load chaos zone: {e.Message}");
            // Fallback: just spawn enemies locally if scene loading fails
            SpawnLocalChaosEvent();
        }
    }

    private void SpawnLocalChaosEvent()
    {
        // Fallback method: create a local chaos event if scene loading isn't available
        Debug.Log("Creating local chaos event as fallback");
        
        // Find nearby spawn points or create temporary ones
        PrefabCollection prefabCollection = FindObjectOfType<PrefabCollection>();
        if (prefabCollection != null)
        {
            int enemyCount = Random.Range(3, 7);
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 spawnPos = transform.position + Random.insideUnitSphere * 5f;
                spawnPos.y = transform.position.y; // Keep same height level
                
                GameObject mob = prefabCollection.GetRandomEnemie();
                if (mob != null)
                {
                    GameObject instanceMob = Instantiate(mob, spawnPos, Quaternion.identity);
                    instanceMob.AddComponent<SummonedMob>();
                }
            }
        }

        // Deactivate portal temporarily
        SetPortalActive(false);
        StartCoroutine(ReactivatePortalAfterDelay(30f));
    }

    private IEnumerator ReactivatePortalAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetPortalActive(true);
    }

    public void SetPortalActive(bool active)
    {
        isActive = active;
        SetupPortalVisuals();
    }

    private void SetupPortalVisuals()
    {
        if (portalGlow != null)
        {
            portalGlow.color = isActive ? activeColor : inactiveColor;
            portalGlow.intensity = isActive ? 1f : 0.3f;
        }

        if (portalEffect != null)
        {
            if (isActive)
                portalEffect.Play();
            else
                portalEffect.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<IsometricPlayer>() != null)
        {
            playerNearby = true;
            if (isActive && AudioManager.instance != null)
                AudioManager.instance.PlaySound("PortalHum");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<IsometricPlayer>() != null)
        {
            playerNearby = false;
        }
    }

    // Public method for other systems to check if portal is usable
    public bool IsPortalReady()
    {
        return isActive && !interactableUsed;
    }
}