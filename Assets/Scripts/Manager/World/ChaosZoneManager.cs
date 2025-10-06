using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChaosZoneManager : MonoBehaviour
{
    public static ChaosZoneManager Instance { get; private set; }

    [Header("Chaos Zone Settings")]
    [SerializeField] private float pressureIntensity = 1.5f;
    [SerializeField] private float zoneDuration = 300f; // 5 minutes
    [SerializeField] private float difficultyMultiplier = 2f;
    [SerializeField] private int waveCount = 3;
    [SerializeField] private float waveCooldown = 30f;

    [Header("Rewards")]
    [SerializeField] private bool guaranteedSocketDrop = true;
    [SerializeField] private float experienceMultiplier = 2.5f;
    [SerializeField] private int bonusLootRolls = 2;

    [Header("Pressure Effects")]
    [SerializeField] private float healthPressureRate = 0.02f; // Health loss per second
    [SerializeField] private float energyPressureRate = 0.03f; // Energy loss per second
    [SerializeField] private Color pressureOverlayColor = new Color(0.8f, 0.2f, 0.2f, 0.1f);

    // Return portal information
    private Vector3 returnPosition;
    private string returnSceneName;
    
    // Zone state
    private bool isInChaosZone = false;
    private float zoneTimer = 0f;
    private int currentWave = 0;
    private float nextWaveTimer = 0f;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    // Effects
    private GameObject pressureOverlay;
    private Light ambientLighting;
    
    // Player reference
    private PlayerStats playerStats;
    private IsometricPlayer player;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Check if we're in a chaos zone scene
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene.Contains("ChaosZone") || currentScene.Contains("Chaos"))
        {
            StartChaosZone();
        }
    }

    private void Update()
    {
        if (isInChaosZone)
        {
            UpdateChaosZone();
        }
    }

    public void StartChaosZone()
    {
        isInChaosZone = true;
        zoneTimer = 0f;
        currentWave = 0;
        nextWaveTimer = 0f;

        // Get player references
        if (PlayerManager.instance != null)
        {
            player = PlayerManager.instance.player;
            if (player != null)
                playerStats = player.GetComponent<PlayerStats>();
        }

        SetupChaosEnvironment();
        StartCoroutine(InitiateChaosSequence());

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySound("ChaosZoneEnter");

        Debug.Log("Chaos Zone initiated! Survive the pressure!");
    }

    private void UpdateChaosZone()
    {
        zoneTimer += Time.deltaTime;
        nextWaveTimer += Time.deltaTime;

        // Apply pressure effects to player
        ApplyPressureEffects();

        // Check for wave spawning
        if (currentWave < waveCount && nextWaveTimer >= waveCooldown)
        {
            SpawnChaosWave();
            nextWaveTimer = 0f;
            currentWave++;
        }

        // Check zone completion conditions
        CheckZoneCompletion();

        // Auto-exit if time limit exceeded
        if (zoneTimer >= zoneDuration)
        {
            CompleteChaosZone(false); // Failed due to timeout
        }
    }

    private void ApplyPressureEffects()
    {
        if (playerStats == null) return;

        // Apply health pressure
        float healthLoss = healthPressureRate * pressureIntensity * Time.deltaTime;
        // Note: This would need to be adapted to the actual health system
        // playerStats.TakeDamage(healthLoss);

        // Apply energy pressure
        float energyLoss = energyPressureRate * pressureIntensity * Time.deltaTime;
        // Note: This would need to be adapted to the actual energy/mana system
        // playerStats.DrainEnergy(energyLoss);

        // Visual pressure effects
        UpdatePressureVisuals();
    }

    private void UpdatePressureVisuals()
    {
        // Pulsing red overlay effect
        if (pressureOverlay != null)
        {
            float pulse = Mathf.Sin(Time.time * 2f) * 0.5f + 0.5f;
            Color overlayColor = pressureOverlayColor;
            overlayColor.a = overlayColor.a * pulse * pressureIntensity;
            // Apply to screen overlay (would need UI system integration)
        }

        // Darken ambient lighting
        if (ambientLighting != null)
        {
            float intensity = Mathf.Lerp(1f, 0.3f, pressureIntensity * 0.5f);
            ambientLighting.intensity = intensity;
        }
    }

    private IEnumerator InitiateChaosSequence()
    {
        // Brief preparation time
        yield return new WaitForSeconds(3f);

        // Start first wave
        SpawnChaosWave();
        currentWave++;
    }

    private void SpawnChaosWave()
    {
        PrefabCollection prefabCollection = FindObjectOfType<PrefabCollection>();
        if (prefabCollection == null) return;

        int enemyCount = Mathf.RoundToInt((currentWave + 2) * difficultyMultiplier);
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject enemyPrefab = prefabCollection.GetRandomEnemie();
            
            if (enemyPrefab != null)
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                
                // Enhance enemy for chaos zone
                EnhanceEnemyForChaos(enemy);
                activeEnemies.Add(enemy);
            }
        }

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySound("ChaosWaveSpawn");

        Debug.Log($"Chaos Wave {currentWave} spawned with {enemyCount} enemies!");
    }

    private void EnhanceEnemyForChaos(GameObject enemy)
    {
        // Increase enemy difficulty for chaos zone
        MobStats mobStats = enemy.GetComponent<MobStats>();
        if (mobStats != null)
        {
            // Apply difficulty multipliers (would need to access actual stat system)
            // mobStats.MultiplyStats(difficultyMultiplier);
        }

        // Add special chaos effects
        SummonedMob summonedComponent = enemy.AddComponent<SummonedMob>();
        
        // Visual enhancement for chaos enemies
        Renderer enemyRenderer = enemy.GetComponent<Renderer>();
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.Lerp(enemyRenderer.material.color, Color.red, 0.3f);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Get player position as reference
        Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;
        
        // Spawn enemies in a ring around the player
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(8f, 15f);
        
        Vector3 spawnPos = playerPos + new Vector3(
            Mathf.Cos(angle) * distance,
            0f,
            Mathf.Sin(angle) * distance
        );

        return spawnPos;
    }

    private void CheckZoneCompletion()
    {
        // Clean up destroyed enemies from list
        activeEnemies.RemoveAll(enemy => enemy == null);

        // Check if all waves completed and enemies defeated
        if (currentWave >= waveCount && activeEnemies.Count == 0)
        {
            CompleteChaosZone(true); // Success
        }
    }

    private void CompleteChaosZone(bool success)
    {
        isInChaosZone = false;

        if (success)
        {
            // Award enhanced rewards
            GiveChaosRewards();
            
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySound("ChaosZoneComplete");
            
            Debug.Log("Chaos Zone completed successfully! Enhanced rewards granted!");
        }
        else
        {
            if (AudioManager.instance != null)
                AudioManager.instance.PlaySound("ChaosZoneFailed");
            
            Debug.Log("Chaos Zone failed. Better luck next time!");
        }

        // Return to original location
        StartCoroutine(ReturnToOrigin());
    }

    private void GiveChaosRewards()
    {
        Vector3 rewardPosition = player != null ? player.transform.position : Vector3.zero;
        
        // Use the dedicated reward system
        ChaosZoneRewardSystem rewardSystem = FindObjectOfType<ChaosZoneRewardSystem>();
        if (rewardSystem != null)
        {
            rewardSystem.GrantChaosZoneRewards(rewardPosition, true, currentWave);
        }
        else
        {
            // Fallback to basic rewards if reward system not available
            if (playerStats != null)
            {
                float bonusXP = 50f * experienceMultiplier;
                playerStats.Gain_xp(bonusXP);
            }

            // Basic loot drops
            for (int i = 0; i < bonusLootRolls; i++)
            {
                if (ItemDatabase.instance != null)
                {
                    ItemDatabase.instance.GetWeightDrop(rewardPosition);
                }
            }
            
            Debug.Log("Basic chaos zone rewards granted (reward system not found)");
        }
    }

    private IEnumerator ReturnToOrigin()
    {
        // Transition effect
        yield return new WaitForSeconds(2f);

        // Load return scene
        if (!string.IsNullOrEmpty(returnSceneName))
        {
            SceneManager.LoadScene(returnSceneName);
            
            // After scene loads, position player at return point
            yield return new WaitForSeconds(0.5f);
            if (player != null)
            {
                player.transform.position = returnPosition;
            }
        }
    }

    private void SetupChaosEnvironment()
    {
        // Create pressure overlay effect
        CreatePressureOverlay();
        
        // Modify ambient lighting
        ambientLighting = FindObjectOfType<Light>();
        if (ambientLighting != null)
        {
            ambientLighting.color = Color.Lerp(ambientLighting.color, Color.red, 0.2f);
        }
    }

    private void CreatePressureOverlay()
    {
        // This would create a UI overlay for pressure effects
        // Implementation would depend on the existing UI system
        GameObject overlayCanvas = GameObject.Find("Canvas");
        if (overlayCanvas != null)
        {
            // Create overlay effect object
            pressureOverlay = new GameObject("PressureOverlay");
            pressureOverlay.transform.SetParent(overlayCanvas.transform);
            // Add UI components for visual effect
        }
    }

    // Public methods for external systems
    public void SetReturnPoint(Vector3 position)
    {
        returnPosition = position;
    }

    public void SetReturnScene(string sceneName)
    {
        returnSceneName = sceneName;
    }

    public bool IsInChaosZone()
    {
        return isInChaosZone;
    }

    public float GetZoneProgress()
    {
        if (!isInChaosZone) return 0f;
        return zoneTimer / zoneDuration;
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetRemainingEnemies()
    {
        return activeEnemies.Count;
    }
}