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
    [SerializeField] private float spawnInterval = 8f; // Time between spawn waves
    [SerializeField] private int baseEnemiesPerWave = 2; // Starting number of enemies per wave
    [SerializeField] private float minSpawnDistance = 15f; // Min distance from player
    [SerializeField] private float maxSpawnDistance = 25f; // Max distance from player
    [SerializeField] private float spawnRadius = 3f; // Radius around spawn point for enemy group
    
    [Header("Difficulty Scaling")]
    [SerializeField] private float difficultyIncreaseInterval = 60f; // Increase difficulty every X seconds
    [SerializeField] private int maxEnemiesPerWave = 8; // Maximum enemies in one wave
    [SerializeField] private float enemyHealthMultiplier = 1.0f; // Multiplier for enemy health over time
    [SerializeField] private float enemyDamageMultiplier = 1.0f; // Multiplier for enemy damage over time
    
    [Header("Debug")]
    [SerializeField] private bool enableSpawning = true;
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
        
        // Calculate number of enemies based on difficulty
        int enemyCount = Mathf.Min(
            baseEnemiesPerWave + currentDifficultyLevel, 
            maxEnemiesPerWave
        );
        
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
            
            SpawnEnemy(finalSpawnPos);
        }
        
        if (debugLogs)
        {
            Debug.Log($"[EnemyWaveSpawner] Spawned wave of {enemyCount} enemies at {spawnCenter}");
        }
    }
    
    private Vector3 FindSpawnPosition()
    {
        Vector3 playerPos = playerTransform.position;
        
        // Find all spawn positions marked with "SpawnPos" tag
        GameObject[] spawnPositions = GameObject.FindGameObjectsWithTag("SpawnPos");
        
        if (spawnPositions.Length == 0)
        {
            if (debugLogs)
            {
                Debug.LogWarning("[EnemyWaveSpawner] No GameObjects with tag 'SpawnPos' found!");
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
        
        // If still no valid positions, return zero (shouldn't happen with proper setup)
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
        return selectedSpawn.transform.position;
    }
    
    private void SpawnEnemy(Vector3 position)
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
        
        // Apply difficulty scaling after a frame to ensure MobStats.Start() has run
        if (currentDifficultyLevel > 0)
        {
            StartCoroutine(ApplyDifficultyScalingDelayed(enemy));
        }
        
        // Set pulled flag to make enemy immediately chase player
        StartCoroutine(ForceChaseStateDelayed(enemy));
    }
    
    private IEnumerator ApplyDifficultyScalingDelayed(GameObject enemy)
    {
        // Wait for MobStats.Start() to run first
        yield return new WaitForEndOfFrame();
        
        if (enemy == null)
            yield break;
            
        var mobStats = enemy.GetComponent<MobStats>();
        if (mobStats == null)
            yield break;
        
        // Scale enemy stats based on difficulty level
        // Keep enemies relatively weak but slightly increase stats over time
        float healthScale = 1.0f + (currentDifficultyLevel * 0.1f); // +10% HP per level
        float damageScale = 1.0f + (currentDifficultyLevel * 0.05f); // +5% damage per level
        
        // Apply scaling
        if (mobStats.Hp != null)
        {
            mobStats.Hp.AddModifier(new StatModifier(mobStats.Hp.BaseValue * (healthScale - 1f), StatModType.Flat));
        }
        
        if (mobStats.AttackPower != null)
        {
            mobStats.AttackPower.AddModifier(new StatModifier(mobStats.AttackPower.BaseValue * (damageScale - 1f), StatModType.Flat));
        }
        
        if (debugLogs)
        {
            Debug.Log($"[EnemyWaveSpawner] Applied difficulty scaling (level {currentDifficultyLevel}) to {enemy.name}");
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
