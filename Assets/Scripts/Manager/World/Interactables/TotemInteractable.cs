using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NEW Wave-Based Totem System
/// Spawns enemy waves that get progressively harder
/// Must be deactivated at an Altar to receive rewards
/// No longer uses Singleton - each map has its own Totem instance
/// </summary>
public class TotemInteractable : BaseInteractable
{
    
    [Header("Totem Settings")]
    [SerializeField] private GameObject[] spawnPoints;
    [SerializeField] private Light lightBulb;
    [SerializeField] private TotemAltarPathGuide pathGuide; // Visual guide to altar
    
    [Header("Wave System Settings")]
    [SerializeField] private float waveInterval = 12f; // Time between waves
    [SerializeField] private int baseEnemiesPerWave = 2; // Starting enemies per wave
    [SerializeField] private int maxEnemiesPerWave = 8; // Maximum enemies per wave
    
    [Header("Spawn Position Settings")]
    [SerializeField] private float altarSpawnRadius = 8f; // Radius around altar for main wave spawns
    [SerializeField] [Range(0f, 1f)] private float totemSpawnPercentage = 0.25f; // Percentage of enemies that spawn at totem (blocks direct path)
    [SerializeField] private int firstWaveTotemEnemies = 5; // Extra enemies at Totem for first wave (4-6 recommended)
    
    [Header("Sound Settings")]
    [SerializeField] private string callSoundName = "Totem_Call"; // Sound bei Aktivierung
    [SerializeField] private string loopSoundName = "Totem_Loop"; // Sound während Challenge
    [SerializeField] private string deactivationSoundName = "Totem_Deactivate"; // Sound bei Deaktivierung
    
    [Header("Visual Effects")]
    [SerializeField] private bool enableVoidEffect = true;
    [SerializeField] private Color totemVignetteColor = new Color(0.254f, 0.027f, 0.302f, 1f); // #41074D
    
    [Header("Void Effect Settings")]
    [SerializeField] private Color voidLiftColor = new Color(0.3f, 0.1f, 0.4f, 1f);
    [SerializeField] private float voidVignetteIntensity = 0.6f;
    [SerializeField] private float voidSaturation = -30f;
    [SerializeField] private float voidTransitionDuration = 2f;
    [SerializeField] private bool enableVoidAnimation = true;
    [SerializeField] private float voidAnimationSpeed = 1.5f;
    [SerializeField] private float voidChromaticAberration = 0.3f;
    [SerializeField] private bool enableChromaticPulse = true;
    [SerializeField] private float chromaticPulseSpeed = 2f;
    
    // Runtime data
    private PrefabCollection prefabCollection;
    private GameObject mobParent;
    private AltarInteractable linkedAltar;
    
    // Challenge state
    private bool challengeActive = false;
    private int waveNumber = 0;
    private float activeTime = 0f;
    private float timeSinceLastWave = 0f; // Track time since last wave spawned
    private Coroutine waveSpawnCoroutine;
    
    // Sound system
    private AudioManager.ActiveSound activeLoopSound;
    
    // Void effect system
    private bool isVoidEffectActive = false;
    private Coroutine voidEffectCoroutine;
    private Vector4 originalLiftColor;
    private float originalVignetteIntensity;
    private Color originalVignetteColor;
    private float originalSaturation;
    private float originalChromaticAberration;
    private bool hasStoredOriginalValues = false;
    
    protected override void Start()
    {
        canBeUsedMultipleTimes = false;
        displayName = "Ancient Totem";
        interactPrompt = "Press [E] to activate Totem Challenge";
        
        mobParent = GameObject.Find("MobParent");
        prefabCollection = FindFirstObjectByType<PrefabCollection>();
        
        // Subscribe to events
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerDied += HandlePlayerDeath;
        }
        
