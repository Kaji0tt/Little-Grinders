using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns enemy waves at intervals outside the player's view
/// Enemies are weak but numerous, creating pressure over time
/// Difficulty increases as time progresses
/// </summary>
public class EnemyWaveSpawner : MonoBehaviour
{
    #region Singleton
    public static EnemyWaveSpawner instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 15f; // Time between spawn waves (longer interval for scattered enemies)
    [SerializeField] private float minSpawnDistance = 25f; // Min distance from player (increased to spawn further away)
    [SerializeField] private float maxSpawnDistance = 40f; // Max distance from player (increased for more variation)
    [SerializeField] private float spawnRadius = 3f; // Radius around spawn point for enemy group (smaller for single enemies)
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float difficultyIncreaseInterval = 60f; // Increase difficulty every X seconds (for scattered spawns only)
    [SerializeField] private int baseEnemiesPerWave = 5; // Base number of enemies per wave (scattered spawns)
    [SerializeField] [Range(0f, 1f)] private float increaseEnemiesPerWavePercent = 0.25f; // +15% enemies per difficulty level (0.15 = +15%)
    [SerializeField] private float enemyHealthMultiplier = 1.1f; // +10% HP per difficulty level
    [SerializeField] private float enemyDamageMultiplier = 1.05f; // +5% damage per difficulty level
    
    [Header("Totem Wave Settings")]
    [SerializeField] private bool useWaveBasedDifficulty = true; // Use wave number for difficulty instead of time
    private int currentWaveNumber = 0; // Controlled by TotemInteractable
    
    [Header("Debug")]
    [SerializeField] private bool enableSpawning = false; // Disabled by default - only active during Totem challenges
    [SerializeField] private bool debugLogs = false;
    
    private float timeSinceLastSpawn = 0f;
    private float gameTime = 0f;
    private int currentDifficultyLevel = 0;
    private PrefabCollection prefabCollection;
    private Transform playerTransform;
    private bool isInitialized = false;
    
    private void Start()
    {
        StartCoroutine(InitializeDelayed());
    }
    
    private IEnumerator InitializeDelayed()
    {
        // Force disable spawning at start - only Totem can enable it
        enableSpawning = false;
        
        // Wait for game systems to initialize
        yield return new WaitForSeconds(2f);
        
        prefabCollection = PrefabCollection.instance;
        if (prefabCollection == null)
        {
            Debug.LogError("[EnemyWaveSpawner] PrefabCollection not found!");
            yield break;
        }
        
        if (PlayerManager.instance?.player != null)
        {
            playerTransform = PlayerManager.instance.player.transform;
        }
        else
        {
            Debug.LogError("[EnemyWaveSpawner] Player not found!");
            yield break;
        }
        
        isInitialized = true;
        
        if (debugLogs)
        {
            Debug.Log("[EnemyWaveSpawner] Initialized successfully. Starting enemy waves.");
        }
    }
    
    private void Update()
    {
        if (!enableSpawning || !isInitialized || playerTransform == null)
            return;
        
        gameTime += Time.deltaTime;
        timeSinceLastSpawn += Time.deltaTime;
        
        // Check if it's time to increase difficulty
        int newDifficultyLevel = Mathf.FloorToInt(gameTime / difficultyIncreaseInterval);
        if (newDifficultyLevel > currentDifficultyLevel)
        {
            currentDifficultyLevel = newDifficultyLevel;
            if (debugLogs)
            {
                Debug.Log($"[EnemyWaveSpawner] Difficulty increased to level {currentDifficultyLevel}");
            }
        }
        
        // Check if it's time to spawn a new wave
        if (timeSinceLastSpawn >= spawnInterval)
        {
            SpawnWave();
            timeSinceLastSpawn = 0f;
        }
    }
    
    private void SpawnWave()
    {
        if (playerTransform == null || prefabCollection == null)
            return;
        
        // Calculate number of enemies based on difficulty with percentage increase
        float enemyCountFloat = baseEnemiesPerWave * (1f + (currentDifficultyLevel * increaseEnemiesPerWavePercent));
        int enemyCount = Mathf.RoundToInt(enemyCountFloat);
        
        // Find a spawn position outside player's view
        Vector3 spawnCenter = FindSpawnPosition();
        
        if (spawnCenter == Vector3.zero)
        {
            if (debugLogs)
            {
                Debug.LogWarning("[EnemyWaveSpawner] Could not find valid spawn position");
            }
            return;
        }
        
        // Spawn enemies in a group around the spawn center
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnOffset = Random.insideUnitSphere * spawnRadius;
            spawnOffset.y = 0; // Keep on ground level
            Vector3 finalSpawnPos = spawnCenter + spawnOffset;
            
            SpawnEnemyWithDifficulty(finalSpawnPos, false);
        }
        
