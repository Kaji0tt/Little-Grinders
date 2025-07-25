using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

[ExecuteInEditMode]
public class PPVolumeManager : MonoBehaviour
{
    #region Singleton
    public static PPVolumeManager instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    #region Inspector

    [Header("Post Processing")]
    public Volume volume;

    [Header("Lichtquelle")]
    [Tooltip("Directional Light für Tageszeit und Atmosphäre")]
    public Light directionalLight;

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

    #endregion

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
        DateTime now = DateTime.Now;
        // Berechne Minuten seit Mitternacht
        float minutes = now.Hour * 60f + now.Minute;
        float time01 = minutes / 1440f; // 1440 Minuten = 24 Stunden

        SetDayNightAmbience(time01);
    }

    public void SetDayNightAmbience(float time01)
    {
        // time01: 0 = Mitternacht, 0.25 = Morgen, 0.5 = Mittag, 0.75 = Abend, 1 = Mitternacht

        // LiftGammaGain-Farbe bestimmen
        Color liftColor = GetLiftColor(time01);

        LiftGammaGain liftGammaGain;
        if (volume.profile.TryGet<LiftGammaGain>(out liftGammaGain))
        {
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
}
