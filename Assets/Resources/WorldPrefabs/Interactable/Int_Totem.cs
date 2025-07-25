using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Int_Totem : MonoBehaviour
{
    [Header("Totem Settings")]
    [SerializeField] private bool isActivated = false;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private int enemyCount = 5; // 5-6 Gegner
    
    [Header("VFX")]
    [SerializeField] private string vfxEffectName = "TotemActivation"; // Name des VFX Effekts
    
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool challengeCompleted = false;

    private void OnTriggerStay(Collider other)
    {
        // Prüfe ob es der Player ist und Totem noch nicht aktiviert
        if (other == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>() && !isActivated)
        {
            // Hole die Pick-Taste aus dem KeyManager
            KeyCode pickKey = KeyManager.MyInstance.Keybinds["PICK"];
            
            if (Input.GetKeyDown(pickKey))
            {
                ActivateTotem();
            }
        }
    }

    private void ActivateTotem()
    {
        if (isActivated) return;
        
        isActivated = true;
        
        // VFX abspielen
        PlayVFX();
        
        // Gegner spawnen
        SpawnEnemies();
        
        // Überwachung der gespawnten Gegner starten
        StartCoroutine(MonitorEnemies());
        
        // LogScript statt Debug.Log
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog("Totem activated! Challenge started!", 3f);
        }
    }

    private void PlayVFX()
    {
        // VFX über VFX_Manager abspielen (falls vorhanden)
        if (VFX_Manager.instance != null)
        {
            VFX_Manager.instance.PlayEffect("VFX_Totem", this.transform.position, Quaternion.identity);
        }
        else
        {
            // LogScript für Warnung
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("VFX Manager not available", 2f);
            }
        }
    }

    private void SpawnEnemies()
    {
        // Aktuelles Map-Theme ermitteln
        if (GlobalMap.instance?.currentMap == null)
        {
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("Error: No current map found!", 3f);
            }
            return;
        }
        
        WorldType currentTheme = GlobalMap.instance.currentMap.mapTheme;
        
        // Passendes PrefabTheme finden
        PrefabTheme matchingTheme = FindMatchingTheme(currentTheme);
        if (matchingTheme == null)
        {
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog($"Error: No theme found for {currentTheme}!", 3f);
            }
            return;
        }
        
        // Zufällige Anzahl zwischen 5-6 Gegnern
        int enemiesToSpawn = Random.Range(5, 7);
        enemyCount = enemiesToSpawn;
        
        // Gegner spawnen
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnRandomEnemy(matchingTheme);
        }
        
        // LogScript statt Debug.Log
        if (LogScript.instance != null)
        {
            LogScript.instance.ShowLog($"Spawned {enemiesToSpawn} enemies!", 2f);
        }
    }

    private PrefabTheme FindMatchingTheme(WorldType targetTheme)
    {
        if (PrefabCollection.instance?.themeCollection == null)
        {
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("Error: PrefabCollection not available!", 3f);
            }
            return null;
        }
        
        foreach (PrefabTheme theme in PrefabCollection.instance.themeCollection)
        {
            if (theme.themeType == targetTheme)
            {
                return theme;
            }
        }
        
        return null;
    }

    private void SpawnRandomEnemy(PrefabTheme theme)
    {
        if (theme.enemiesPF == null || theme.enemiesPF.Length == 0)
        {
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("Error: No enemies available in theme!", 3f);
            }
            return;
        }
        
        // Zufälligen Gegner aus dem Theme wählen
        GameObject enemyPrefab = theme.enemiesPF[Random.Range(0, theme.enemiesPF.Length)];
        
        // Zufällige Spawn-Position um das Totem berechnen
        Vector3 spawnPosition = CalculateSpawnPosition();
        
        // Gegner spawnen
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Zur Liste hinzufügen
        spawnedEnemies.Add(spawnedEnemy);
    }

    private Vector3 CalculateSpawnPosition()
    {
        Vector3 totemPosition = transform.position;
        
        // Zufällige Position in einem Kreis um das Totem
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(2f, spawnRadius);
        
        float x = totemPosition.x + distance * Mathf.Cos(angle);
        float z = totemPosition.z + distance * Mathf.Sin(angle);
        
        return new Vector3(x, totemPosition.y, z);
    }

    private IEnumerator MonitorEnemies()
    {
        while (!challengeCompleted)
        {
            // Überprüfe alle gespawnten Gegner
            bool allEnemiesDead = true;
            
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                GameObject enemy = spawnedEnemies[i];
                
                // Null-Check (Gegner wurde zerstört)
                if (enemy == null)
                {
                    spawnedEnemies.RemoveAt(i);
                    continue;
                }
                
                // Prüfe ob Gegner tot ist
                var enemyStats = enemy.GetComponent<MobStats>();
                if (enemyStats != null && enemyStats.isDead)
                {
                    spawnedEnemies.RemoveAt(i);
                }
                else
                {
                    allEnemiesDead = false;
                }
            }
            
            // Wenn alle Gegner tot sind, Challenge abschließen
            if (allEnemiesDead && spawnedEnemies.Count == 0)
            {
                CompleteChallenge();
                break;
            }
            
            yield return new WaitForSeconds(0.5f); // Alle 0.5 Sekunden prüfen
        }
    }

    private void CompleteChallenge()
    {
        challengeCompleted = true;
        
        // Spieler bekommt einen zusätzlichen Skillpunkt
        if (PlayerManager.instance?.playerStats != null)
        {
            int currentSkillPoints = PlayerManager.instance.playerStats.Get_SkillPoints();
            PlayerManager.instance.playerStats.Set_SkillPoints(currentSkillPoints + 1);
            
            // LogScript für Erfolgsmeldung
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("Totem Challenge completed! +1 Skill Point!", 4f);
            }
        }
        else
        {
            // LogScript für Fehler
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("Error: PlayerManager not found!", 3f);
            }
        }
    }

    // Gizmo für Spawn-Radius im Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f); // Mindest-Spawn-Distanz
    }
}