        base.Start();
    }
    
    protected override bool CanInteract()
    {
        return base.CanInteract() && !challengeActive;
    }
    
    protected override void OnInteract()
    {
        StartChallenge();
    }
    
    private void StartChallenge()
    {
        challengeActive = true;
        waveNumber = 0;
        activeTime = 0f;
        timeSinceLastWave = 0f;
        
        Debug.Log("[TotemInteractable] Challenge started!");
        
        // Play sound
        if (AudioManager.instance != null && !string.IsNullOrEmpty(callSoundName))
        {
            AudioManager.instance.PlaySound(callSoundName);
        }
        
        // Turn off light
        if (lightBulb != null)
        {
            lightBulb.intensity = 0;
        }
        
        // Enable EnemyWaveSpawner for totem waves
        if (EnemyWaveSpawner.instance != null)
        {
            EnemyWaveSpawner.instance.SetSpawningEnabled(true);
        }
        
        // Activate void effect
        if (enableVoidEffect && PPVolumeManager.instance != null)
        {
            ActivateTotemVoidEffect();
        }
        
        // Start loop sound
        StartLoopSound();
        
        // Notify altar
        if (linkedAltar != null)
        {
            linkedAltar.OnTotemActivated();
        }
        
        // Show Challenge UI
        UI_TotemChallenge challengeUI = FindFirstObjectByType<UI_TotemChallenge>();
        if (challengeUI != null)
        {
            challengeUI.ShowChallengeUI(this);
        }
        
        // Show Void Essence UI during challenge
        UI_VoidEssence voidEssenceUI = FindFirstObjectByType<UI_VoidEssence>();
        if (voidEssenceUI != null)
        {
            voidEssenceUI.ShowForChallenge();
        }
        
        // Show path guide to Altar
        if (pathGuide != null && linkedAltar != null)
        {
            pathGuide.ShowPath(transform, linkedAltar.transform);
        }
        
        // Start wave spawning
        waveSpawnCoroutine = StartCoroutine(WaveSpawnLoop());
        
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog("Totem Challenge activated! Survive the waves!", 3f);
        }
    }
    
    private IEnumerator WaveSpawnLoop()
    {
        while (challengeActive)
        {
            // Increment wave
            waveNumber++;
            timeSinceLastWave = 0f; // Reset timer
            
            // Spawn wave
            SpawnWave(waveNumber);
            
            // Wait for next wave
            float timeWaited = 0f;
            while (timeWaited < waveInterval && challengeActive)
            {
                timeWaited += Time.deltaTime;
                timeSinceLastWave += Time.deltaTime;
                activeTime += Time.deltaTime;
                yield return null;
            }
        }
    }
    
    private void SpawnWave(int wave)
    {
        if (EnemyWaveSpawner.instance == null)
        {
            Debug.LogError("[TotemInteractable] Cannot spawn wave - EnemyWaveSpawner not found!");
            return;
        }
        
        if (linkedAltar == null)
        {
            Debug.LogWarning("[TotemInteractable] Cannot spawn wave - no linked Altar found!");
            return;
        }
        
        // Calculate enemy count
        int enemyCount = Mathf.Min(baseEnemiesPerWave + wave - 1, maxEnemiesPerWave);
        
        // Special handling for first wave: spawn more enemies at Totem
        float currentTotemPercentage = totemSpawnPercentage;
        if (wave == 1 && firstWaveTotemEnemies > 0)
        {
            // Override: spawn fixed number at Totem for wave 1
            enemyCount = Mathf.Max(enemyCount, firstWaveTotemEnemies);
            currentTotemPercentage = (float)firstWaveTotemEnemies / enemyCount;
            Debug.Log($"[TotemInteractable] Wave 1 - Spawning {firstWaveTotemEnemies} enemies at Totem, {enemyCount - firstWaveTotemEnemies} at Altar");
        }
        
        Debug.Log($"[TotemInteractable] Requesting Wave {wave} spawn with {enemyCount} enemies from EnemyWaveSpawner");
        
        // Update wave number in EnemyWaveSpawner for difficulty calculation
        EnemyWaveSpawner.instance.SetWaveNumber(wave);
        
        // Collect totem spawn positions
        Vector3[] totemPositions = null;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            totemPositions = new Vector3[spawnPoints.Length];
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                totemPositions[i] = spawnPoints[i].transform.position;
            }
        }
        
        // Delegate spawning to EnemyWaveSpawner
        EnemyWaveSpawner.instance.SpawnTotemWave(
            enemyCount,
            linkedAltar.transform.position,
            altarSpawnRadius,
            totemPositions,
            currentTotemPercentage
        );
    }
    
    /// <summary>
    /// Called by Altar when player deactivates the totem
    /// </summary>
    public void DeactivateChallenge()
    {
        if (!challengeActive) return;
        
        Debug.Log("[TotemInteractable] Challenge deactivated by Altar");
        
        challengeActive = false;
        
        // Stop wave spawning
        if (waveSpawnCoroutine != null)
        {
            StopCoroutine(waveSpawnCoroutine);
            waveSpawnCoroutine = null;
        }
        
        // Disable EnemyWaveSpawner
        if (EnemyWaveSpawner.instance != null)
        {
            EnemyWaveSpawner.instance.SetSpawningEnabled(false);
        }
        
        // Stop sounds
        StopLoopSound();
        if (AudioManager.instance != null && !string.IsNullOrEmpty(deactivationSoundName))
        {
            AudioManager.instance.PlaySound(deactivationSoundName);
        }
        
        // Deactivate void effect
        if (enableVoidEffect && PPVolumeManager.instance != null)
        {
            DeactivateTotemVoidEffect();
        }
        
        // Turn light back on
        if (lightBulb != null)
        {
            lightBulb.color = Color.white;
            lightBulb.intensity = 0.7f;
        }
        
        // Clean up summoned mobs
        CleanupSummonedMobs();
        
        // Hide Challenge UI
        UI_TotemChallenge challengeUI = FindFirstObjectByType<UI_TotemChallenge>();
        if (challengeUI != null)
        {
            challengeUI.HideChallengeUI();
        }
        
        // NOTE: UI_VoidEssence.HideForChallenge() moved to AltarInteractable.OnInteract()
        // to ensure UI stays visible during reward animation
        
        // Hide path guide
        if (pathGuide != null)
        {
            pathGuide.HidePath();
        }
        
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog("Totem Challenge deactivated!", 2f);
        }
    }
    
    private void CleanupSummonedMobs()
    {
        SummonedMob[] summonedMobs = FindObjectsByType<SummonedMob>(FindObjectsSortMode.None);
        foreach (SummonedMob mob in summonedMobs)
        {
            if (mob != null)
            {
                Destroy(mob.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Called when player dies or map changes - cancel without rewards
    /// </summary>
    private void HandlePlayerDeath()
    {
        if (challengeActive)
        {
            CancelChallenge();
        }
    }
    
    public void CancelChallenge()
    {
        if (!challengeActive) return;
        
        Debug.Log("[TotemInteractable] Challenge cancelled (no rewards)");
        
        challengeActive = false;
        
        // Stop wave spawning
        if (waveSpawnCoroutine != null)
        {
            StopCoroutine(waveSpawnCoroutine);
            waveSpawnCoroutine = null;
        }
        
        // Disable spawner
        if (EnemyWaveSpawner.instance != null)
        {
            EnemyWaveSpawner.instance.SetSpawningEnabled(false);
        }
        
        // Stop sounds & effects
        StopLoopSound();
        
        // IMPORTANT: Reset PostProcessing IMMEDIATELY when cancelled (player died - Time.timeScale = 0)
        if (enableVoidEffect && PPVolumeManager.instance != null)
        {
            // Stop any running coroutine
            if (voidEffectCoroutine != null)
            {
                StopCoroutine(voidEffectCoroutine);
                voidEffectCoroutine = null;
            }
            
            // Reset immediately without animation
            isVoidEffectActive = false;
            PPVolumeManager.instance.SetDayNightUpdates(true);
            
            if (hasStoredOriginalValues)
            {
                PPVolumeManager.instance.SetLiftGammaGain(originalLiftColor);
                PPVolumeManager.instance.SetVignetteIntensity(originalVignetteIntensity);
                PPVolumeManager.instance.SetVignetteColor(originalVignetteColor);
                PPVolumeManager.instance.SetSaturation(0f);
                PPVolumeManager.instance.SetChromaticAberration(originalChromaticAberration);
                
                Debug.Log("[TotemInteractable] PostProcessing reset immediately (cancelled)");
            }
        }
        
        // Notify altar
        if (linkedAltar != null)
        {
            linkedAltar.OnTotemDeactivated();
        }
        
        // Hide Challenge UI
        UI_TotemChallenge challengeUI = FindFirstObjectByType<UI_TotemChallenge>();
        if (challengeUI != null)
        {
            challengeUI.HideChallengeUI();
        }
        
        // Hide Void Essence UI
        UI_VoidEssence voidEssenceUI = FindFirstObjectByType<UI_VoidEssence>();
        if (voidEssenceUI != null)
        {
            voidEssenceUI.HideForChallenge();
        }
        
        // Hide path guide
        if (pathGuide != null)
        {
            pathGuide.HidePath();
        }
        
        // Cleanup
        CleanupSummonedMobs();
        
        if (lightBulb != null)
        {
            lightBulb.color = Color.white;
            lightBulb.intensity = 0.7f;
        }
    }
    
    // Public getters for Altar and UI
    public bool IsChallengeActive() => challengeActive;
    public int GetWaveNumber() => waveNumber;
    public float GetActiveTime() => activeTime;
    public float GetTimeSinceLastWave() => timeSinceLastWave;
    public void SetLinkedAltar(AltarInteractable altar) => linkedAltar = altar;
    
    #region Sound Management
    private void StartLoopSound()
    {
        if (AudioManager.instance != null && !string.IsNullOrEmpty(loopSoundName))
        {
            activeLoopSound = AudioManager.instance.PlaySound(loopSoundName, loop: true, trackActive: true);
        }
    }
    
    private void StopLoopSound()
    {
        if (activeLoopSound != null && AudioManager.instance != null)
        {
            AudioManager.instance.StopActiveSound(activeLoopSound);
            activeLoopSound = null;
        }
    }
    #endregion
    
    #region Void Effect Management
    private void ActivateTotemVoidEffect()
    {
        if (isVoidEffectActive || PPVolumeManager.instance == null) return;
        
        isVoidEffectActive = true;
        PPVolumeManager.instance.SetDayNightUpdates(false);
        
        if (!hasStoredOriginalValues)
        {
            StoreOriginalPostProcessingValues();
        }
        
        if (voidEffectCoroutine != null)
        {
            StopCoroutine(voidEffectCoroutine);
        }
        
        voidEffectCoroutine = StartCoroutine(TotemVoidEffectRoutine(true));
    }
    
    private void DeactivateTotemVoidEffect()
    {
        if (!isVoidEffectActive) return;
        
        isVoidEffectActive = false;
        PPVolumeManager.instance.SetDayNightUpdates(true);
        
        if (voidEffectCoroutine != null)
        {
            StopCoroutine(voidEffectCoroutine);
        }
        
        voidEffectCoroutine = StartCoroutine(TotemVoidEffectRoutine(false));
    }
    
    private IEnumerator TotemVoidEffectRoutine(bool activate)
    {
        var currentVignetteValues = PPVolumeManager.instance.GetVignetteValues();
        Vector4 currentLiftColor = PPVolumeManager.instance.GetLiftGammaGainValues();
        float currentChromaticAberration = PPVolumeManager.instance.GetChromaticAberrationValue();
        
        Vector4 startLiftColor, targetLiftColor;
        float startVignetteIntensity, targetVignetteIntensity;
        Color startVignetteColor, targetVignetteColor;
        float startSaturation, targetSaturation;
        float startChromaticAberration, targetChromaticAberration;
        
        if (activate)
        {
            startLiftColor = currentLiftColor;
            targetLiftColor = new Vector4(
                voidLiftColor.r * 0.1f - 0.05f,
                voidLiftColor.g * 0.05f - 0.02f,
                voidLiftColor.b * 0.15f - 0.05f,
                0f
            );
            
            startVignetteIntensity = currentVignetteValues.intensity;
            targetVignetteIntensity = voidVignetteIntensity;
            
            startVignetteColor = currentVignetteValues.color;
            targetVignetteColor = totemVignetteColor;
            
            startSaturation = 0f;
            targetSaturation = voidSaturation;
            
            startChromaticAberration = currentChromaticAberration;
            targetChromaticAberration = voidChromaticAberration;
        }
        else
        {
            startLiftColor = currentLiftColor;
            targetLiftColor = originalLiftColor;
            
            startVignetteIntensity = currentVignetteValues.intensity;
            targetVignetteIntensity = originalVignetteIntensity;
            
            startVignetteColor = currentVignetteValues.color;
            targetVignetteColor = originalVignetteColor;
            
            startSaturation = voidSaturation;
            targetSaturation = 0f;
            
            startChromaticAberration = currentChromaticAberration;
            targetChromaticAberration = originalChromaticAberration;
        }
        
        float transitionProgress = 0f;
        while (transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime / voidTransitionDuration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, transitionProgress);
            
            Vector4 currentLift = Vector4.Lerp(startLiftColor, targetLiftColor, smoothProgress);
            PPVolumeManager.instance.SetLiftGammaGain(currentLift);
            
            float currentIntensity = Mathf.Lerp(startVignetteIntensity, targetVignetteIntensity, smoothProgress);
            PPVolumeManager.instance.SetVignetteIntensity(currentIntensity);
            
            Color currentVignetteCol = Color.Lerp(startVignetteColor, targetVignetteColor, smoothProgress);
            PPVolumeManager.instance.SetVignetteColor(currentVignetteCol);
            
            float currentSat = Mathf.Lerp(startSaturation, targetSaturation, smoothProgress);
            PPVolumeManager.instance.SetSaturation(currentSat);
            
            float currentChromatic = Mathf.Lerp(startChromaticAberration, targetChromaticAberration, smoothProgress);
            PPVolumeManager.instance.SetChromaticAberration(currentChromatic);
            
            yield return null;
        }
        
        PPVolumeManager.instance.SetLiftGammaGain(targetLiftColor);
        PPVolumeManager.instance.SetVignetteIntensity(targetVignetteIntensity);
        PPVolumeManager.instance.SetVignetteColor(targetVignetteColor);
        PPVolumeManager.instance.SetSaturation(targetSaturation);
        PPVolumeManager.instance.SetChromaticAberration(targetChromaticAberration);
        
        if (activate && isVoidEffectActive && enableVoidAnimation)
        {
            yield return TotemVoidAnimationLoop();
        }
    }
    
    private IEnumerator TotemVoidAnimationLoop()
    {
        Vector4 baseLiftColor = new Vector4(
            voidLiftColor.r * 0.1f - 0.05f,
            voidLiftColor.g * 0.05f - 0.02f,
            voidLiftColor.b * 0.15f - 0.05f,
            0f
        );
        
        while (isVoidEffectActive)
        {
            float time = Time.time * voidAnimationSpeed;
            float waveIntensity = (Mathf.Sin(time) + 1f) * 0.5f;
            
            Vector4 animatedVoidColor = Vector4.Lerp(
                baseLiftColor * 0.8f,
                baseLiftColor * 1.2f,
                waveIntensity * 0.3f
            );
            
            PPVolumeManager.instance.SetLiftGammaGain(animatedVoidColor);
            
            float pulseFactor = (Mathf.Sin(time * 1.5f) + 1f) * 0.5f;
            float animatedIntensity = Mathf.Lerp(voidVignetteIntensity * 0.8f, voidVignetteIntensity * 1.1f, pulseFactor * 0.3f);
            PPVolumeManager.instance.SetVignetteIntensity(animatedIntensity);
            
            if (enableChromaticPulse)
            {
                float chromaticTime = Time.time * chromaticPulseSpeed;
                float chromaticPulseFactor = (Mathf.Sin(chromaticTime) + 1f) * 0.5f;
                float animatedChromatic = Mathf.Lerp(
                    voidChromaticAberration * 0.7f,
                    voidChromaticAberration * 1.3f,
                    chromaticPulseFactor
                );
                PPVolumeManager.instance.SetChromaticAberration(animatedChromatic);
            }
            
            yield return null;
        }
    }
    
    private void StoreOriginalPostProcessingValues()
    {
        if (PPVolumeManager.instance != null)
        {
            originalLiftColor = PPVolumeManager.instance.GetLiftGammaGainValues();
            var vignetteValues = PPVolumeManager.instance.GetVignetteValues();
            originalVignetteIntensity = vignetteValues.intensity;
            originalVignetteColor = vignetteValues.color;
            originalSaturation = 0f;
            originalChromaticAberration = PPVolumeManager.instance.GetChromaticAberrationValue();
            hasStoredOriginalValues = true;
        }
    }
    #endregion
    
    void OnDestroy()
    {
        if (GameEvents.Instance != null)
        {
            GameEvents.Instance.OnPlayerDied -= HandlePlayerDeath;
        }
        
        StopLoopSound();
        
        if (challengeActive)
        {
            CancelChallenge();
        }
        
        if (enableVoidEffect && PPVolumeManager.instance != null)
        {
            DeactivateTotemVoidEffect();
        }
    }
    
    protected override string GetCustomSaveData()
    {
        // No save support for now
        return string.Empty;
    }
    
    protected override void ApplyCustomSaveData(string data)
    {
        // No save support for now
    }
}
