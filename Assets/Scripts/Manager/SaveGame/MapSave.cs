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

    // GEÄNDERT: gotTeleporter durch isCleared ersetzt
    public bool isCleared = false;
    
    // NEU: Ob diese Map bereits besucht wurde
    public bool isVisited = false;
    
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

        isCleared = false;
        isVisited = true; // Diese Map wird gerade erstellt/besucht

        //Fetch the current Theme and save it
        if(PrefabCollection.instance != null)
        mapTheme = PrefabCollection.instance.worldType;

        // NEU: Nur für aktuell geladene Maps Interactables sammeln
        // Für pre-generierte Maps passiert das nicht
        if (MapGenHandler.instance != null && MapGenHandler.instance.fieldPosSave != null)
        {
            SaveCurrentMapInteractables();
        }
    }

    // NEU: Konstruktor für pre-generierte Maps (ohne aktuelle Szene)
    public MapSave(int x, int y, int level, WorldType theme, FieldType[] fieldTypes)
    {
        mapIndexX = x;
        mapIndexY = y;
        mapLevel = level;
        mapTheme = theme;
        fieldType = fieldTypes ?? new FieldType[81];
        isCleared = false;
        isVisited = false; // Generierte Maps sind erstmal unbesucht
        mapInteractables = new List<InteractableSaveData>();
    }

    private void CalculateMapLevel()
    {
        
        if (GlobalMap.instance.currentPosition.x > mapScaling || GlobalMap.instance.currentPosition.y > mapScaling)
        {
            mapLevel += 1;
        }
        
    }

    // NEU: Interactables für diese Map
    public List<InteractableSaveData> mapInteractables = new List<InteractableSaveData>();

    private void SaveCurrentMapInteractables()
    {
        if (InteractableManager.instance != null)
        {
            var allStates = InteractableManager.instance.GetAllStates();
            
            // Filtere nur Interactables der aktuellen Map (basierend auf Position oder Map-ID)
            foreach (var state in allStates.Values)
            {
                if (IsInteractableOnCurrentMap(state))
                {
                    mapInteractables.Add(state);
                }
            }
        }
    }
    
    private bool IsInteractableOnCurrentMap(InteractableSaveData data)
    {
        // Implementiere Logik um zu prüfen ob Interactable zur aktuellen Map gehört
        // z.B. über Position oder Map-spezifische IDs
        return true; // Placeholder
    }
}


