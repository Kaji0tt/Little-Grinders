using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The GlobalMap stores all abstract variables neccessary for the coordinating the Maps (MapSaves) and depicting them in the UI (UI_GlobalMap)
public class GlobalMap : MonoBehaviour
{
    #region Singleton
    public static GlobalMap instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    public event EventHandler OnMapListChanged;

    //List of all Maps. Each MapSave that is constructed will be added to this List.
    public List<MapSave> exploredMaps = new List<MapSave>();

    //Current Map the Player is moving on. Set on every SceneLoad / MapSwap (Scene_OnSceneLoad.cs)
    public MapSave currentMap { get; private set; }

    public void Set_CurrentMap(MapSave map)
    {
        Debug.Log($"[GlobalMap.Set_CurrentMap] Setting current map to ({map.mapIndexX}, {map.mapIndexY}) - Level={map.mapLevel}, Theme={map.mapTheme}, isVisited={map.isVisited}");
        
        currentMap = map;
        // ❌ REMOVED: currentPosition = new Vector2(map.mapIndexX, map.mapIndexY);
        // ✅ FIX: currentPosition wurde bereits von ScanExitDirection() gesetzt!
        // Ein erneutes Setzen würde zu falschen Koordinaten führen.
        
        // Validierung: Prüfe ob Map-Koordinaten mit currentPosition übereinstimmen
        if (map.mapIndexX != currentPosition.x || map.mapIndexY != currentPosition.y)
        {
            Debug.LogWarning($"[GlobalMap.Set_CurrentMap] ⚠️ MISMATCH! currentPosition=({currentPosition.x},{currentPosition.y}) but map=({map.mapIndexX},{map.mapIndexY})");
        }
        else
        {
            Debug.Log($"[GlobalMap.Set_CurrentMap] ✅ Position validated: ({currentPosition.x}, {currentPosition.y})");
        }
        
        // Markiere Map als besucht
        map.isVisited = true;
        
        // ✅ Zeige dem Spieler seine aktuelle Position
        if (LogScript.instance != null)
        {
            string posText = $"Map Position: ({currentPosition.x}, {currentPosition.y}) | Level: {map.mapLevel}";
            LogScript.instance.ShowLog(posText, 4f);
        }
        
        // Generiere neue Nachbarn um die neue Position (aber nicht sofort, um Konflikte zu vermeiden)
        StartCoroutine(DelayedNeighborGeneration());
        
        OnMapListChanged?.Invoke(exploredMaps, EventArgs.Empty);
    }
    
    // NEU: Öffentliche Methode um das Event von außen zu triggern
    public void TriggerMapListChanged()
    {
        OnMapListChanged?.Invoke(exploredMaps, EventArgs.Empty);
    }
    
