using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages a group of enemies that spawn together and pull as a group.
/// Can be configured as a stationary pack or a patrolling group.
/// </summary>
public class EnemyGroup : MonoBehaviour
{
    [Header("Group Settings")]
    [Tooltip("If true, this group patrols along waypoints")]
    public bool isPatrol = false;
    
    [Tooltip("Spawn radius for enemies in this group")]
    [SerializeField] private float spawnRadius = 2f;
    
    [Header("Patrol Settings")]
    [Tooltip("Speed multiplier for patrol movement (e.g., 0.5 = 50% of normal speed)")]
    [SerializeField] private float patrolSpeedMultiplier = 0.5f;
    
    [Tooltip("Distance to waypoint before moving to next one")]
    [SerializeField] private float waypointReachDistance = 1.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    [SerializeField] private bool showPatrolGizmos = true;
    
    // Runtime data
    public List<EnemyController> members = new List<EnemyController>();
    public List<Vector3> patrolWaypoints = new List<Vector3>();
    
    private int currentWaypointIndex = 0;
    private bool isGroupPulled = false;
    private Dictionary<EnemyController, float> originalSpeeds = new Dictionary<EnemyController, float>();
    
    /// <summary>
    /// Spawns a group of enemies at the specified center position
    /// </summary>
    public void SpawnGroup(Vector3 centerPos, int enemyCount, PrefabCollection prefabCollection, Transform parentTransform)
    {
        if (prefabCollection == null)
        {
            Debug.LogError("[EnemyGroup] Cannot spawn group - PrefabCollection is null!");
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[EnemyGroup] Spawning {enemyCount} enemies at {centerPos} (Patrol: {isPatrol})");
        }
        
        for (int i = 0; i < enemyCount; i++)
        {
            // Random position in circle around center
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
            Vector3 spawnPos = centerPos + spawnOffset;
            
            // Sample NavMesh to ensure valid position
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(spawnPos, out navHit, 5f, NavMesh.AllAreas))
            {
                spawnPos = navHit.position;
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.LogWarning($"[EnemyGroup] Could not find NavMesh at spawn position {spawnPos}, using center position");
                }
                spawnPos = centerPos;
            }
            
            // Spawn enemy
            GameObject enemyPrefab = prefabCollection.GetRandomEnemie();
            if (enemyPrefab == null)
            {
                Debug.LogError("[EnemyGroup] GetRandomEnemie() returned null!");
                continue;
            }
            
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemyObj.name = enemyPrefab.name;
            
            // Set parent
            if (parentTransform != null)
            {
                enemyObj.transform.SetParent(parentTransform);
            }
            
            // Remove "(Clone)" from name
            if (enemyObj.name.Contains("(Clone)"))
            {
                enemyObj.name = enemyObj.name.Replace("(Clone)", "").Trim();
            }
            
            // Get EnemyController and register to group
            EnemyController controller = enemyObj.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.myGroup = this;
                members.Add(controller);
                
