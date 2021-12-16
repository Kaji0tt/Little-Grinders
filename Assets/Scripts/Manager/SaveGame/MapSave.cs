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

    //To Save Mobs, declare an enum for every type of Mob. 
    //For Enemy[] loadedEnemies FindObjectsOfType<Enemy>



    //public float [][][] enemyPosition;


    ///If we start to use the "SaveLevelPrefab" Method shared by OneWheelStudio, we should be using 
    ///following enum LevelObjectTypes: "Env, Enemies, Tiles"
    ///After Save, those types could be loaded as childs to according parent GameObjects.
    ///

    ///Im Prinzip muss es eine Funktion geben, welche FieldPos[] der 
    ///
    public MapSave()
    {
        //CalculateMapLevel();


        //Save the Map Coordinates
        if(GlobalMap.instance != null)
        {
            //Fetch the current coordinates of the Global Map and save them to serializable floats
            Vector2 v2 = GlobalMap.instance.currentPosition;

            mapIndexX = v2.x;
            mapIndexY = v2.y;

            //Calculate Map-Level in dependency of floats
            mapLevel = Mathf.Abs(v2.x) > Mathf.Abs(v2.y) ? (int)Mathf.Abs(v2.x) : (int)Mathf.Abs(v2.y);

        }

        gotTeleporter = false;

        //Fetch the current Theme and save it
        if(PrefabCollection.instance != null)
        mapTheme = PrefabCollection.instance.worldType;

    }



    private void CalculateMapLevel()
    {
        
        if (GlobalMap.instance.currentPosition.x > mapScaling || GlobalMap.instance.currentPosition.y > mapScaling)
        {
            mapLevel += 1;
        }
        
    }


}