    private IEnumerator DelayedNeighborGeneration()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f); // Kurze Verzögerung
        CheckAndGenerateNewNeighbors();
    }

    //The current Position of WorldChunks/Cordinates loaded
    public Vector2 currentPosition;

    //Saving the last SpawnPoint, the player entered the Map in.
    public SpawnPoint lastSpawnpoint = SpawnPoint.SpawnRight;

    //Currently not used Method to get the Coordinates of a specific map.
    public Vector2 GetMapCoordinates(MapSave map)
    {
        return new Vector2(map.mapIndexX, map.mapIndexY);
    }

    public MapSave ScanIfNextMapIsExplored()
    {
        return GetMapByCords(currentPosition);
    }

    public MapSave GetMapByCords(Vector2 cords)
    {
        Debug.Log($"[GlobalMap.GetMapByCords] Searching for map at ({cords.x}, {cords.y}) - Total maps: {exploredMaps.Count}");
        
        MapSave visitedMap = null;
        MapSave pregeneratedMap = null;
        
        foreach(MapSave map in exploredMaps)
        {
            Debug.Log($"  - Checking map ({map.mapIndexX}, {map.mapIndexY}): visited={map.isVisited}, cleared={map.isCleared}, level={map.mapLevel}, fieldType[0]={map.fieldType?[0] ?? FieldType.Road}");
            
            if (map.mapIndexX == cords.x && map.mapIndexY == cords.y)
            {
                if (map.isVisited)
                {
                    visitedMap = map;
                    Debug.Log($"[GlobalMap.GetMapByCords] ✓ Found VISITED map at ({cords.x}, {cords.y}) - Level={map.mapLevel}");
                }
                else if (pregeneratedMap == null)
                {
                    pregeneratedMap = map;
                    Debug.Log($"[GlobalMap.GetMapByCords] Found pre-generated map at ({cords.x}, {cords.y}) - Level={map.mapLevel}");
                }
            }
        }
        
        // ✅ FIX: Always prioritize visited maps over pre-generated ones
        if (visitedMap != null)
        {
            Debug.Log($"[GlobalMap.GetMapByCords] Returning VISITED map (Level={visitedMap.mapLevel})");
            return visitedMap;
        }
        else if (pregeneratedMap != null)
        {
            Debug.Log($"[GlobalMap.GetMapByCords] Returning PRE-GENERATED map (Level={pregeneratedMap.mapLevel})");
            return pregeneratedMap;
        }

        Debug.Log($"[GlobalMap.GetMapByCords] ✗ Could not find map at ({cords.x}, {cords.y})");
        return null;
    }

    // NEU: Prüfe ob Map bereits existiert (in beiden Listen)
    public bool MapExistsAtCords(Vector2 cords)
    {
        return GetMapByCords(cords) != null;
    }

    // NEU: Prüfe ob Map besucht wurde
    public bool IsMapExplored(Vector2 cords)
    {
        MapSave map = GetMapByCords(cords);
        return map != null && map.isVisited;
    }

    /// <summary>
    /// NEU: Aktualisiert eine existierende Map oder erstellt eine neue
    /// Verhindert Duplikate wenn pre-generierte Maps existieren
    /// </summary>
    public void UpdateOrCreateCurrentMap()
    {
        Vector2 currentPos = currentPosition;
        Debug.Log($"[GlobalMap.UpdateOrCreateCurrentMap] === START === Position: ({currentPos.x}, {currentPos.y})");
        
        // Prüfe ob bereits eine Map an dieser Position existiert
        MapSave existingMap = GetMapByCords(currentPos);
        
        if (existingMap != null)
        {
            Debug.Log($"[GlobalMap.UpdateOrCreateCurrentMap] ✓ Map existiert bereits - aktualisiere sie");
            Debug.Log($"[GlobalMap.UpdateOrCreateCurrentMap]   Pre-existing: Level={existingMap.mapLevel}, fieldType[0]={existingMap.fieldType[0]}, Theme={existingMap.mapTheme}, isVisited={existingMap.isVisited}");
            
            // ❌ NICHT überschreiben! FieldTypes wurden bereits in LoadExistingMap() korrekt geladen
            // Die Map wurde mit den pre-generierten FieldTypes geladen und in die Szene gebaut
            // Ein Überschreiben würde die bereits geladenen (korrekten) Werte zerstören
            
            // ✅ NUR Status und Interactables aktualisieren
            existingMap.isVisited = true;
            existingMap.SaveInteractables();
            
            // ✅ FIX: Use Set_CurrentMap for consistency
            Set_CurrentMap(existingMap);
            
            Debug.Log($"[GlobalMap.UpdateOrCreateCurrentMap] Map Status aktualisiert: Level={existingMap.mapLevel}, Theme={existingMap.mapTheme}, isVisited={existingMap.isVisited}");
        }
        else
        {
            Debug.Log($"[GlobalMap.UpdateOrCreateCurrentMap] ✗ Map existiert nicht - erstelle neue");
            
            // Erstelle komplett neue Map
            MapSave newMap = new MapSave();
            newMap.isVisited = true;

            for (int i = 0; i < 81; i++)
            {
                newMap.fieldType[i] = MapGenHandler.instance.fieldPosSave[i].GetComponent<FieldPos>().Type;
            }

            // Save current interactables
            newMap.SaveInteractables();

            exploredMaps.Add(newMap);
            
            // ✅ FIX: Use Set_CurrentMap for consistency (triggers neighbor generation and player notification)
            Set_CurrentMap(newMap);
            
            Debug.Log($"[GlobalMap.UpdateOrCreateCurrentMap] ✓ Neue Map erstellt: Level={newMap.mapLevel}, Theme={newMap.mapTheme}");
        }

        OnMapListChanged?.Invoke(exploredMaps, EventArgs.Empty);
        Debug.Log($"[GlobalMap.UpdateOrCreateCurrentMap] === ENDE === Total maps: {exploredMaps.Count}");
    }
    
    /// <summary>
    /// DEPRECATED: Verwende UpdateOrCreateCurrentMap() stattdessen
    /// </summary>
    [System.Obsolete("Use UpdateOrCreateCurrentMap() instead to prevent duplicates")]
    public void CreateAndSaveNewMap()
    {
        UpdateOrCreateCurrentMap();
    }

    /// <summary>
    /// NEU: Generiert alle Nachbar-Maps um die aktuelle Position
    /// </summary>
    public void GenerateNeighborMaps(int radius = 1)
    {
        Debug.Log($"[MapBug][{Time.time:F2}s] GenerateNeighborMaps START - Radius: {radius}, currentPos: ({currentPosition.x}, {currentPosition.y}), exploredMaps.Count: {exploredMaps.Count}");
        
        List<Vector2> newMapPositions = new List<Vector2>();
        
        // Generiere alle Positionen im angegebenen Radius
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2 neighborPos = new Vector2(currentPosition.x + x, currentPosition.y + y);
                
                // ✅ FIX: NEVER generate at the exact currentPosition - it's being loaded!
                if (neighborPos.x == currentPosition.x && neighborPos.y == currentPosition.y)
                {
                    Debug.Log($"[MapBug][{Time.time:F2}s] Skipping currentPosition itself: ({neighborPos.x}, {neighborPos.y})");
                    continue;
                }
                
                // ✅ FIX: Check if a VISITED map already exists - don't regenerate visited maps!
                MapSave existingMap = GetMapByCords(neighborPos);
                if (existingMap != null)
                {
                    if (existingMap.isVisited)
                    {
                        Debug.Log($"[MapBug][{Time.time:F2}s] Skipping VISITED map: ({neighborPos.x}, {neighborPos.y}) - Level={existingMap.mapLevel}");
                        continue;
                    }
                    else
                    {
                        Debug.Log($"[MapBug][{Time.time:F2}s] Pre-generated map already exists: ({neighborPos.x}, {neighborPos.y})");
                        continue;
                    }
                }
                
                newMapPositions.Add(neighborPos);
                Debug.Log($"[MapBug][{Time.time:F2}s] Will generate: ({neighborPos.x}, {neighborPos.y})");
            }
        }
        
        // Generiere alle neuen Maps und füge sie direkt zu exploredMaps hinzu (aber als unbesucht)
        foreach (Vector2 mapPos in newMapPositions)
        {
            MapSave newMap = MapGenHandler.GenerateMapDataOnly((int)mapPos.x, (int)mapPos.y);
            newMap.isVisited = false; // Als unbesucht markieren
            exploredMaps.Add(newMap);
            Debug.Log($"[MapBug][{Time.time:F2}s] ✓ Generated map ({mapPos.x}, {mapPos.y}): Theme={newMap.mapTheme}, Level={newMap.mapLevel}, fieldType[0]={newMap.fieldType[0]}");
        }
        
        Debug.Log($"[MapBug][{Time.time:F2}s] GenerateNeighborMaps END - {newMapPositions.Count} neue Maps generiert. Total: {exploredMaps.Count}");
        
        // Benachrichtige UI über Änderungen
        OnMapListChanged?.Invoke(exploredMaps, EventArgs.Empty);
    }

    /// <summary>
    /// NEU: Prüft ob neue Nachbar-Maps generiert werden müssen
    /// </summary>
    public void CheckAndGenerateNewNeighbors()
    {
        GenerateNeighborMaps(2); // Größerer Radius für bessere Übersicht
    }

    // Getter für alle Maps für UI
    public List<MapSave> GetAllMaps()
    {
        return exploredMaps;
    }
    
    // Getter für nur besuchte Maps (für Save-System Kompatibilität)
    public List<MapSave> GetVisitedMaps()
    {
        List<MapSave> visitedMaps = new List<MapSave>();
        foreach(MapSave map in exploredMaps)
        {
            if(map.isVisited)
            {
                visitedMaps.Add(map);
            }
        }
        return visitedMaps;
    }
}
