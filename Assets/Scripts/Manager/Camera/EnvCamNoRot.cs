using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnvCamNoRot : MonoBehaviour
{
    private Vector3 CameraPosition;
    //private float distSelfCamera;

    [SerializeField]
    private int sortingOrderBase = 5000;
    public int sO_OffSet;
    
    [Header("NavMesh Obstacle Settings")]
    [SerializeField] private bool autoConfigureNavMeshObstacles = true;
    [SerializeField] private bool carveNavMesh = true;
    [SerializeField] private bool carveOnlyStationary = true;
    [SerializeField] [Range(0.1f, 1.0f)] private float carvingSizeMultiplier = 0.7f; // Reduce carving size (0.7 = 70% of collider)

    private SpriteRenderer sprite;
    private ParticleSystemRenderer particle;

    private int no_children;
    private Transform child;

    void Awake()
    {
        // Configure NavMesh Obstacles BEFORE Start (before NavMesh baking)
        // NOTE: For procedural maps, this will be called again explicitly after prefab loading
        if (autoConfigureNavMeshObstacles)
        {
            ConfigureNavMeshObstacles();
        }
    }
    
    /// <summary>
    /// PUBLIC method to manually trigger NavMeshObstacle configuration.
    /// Called by MapGenHandler after all environment prefabs are loaded.
    /// </summary>
    public void ConfigureObstaclesForProcedularMap()
    {
        Debug.Log("[EnvCamNoRot] Manual NavMeshObstacle configuration triggered for procedural map");
        ConfigureNavMeshObstacles();
    }

    void Start()
    {

        no_children = this.gameObject.transform.childCount;

        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);


            sprite = child.GetComponent<SpriteRenderer>();
            //child.Rotate(25, 0, 0);

            if (sprite)
                //Setze das Tag, damit CameraFollow.cs weiß, welche GO's zwischen Spieler und Kamera liegen für Transparenz.
                child.gameObject.tag = "Env";
            else if (child.GetComponent<ParticleSystemRenderer>())
                child.gameObject.tag = "Env";

        }
    }
    
    void Update()
    {
        no_children = this.gameObject.transform.childCount;

        //Folgender Abschnitt soll für sämtliche Child-GameObjekte angewandt werden.
        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);




            if (child.GetComponent<SpriteRenderer>())
            {
                SortSriteLayer();
            }
            else if (child.GetComponent<ParticleSystem>())
            {
                SortParticelLayer();
            }


        }

    }

    void SortSriteLayer()
    {
        sprite = child.GetComponent<SpriteRenderer>();
        //Enable Shadow Casting for the Sprite
        sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        //Finde Position der Kamera
        CameraPosition = CameraManager.instance.GetCameraPosition();

        //Berechne Abstand zur Kamera
        float distSelfCamera = (child.position - CameraPosition).sqrMagnitude;

        //Verändere die Sorting-Order entsprechend zum Abstand. Damit Layern die Sprite's schließlich automatisch korrekt.
        sprite.sortingOrder = (int)(sortingOrderBase - distSelfCamera) + sO_OffSet;
        sprite.sortingLayerName = "Umgebung_col Layer";

    }

    void SortParticelLayer()
    {
        particle = child.GetComponent<ParticleSystemRenderer>();

        //Finde Position der Kamera
        CameraPosition = CameraManager.instance.GetCameraPosition();

        //Berechne Abstand zur Kamera
        float distSelfCamera = (child.position - CameraPosition).sqrMagnitude;

        //Verändere die Sorting-Order entsprechend zum Abstand. Damit Layern die Sprite's schließlich automatisch korrekt.
        particle.sortingOrder = (int)(sortingOrderBase - distSelfCamera) + sO_OffSet;
        particle.sortingLayerName = "Umgebung_col Layer";

    }

    /// <summary>
    /// Automatically configures NavMeshObstacle components for all objects with colliders.
    /// This ensures obstacles properly carve into the NavMesh for pathfinding.
    /// Called in Awake() before NavMesh baking.
    /// </summary>
    private void ConfigureNavMeshObstacles()
    {
        Debug.Log("[EnvCamNoRot] Configuring NavMesh Obstacles for environment objects...");
        
        int configuredCount = 0;
        int addedCount = 0;
        
        // Get all colliders in this GameObject and all children
        Collider[] allColliders = GetComponentsInChildren<Collider>(true);
        
        foreach (Collider col in allColliders)
        {
            // Skip if it's a trigger collider (not a physical obstacle)
            if (col.isTrigger)
                continue;
            
            GameObject obj = col.gameObject;
            
            // Check if NavMeshObstacle already exists
            NavMeshObstacle obstacle = obj.GetComponent<NavMeshObstacle>();
            
            if (obstacle == null)
            {
                // Add NavMeshObstacle component
                obstacle = obj.AddComponent<NavMeshObstacle>();
                addedCount++;
                Debug.Log($"[EnvCamNoRot] Added NavMeshObstacle to: {obj.name}");
            }
            
            // Configure obstacle settings
            obstacle.carving = carveNavMesh;
            obstacle.carveOnlyStationary = carveOnlyStationary;
            
            // Auto-size based on collider type
            if (col is BoxCollider)
            {
                BoxCollider box = col as BoxCollider;
                obstacle.shape = NavMeshObstacleShape.Box;
                obstacle.center = box.center;
                obstacle.size = box.size * carvingSizeMultiplier; // Apply size reduction
            }
            else if (col is CapsuleCollider)
            {
                CapsuleCollider capsule = col as CapsuleCollider;
                obstacle.shape = NavMeshObstacleShape.Capsule;
                obstacle.center = capsule.center;
                obstacle.radius = capsule.radius * carvingSizeMultiplier; // Apply size reduction
                obstacle.height = capsule.height * carvingSizeMultiplier;
            }
            else if (col is SphereCollider)
            {
                SphereCollider sphere = col as SphereCollider;
                obstacle.shape = NavMeshObstacleShape.Capsule;
                obstacle.center = sphere.center;
                obstacle.radius = sphere.radius * carvingSizeMultiplier; // Apply size reduction
                obstacle.height = sphere.radius * 2f * carvingSizeMultiplier;
            }
            else if (col is MeshCollider)
            {
                // MeshColliders: Use bounding box as approximation
                MeshCollider mesh = col as MeshCollider;
                Bounds bounds = mesh.bounds;
                obstacle.shape = NavMeshObstacleShape.Box;
                obstacle.center = mesh.transform.InverseTransformPoint(bounds.center);
                obstacle.size = bounds.size * carvingSizeMultiplier; // Apply size reduction
            }
            
            configuredCount++;
        }
        
        Debug.Log($"[EnvCamNoRot] ✅ NavMesh Obstacles configured: {configuredCount} total ({addedCount} added, {configuredCount - addedCount} updated)");
        Debug.Log($"[EnvCamNoRot] Settings: Carving={carveNavMesh}, CarveOnlyStationary={carveOnlyStationary}, SizeMultiplier={carvingSizeMultiplier:F2}");
    }


    //LayerSprites war ein Ansatz für das Layering im Aufbau von ProcedualMap Generation
    /*
    public void LayerSprites()
    {
        no_children = this.gameObject.transform.childCount;

        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);
            sprite = child.GetComponent<SpriteRenderer>();
            //child.LookAt(Camera.main.transform); //LookAt ist fragwürdig, es würde mehr Illusion von 3D erzeugt werden ohne LookAt
            //Rather: Automatische Rotation bis Grad x° für mehr Tiefe.
            //child.Rotate(0, 25, 0);                           

            CameraPosition = GameObject.FindGameObjectWithTag("MainCamera").transform.position;
            DistSelfCamera = (child.position - CameraPosition).sqrMagnitude;
            sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera) + sO_OffSet;
            sprite.sortingLayerName = "Umgebung_col Layer";
        }

    }
    */
}

