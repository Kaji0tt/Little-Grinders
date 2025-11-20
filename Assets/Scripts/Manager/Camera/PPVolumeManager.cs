using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using System;

[ExecuteInEditMode]
public class PPVolumeManager : MonoBehaviour
{
    #region Singleton
    public static PPVolumeManager instance;
    private void Awake()
    {
        // Singleton-Pattern mit DontDestroyOnLoad
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            SetUrpBasedOnScene();
            
            // Event-Listener für Szenen-Wechsel registrieren
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            Debug.Log("[PPVolumeManager] Singleton instance created and set to persist across scenes.");
        }
        else if (instance != this)
        {
            // Falls bereits eine Instanz existiert, zerstöre diese neue
            Debug.Log("[PPVolumeManager] Duplicate instance found, destroying this one to maintain singleton pattern.");
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // Event-Listener entfernen um Memory Leaks zu vermeiden
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Singleton-Referenz zurücksetzen, falls diese Instanz zerstört wird
        if (instance == this)
        {
            instance = null;
            Debug.Log("[PPVolumeManager] Singleton instance destroyed and reference cleared.");
        }
    }
    
    /// <summary>
    /// Wird aufgerufen wenn eine neue Szene geladen wird
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetUrpBasedOnScene();
    }
    
    /// <summary>
    /// Zerstört das Singleton manuell (nützlich für Testing oder explizites Reset)
    /// </summary>
    public static void DestroySingleton()
    {
        if (instance != null)
        {
            Debug.Log("[PPVolumeManager] Manually destroying singleton instance.");
            Destroy(instance.gameObject);
            instance = null;
        }
    }
    
    /// <summary>
    /// Überprüft ob eine gültige Singleton-Instanz existiert
    /// </summary>
    public static bool HasValidInstance()
    {
        return instance != null;
    }
    #endregion

    #region Inspector

    [Header("Post Processing")]
    public Volume volume;

    [Header("Lichtquelle")]
    [Tooltip("Directional Light für Tageszeit und Atmosphäre")]
    public Light directionalLight;

    [Space]
    [Header("URP Settings")]
    [Tooltip("URP Asset für das Hauptmenü (ohne Schatten)")]
    public UniversalRenderPipelineAsset menuUrpAsset;
    [Tooltip("URP Asset für Gameplay-Szenen (mit Schatten)")]
    public UniversalRenderPipelineAsset gameplayUrpAsset;

    [Space]
    [Header("Test Controls")]
    [Tooltip("Dauer des Test-Zyklus in Sekunden")]
    public float testDuration = 5f;
    
    [Space]
    [Header("Lift Gamma Gain Atmosphere Colors")]
    [Tooltip("Farbe für Morgenstimmung (Lift Gamma Gain)")]
    public Color liftMorningColor;
    [Tooltip("Farbe für Mittagsstimmung (Lift Gamma Gain)")]
    public Color liftNoonColor;
    [Tooltip("Farbe für Abendstimmung (Lift Gamma Gain)")]
    public Color liftEveningColor;
    [Tooltip("Farbe für Nachtstimmung (Lift Gamma Gain)")]
    public Color liftNightColor;

    [Space]
    [Header("Directional Light Colors")]
    [Tooltip("Farbe für Morgenstimmung (Directional Light)")]
    public Color morningColor;
    [Tooltip("Farbe für Mittagsstimmung (Directional Light)")]
    public Color noonColor;
    [Tooltip("Farbe für Abendstimmung (Directional Light)")]
    public Color eveningColor;
    [Tooltip("Farbe für Nachtstimmung (Directional Light)")]
    public Color nightColor;

    [Space]
    [Header("Void Effect Settings - DEPRECATED")]
    [Tooltip("Diese Einstellungen sind deprecated - Void-Effekte werden jetzt in spezifischen Komponenten verwaltet")]
    public bool voidEffectEnabled = true;
    public Color voidColor = new Color(0.5f, 0.2f, 0.8f, 1f);
    [Range(0f, 1f)]
    public float voidIntensity = 0.7f;
    public float voidAnimationSpeed = 2f;
    public float voidTransitionDuration = 2f;

    #endregion

    // Private Variablen - nur für allgemeine Funktionen
    private bool statusActive = false; // Flag to prevent day/night updates
    
    void Start()
    {
        //TestDayNightCycle(5f); // Testen Sie den Tag-Nacht-Zyklus für 5 Sekunden
    }

    void Update()
    {
        UpdateDayNightFromSystemTime();
    }

    // Diese Methode wird vom Inspector-Button aufgerufen
    [ContextMenu("Test Day Night Cycle")]
    public void TestDayNightCycleButton()
    {
        TestDayNightCycle(testDuration);
    }

    public void UpdateDayNightFromSystemTime()
    {
        // Skip day/night updates if void effect is active
        if (statusActive) return;
        
        DateTime now = DateTime.Now;
        // Berechne Minuten seit Mitternacht
        float minutes = now.Hour * 60f + now.Minute;
        float time01 = minutes / 1440f; // 1440 Minuten = 24 Stunden

        SetDayNightAmbience(time01);
    }

    public void SetDayNightAmbience(float time01)
    {
        // Skip day/night updates if void effect is active
        if (statusActive) return;
        
        // time01: 0 = Mitternacht, 0.25 = Morgen, 0.5 = Mittag, 0.75 = Abend, 1 = Mitternacht

        // LiftGammaGain-Farbe bestimmen
        Color liftColor = GetLiftColor(time01);

        LiftGammaGain liftGammaGain;
        if (volume.profile.TryGet<LiftGammaGain>(out liftGammaGain))
        {
            liftGammaGain.lift.overrideState = true; // KORREKTUR: Override aktivieren
            liftGammaGain.lift.value = liftColor;
            //Debug.Log($"[PPVolumeManager] LiftGammaGain set to {liftColor} at time01={time01:F2}");
        }

        // Directional Light-Farbe bestimmen
        Color lightColor = GetLightColor(time01);

        if (directionalLight != null)
        {
            directionalLight.color = lightColor;

            // Einfache Sinus-Kurve für realistische Helligkeitsverteilung
            float intensity = Mathf.Lerp(0.7f, 1f, Mathf.Sin(time01 * Mathf.PI));
            directionalLight.intensity = intensity;

            // Y-Rotation von +80 (Morgen) bis -80 (Abend) interpolieren
            float yRotation = Mathf.Lerp(80f, -80f, time01);
            Vector3 currentEuler = directionalLight.transform.eulerAngles;
            directionalLight.transform.eulerAngles = new Vector3(currentEuler.x, yRotation, currentEuler.z);
        }
    }

    // Hilfsmethode für LiftGammaGain-Farbe
    private Color GetLiftColor(float time01)
    {
        if (time01 < 0.25f) return Color.Lerp(liftNightColor, liftMorningColor, time01 * 4f);
        else if (time01 < 0.5f) return Color.Lerp(liftMorningColor, liftNoonColor, (time01 - 0.25f) * 4f);
        else if (time01 < 0.75f) return Color.Lerp(liftNoonColor, liftEveningColor, (time01 - 0.5f) * 4f);
        else return Color.Lerp(liftEveningColor, liftNightColor, (time01 - 0.75f) * 4f);
    }

    // Hilfsmethode für Directional Light-Farbe
    private Color GetLightColor(float time01)
    {
        if (time01 < 0.15f) return Color.Lerp(nightColor, morningColor, time01 * 4f);
        else if (time01 < 0.5f) return Color.Lerp(morningColor, noonColor, (time01 - 0.25f) * 4f);
        else if (time01 < 0.85f) return Color.Lerp(noonColor, eveningColor, (time01 - 0.5f) * 4f);
        else return Color.Lerp(eveningColor, nightColor, (time01 - 0.75f) * 4f);
    }

    public void LowHealthPP(float value)
    {
        float vigMaxVol = 0.5f;
        float vigStandardVol = 0.32f;

        Vignette vignette;

        if (volume.profile.TryGet<Vignette>(out vignette))
        {
            vignette.intensity.overrideState = true; // KORREKTUR: Override aktivieren
            vignette.intensity.value = Mathf.Lerp(vigMaxVol, vigStandardVol, value);
        }
    }

    public void TestDayNightCycle(float duration = 5f)
    {
        Debug.Log($"[PPVolumeManager] Starting Day-Night Cycle Test for {duration} seconds");
        StartCoroutine(DayNightTestRoutine(duration));
    }

    private IEnumerator DayNightTestRoutine(float duration)
    {
        float[] times = { 0f, 0.25f, 0.5f, 0.75f, 1f }; // Mitternacht, Morgen, Mittag, Abend, Mitternacht
        string[] timeNames = { "Mitternacht", "Morgen", "Mittag", "Abend", "Mitternacht" };
        
        for (int i = 0; i < times.Length - 1; i++)
        {
            Debug.Log($"[PPVolumeManager] Übergang von {timeNames[i]} zu {timeNames[i + 1]}");
            float t = 0f;
            while (t < 1f)
            {
                float lerpTime = Mathf.Lerp(times[i], times[i + 1], t);
                SetDayNightAmbience(lerpTime);
                t += Time.deltaTime / (duration / 4f); // Teile durch 4 für 4 Übergänge
                yield return null;
            }
        }
        // Am Ende wieder Mitternacht setzen
        Debug.Log("[PPVolumeManager] Day-Night Cycle Test completed, resetting to midnight.");
        SetDayNightAmbience(0f);
    }

    #region Generic Post-Processing Methods
    
    /// <summary>
    /// Setzt die Vignetten-Farbe des aktiven Profils
    /// </summary>
    /// <param name="newColor">Die neue Farbe für die Vignette</param>
    public void SetVignetteColor(Color newColor)
    {
        if (volume != null && volume.profile != null)
        {
            if (volume.profile.TryGet<Vignette>(out var vignette))
            {
                vignette.color.overrideState = true;
                vignette.color.value = newColor;
                Debug.Log($"[PPVolumeManager] Vignetten-Farbe geändert zu: {newColor}");
                
                // Erzwinge Volume-Update
                RefreshVolume();
            }
            else
            {
                Debug.LogWarning("[PPVolumeManager] Vignette-Effekt nicht im aktiven Profil gefunden!");
            }
        }
        else
        {
            Debug.LogWarning("[PPVolumeManager] Volume oder Profil ist null!");
        }
    }
    
    /// <summary>
    /// Setzt die Vignetten-Intensität des aktiven Profils
    /// </summary>
    /// <param name="intensity">Neue Intensität für die Vignette (0-1)</param>
    public void SetVignetteIntensity(float intensity)
    {
        if (volume != null && volume.profile != null)
        {
            if (volume.profile.TryGet<Vignette>(out var vignette))
            {
                vignette.intensity.overrideState = true;
                vignette.intensity.value = Mathf.Clamp01(intensity);
                RefreshVolume();
            }
        }
    }
    
    /// <summary>
    /// Setzt die Lift Gamma Gain Werte für Farbkorrektur
    /// </summary>
    /// <param name="liftColor">Neue Lift-Farbe als Vector4</param>
    public void SetLiftGammaGain(Vector4 liftColor)
    {
        if (volume != null && volume.profile != null)
        {
            if (volume.profile.TryGet<LiftGammaGain>(out var liftGammaGain))
            {
                liftGammaGain.lift.overrideState = true;
                liftGammaGain.lift.value = liftColor;
                RefreshVolume();
            }
        }
    }
    
    /// <summary>
    /// Setzt die Sättigung für Color Adjustments
    /// </summary>
    /// <param name="saturation">Neue Sättigung (-100 bis 100)</param>
    public void SetSaturation(float saturation)
    {
        if (volume != null && volume.profile != null)
        {
            if (volume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
            {
                colorAdjustments.saturation.overrideState = true;
                colorAdjustments.saturation.value = Mathf.Clamp(saturation, -100f, 100f);
                RefreshVolume();
            }
        }
    }
    
    /// <summary>
    /// Setzt die Chromatic Aberration Intensität
    /// </summary>
    /// <param name="intensity">Neue Intensität für Chromatic Aberration (0-1)</param>
    public void SetChromaticAberration(float intensity)
    {
        if (volume != null && volume.profile != null)
        {
            if (volume.profile.TryGet<ChromaticAberration>(out var chromaticAberration))
            {
                chromaticAberration.intensity.overrideState = true;
                chromaticAberration.intensity.value = Mathf.Clamp01(intensity);
                RefreshVolume();
            }
            else
            {
                Debug.LogWarning("[PPVolumeManager] Chromatic Aberration-Effekt nicht im aktiven Profil gefunden!");
            }
        }
    }
    
    /// <summary>
    /// Aktiviert oder deaktiviert Day/Night-Updates
    /// </summary>
    /// <param name="active">Ob Day/Night-Updates aktiv sein sollen</param>
    public void SetDayNightUpdates(bool active)
    {
        statusActive = !active; // statusActive verhindert Day/Night-Updates
        Debug.Log($"[PPVolumeManager] Day/Night-Updates {(active ? "aktiviert" : "deaktiviert")}");
    }
    
    /// <summary>
    /// Gibt die aktuellen Vignette-Werte zurück
    /// </summary>
    /// <returns>Tuple mit (Farbe, Intensität)</returns>
    public (Color color, float intensity) GetVignetteValues()
    {
        if (volume != null && volume.profile != null)
        {
            if (volume.profile.TryGet<Vignette>(out var vignette))
            {
                return (vignette.color.value, vignette.intensity.value);
            }
        }
        return (Color.black, 0.32f);
    }
    
    /// <summary>
    /// Gibt die aktuellen Lift Gamma Gain Werte zurück
    /// </summary>
    /// <returns>Aktuelle Lift-Farbe als Vector4</returns>
    public Vector4 GetLiftGammaGainValues()
    {
        if (volume != null && volume.profile != null)
        {
            if (volume.profile.TryGet<LiftGammaGain>(out var liftGammaGain))
            {
                return liftGammaGain.lift.value;
            }
        }
        return Vector4.zero;
    }
    
    /// <summary>
    /// Gibt die aktuelle Chromatic Aberration Intensität zurück
    /// </summary>
    /// <returns>Aktuelle Chromatic Aberration Intensität</returns>
    public float GetChromaticAberrationValue()
    {
        if (volume != null && volume.profile != null)
        {
            if (volume.profile.TryGet<ChromaticAberration>(out var chromaticAberration))
            {
                return chromaticAberration.intensity.value;
            }
        }
        return 0f;
    }
    
    /// <summary>
    /// Erzwingt ein Update des Volume-Profiles um sicherzustellen dass Änderungen sofort sichtbar werden
    /// </summary>
    private void RefreshVolume()
    {
        if (volume != null)
        {
            // Methode 1: Volume kurz deaktivieren und wieder aktivieren
            volume.enabled = false;
            volume.enabled = true;
            
            // Methode 2: Profile als "dirty" markieren falls möglich
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(volume.profile);
            #endif
        }
    }
    
    /// <summary>
    /// Setzt das entsprechende URP Asset basierend auf der aktuellen Szene
    /// Build Index 0 = Hauptmenü (ohne Schatten)
    /// Alle anderen Szenen = Gameplay (mit Schatten)
    /// </summary>
    private void SetUrpBasedOnScene()
    {
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        
        if (currentBuildIndex == 0)
        {
            // Hauptmenü - URP ohne Schatten verwenden
            if (menuUrpAsset != null)
            {
                QualitySettings.renderPipeline = menuUrpAsset;
                Debug.Log($"[PPVolumeManager] Switched to Menu URP (no shadows) for scene build index {currentBuildIndex}");
            }
            else
            {
                Debug.LogWarning("[PPVolumeManager] Menu URP Asset ist nicht zugewiesen!");
            }
        }
        else
        {
            // Gameplay-Szenen - URP mit Schatten verwenden
            if (gameplayUrpAsset != null)
            {
                QualitySettings.renderPipeline = gameplayUrpAsset;
                Debug.Log($"[PPVolumeManager] Switched to Gameplay URP (with shadows) for scene build index {currentBuildIndex}");
            }
            else
            {
                Debug.LogWarning("[PPVolumeManager] Gameplay URP Asset ist nicht zugewiesen!");
            }
        }
    }
    
    /// <summary>
    /// Öffentliche Methode zum manuellen Wechseln des URP Assets
    /// </summary>
    /// <param name="useMenuUrp">True für Menu URP (ohne Schatten), False für Gameplay URP (mit Schatten)</param>
    public void SwitchUrpAsset(bool useMenuUrp)
    {
        if (useMenuUrp && menuUrpAsset != null)
        {
            QualitySettings.renderPipeline = menuUrpAsset;
            Debug.Log("[PPVolumeManager] Manually switched to Menu URP (no shadows)");
        }
        else if (!useMenuUrp && gameplayUrpAsset != null)
        {
            QualitySettings.renderPipeline = gameplayUrpAsset;
            Debug.Log("[PPVolumeManager] Manually switched to Gameplay URP (with shadows)");
        }
        else
        {
            Debug.LogWarning("[PPVolumeManager] Could not switch URP Asset - asset is null!");
        }
    }
    
    #endregion
}
