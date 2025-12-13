using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles transitions between maps in the procedural game world.
/// When player enters an exit trigger, saves progress and loads the adjacent map.
/// </summary>
public class MapTransitionTrigger : MonoBehaviour
{
    // ✅ FIX: Prevent multiple trigger calls during transition
    // ⚠️ MUST BE STATIC - there are multiple MapTransitionTrigger instances (one per exit)!
    private static bool isTransitioning = false;
    
    // Reset flag when scene is destroyed (when new map loads)
    private void OnDestroy()
    {
        isTransitioning = false;
        Debug.Log($"[MapBug][{Time.time:F2}s] MapTransitionTrigger destroyed - reset isTransitioning flag");
    }
    
    //Falls der Spieler das entsprechende Spielobjekt kollidiert, soll die neue Szene geladen werden und der Spieler gespeichert.
    private void OnTriggerEnter(Collider collider)
    {
        // ✅ FIX: Ignore if already transitioning (prevents double/triple increments)
        if (isTransitioning)
        {
            Debug.Log($"[MapBug][{Time.time:F2}s] Already transitioning - ignoring trigger from '{gameObject.name}'");
            return;
        }
        
        //Um auf den Spieler zuzugreifen, muss auf diesen mit PlayerManager.instance.play referiert werden.
        if (collider == PlayerManager.instance.player.gameObject.GetComponentInChildren<Collider>())
        {
            Debug.Log($"[MapBug][{Time.time:F2}s] Player entered exit trigger '{gameObject.name}' - FIRST TIME");
            
            // ✅ FIX: Set flag immediately to prevent re-entry
            isTransitioning = true;
            
            //Wenn wir im Tutorial sind (buildIndex 1), erschaffe eine Map und füge sie der GlobalMap Instanz hinzu.
            if (SceneManager.GetActiveScene().buildIndex != 2)
            {
                Debug.Log($"[MapBug][{Time.time:F2}s] Tutorial -> Procedural transition");
                // Speichere den aktuellen Fortschritt
                PlayerSave currentSave = new PlayerSave();
                SaveSystem.SavePlayer(currentSave);

                // WICHTIG: SETZE das Load-Flag - wir wollen den Tutorial-Fortschritt laden!
                PlayerPrefs.SetInt("Load", 1);

                //Lade die nächste Szene
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            //Falls der Spieler den Collider zum MapWechsel berührt, während er bereits in der Prozeduralen Szene ist, ScanExitDirection
            else
            {
                Vector2 oldPos = GlobalMap.instance.currentPosition;
                Debug.Log($"[MapBug][{Time.time:F2}s] === MAP TRANSITION START === Exit: {gameObject.name}");
                Debug.Log($"[MapBug][{Time.time:F2}s] Current Position: ({oldPos.x}, {oldPos.y})");
                
                // HIER: Verwende den aktuellen Save, erstelle KEINEN neuen!
                PlayerSave currentSave = new PlayerSave(); // Das erstellt einen Save aus dem aktuellen Spielzustand
                Debug.Log($"[MapBug][{Time.time:F2}s] Created PlayerSave - exploredMaps.Count: {currentSave.exploredMaps?.Count ?? 0}");
                SaveSystem.SavePlayer(currentSave);

                LoadNextMap(ScanExitDirection(gameObject.name));
            }
        }
    }

    private SpawnPoint ScanExitDirection(string exitDirection)
    {
        Vector2 oldPos = GlobalMap.instance.currentPosition;
        SpawnPoint spawnPoint;
        
        Debug.Log($"[MapBug][{Time.time:F2}s] ScanExitDirection START - Exit: {exitDirection}, Current Position: ({oldPos.x}, {oldPos.y})");
        
        switch (exitDirection)
        {
            case "ExitRight":
                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x + 1, GlobalMap.instance.currentPosition.y);
                spawnPoint = SpawnPoint.SpawnLeft;
                Debug.Log($"[MapBug][{Time.time:F2}s] ExitRight - Moving EAST → NEW POS: ({GlobalMap.instance.currentPosition.x}, {GlobalMap.instance.currentPosition.y})");
                break;

            case "ExitLeft":
                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x - 1, GlobalMap.instance.currentPosition.y);
                spawnPoint = SpawnPoint.SpawnRight;
                Debug.Log($"[MapBug][{Time.time:F2}s] ExitLeft - Moving WEST → NEW POS: ({GlobalMap.instance.currentPosition.x}, {GlobalMap.instance.currentPosition.y})");
                break;

            case "ExitTop":
                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x, GlobalMap.instance.currentPosition.y + 1);
                spawnPoint = SpawnPoint.SpawnBot;
                Debug.Log($"[MapBug][{Time.time:F2}s] ExitTop - Moving NORTH → NEW POS: ({GlobalMap.instance.currentPosition.x}, {GlobalMap.instance.currentPosition.y})");
                break;

            case "ExitBot":
                GlobalMap.instance.currentPosition = new Vector2(GlobalMap.instance.currentPosition.x, GlobalMap.instance.currentPosition.y - 1);
                spawnPoint = SpawnPoint.SpawnTop;
                Debug.Log($"[MapBug][{Time.time:F2}s] ExitBot - Moving SOUTH → NEW POS: ({GlobalMap.instance.currentPosition.x}, {GlobalMap.instance.currentPosition.y})");
                break;

            default:
                Debug.LogWarning($"[MapBug][{Time.time:F2}s] Unknown exit direction: {exitDirection}");
                spawnPoint = SpawnPoint.SpawnRight;
                break;
        }
        
        Vector2 newPos = GlobalMap.instance.currentPosition;
        Debug.Log($"[MapBug][{Time.time:F2}s] ScanExitDirection END - Position: ({oldPos.x}, {oldPos.y}) → ({newPos.x}, {newPos.y}), SpawnPoint: {spawnPoint}");
        return spawnPoint;
    }

    private void LoadNextMap(SpawnPoint nextSpawnpoint)
    {
        Debug.Log($"[MapBug][{Time.time:F2}s] LoadNextMap START - SpawnPoint: {nextSpawnpoint}");
        
        //Setting the globalMap to safe nextSpawnpoint as lasSpawnpoint for Save & Load purposes.
        GlobalMap.instance.lastSpawnpoint = nextSpawnpoint;

        //Reset the Map
        Debug.Log($"[MapBug][{Time.time:F2}s] Resetting current map...");
        MapGenHandler.instance.ResetThisMap();

        //Play Next Map Sound at Random
        if(AudioManager.instance != null)
        {
            string[] nextMapSound = { "NextMap1", "NextMap2" };
            AudioManager.instance.PlaySound(nextMapSound[UnityEngine.Random.Range(0, 2)]);
        }

        //Tell the MaGenHandler to either create a NewMap, if its not explored yet or Load the explored one.
        Debug.Log($"[MapBug][{Time.time:F2}s] Loading map for position ({GlobalMap.instance.currentPosition.x}, {GlobalMap.instance.currentPosition.y})");
        MapGenHandler.instance.LoadOrCreateMapForCurrentPosition(nextSpawnpoint);
        
        Debug.Log($"[MapBug][{Time.time:F2}s] LoadNextMap END");
    }
}