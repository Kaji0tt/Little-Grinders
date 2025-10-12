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
        currentMap = map;
        currentPosition = new Vector2(map.mapIndexX, map.mapIndexY);
        
        // Markiere Map als besucht
        map.isVisited = true;
        
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
        foreach(MapSave map in exploredMaps)
        {
            if (map.mapIndexX == cords.x && map.mapIndexY == cords.y)
            {
                return map;
            }
        }

        Debug.Log("Could not find a Map with Cords: " + cords);
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

    public void CreateAndSaveNewMap()
    {
        MapSave newMap = new MapSave();
        newMap.isVisited = true; // Diese Map wird gerade besucht

        for (int i = 0; i < 81; i++)
        {
            newMap.fieldType[i] = MapGenHandler.instance.fieldPosSave[i].GetComponent<FieldPos>().Type;
        }

        // Save current interactables
        newMap.SaveInteractables();

        currentMap = newMap;
        exploredMaps.Add(newMap);

        OnMapListChanged?.Invoke(exploredMaps, EventArgs.Empty);
    }

    /// <summary>
    /// NEU: Generiert alle Nachbar-Maps um die aktuelle Position
    /// </summary>
    public void GenerateNeighborMaps(int radius = 1)
    {
        Debug.Log($"[GlobalMap] Generiere Nachbar-Maps mit Radius {radius} um Position {currentPosition}");
        
        List<Vector2> newMapPositions = new List<Vector2>();
        
        // Generiere alle Positionen im angegebenen Radius
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2 neighborPos = new Vector2(currentPosition.x + x, currentPosition.y + y);
                
                // Prüfe ob Map bereits existiert
                if (!MapExistsAtCords(neighborPos))
                {
                    newMapPositions.Add(neighborPos);
                }
            }
        }
        
        // Generiere alle neuen Maps und füge sie direkt zu exploredMaps hinzu (aber als unbesucht)
        foreach (Vector2 mapPos in newMapPositions)
        {
            MapSave newMap = MapGenHandler.GenerateMapDataOnly((int)mapPos.x, (int)mapPos.y);
            newMap.isVisited = false; // Als unbesucht markieren
            exploredMaps.Add(newMap);
            Debug.Log($"[GlobalMap] Neue Map generiert: ({mapPos.x}, {mapPos.y}) - Theme: {newMap.mapTheme}");
        }
        
        Debug.Log($"[GlobalMap] {newMapPositions.Count} neue Nachbar-Maps generiert. Total maps: {exploredMaps.Count}");
        
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
