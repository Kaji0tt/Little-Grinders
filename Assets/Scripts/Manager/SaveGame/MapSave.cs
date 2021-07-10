using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapSave
{
    //MapSave muss wissen:
    public float mapIndexX;
    public float mapIndexY;

    //public bool[][] isMapExplored = new bool[][] { };


    //Map Level

    //Map Theme

    //Special Map??

    //---> Specific OutSideVeg Prefabs <---

    //Field Type
    public FieldType[] fieldType = new FieldType[81];

    ///Auf der Weltkarte gesehen, wo befinden wir uns? Zunächst (0,0), sobald der Spieler den Rand erreicht, wird der entsprechende Vektor überarbeitet.
    ///Wird die Variabel sein, von welcher aus die Interface Map geladen wird
    ///Dazu würden sich ggf. zwei Int-Werte in den PlayerPrefs setzen lassen
    //FieldLayout(x,y)

    ///Welcher MapTile ist wie und wo gespawnt? (NoVeg, LowVeg, HighVeg, Road, Exits etc.)
    ///Wir müssen wissen, welches Field welchen Tile zugewiesen bekommen hat.
    //Bool isSomethingSpawned[] und Int spawnedGameObject[]
    ///Einen Array aus allen SpawnPoints erstellen. Falls auf diesem Array etwas gespawned ist, muss auslesbar sein, was dort gespawned ist.
    ///Falls(isSomethingSpawned[i])
    ///InstantiatespawnedGameObject[]
    ///

    ///Im Prinzip muss es eine Funktion geben, welche FieldPos[] der 
    ///
    public MapSave()
    {
        Debug.Log("Map should get constructed");
        //Save the Map Coordinates
        mapIndexX = GlobalMap.currentPosition.x;
        mapIndexY = GlobalMap.currentPosition.y;

        //Save the FieldLayout
        for(int i = 0; i < 81; i++)
        {       
            fieldType[i] = MapGenHandler.instance.fieldPosSave[i].GetComponent<FieldPos>().Type;

            //Debug.Log(fieldType[i]);
        }
        
        GlobalMap.AddCurrentMapToExploredMaps(this);

        Debug.Log("There currently are " + GlobalMap.exploredMaps.Count + " Maps explored.");
    }
}

public static class GlobalMap
{
    //List of explored Map. Each MapSave that is constructed will be added to this List.
    public static List<MapSave> exploredMaps = new List<MapSave>();

    //Current Map the Player is moving on. Set on every SceneLoad / MapSwap (Scene_OnSceneLoad.cs)
    public static MapSave currentMap;

    //The current Position of WorldChunks/Cordinates loaded
    public static Vector2 currentPosition;

    //Saving the last SpawnPoint, the player entered the Map in.
    public static string lastSpawnpoint;

    public static void AddCurrentMapToExploredMaps(MapSave map)
    {
        exploredMaps.Add(map);
    }

    //Currently not used Method to get the Coordinates of a specific map.
    public static Vector2 GetMapCoordinates(MapSave map)
    {
        return new Vector2(map.mapIndexX, map.mapIndexY);
    }

    public static MapSave GetNextMap()
    {
        foreach (MapSave map in exploredMaps)
        {
            if (map.mapIndexX == currentPosition.x && map.mapIndexY == currentPosition.y)
            {
                MapGenHandler.instance.LoadMap(map, lastSpawnpoint);

                return map;
            }

        }

        if (lastSpawnpoint != null)
            MapGenHandler.instance.CreateANewMap(lastSpawnpoint);
        else
            MapGenHandler.instance.CreateANewMap("SpawnRight");

        return null;
    }

    public static bool ScanIfNextMapIsExplored()
    {
        if (GetNextMap() != null)
            return true;

        //MapGenHandler.instance.LoadMap(GetCurrentMap(), lastSpawnpoint);
        else
            return false;
            //
    }

}