        if (debugLogs)
        {
            Debug.Log($"[EnemyWaveSpawner] Spawned wave of {enemyCount} enemies at {spawnCenter}");
        }
    }
    
    private Vector3 FindSpawnPosition()
    {
        Vector3 playerPos = playerTransform.position;
        
        // Spawn in random direction around player for true 360° coverage
        // This prevents predictable spawns and forces player to watch all directions
        
        int maxAttempts = 15;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Random direction (360 degrees)
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(randomAngle), 0f, Mathf.Sin(randomAngle));
            
            // Random distance within range
            float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
            
            // Calculate spawn position
            Vector3 potentialSpawnPos = playerPos + direction * randomDistance;
            
            // Check if position is on NavMesh
            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(potentialSpawnPos, out navHit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // Valid spawn position found
                if (debugLogs)
                {
                    Debug.Log($"[EnemyWaveSpawner] Found spawn position at {navHit.position} (distance: {Vector3.Distance(playerPos, navHit.position):F1}m, angle: {randomAngle * Mathf.Rad2Deg:F0}°)");
                }
                return navHit.position;
            }
        }
        
        // Fallback: Try using tagged SpawnPos if random positioning failed
        GameObject[] spawnPositions = GameObject.FindGameObjectsWithTag("SpawnPos");
        
        if (spawnPositions.Length == 0)
        {
            if (debugLogs)
            {
                Debug.LogWarning("[EnemyWaveSpawner] No GameObjects with tag 'SpawnPos' found and NavMesh spawn failed!");
            }
            return Vector3.zero;
        }
        
        // Filter spawn positions by distance from player
        List<GameObject> validSpawnPositions = new List<GameObject>();
        
        foreach (GameObject spawnPos in spawnPositions)
        {
            float distance = Vector3.Distance(playerPos, spawnPos.transform.position);
            
            // Check if spawn position is within valid distance range
            if (distance >= minSpawnDistance && distance <= maxSpawnDistance)
            {
                validSpawnPositions.Add(spawnPos);
            }
        }
        
        // If no spawn positions in range, use any spawn position outside minimum distance
        if (validSpawnPositions.Count == 0)
        {
            foreach (GameObject spawnPos in spawnPositions)
            {
                float distance = Vector3.Distance(playerPos, spawnPos.transform.position);
                if (distance >= minSpawnDistance)
                {
                    validSpawnPositions.Add(spawnPos);
                }
            }
        }
        
        // If still no valid positions, return zero
        if (validSpawnPositions.Count == 0)
        {
            if (debugLogs)
            {
                Debug.LogWarning("[EnemyWaveSpawner] No valid spawn positions found within distance constraints!");
            }
            return Vector3.zero;
        }
        
        // Return random valid spawn position
        GameObject selectedSpawn = validSpawnPositions[Random.Range(0, validSpawnPositions.Count)];
        if (debugLogs)
        {
            Debug.Log($"[EnemyWaveSpawner] Using fallback SpawnPos: {selectedSpawn.name}");
        }
        return selectedSpawn.transform.position;
    }
    
    /// <summary>
    /// Spawns a single enemy with difficulty scaling applied
    /// </summary>
    /// <param name="position">Spawn position</param>
    /// <param name="isSummonedMob">If true, adds SummonedMob component for cleanup</param>
    private void SpawnEnemyWithDifficulty(Vector3 position, bool isSummonedMob = false)
    {
        GameObject enemyPrefab = prefabCollection.GetRandomEnemie();
        if (enemyPrefab == null)
        {
            if (debugLogs)
            {
                Debug.LogWarning("[EnemyWaveSpawner] No enemy prefab available");
            }
            return;
        }
        
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemy.name = enemyPrefab.name;
        
        // Set parent
        if (MapGenHandler.instance?.mobParentObj != null)
        {
            enemy.transform.SetParent(MapGenHandler.instance.mobParentObj.transform);
        }
        
        // Remove "(Clone)" from name
        if (enemy.name.Contains("(Clone)"))
        {
            enemy.name = enemy.name.Replace("(Clone)", "").Trim();
        }
        
        // Add SummonedMob component if this is a Totem-spawned enemy
        if (isSummonedMob)
        {
            enemy.AddComponent<SummonedMob>();
        }
        
        // Apply difficulty scaling
        int difficultyLevel = useWaveBasedDifficulty ? currentWaveNumber : currentDifficultyLevel;
        
        if (difficultyLevel > 0)
        {
            StartCoroutine(ApplyDifficultyScalingDelayed(enemy, difficultyLevel));
        }
        
        // Set pulled flag to make enemy immediately chase player
        StartCoroutine(ForceChaseStateDelayed(enemy));
    }
    
    private IEnumerator ApplyDifficultyScalingDelayed(GameObject enemy, int difficultyLevel)
    {
        // Wait for MobStats.Start() to run first
        yield return new WaitForEndOfFrame();
        
        if (enemy == null)
            yield break;
            
        var mobStats = enemy.GetComponent<MobStats>();
        if (mobStats == null)
            yield break;
        
        // Scale enemy stats based on difficulty level
        // For wave-based: difficultyLevel = wave number (1, 2, 3...)
        // For time-based: difficultyLevel = time/interval (0, 1, 2...)
        
        // Use power formula for exponential scaling
        float healthScale = Mathf.Pow(enemyHealthMultiplier, difficultyLevel);
        float damageScale = Mathf.Pow(enemyDamageMultiplier, difficultyLevel);
        
        // Apply scaling
        if (mobStats.Hp != null)
        {
            float healthBonus = mobStats.Hp.BaseValue * (healthScale - 1f);
            mobStats.Hp.AddModifier(new StatModifier(healthBonus, StatModType.Flat));
        }
        
        if (mobStats.AttackPower != null)
        {
            float damageBonus = mobStats.AttackPower.BaseValue * (damageScale - 1f);
            mobStats.AttackPower.AddModifier(new StatModifier(damageBonus, StatModType.Flat));
        }
        
        if (debugLogs)
        {
            Debug.Log($"[EnemyWaveSpawner] Applied difficulty scaling (level {difficultyLevel}) to {enemy.name}: HP x{healthScale:F2}, Damage x{damageScale:F2}");
        }
    }
    
    private IEnumerator ForceChaseStateDelayed(GameObject enemy)
    {
        // Länger warten, bis der EnemyController vollständig initialisiert ist
        yield return new WaitForSeconds(1f);
        
        if (enemy == null)
        {
            if (debugLogs)
            {
                Debug.LogWarning("[EnemyWaveSpawner] Enemy object is null, cannot set pulled flag");
            }
            yield break;
        }
            
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController == null)
        {
            if (debugLogs)
            {
                Debug.LogWarning($"[EnemyWaveSpawner] No EnemyController found on {enemy.name}");
            }
            yield break;
        }
        
        // Setze Flag UND forciere State-Wechsel als Backup
        enemyController.pulled = true;
        
        if (debugLogs)
        {
            Debug.Log($"[EnemyWaveSpawner] Set pulled=true on {enemy.name}");
        }
        
        // Backup: Falls IdleState Update nicht funktioniert, direkt State wechseln
        yield return new WaitForSeconds(0.1f);
        
        if (enemyController != null && !enemyController.isDead)
        {
            enemyController.TransitionTo(new ChaseState(enemyController));
            
            if (debugLogs)
            {
                Debug.Log($"[EnemyWaveSpawner] Forced ChaseState on {enemy.name}");
            }
        }
    }
    
    /// <summary>
    /// Enable or disable spawning (useful for events, boss fights, etc.)
    /// </summary>
    public void SetSpawningEnabled(bool enabled)
    {
        enableSpawning = enabled;
        
        if (debugLogs)
        {
            Debug.Log($"[EnemyWaveSpawner] Spawning {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    /// <summary>
    /// Set the current wave number for difficulty calculation (called by TotemInteractable)
    /// </summary>
    public void SetWaveNumber(int waveNumber)
    {
        currentWaveNumber = waveNumber;
        
        if (debugLogs)
        {
            Debug.Log($"[EnemyWaveSpawner] Wave number set to {waveNumber}");
        }
    }
    
    /// <summary>
    /// Spawn a Totem wave at specific positions (Altar + Totem)
    /// Called by TotemInteractable each wave
    /// </summary>
    public void SpawnTotemWave(int enemyCount, Vector3 altarPosition, float altarRadius, Vector3[] totemPositions, float totemPercentage)
    {
        if (!isInitialized || prefabCollection == null)
        {
            Debug.LogWarning("[EnemyWaveSpawner] Cannot spawn totem wave - not initialized!");
            return;
        }
        
        // Split enemies between Altar and Totem
        int totemEnemies = Mathf.RoundToInt(enemyCount * totemPercentage);
        int altarEnemies = enemyCount - totemEnemies;
        
        if (debugLogs)
        {
            Debug.Log($"[EnemyWaveSpawner] Spawning Totem Wave {currentWaveNumber}: {enemyCount} total ({altarEnemies} at Altar, {totemEnemies} at Totem)");
        }
        
        // Spawn at Altar
        for (int i = 0; i < altarEnemies; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * altarRadius;
            Vector3 spawnPos = altarPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
            
            // Ensure on NavMesh
            UnityEngine.AI.NavMeshHit navHit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPos = navHit.position;
            }
            
            SpawnEnemyWithDifficulty(spawnPos, true);
        }
        
        // Spawn at Totem positions
        if (totemPositions != null && totemPositions.Length > 0)
        {
            for (int i = 0; i < totemEnemies; i++)
            {
                Vector3 spawnPos = totemPositions[Random.Range(0, totemPositions.Length)];
                SpawnEnemyWithDifficulty(spawnPos, true);
            }
        }
        else if (totemEnemies > 0)
        {
            // Fallback: spawn remaining enemies at Altar
            for (int i = 0; i < totemEnemies; i++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * altarRadius;
                Vector3 spawnPos = altarPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
                
                UnityEngine.AI.NavMeshHit navHit;
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    spawnPos = navHit.position;
                }
                
                SpawnEnemyWithDifficulty(spawnPos, true);
            }
        }
    }
    
    /// <summary>
    /// Reset the spawn timer and difficulty
    /// </summary>
    public void ResetSpawner()
    {
        timeSinceLastSpawn = 0f;
        gameTime = 0f;
        currentDifficultyLevel = 0;
        
        if (debugLogs)
        {
            Debug.Log("[EnemyWaveSpawner] Spawner reset");
        }
    }
}
