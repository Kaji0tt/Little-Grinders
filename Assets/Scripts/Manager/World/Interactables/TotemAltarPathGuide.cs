using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Visual guide that shows a pulsing energy path from Totem to Altar
/// Uses NavMesh pathfinding to follow ground path around obstacles
/// Automatically creates particle effects for enhanced visibility
/// </summary>
public class TotemAltarPathGuide : MonoBehaviour
{
    [Header("Line Renderer Settings")]
    [SerializeField] private bool useLineRenderer = false; // Deactivated for testing
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.4f;
    [SerializeField] private Color pulseColor = new Color(153f/255f, 51f/255f, 204f/255f, 1f); // Lila
    
    [Header("Trail Renderer Settings")]
    [SerializeField] private bool useTrailRenderer = true; // NEW: Trail system
    [SerializeField] private GameObject trailRendererPrefab; // Prefab with TrailRenderer component
    [SerializeField] private float trailMoveSpeed = 5f; // How fast trail moves along path
    [SerializeField] private bool loopTrail = true; // Loop back to start when reaching end
    [SerializeField] private bool overrideTrailColor = false; // If true, uses pulseColor instead of prefab settings
    
    [Header("Path Settings")]
    [SerializeField] private float heightOffset = 0.2f; // Slightly above ground
    [SerializeField] private bool useNavMeshPath = true; // Follow NavMesh or direct arc
    [SerializeField] private float pathSimplificationTolerance = 0.5f; // Simplify path corners
    [SerializeField] private float navMeshSearchRadius = 15f; // How far to search for nearest NavMesh point
    
    [Header("Pulse Animation")]
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float pulseLength = 0.25f;
    [SerializeField] private float pulseInterval = 0.8f;
    [SerializeField] private AnimationCurve pulseFadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Particle Settings")]
    [SerializeField] private bool spawnParticles = true;
    [SerializeField] private int particlesPerPulse = 8;
    [SerializeField] private float particleSize = 0.3f;
    [SerializeField] private float particleLifetime = 1.5f;
    
    private LineRenderer lineRenderer;
    private ParticleSystem pulseParticles;
    private GameObject trailObject; // Spawned trail object
    private TrailRenderer trailRenderer; // Reference to trail component
    private float trailProgress = 0f; // 0-1 position along path
    private Transform startTransform;
    private Transform endTransform;
    private bool isActive = false;
    private float pulseProgress = 0f;
    private float timeSinceLastPulse = 0f;
    
    private NavMeshPath navMeshPath;
    private Vector3[] pathPoints;
    private float[] pathDistances; // Cumulative distances for pulse positioning
    private float totalPathLength;
    
    private void Awake()
    {
        if (useLineRenderer)
        {
            SetupLineRenderer();
        }
        
        if (spawnParticles && useLineRenderer) // Only with LineRenderer
        {
            SetupParticleSystem();
        }
        
        navMeshPath = new NavMeshPath();
        
        // Check NavMesh on startup (delayed to ensure map is loaded)
        StartCoroutine(CheckNavMeshOnStartup());
    }

    private IEnumerator CheckNavMeshOnStartup()
    {
        yield return new WaitForSeconds(0.5f); // NavMesh wird jetzt vor Prefab-Loading gebaked
        
        Debug.Log("=== TotemAltarPathGuide NavMesh Startup Check ===");
        
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        bool hasNavMesh = navMeshData.vertices.Length > 0;
        
        if (hasNavMesh)
        {
            Debug.Log($"✅ NavMesh detected: {navMeshData.vertices.Length} vertices, {navMeshData.indices.Length / 3} triangles");
        }
        else
        {
            Debug.LogError("❌ NO NAVMESH DETECTED!");
            Debug.LogError("SOLUTION:");
            Debug.LogError("1. Check MapGenHandler: navMeshSurface.BuildNavMesh() should be called BEFORE LoadPrefabs()");
            Debug.LogError("2. Make sure NavMeshSurface has settings: Agent Type = Humanoid, Include Layers = Ground");
            Debug.LogError("3. Check Console for '[MapGenHandler] Baking NavMesh BEFORE loading prefabs...' message");
        }
        
        // Find NavMeshSurface in scene
        NavMeshSurface[] surfaces = FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);
        Debug.Log($"NavMeshSurface components found in scene: {surfaces.Length}");
        
