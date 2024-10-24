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

    //List of explored Map. Each MapSave that is constructed will be added to this List.
    public List<MapSave> exploredMaps = new List<MapSave>();

    //Current Map the Player is moving on. Set on every SceneLoad / MapSwap (Scene_OnSceneLoad.cs)
    public MapSave currentMap { get; private set; }

    public void Set_CurrentMap(MapSave map)
    {
        currentMap = map;
        currentPosition = new Vector2(map.mapIndexX, map.mapIndexY);
    }

    //The current Position of WorldChunks/Cordinates loaded
    public Vector2 currentPosition;

    //Saving the last SpawnPoint, the player entered the Map in.
    public string lastSpawnpoint;

    //Currently not used Method to get the Coordinates of a specific map.
    public Vector2 GetMapCoordinates(MapSave map)
    {
        return new Vector2(map.mapIndexX, map.mapIndexY);
    }

    public MapSave ScanIfNextMapIsExplored()
    {
        foreach (MapSave map in exploredMaps)
        {
            if (map.mapIndexX == currentPosition.x && map.mapIndexY == currentPosition.y)
            {
                return map;
            }
            return null;

        }
        return null;

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

    public void CreateAndSaveNewMap()
    {

        MapSave newMap = new MapSave();

        for (int i = 0; i < 81; i++)
        {
            newMap.fieldType[i] = MapGenHandler.instance.fieldPosSave[i].GetComponent<FieldPos>().Type;

            //Debug.Log(fieldType[i]);
        }

        currentMap = newMap;

        exploredMaps.Add(newMap);

        OnMapListChanged?.Invoke(exploredMaps, EventArgs.Empty);
    }


}
