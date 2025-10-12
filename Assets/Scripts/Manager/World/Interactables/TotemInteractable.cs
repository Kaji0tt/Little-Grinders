using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotemInteractable : BaseInteractable
{
    [Header("Totem Settings")]
    [SerializeField] private GameObject[] spawnPoints;
    [SerializeField] private Light lightBulb;
    [SerializeField] private int xpReward = 20;
    [SerializeField] private float dropInterval = 0.5f;
    [SerializeField] private float maxDropTime = 5f;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnChance = 0.5f; // 50% Chance pro Spawnpoint
    
    [Header("Sound Settings")]
    [SerializeField] private string callSoundName = "Totem_Call"; // Sound bei Aktivierung
    [SerializeField] private string loopSoundName = "Totem_Loop"; // Sound während Challenge
    [SerializeField] private string clearSoundName = "Totem_Clear"; // Sound bei Completion
    
    [Header("Visual Effects")]
    [SerializeField] private bool enableVoidEffect = true; // Im Inspector einstellbar
    [SerializeField] private Color totemVignetteColor = new Color(0.254f, 0.027f, 0.302f, 1f); // #41074D
    
    [Header("Void Effect Settings")]
    [SerializeField] private Color voidLiftColor = new Color(0.3f, 0.1f, 0.4f, 1f); // Lila Lift-Farbe
    [SerializeField] private float voidVignetteIntensity = 0.6f; // Vignette-Intensität
    [SerializeField] private float voidSaturation = -30f; // Sättigung
    [SerializeField] private float voidTransitionDuration = 2f; // Übergangszeit
    [SerializeField] private bool enableVoidAnimation = true; // Animation aktivieren
    [SerializeField] private float voidAnimationSpeed = 1.5f; // Animationsgeschwindigkeit
    
    // NEU: Chromatic Aberration Settings
    [SerializeField] private float voidChromaticAberration = 0.3f; // Chromatic Aberration Intensität
    [SerializeField] private bool enableChromaticPulse = true; // Chromatic Aberration Pulsieren
    [SerializeField] private float chromaticPulseSpeed = 2f; // Pulsgeschwindigkeit
    
    private PrefabCollection prefabCollection;
    private GameObject mobParent;
    private List<GameObject> spawnedMobs = new List<GameObject>();
    
    // Challenge-Status
    private bool challengeActive = false;
    private bool challengeCompleted = false;
    private bool soundPlayed = false;
    
    // Sound-System
    private bool isLoopSoundPlaying = false;
    private AudioManager.ActiveSound activeLoopSound;
    
    // Void-Effekt System
    private bool isVoidEffectActive = false;
    private Coroutine voidEffectCoroutine;
    private Vector4 originalLiftColor;
    private float originalVignetteIntensity;
    private Color originalVignetteColor;
    private float originalSaturation;
    private float originalChromaticAberration; // NEU
    private bool hasStoredOriginalValues = false;
    
    // Drop-System
    private float dropTimeStamp = 0f;
    private float currentDropInterval;
    
    protected override void Start()
    {
        // Standard-Einstellungen für Totem
        canBeUsedMultipleTimes = false; // Totem kann nur einmal aktiviert werden
        displayName = "Ancient Totem";
        interactPrompt = "Press [E] to activate Totem";
        
        // Initialisierung
        mobParent = GameObject.Find("MobParent");
        prefabCollection = FindFirstObjectByType<PrefabCollection>();
        currentDropInterval = dropInterval;
        
        base.Start();
    }
    
    protected override bool CanInteract()
    {
        return base.CanInteract() && !challengeActive && !challengeCompleted;
    }
    
    protected override void OnInteract()
    {
        challengeActive = true;
        
        // Spiele Call-Sound ab
        if (AudioManager.instance != null && !string.IsNullOrEmpty(callSoundName))
        {
            AudioManager.instance.PlaySound(callSoundName);
        }
        
        SpawnEnemies();
        
        // Licht ausschalten
        if (lightBulb != null)
        {
            lightBulb.intensity = 0;
        }
        
        // NEU: Aktiviere Void-Effekt mit spezifischer Totem-Farbe
        if (enableVoidEffect && PPVolumeManager.instance != null)
        {
            ActivateTotemVoidEffect();
            Debug.Log("Void-Effekt aktiviert für Totem-Challenge mit Farbe #41074D");
        }
        
        // Starte Loop-Sound
        StartLoopSound();
        
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog("Totem Challenge activated!", 3f);
        }
        
        StartCoroutine(MonitorChallenge());
    }
    
    private void SpawnEnemies()
    {
        if (prefabCollection == null)
        {
            Debug.LogWarning("PrefabCollection not found!");
            return;
        }
        
        foreach (GameObject spawnPoint in spawnPoints)
        {
            // Zufällige Spawn-Chance
            if (Random.value <= spawnChance)
            {
                GameObject mob = prefabCollection.GetRandomEnemie();
                
                if (mob != null)
                {
                    GameObject instanceMob = Instantiate(mob, spawnPoint.transform.position, Quaternion.identity);
                    
                    // Parent setzen
                    if (mobParent != null)
                    {
                        instanceMob.transform.SetParent(mobParent.transform);
                    }
                    
                    // SummonedMob-Component hinzufügen
                    instanceMob.AddComponent<SummonedMob>();
                    spawnedMobs.Add(instanceMob);
                }
            }
        }
        
        Debug.Log($"Totem spawned {spawnedMobs.Count} enemies");
    }
    
    private IEnumerator MonitorChallenge()
    {
        while (challengeActive && !challengeCompleted)
        {
            // Entferne null-Referenzen (zerstörte Mobs)
            spawnedMobs.RemoveAll(mob => mob == null);
            
            // Alternative Überprüfung über SummonedMob-Components
            SummonedMob[] summonedMobs = FindObjectsByType<SummonedMob>(FindObjectsSortMode.None);
            
            if (summonedMobs.Length == 0 && spawnedMobs.Count == 0)
            {
                CompleteChallenge();
                break;
            }
            
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    private void CompleteChallenge()
    {
        challengeCompleted = true;
        challengeActive = false;
        
        // WICHTIG: Stoppe Loop-Sound SOFORT und warte kurz
        StopLoopSound();
        
        // Warte einen Frame um sicherzustellen dass Loop-Sound gestoppt ist
        StartCoroutine(PlayClearSoundAfterDelay());
        
        // NEU: Deaktiviere Void-Effekt
        if (enableVoidEffect && PPVolumeManager.instance != null)
        {
            DeactivateTotemVoidEffect();
            Debug.Log("Void-Effekt deaktiviert nach Totem-Completion");
        }
        
        // NEU: Markiere die aktuelle Map als cleared
        if (GlobalMap.instance != null && GlobalMap.instance.currentMap != null)
        {
            GlobalMap.instance.currentMap.isCleared = true;
            Debug.Log("Map als 'cleared' markiert!");
            
            // Benachrichtige UI über Änderung
            GlobalMap.instance.TriggerMapListChanged();
        }
    }
    
    private IEnumerator DropRewards()
    {
        dropTimeStamp = 0f;
        
        while (dropTimeStamp < maxDropTime)
        {
            yield return new WaitForSeconds(currentDropInterval);
            
            // Drop ein Item
            if (ItemDatabase.instance != null)
            {
                ItemDatabase.instance.GetWeightDrop(transform.position);
            }
            
            dropTimeStamp += currentDropInterval;
            currentDropInterval += dropInterval; // Intervall wird länger
        }
    }
    
    /// <summary>
    /// Spielt den Clear-Sound nach einer kurzen Verzögerung ab, um sicherzustellen dass der Loop-Sound gestoppt ist
    /// </summary>
    private IEnumerator PlayClearSoundAfterDelay()
    {
        // Warte einen kurzen Moment um sicherzustellen dass Loop-Sound gestoppt ist
        yield return new WaitForSeconds(0.1f);
        
        // Erfolgs-Sound abspielen
        if (AudioManager.instance != null && !soundPlayed && !string.IsNullOrEmpty(clearSoundName))
        {
            AudioManager.instance.PlaySound(clearSoundName);
            soundPlayed = true;
        }
        
        // XP-Belohnung geben
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.Gain_xp(xpReward);
        }
        
        // Licht wieder anschalten
        if (lightBulb != null)
        {
            lightBulb.color = Color.white;
            lightBulb.intensity = 0.7f;
        }
        
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog($"Totem Challenge completed! Map cleared! +{xpReward} XP!", 4f);
        }
        
        // Starte Drop-Belohnungen
        StartCoroutine(DropRewards());
        
        // Speichere Zustand
        SaveState();
    }
    
    #region Sound Management

    /// <summary>
    /// Startet den Loop-Sound der während der Challenge abgespielt wird
    /// </summary>
    private void StartLoopSound()
    {
        if (AudioManager.instance != null && !string.IsNullOrEmpty(loopSoundName) && !isLoopSoundPlaying)
        {
            // Verwende das neue System für Loop-Sounds
            activeLoopSound = AudioManager.instance.PlaySound(loopSoundName, loop: true, trackActive: true);
            
            if (activeLoopSound != null)
            {
                isLoopSoundPlaying = true;
                Debug.Log($"Loop-Sound '{loopSoundName}' gestartet");
            }
        }
    }
    
    /// <summary>
    /// Stoppt den Loop-Sound sofort
    /// </summary>
    private void StopLoopSound()
    {
        if (activeLoopSound != null)
        {
            AudioManager.instance.StopActiveSound(activeLoopSound);
            activeLoopSound = null;
            Debug.Log("Loop-Sound gestoppt");
        }
        
        // Fallback: Stoppe alle Sounds dieser Gruppe
        if (AudioManager.instance != null && !string.IsNullOrEmpty(loopSoundName))
        {
            AudioManager.instance.StopSound(loopSoundName);
        }
        
        isLoopSoundPlaying = false;
    }
    #endregion
    
    #region Void Effect Management
    
    /// <summary>
    /// Aktiviert den Totem-spezifischen Void-Effekt
    /// </summary>
    private void ActivateTotemVoidEffect()
    {
        if (isVoidEffectActive || PPVolumeManager.instance == null) return;
        
        Debug.Log("[TotemInteractable] Aktiviere Totem Void-Effekt");
        isVoidEffectActive = true;
        
        // Deaktiviere Day/Night-Updates
        PPVolumeManager.instance.SetDayNightUpdates(false);
        
        // Speichere originale Werte
        if (!hasStoredOriginalValues)
        {
            StoreOriginalPostProcessingValues();
        }
        
        // Stoppe vorherige Coroutine falls aktiv
        if (voidEffectCoroutine != null)
        {
            StopCoroutine(voidEffectCoroutine);
        }
        
        voidEffectCoroutine = StartCoroutine(TotemVoidEffectRoutine(true));
    }
    
    /// <summary>
    /// Deaktiviert den Totem-spezifischen Void-Effekt
    /// </summary>
    private void DeactivateTotemVoidEffect()
    {
        if (!isVoidEffectActive) return;
        
        Debug.Log("[TotemInteractable] Deaktiviere Totem Void-Effekt");
        isVoidEffectActive = false;
        
        // Reaktiviere Day/Night-Updates
        PPVolumeManager.instance.SetDayNightUpdates(true);
        
        // Stoppe vorherige Coroutine falls aktiv
        if (voidEffectCoroutine != null)
        {
            StopCoroutine(voidEffectCoroutine);
        }
        
        voidEffectCoroutine = StartCoroutine(TotemVoidEffectRoutine(false));
    }
    
    /// <summary>
    /// Hauptroutine für den Totem Void-Effekt mit sanften Übergängen und Animation
    /// </summary>
    private IEnumerator TotemVoidEffectRoutine(bool activate)
    {
        // Hole aktuelle Werte vom PPVolumeManager
        var currentVignetteValues = PPVolumeManager.instance.GetVignetteValues();
        Vector4 currentLiftColor = PPVolumeManager.instance.GetLiftGammaGainValues();
        float currentChromaticAberration = PPVolumeManager.instance.GetChromaticAberrationValue(); // NEU
        
        // Start- und Zielwerte definieren
        Vector4 startLiftColor, targetLiftColor;
        float startVignetteIntensity, targetVignetteIntensity;
        Color startVignetteColor, targetVignetteColor;
        float startSaturation, targetSaturation = 0f;
        float startChromaticAberration, targetChromaticAberration; // NEU
        
        if (activate)
        {
            // Aktivierung: Von aktuellen Werten zu Totem-Void-Werten
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
            
            startSaturation = 0f; // Annahme: Startwert
            targetSaturation = voidSaturation;
            
            // NEU: Chromatic Aberration
            startChromaticAberration = currentChromaticAberration;
            targetChromaticAberration = voidChromaticAberration;
        }
        else
        {
            // Deaktivierung: Von Void-Werten zurück zu originalen Werten
            startLiftColor = currentLiftColor;
            targetLiftColor = originalLiftColor;
            
            startVignetteIntensity = currentVignetteValues.intensity;
            targetVignetteIntensity = originalVignetteIntensity;
            
            startVignetteColor = currentVignetteValues.color;
            targetVignetteColor = originalVignetteColor;
            
            startSaturation = voidSaturation;
            targetSaturation = 0f;
            
            // NEU: Chromatic Aberration zurücksetzen
            startChromaticAberration = currentChromaticAberration;
            targetChromaticAberration = originalChromaticAberration;
        }
        
        // Sanfter Übergang
        float transitionProgress = 0f;
        while (transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime / voidTransitionDuration;
            float smoothProgress = Mathf.SmoothStep(0f, 1f, transitionProgress);
            
            // Lift Gamma Gain (Hauptfarbe)
            Vector4 currentLift = Vector4.Lerp(startLiftColor, targetLiftColor, smoothProgress);
            PPVolumeManager.instance.SetLiftGammaGain(currentLift);
            
            // Vignette Intensität
            float currentIntensity = Mathf.Lerp(startVignetteIntensity, targetVignetteIntensity, smoothProgress);
            PPVolumeManager.instance.SetVignetteIntensity(currentIntensity);
            
            // Vignette Farbe
            Color currentVignetteCol = Color.Lerp(startVignetteColor, targetVignetteColor, smoothProgress);
            PPVolumeManager.instance.SetVignetteColor(currentVignetteCol);
            
            // Sättigung
            float currentSat = Mathf.Lerp(startSaturation, targetSaturation, smoothProgress);
            PPVolumeManager.instance.SetSaturation(currentSat);
            
            // NEU: Chromatic Aberration
            float currentChromatic = Mathf.Lerp(startChromaticAberration, targetChromaticAberration, smoothProgress);
            PPVolumeManager.instance.SetChromaticAberration(currentChromatic);
            
            yield return null;
        }
        
        // Finale Werte setzen
        PPVolumeManager.instance.SetLiftGammaGain(targetLiftColor);
        PPVolumeManager.instance.SetVignetteIntensity(targetVignetteIntensity);
        PPVolumeManager.instance.SetVignetteColor(targetVignetteColor);
        PPVolumeManager.instance.SetSaturation(targetSaturation);
        PPVolumeManager.instance.SetChromaticAberration(targetChromaticAberration); // NEU
        
        // Wenn aktiviert und Animation enabled, starte kontinuierliche Animation
        if (activate && isVoidEffectActive && enableVoidAnimation)
        {
            yield return TotemVoidAnimationLoop();
        }
    }
    
    /// <summary>
    /// Kontinuierliche Animation während der Totem Void-Effekt aktiv ist
    /// </summary>
    private IEnumerator TotemVoidAnimationLoop()
    {
        // Basis-Werte für Animation
        Vector4 baseLiftColor = new Vector4(
            voidLiftColor.r * 0.1f - 0.05f,
            voidLiftColor.g * 0.05f - 0.02f,
            voidLiftColor.b * 0.15f - 0.05f,
            0f
        );
        
        while (isVoidEffectActive)
        {
            float time = Time.time * voidAnimationSpeed;
            
            // Wabernde Farbeffekte - subtil
            float waveIntensity = (Mathf.Sin(time) + 1f) * 0.5f; // 0 bis 1
            
            // Sehr subtile Variation um die Basis-Farbe
            Vector4 animatedVoidColor = Vector4.Lerp(
                baseLiftColor * 0.8f, // Dunklere Version
                baseLiftColor * 1.2f, // Hellere Version
                waveIntensity * 0.3f // Reduzierte Intensität
            );
            
            PPVolumeManager.instance.SetLiftGammaGain(animatedVoidColor);
            
            // Pulsierende Vignette - subtil
            float pulseFactor = (Mathf.Sin(time * 1.5f) + 1f) * 0.5f; // 0 bis 1
            float animatedIntensity = Mathf.Lerp(voidVignetteIntensity * 0.8f, voidVignetteIntensity * 1.1f, pulseFactor * 0.3f);
            PPVolumeManager.instance.SetVignetteIntensity(animatedIntensity);
            
            // NEU: Pulsierende Chromatic Aberration
            if (enableChromaticPulse)
            {
                float chromaticTime = Time.time * chromaticPulseSpeed;
                float chromaticPulseFactor = (Mathf.Sin(chromaticTime) + 1f) * 0.5f; // 0 bis 1
                
                // Pulsiere zwischen 70% und 130% der Basis-Intensität
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
    
    /// <summary>
    /// Speichert die aktuellen Post-Processing-Werte bevor der Void-Effekt angewendet wird
    /// </summary>
    private void StoreOriginalPostProcessingValues()
    {
        if (PPVolumeManager.instance != null)
        {
            originalLiftColor = PPVolumeManager.instance.GetLiftGammaGainValues();
            var vignetteValues = PPVolumeManager.instance.GetVignetteValues();
            originalVignetteIntensity = vignetteValues.intensity;
            originalVignetteColor = vignetteValues.color;
            originalSaturation = 0f; // Annahme für Standard-Sättigung
            originalChromaticAberration = PPVolumeManager.instance.GetChromaticAberrationValue(); // NEU
            
            hasStoredOriginalValues = true;
            Debug.Log($"[TotemInteractable] Originale PP-Werte gespeichert: Lift={originalLiftColor}, Vignette={originalVignetteIntensity}, VignetteColor={originalVignetteColor}, ChromaticAberration={originalChromaticAberration}");
        }
    }
    
    #endregion
    
    protected override string GetCustomSaveData()
    {
        return JsonUtility.ToJson(new TotemSaveData
        {
            challengeActive = this.challengeActive,
            challengeCompleted = this.challengeCompleted,
            soundPlayed = this.soundPlayed,
            isLoopSoundPlaying = this.isLoopSoundPlaying,
            lightIntensity = lightBulb != null ? lightBulb.intensity : 0f,
            lightColorR = lightBulb != null ? lightBulb.color.r : 1f,
            lightColorG = lightBulb != null ? lightBulb.color.g : 1f,
            lightColorB = lightBulb != null ? lightBulb.color.b : 1f,
            enableVoidEffect = this.enableVoidEffect,
            isVoidEffectActive = this.isVoidEffectActive, // NEU: Speichere Void-Effekt-Zustand
            // NEU: Chromatic Aberration Settings
            enableChromaticPulse = this.enableChromaticPulse,
            voidChromaticAberration = this.voidChromaticAberration,
            chromaticPulseSpeed = this.chromaticPulseSpeed
        });
    }
    
    protected override void ApplyCustomSaveData(string data)
    {
        if (!string.IsNullOrEmpty(data))
        {
            try
            {
                var saveData = JsonUtility.FromJson<TotemSaveData>(data);
                
                this.challengeActive = saveData.challengeActive;
                this.challengeCompleted = saveData.challengeCompleted;
                this.soundPlayed = saveData.soundPlayed;
                this.isLoopSoundPlaying = saveData.isLoopSoundPlaying;
                this.enableVoidEffect = saveData.enableVoidEffect;
                this.isVoidEffectActive = saveData.isVoidEffectActive; // NEU: Lade Void-Effekt-Zustand
                
                // NEU: Lade Chromatic Aberration Settings
                this.enableChromaticPulse = saveData.enableChromaticPulse;
                this.voidChromaticAberration = saveData.voidChromaticAberration;
                this.chromaticPulseSpeed = saveData.chromaticPulseSpeed;
                
                // Loop-Sound wiederherstellen falls aktiv
                if (this.challengeActive && !this.challengeCompleted && this.isLoopSoundPlaying)
                {
                    StartLoopSound();
                }
                
                // NEU: Void-Effekt mit Totem-Farbe wiederherstellen
                if (this.challengeActive && !this.challengeCompleted && this.enableVoidEffect)
                {
                    if (PPVolumeManager.instance != null)
                    {
                        ActivateTotemVoidEffect();
                        Debug.Log("Void-Effekt nach Laden wiederhergestellt mit Totem-Farbe");
                    }
                }
                
                // Licht-Zustand wiederherstellen
                if (lightBulb != null)
                {
                    lightBulb.intensity = saveData.lightIntensity;
                    lightBulb.color = new Color(saveData.lightColorR, saveData.lightColorG, saveData.lightColorB);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to load Totem save data: {e.Message}");
            }
        }
    }
    
    [System.Serializable]
    private class TotemSaveData
    {
        public bool challengeActive;
        public bool challengeCompleted;
        public bool soundPlayed;
        public bool isLoopSoundPlaying;
        public float lightIntensity;
        public float lightColorR, lightColorG, lightColorB;
        public bool enableVoidEffect; // NEU
        public bool isVoidEffectActive; // NEU: Void-Effekt-Zustand
        // NEU: Chromatic Aberration Settings
        public bool enableChromaticPulse;
        public float voidChromaticAberration;
        public float chromaticPulseSpeed;
    }
    
    void OnDestroy()
    {
        // Stoppe alle laufenden Sound-Coroutines
        StopLoopSound();
        
        // NEU: Stelle sicher dass Void-Effekt deaktiviert wird
        if (enableVoidEffect && PPVolumeManager.instance != null && challengeActive)
        {
            DeactivateTotemVoidEffect();
        }
    }
    
    void Update()
    {
        // Update-Logik wird jetzt komplett über Coroutines gehandhabt
        // Nur für Debug-Zwecke
        if (challengeActive)
        {
            // Optional: Debug-Anzeige der verbleibenden Mobs
            SummonedMob[] remainingMobs = FindObjectsByType<SummonedMob>(FindObjectsSortMode.None);
            if (Time.frameCount % 60 == 0) // Nur jede Sekunde loggen
            {
                Debug.Log($"Totem Challenge: {remainingMobs.Length} mobs remaining, Loop Sound: {(isLoopSoundPlaying ? "Playing" : "Stopped")}");
            }
        }
    }
}