        foreach (var surface in surfaces)
        {
            Debug.Log($"  - {surface.gameObject.name}: Active={surface.gameObject.activeInHierarchy}, NavMeshData={(surface.navMeshData != null ? "Present" : "NULL")}");
        }
    }
    
    private void SetupLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.numCornerVertices = 5;
        lineRenderer.numCapVertices = 5;
        
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        else
        {
            Debug.LogWarning("[TotemAltarPathGuide] No material assigned, creating default.");
            Material defaultMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            defaultMat.SetColor("_BaseColor", pulseColor);
            lineRenderer.material = defaultMat;
        }
        
        lineRenderer.sortingOrder = 100;
        lineRenderer.enabled = false;
    }
    
    private void SetupParticleSystem()
    {
        GameObject particleObj = new GameObject("PulseParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        pulseParticles = particleObj.AddComponent<ParticleSystem>();
        
        // Main module
        var main = pulseParticles.main;
        main.startLifetime = particleLifetime;
        main.startSpeed = 0.5f;
        main.startSize = particleSize;
        main.startColor = pulseColor;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        // Emission - manual control
        var emission = pulseParticles.emission;
        emission.enabled = false;
        
        // Shape
        var shape = pulseParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;
        
        // Renderer
        var renderer = pulseParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        // Create particle material
        Material particleMat;
        if (lineMaterial != null)
        {
            particleMat = new Material(lineMaterial);
        }
        else
        {
            particleMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            particleMat.SetColor("_BaseColor", pulseColor);
        }
        particleMat.SetFloat("_Surface", 1); // Transparent
        renderer.material = particleMat;
        
        // Color over lifetime for fade
        var colorOverLifetime = pulseParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = grad;
        
        pulseParticles.Stop();
        
        Debug.Log("[TotemAltarPathGuide] Particle system created automatically");
    }
    
    public void ShowPath(Transform start, Transform end)
    {
        startTransform = start;
        endTransform = end;
        isActive = true;
        pulseProgress = 0f;
        timeSinceLastPulse = pulseInterval;
        trailProgress = 0f;
        
        CalculatePath();
        
        // LineRenderer system
        if (useLineRenderer && lineRenderer != null)
        {
            lineRenderer.enabled = true;
            Debug.Log("[TotemAltarPathGuide] LineRenderer activated");
        }
        else if (useLineRenderer)
        {
            Debug.LogWarning("[TotemAltarPathGuide] UseLineRenderer=true but lineRenderer is null!");
        }
        
        if (spawnParticles && pulseParticles != null && useLineRenderer)
        {
            pulseParticles.Play();
        }
        
        // Trail Renderer system
        if (useTrailRenderer)
        {
            if (trailRendererPrefab != null)
            {
                Debug.Log($"[TotemAltarPathGuide] Spawning trail - Prefab: {trailRendererPrefab.name}, Path points: {pathPoints?.Length ?? 0}");
                SpawnTrailRenderer();
            }
            else
            {
                Debug.LogError("[TotemAltarPathGuide] UseTrailRenderer=true but trailRendererPrefab is NOT ASSIGNED in Inspector!");
            }
        }
        
        Debug.Log($"[TotemAltarPathGuide] Path guide activated - Path length: {totalPathLength:F1}m, Points: {pathPoints?.Length ?? 0}, LineRenderer: {useLineRenderer}, Trail: {useTrailRenderer}");
    }
    
    public void HidePath()
    {
        isActive = false;
        
        if (lineRenderer != null)
            lineRenderer.enabled = false;
        
        if (pulseParticles != null)
        {
            pulseParticles.Stop();
            pulseParticles.Clear();
        }
        
        // Destroy trail object
        if (trailObject != null)
        {
            Destroy(trailObject);
            trailObject = null;
            trailRenderer = null;
        }
        
        Debug.Log("[TotemAltarPathGuide] Path guide deactivated");
    }
    
    private void CalculatePath()
    {
        if (startTransform == null || endTransform == null)
            return;
        
        Vector3 startPos = startTransform.position;
        Vector3 endPos = endTransform.position;
        
        // DEBUG: Check NavMesh status
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        bool hasNavMesh = navMeshData.vertices.Length > 0;
        
        Debug.Log($"[TotemAltarPathGuide] === NavMesh Debug Info ===");
        Debug.Log($"[TotemAltarPathGuide] NavMesh exists: {hasNavMesh}");
        Debug.Log($"[TotemAltarPathGuide] NavMesh vertices: {navMeshData.vertices.Length}");
        Debug.Log($"[TotemAltarPathGuide] NavMesh triangles: {navMeshData.indices.Length / 3}");
        Debug.Log($"[TotemAltarPathGuide] Use NavMesh Path: {useNavMeshPath}");
        Debug.Log($"[TotemAltarPathGuide] Start Position: {startPos}");
        Debug.Log($"[TotemAltarPathGuide] End Position: {endPos}");
        Debug.Log($"[TotemAltarPathGuide] Distance: {Vector3.Distance(startPos, endPos):F2}m");
        
        // Check if start/end positions are on NavMesh
        NavMeshHit startHit, endHit;
        bool startOnNavMesh = NavMesh.SamplePosition(startPos, out startHit, navMeshSearchRadius, NavMesh.AllAreas);
        bool endOnNavMesh = NavMesh.SamplePosition(endPos, out endHit, navMeshSearchRadius, NavMesh.AllAreas);
        
        Debug.Log($"[TotemAltarPathGuide] Start on NavMesh: {startOnNavMesh} (nearest: {(startOnNavMesh ? startHit.distance.ToString("F2") + "m" : "N/A")})");
        Debug.Log($"[TotemAltarPathGuide] End on NavMesh: {endOnNavMesh} (nearest: {(endOnNavMesh ? endHit.distance.ToString("F2") + "m" : "N/A")})");
        
        // Use nearest NavMesh positions if not directly on NavMesh
        if (startOnNavMesh)
        {
            if (startHit.distance > 0.1f)
            {
                startPos = startHit.position;
                Debug.Log($"[TotemAltarPathGuide] 🔧 Corrected start position to nearest NavMesh point ({startHit.distance:F2}m away)");
            }
        }
        else
        {
            Debug.LogError($"[TotemAltarPathGuide] ❌ Start position NOT on NavMesh within {navMeshSearchRadius}m!");
            Debug.LogError($"[TotemAltarPathGuide] → Check that ground tiles have walkable NavMesh");
            Debug.LogError($"[TotemAltarPathGuide] → Totem spawned at: {startTransform.position}");
        }
        
        if (endOnNavMesh)
        {
            if (endHit.distance > 0.1f)
            {
                endPos = endHit.position;
                Debug.Log($"[TotemAltarPathGuide] 🔧 Corrected end position to nearest NavMesh point ({endHit.distance:F2}m away)");
            }
        }
        else
        {
            Debug.LogError($"[TotemAltarPathGuide] ❌ End position NOT on NavMesh within {navMeshSearchRadius}m!");
            Debug.LogError($"[TotemAltarPathGuide] → Check that ground tiles have walkable NavMesh");
            Debug.LogError($"[TotemAltarPathGuide] → Altar spawned at: {endTransform.position}");
        }
        
        if (useNavMeshPath && hasNavMesh && startOnNavMesh && endOnNavMesh)
        {
            // Try to calculate NavMesh path
            bool pathFound = NavMesh.CalculatePath(startPos, endPos, NavMesh.AllAreas, navMeshPath);
            
            Debug.Log($"[TotemAltarPathGuide] NavMesh.CalculatePath result: {pathFound}");
            Debug.Log($"[TotemAltarPathGuide] NavMesh path status: {navMeshPath.status}");
            Debug.Log($"[TotemAltarPathGuide] NavMesh path corners: {navMeshPath.corners.Length}");
            
            if (pathFound && navMeshPath.status == NavMeshPathStatus.PathComplete)
            {
                // Use NavMesh path
                pathPoints = SimplifyPath(navMeshPath.corners, pathSimplificationTolerance);
                
                Debug.Log($"[TotemAltarPathGuide] ✅ Using NavMesh path with {pathPoints.Length} points (simplified from {navMeshPath.corners.Length})");
                
                // Log all path corners for debugging
                for (int i = 0; i < navMeshPath.corners.Length; i++)
                {
                    Debug.Log($"[TotemAltarPathGuide]   Corner {i}: {navMeshPath.corners[i]}");
                }
            }
            else
            {
                // Fallback to direct path
                pathPoints = new Vector3[] { startTransform.position, endTransform.position };
                Debug.LogWarning($"[TotemAltarPathGuide] ⚠️ NavMesh path incomplete (status: {navMeshPath.status}), using direct path");
            }
        }
        else
        {
            // Direct path fallback
            pathPoints = new Vector3[] { startTransform.position, endTransform.position };
            
            if (!hasNavMesh)
            {
                Debug.LogError("[TotemAltarPathGuide] ❌ NO NAVMESH FOUND! Check if NavMeshSurface is baked.");
            }
            else if (!startOnNavMesh || !endOnNavMesh)
            {
                Debug.LogError("[TotemAltarPathGuide] ❌ START OR END NOT ON NAVMESH!");
                Debug.LogError($"[TotemAltarPathGuide] → Totem/Altar must spawn on walkable ground");
                Debug.LogError($"[TotemAltarPathGuide] → Check OutsideVegLoader.SpawnTotemAndAltar() - ensure NoVeg fields have NavMesh");
                Debug.LogError($"[TotemAltarPathGuide] → Fallback: Using direct line instead of ground path");
            }
            else if (!useNavMeshPath)
            {
                Debug.Log("[TotemAltarPathGuide] ℹ️ NavMesh disabled in settings, using direct path");
            }
        }
        
        // Add height offset
        for (int i = 0; i < pathPoints.Length; i++)
        {
            pathPoints[i].y += heightOffset;
        }
        
        // Calculate cumulative distances
        CalculatePathDistances();
        
        // Update LineRenderer (only if enabled)
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = pathPoints.Length;
            lineRenderer.SetPositions(pathPoints);
        }
        
        Debug.Log($"[TotemAltarPathGuide] === Path Calculation Complete ===");
        Debug.Log($"[TotemAltarPathGuide] Final path length: {totalPathLength:F1}m");
        Debug.Log($"[TotemAltarPathGuide] Final path points: {pathPoints.Length}");
    }
    
    private Vector3[] SimplifyPath(Vector3[] corners, float tolerance)
    {
        if (corners.Length <= 2)
            return corners;
        
        List<Vector3> simplified = new List<Vector3>();
        simplified.Add(corners[0]);
        
        for (int i = 1; i < corners.Length - 1; i++)
        {
            Vector3 prev = simplified[simplified.Count - 1];
            Vector3 current = corners[i];
            Vector3 next = corners[i + 1];
            
            // Check if current point is significant (angle change)
            Vector3 dir1 = (current - prev).normalized;
            Vector3 dir2 = (next - current).normalized;
            float angle = Vector3.Angle(dir1, dir2);
            
            if (angle > 15f || Vector3.Distance(prev, current) > tolerance * 2f)
            {
                simplified.Add(current);
            }
        }
        
        simplified.Add(corners[corners.Length - 1]);
        return simplified.ToArray();
    }
    
    private void CalculatePathDistances()
    {
        pathDistances = new float[pathPoints.Length];
        pathDistances[0] = 0f;
        totalPathLength = 0f;
        
        for (int i = 1; i < pathPoints.Length; i++)
        {
            float segmentLength = Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
            totalPathLength += segmentLength;
            pathDistances[i] = totalPathLength;
        }
    }
    
    private void Update()
    {
        if (!isActive || startTransform == null || endTransform == null)
            return;
        
        // Recalculate path if endpoints moved significantly
        if (pathPoints != null && pathPoints.Length > 0 && 
            Vector3.Distance(pathPoints[pathPoints.Length - 1], endTransform.position + Vector3.up * heightOffset) > 1f)
        {
            CalculatePath();
            
            // Reset trail if path changed
            if (useTrailRenderer && trailObject != null)
            {
                trailProgress = 0f;
                trailObject.transform.position = pathPoints[0];
            }
        }
        
        // Update LineRenderer pulse animation
        if (useLineRenderer)
        {
            UpdatePulseAnimation();
        }
        
        // Update TrailRenderer movement
        if (useTrailRenderer && trailObject != null)
        {
            UpdateTrailMovement();
        }
    }
    
    private void UpdatePulseAnimation()
    {
        timeSinceLastPulse += Time.deltaTime;
        
        if (timeSinceLastPulse >= pulseInterval)
        {
            pulseProgress = 0f;
            timeSinceLastPulse = 0f;
        }
        
        pulseProgress += Time.deltaTime * pulseSpeed;
        
        if (pulseProgress > 1f + pulseLength)
        {
            pulseProgress = 1f + pulseLength;
        }
        
        // Calculate pulse position along path
        float pulseDistance = pulseProgress * totalPathLength;
        Vector3 pulseWorldPos = GetPositionAtDistance(pulseDistance);
        
        // Spawn particles at pulse position
        if (spawnParticles && pulseParticles != null && pulseProgress <= 1f)
        {
            SpawnParticlesAtPulse(pulseWorldPos);
        }
        
        // Build color gradient for line
        GradientColorKey[] colorKeys = new GradientColorKey[pathPoints.Length];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[pathPoints.Length];
        
        for (int i = 0; i < pathPoints.Length; i++)
        {
            float normalizedDist = pathDistances[i] / totalPathLength;
            float distanceFromPulse = Mathf.Abs(normalizedDist - pulseProgress);
            
            float alpha = 0f;
            if (distanceFromPulse < pulseLength)
            {
                float normalizedPulseDist = distanceFromPulse / pulseLength;
                alpha = pulseFadeCurve.Evaluate(1f - normalizedPulseDist);
            }
            
            colorKeys[i] = new GradientColorKey(pulseColor, normalizedDist);
            alphaKeys[i] = new GradientAlphaKey(alpha, normalizedDist);
        }
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = gradient;
    }
    
    private Vector3 GetPositionAtDistance(float distance)
    {
        if (distance <= 0f)
            return pathPoints[0];
        if (distance >= totalPathLength)
            return pathPoints[pathPoints.Length - 1];
        
        for (int i = 1; i < pathPoints.Length; i++)
        {
            if (pathDistances[i] >= distance)
            {
                float segmentStart = pathDistances[i - 1];
                float segmentEnd = pathDistances[i];
                float segmentLength = segmentEnd - segmentStart;
                float t = (distance - segmentStart) / segmentLength;
                
                return Vector3.Lerp(pathPoints[i - 1], pathPoints[i], t);
            }
        }
        
        return pathPoints[pathPoints.Length - 1];
    }
    
    private float lastParticleSpawn = 0f;
    private void SpawnParticlesAtPulse(Vector3 position)
    {
        if (Time.time - lastParticleSpawn < 0.05f) // Limit spawn rate
            return;
        
        lastParticleSpawn = Time.time;
        
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = position;
        emitParams.startColor = pulseColor;
        emitParams.startSize = particleSize;
        emitParams.startLifetime = particleLifetime;
        
        pulseParticles.Emit(emitParams, particlesPerPulse);
    }
    
    #region Trail Renderer System
    
    /// <summary>
    /// Spawns the trail renderer object at the start of the path
    /// </summary>
    private void SpawnTrailRenderer()
    {
        if (trailRendererPrefab == null)
        {
            Debug.LogError("[TotemAltarPathGuide] TrailRenderer prefab not assigned!");
            return;
        }
        
        if (pathPoints == null || pathPoints.Length < 2)
        {
            Debug.LogError($"[TotemAltarPathGuide] Cannot spawn trail - invalid path! PathPoints: {pathPoints?.Length ?? 0}");
            return;
        }
        
        // Spawn trail object at start position
        Vector3 spawnPos = pathPoints[0];
        Debug.Log($"[TotemAltarPathGuide] Instantiating trail at {spawnPos}");
        
        trailObject = Instantiate(trailRendererPrefab, spawnPos, Quaternion.identity);
        trailObject.transform.SetParent(transform);
        trailObject.SetActive(true); // Ensure it's active
        
        Debug.Log($"[TotemAltarPathGuide] Trail object created: {trailObject.name}, Active: {trailObject.activeSelf}");
        
        // Get TrailRenderer component
        trailRenderer = trailObject.GetComponent<TrailRenderer>();
        
        if (trailRenderer == null)
        {
            Debug.LogError($"[TotemAltarPathGuide] TrailRenderer component not found on prefab '{trailRendererPrefab.name}'! Available components:");
            Component[] components = trailObject.GetComponents<Component>();
            foreach (var comp in components)
            {
                Debug.LogError($"  - {comp.GetType().Name}");
            }
            Destroy(trailObject);
            trailObject = null;
            return;
        }
        
        Debug.Log($"[TotemAltarPathGuide] TrailRenderer found! Enabled: {trailRenderer.enabled}, Time: {trailRenderer.time}, Material: {trailRenderer.material?.name ?? "null"}");
        
        // Ensure trail is enabled
        trailRenderer.enabled = true;
        trailRenderer.emitting = true;
        
        // Optional: Apply color to trail (only if override is enabled)
        if (overrideTrailColor && trailRenderer.material != null)
        {
            trailRenderer.startColor = pulseColor;
            trailRenderer.endColor = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 0f);
            Debug.Log($"[TotemAltarPathGuide] Trail colors OVERRIDDEN: Start={pulseColor}, End=transparent");
        }
        else
        {
            Debug.Log($"[TotemAltarPathGuide] Trail using PREFAB colors: Start={trailRenderer.startColor}, End={trailRenderer.endColor}");
        }
        
        trailProgress = 0f;
        
        Debug.Log($"[TotemAltarPathGuide] ✅ Trail spawned successfully at {spawnPos}");
    }
    
    /// <summary>
    /// Moves trail object along the calculated path
    /// </summary>
    private void UpdateTrailMovement()
    {
        if (trailObject == null)
        {
            Debug.LogWarning("[TotemAltarPathGuide] UpdateTrailMovement: trailObject is null!");
            return;
        }
        
        if (pathPoints == null || pathPoints.Length < 2)
        {
            Debug.LogWarning($"[TotemAltarPathGuide] UpdateTrailMovement: Invalid pathPoints! Length: {pathPoints?.Length ?? 0}");
            return;
        }
        
        // Move progress forward based on speed and path length
        float progressDelta = (Time.deltaTime * trailMoveSpeed) / totalPathLength;
        trailProgress += progressDelta;
        
        // Loop or stop at end
        if (trailProgress > 1f)
        {
            if (loopTrail)
            {
                // Destroy old trail and spawn new one to avoid long streak
                Debug.Log("[TotemAltarPathGuide] Trail reached end - destroying and respawning");
                Destroy(trailObject);
                trailObject = null;
                trailRenderer = null;
                
                // Spawn new trail at start
                SpawnTrailRenderer();
                return; // Skip rest of update this frame
            }
            else
            {
                trailProgress = 1f;
            }
        }
        
        // Calculate position along path
        float distanceAlongPath = trailProgress * totalPathLength;
        Vector3 targetPosition = GetPositionAtDistance(distanceAlongPath);
        
        // Move trail object
        trailObject.transform.position = targetPosition;
        
        // Optional: Orient trail in movement direction
        if (trailProgress < 1f)
        {
            float lookAheadDistance = distanceAlongPath + 0.5f;
            if (lookAheadDistance <= totalPathLength)
            {
                Vector3 lookAheadPos = GetPositionAtDistance(lookAheadDistance);
                Vector3 direction = (lookAheadPos - targetPosition).normalized;
                if (direction.sqrMagnitude > 0.01f)
                {
                    trailObject.transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
    }
    
    #endregion
    
    private void OnDestroy()
    {
        HidePath();
        
        if (pulseParticles != null)
        {
            Destroy(pulseParticles.gameObject);
        }
    }
}
