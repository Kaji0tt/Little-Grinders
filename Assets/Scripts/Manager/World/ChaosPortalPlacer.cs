using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChaosPortalSpawnPoint
{
    public Vector3 position;
    public string chaosZoneSceneName = "ChaosZone";
    public bool spawnOnIntroLevel = true;
    public float activationDelay = 0f; // Delay before portal becomes active
}

public class ChaosPortalPlacer : MonoBehaviour
{
    [Header("Chaos Portal Spawn Settings")]
    [SerializeField] private GameObject chaosPortalPrefab;
    [SerializeField] private ChaosPortalSpawnPoint[] spawnPoints;
    [SerializeField] private bool autoPlaceOnStart = true;
    
    [Header("Default Portal Settings")]
    [SerializeField] private Material chaosPortalMaterial;
    [SerializeField] private ParticleSystem chaosPortalEffect;
    
    [Header("Scene Integration")]
    [SerializeField] private bool enableInIntroLevel = true;
    [SerializeField] private bool enableInProceduralMap = true;
    [SerializeField] private int maxPortalsPerScene = 3;

    private List<GameObject> spawnedPortals = new List<GameObject>();

    private void Start()
    {
        if (autoPlaceOnStart)
        {
            PlaceChaosPortals();
        }
    }

    public void PlaceChaosPortals()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Check if we should place portals in this scene
        if (!ShouldPlacePortalsInScene(currentSceneName))
        {
            Debug.Log($"Chaos portals not enabled for scene: {currentSceneName}");
            return;
        }

        // Clear any existing portals
        ClearExistingPortals();

        int portalsPlaced = 0;
        foreach (ChaosPortalSpawnPoint spawnPoint in spawnPoints)
        {
            if (portalsPlaced >= maxPortalsPerScene) break;
            
            if (ShouldSpawnPortalAtPoint(spawnPoint, currentSceneName))
            {
                CreateChaosPortal(spawnPoint);
                portalsPlaced++;
            }
        }

        Debug.Log($"Placed {portalsPlaced} chaos portals in {currentSceneName}");
    }

    private bool ShouldPlacePortalsInScene(string sceneName)
    {
        if (sceneName.Contains("Intro") && enableInIntroLevel) return true;
        if (sceneName.Contains("ProceduralMap") && enableInProceduralMap) return true;
        if (sceneName.Contains("Level") && enableInProceduralMap) return true;
        
        return false;
    }

    private bool ShouldSpawnPortalAtPoint(ChaosPortalSpawnPoint spawnPoint, string currentSceneName)
    {
        if (currentSceneName.Contains("Intro") && spawnPoint.spawnOnIntroLevel) return true;
        if (!currentSceneName.Contains("Intro") && !spawnPoint.spawnOnIntroLevel) return true;
        
        return false;
    }

    private void CreateChaosPortal(ChaosPortalSpawnPoint spawnPoint)
    {
        GameObject portalObject;
        
        // Try to use provided prefab, otherwise create from scratch
        if (chaosPortalPrefab != null)
        {
            portalObject = Instantiate(chaosPortalPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            portalObject = CreateChaosPortalFromScratch(spawnPoint.position);
        }

        // Configure the chaos portal component
        ChaosPortal chaosPortal = portalObject.GetComponent<ChaosPortal>();
        if (chaosPortal == null)
        {
            chaosPortal = portalObject.AddComponent<ChaosPortal>();
        }

        // Setup portal properties via reflection or direct access
        SetupChaosPortal(chaosPortal, spawnPoint);

        // Add to spawned portals list
        spawnedPortals.Add(portalObject);

        Debug.Log($"Created chaos portal at {spawnPoint.position} targeting {spawnPoint.chaosZoneSceneName}");
    }

    private GameObject CreateChaosPortalFromScratch(Vector3 position)
    {
        // Create main portal object
        GameObject portal = new GameObject("ChaosPortal");
        portal.transform.position = position;

        // Add collider for interaction
        SphereCollider collider = portal.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 2f;

        // Add visual components
        CreatePortalVisuals(portal);

        return portal;
    }

    private void CreatePortalVisuals(GameObject portal)
    {
        // Create visual representation
        GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visualObject.name = "PortalVisual";
        visualObject.transform.SetParent(portal.transform);
        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localScale = new Vector3(2f, 0.1f, 2f);

        // Remove the collider from visual object (we have one on parent)
        Collider visualCollider = visualObject.GetComponent<Collider>();
        if (visualCollider != null) DestroyImmediate(visualCollider);

        // Apply chaos portal material if available
        Renderer renderer = visualObject.GetComponent<Renderer>();
        if (renderer != null && chaosPortalMaterial != null)
        {
            renderer.material = chaosPortalMaterial;
        }
        else if (renderer != null)
        {
            // Create a basic red/dark material for chaos portal
            Material chaosmat = new Material(Shader.Find("Standard"));
            chaosmat.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            chaosmat.SetFloat("_Metallic", 0.3f);
            chaosmat.SetFloat("_Smoothness", 0.7f);
            renderer.material = chaosmat;
        }

        // Add light for portal glow
        GameObject lightObject = new GameObject("PortalLight");
        lightObject.transform.SetParent(portal.transform);
        lightObject.transform.localPosition = new Vector3(0, 1f, 0);
        
        Light portalLight = lightObject.AddComponent<Light>();
        portalLight.type = LightType.Point;
        portalLight.color = Color.red;
        portalLight.intensity = 1f;
        portalLight.range = 8f;

        // Add particle effect if available
        if (chaosPortalEffect != null)
        {
            GameObject effectObject = Instantiate(chaosPortalEffect.gameObject, portal.transform);
            effectObject.transform.localPosition = Vector3.zero;
        }
    }

    private void SetupChaosPortal(ChaosPortal chaosPortal, ChaosPortalSpawnPoint spawnPoint)
    {
        // Note: Since Unity's Inspector fields are private, we'd need public setters
        // or use reflection to set these values. For now, we'll use a simple approach.
        
        // The chaos portal will use its default settings unless we add public setters
        // This is a limitation of working with serialized fields in Unity
        
        if (spawnPoint.activationDelay > 0)
        {
            StartCoroutine(ActivatePortalAfterDelay(chaosPortal, spawnPoint.activationDelay));
        }
    }

    private IEnumerator ActivatePortalAfterDelay(ChaosPortal portal, float delay)
    {
        portal.SetPortalActive(false);
        yield return new WaitForSeconds(delay);
        portal.SetPortalActive(true);
        
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySound("PortalActivate");
    }

    private void ClearExistingPortals()
    {
        foreach (GameObject portal in spawnedPortals)
        {
            if (portal != null)
            {
                DestroyImmediate(portal);
            }
        }
        spawnedPortals.Clear();
    }

    // Public methods for runtime portal management
    public void CreatePortalAtPosition(Vector3 position, string targetScene = "ChaosZone")
    {
        ChaosPortalSpawnPoint tempSpawnPoint = new ChaosPortalSpawnPoint
        {
            position = position,
            chaosZoneSceneName = targetScene,
            spawnOnIntroLevel = true,
            activationDelay = 0f
        };
        
        CreateChaosPortal(tempSpawnPoint);
    }

    public void RemoveAllPortals()
    {
        ClearExistingPortals();
    }

    public int GetPortalCount()
    {
        return spawnedPortals.Count;
    }

    // Editor helper methods
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw gizmos for spawn points in editor
        if (spawnPoints != null)
        {
            foreach (ChaosPortalSpawnPoint spawnPoint in spawnPoints)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(spawnPoint.position, 1f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(spawnPoint.position, 2f);
            }
        }
    }
#endif
}