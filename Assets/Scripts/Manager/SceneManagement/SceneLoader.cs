using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public enum Scene
    {
        Intro,
        ChaosZone,
        ChaosZone_Forest,
        ChaosZone_Cave,
        ChaosZone_Ruins
    }
    
    public static void Load(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString());
    }

    public static void LoadChaosZone(string chaosZoneSceneName = "ChaosZone")
    {
        // Load specific chaos zone scene or fallback to default
        try
        {
            SceneManager.LoadScene(chaosZoneSceneName);
        }
        catch (System.Exception)
        {
            // Fallback to default chaos zone
            SceneManager.LoadScene(Scene.ChaosZone.ToString());
        }
    }

    public static void LoadRandomChaosZone()
    {
        // Load a random chaos zone variant
        Scene[] chaosZones = {
            Scene.ChaosZone,
            Scene.ChaosZone_Forest,
            Scene.ChaosZone_Cave,
            Scene.ChaosZone_Ruins
        };
        
        Scene randomZone = chaosZones[Random.Range(0, chaosZones.Length)];
        Load(randomZone);
    }

    public static bool IsChaosZoneScene(string sceneName)
    {
        return sceneName.Contains("ChaosZone") || sceneName.Contains("Chaos");
    }
}