                // Store original speed for later restoration
                StartCoroutine(StoreOriginalSpeedDelayed(controller));
            }
            else
            {
                Debug.LogError($"[EnemyGroup] Spawned enemy {enemyObj.name} has no EnemyController component!");
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[EnemyGroup] Successfully spawned {members.Count}/{enemyCount} enemies");
        }
    }
    
    /// <summary>
    /// Store the original movement speed after MobStats.Start() has run
    /// </summary>
    private IEnumerator StoreOriginalSpeedDelayed(EnemyController controller)
    {
        // Wait for MobStats to initialize
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        if (controller != null && controller.mobStats != null)
        {
            float originalSpeed = controller.mobStats.MovementSpeed.Value;
            originalSpeeds[controller] = originalSpeed;
            
            // If this is a patrol, set patrol speed immediately
            if (isPatrol && controller.myNavMeshAgent != null)
            {
                controller.myNavMeshAgent.speed = originalSpeed * patrolSpeedMultiplier;
            }
        }
    }
    
    /// <summary>
    /// Sets up patrol behavior by finding connected road tiles
    /// </summary>
    public void SetupPatrol(List<Vector3> waypoints)
    {
        if (waypoints == null || waypoints.Count < 2)
        {
            Debug.LogWarning("[EnemyGroup] Cannot setup patrol - need at least 2 waypoints");
            isPatrol = false;
            return;
        }
        
        patrolWaypoints = new List<Vector3>(waypoints);
        isPatrol = true;
        currentWaypointIndex = 0;
        
        if (showDebugLogs)
        {
            Debug.Log($"[EnemyGroup] Patrol setup with {patrolWaypoints.Count} waypoints");
        }
        
        // Apply patrol speed to all members
        foreach (var member in members)
        {
            if (member != null && member.myNavMeshAgent != null && originalSpeeds.ContainsKey(member))
            {
                member.myNavMeshAgent.speed = originalSpeeds[member] * patrolSpeedMultiplier;
            }
        }
    }
    
    /// <summary>
    /// Pulls the entire group - all members chase player immediately
    /// </summary>
    public void PullGroup()
    {
        if (isGroupPulled)
            return; // Already pulled
        
        isGroupPulled = true;
        
        if (showDebugLogs)
        {
            Debug.Log($"[EnemyGroup] Group pulled! Activating {members.Count} members");
        }
        
        foreach (var member in members)
        {
            if (member != null && !member.isDead)
            {
                // Set pulled flag
                member.pulled = true;
                
                // Restore original movement speed
                if (originalSpeeds.ContainsKey(member) && member.myNavMeshAgent != null)
                {
                    member.myNavMeshAgent.speed = originalSpeeds[member];
                }
                
                // Force transition to chase state
                member.TransitionTo(new ChaseState(member));
            }
        }
    }
    
    void Update()
    {
        // Only update patrol if group is not pulled and is configured as patrol
        if (isGroupPulled || !isPatrol || patrolWaypoints.Count == 0)
            return;
        
        UpdatePatrol();
    }
    
    /// <summary>
    /// Handles patrol movement for all group members
    /// </summary>
    private void UpdatePatrol()
    {
        if (patrolWaypoints.Count == 0)
            return;
        
        Vector3 targetWaypoint = patrolWaypoints[currentWaypointIndex];
        
        // Move all living members towards current waypoint
        bool allReachedWaypoint = true;
        int aliveMembersMoving = 0;
        
        foreach (var member in members)
        {
            if (member != null && !member.isDead && !member.pulled)
            {
                aliveMembersMoving++;
                
                // Set destination to current waypoint
                if (member.myNavMeshAgent != null && member.myNavMeshAgent.isActiveAndEnabled)
                {
                    member.myNavMeshAgent.SetDestination(targetWaypoint);
                    
                    // Check if this member has reached the waypoint
                    float distanceToWaypoint = Vector3.Distance(member.transform.position, targetWaypoint);
                    if (distanceToWaypoint > waypointReachDistance)
                    {
                        allReachedWaypoint = false;
                    }
                }
            }
        }
        
        // If all alive members reached waypoint, move to next one
        if (allReachedWaypoint && aliveMembersMoving > 0)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Count;
            
            if (showDebugLogs)
            {
                Debug.Log($"[EnemyGroup] Patrol reached waypoint {currentWaypointIndex}/{patrolWaypoints.Count}");
            }
        }
    }
    
    /// <summary>
    /// Removes dead or destroyed members from the group
    /// </summary>
    public void CleanupDeadMembers()
    {
        members.RemoveAll(m => m == null || m.isDead);
    }
    
    void OnDrawGizmos()
    {
        if (!showPatrolGizmos || !isPatrol || patrolWaypoints.Count == 0)
            return;
        
        // Draw patrol path
        Gizmos.color = Color.cyan;
        for (int i = 0; i < patrolWaypoints.Count; i++)
        {
            Vector3 currentWaypoint = patrolWaypoints[i];
            Vector3 nextWaypoint = patrolWaypoints[(i + 1) % patrolWaypoints.Count];
            
            // Draw waypoint sphere
            Gizmos.DrawWireSphere(currentWaypoint, 0.5f);
            
            // Draw line to next waypoint
            Gizmos.DrawLine(currentWaypoint, nextWaypoint);
        }
        
        // Draw current target waypoint in different color
        if (Application.isPlaying && currentWaypointIndex < patrolWaypoints.Count)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(patrolWaypoints[currentWaypointIndex], 0.7f);
        }
    }
}