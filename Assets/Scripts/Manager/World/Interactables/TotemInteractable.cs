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
    
    private PrefabCollection prefabCollection;
    private GameObject mobParent;
    private List<GameObject> spawnedMobs = new List<GameObject>();
    
    // Challenge-Status
    private bool challengeActive = false;
    private bool challengeCompleted = false;
    private bool soundPlayed = false;
    
    // Drop-System
    private float dropTimeStamp = 0f;
    private float currentDropInterval;
    
    protected override void Start()
    {
        // Standard-Einstellungen für Totem
        canBeUsedMultipleTimes = false; // Totem kann nur einmal aktiviert werden
        displayName = "Ancient Totem";
        interactPrompt = "Press [E] to activate Totem";
        
        // Standard VFX/Audio Namen
        if (string.IsNullOrEmpty(useSoundName))
            useSoundName = "TotemCall";
        
        // Initialisierung
        mobParent = GameObject.Find("MobParent");
        prefabCollection = FindObjectOfType<PrefabCollection>();
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
        SpawnEnemies();
        
        // Licht ausschalten
        if (lightBulb != null)
        {
            lightBulb.intensity = 0;
        }
        
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
            SummonedMob[] summonedMobs = FindObjectsOfType<SummonedMob>();
            
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
        
        // Erfolgs-Sound abspielen
        if (AudioManager.instance != null && !soundPlayed)
        {
            AudioManager.instance.PlaySound("TotemClear");
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
            LogScript.instance.ShowLog($"Totem Challenge completed! +{xpReward} XP!", 4f);
        }
        
        // Starte Drop-Belohnungen
        StartCoroutine(DropRewards());
        
        // Speichere Zustand
        SaveState();
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
    
    protected override string GetCustomSaveData()
    {
        return JsonUtility.ToJson(new TotemSaveData
        {
            challengeActive = this.challengeActive,
            challengeCompleted = this.challengeCompleted,
            soundPlayed = this.soundPlayed,
            lightIntensity = lightBulb != null ? lightBulb.intensity : 0f,
            lightColorR = lightBulb != null ? lightBulb.color.r : 1f,
            lightColorG = lightBulb != null ? lightBulb.color.g : 1f,
            lightColorB = lightBulb != null ? lightBulb.color.b : 1f
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
        public float lightIntensity;
        public float lightColorR, lightColorG, lightColorB;
    }
    
    void Update()
    {
        // Update-Logik wird jetzt komplett über Coroutines gehandhabt
        // Nur für Debug-Zwecke
        if (challengeActive)
        {
            // Optional: Debug-Anzeige der verbleibenden Mobs
            SummonedMob[] remainingMobs = FindObjectsOfType<SummonedMob>();
            if (Time.frameCount % 60 == 0) // Nur jede Sekunde loggen
            {
                Debug.Log($"Totem Challenge: {remainingMobs.Length} mobs remaining");
            }
        }
    }
}