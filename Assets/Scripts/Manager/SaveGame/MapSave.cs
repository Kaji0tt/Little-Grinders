using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class MapSave 
{
    //MapSave muss wissen:
    public float mapIndexX;
    public float mapIndexY;

    //public bool[][] isMapExplored = new bool[][] { };

    public bool gotTeleporter;
    //Map Level
    public int mapLevel;

    private float mapScaling = 5;

    //Map Theme
    public WorldType mapTheme;
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
        //CalculateMapLevel();


        //Save the Map Coordinates
        if(GlobalMap.instance != null)
        {
            Vector2 v2 = GlobalMap.instance.currentPosition;

            mapIndexX = v2.x;
            mapIndexY = v2.y;


            mapLevel = Mathf.Abs(v2.x) > Mathf.Abs(v2.y) ? (int)Mathf.Abs(v2.x) : (int)Mathf.Abs(v2.y);

        }

        gotTeleporter = false;

        if(PrefabCollection.instance != null)
        mapTheme = PrefabCollection.instance.worldType;


        //GlobalMap.instance.AddNewMap(this);

        //Debug.Log("There currently are " + GlobalMap.instance.exploredMaps.Count + " Maps explored.");
    }

    private void CalculateMapLevel()
    {
        ///There should be a global Variable, which increases everytime the player hits a certain level.
        ///Also, this should be accessable and written each time the player loads the game.
        ///On Awake, get the player Level then recallculate the globalModifier.

        
        if (GlobalMap.instance.currentPosition.x > mapScaling || GlobalMap.instance.currentPosition.y > mapScaling)
        {
            mapLevel += 1;
        }
        
    }


}


