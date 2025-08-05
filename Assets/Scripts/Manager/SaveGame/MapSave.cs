using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class InteractableSave
{
    public float positionX;
    public float positionY;
    public float positionZ;
    public string interactableType; // Name/type of the interactable prefab
    public bool isUsed;
    
    public InteractableSave(Vector3 position, string type, bool used)
    {
        positionX = position.x;
        positionY = position.y;
        positionZ = position.z;
        interactableType = type;
        isUsed = used;
    }
    
    public Vector3 GetPosition()
    {
        return new Vector3(positionX, positionY, positionZ);
    }
}

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

    //Interactables on this map
    public List<InteractableSave> interactables = new List<InteractableSave>();

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

    /// <summary>
    /// Collects all interactables currently in the scene and saves their state
    /// </summary>
    public void SaveInteractables()
    {
        interactables.Clear();
        
        // Find all interactables in the scene
        Interactable[] sceneInteractables = UnityEngine.Object.FindObjectsOfType<Interactable>();
        
        foreach (Interactable interactable in sceneInteractables)
        {
            // Get the interactable's name without "(Clone)" suffix
            string interactableType = interactable.gameObject.name.Replace("(Clone)", "").Trim();
            
            // Create save data for this interactable
            InteractableSave saveData = new InteractableSave(
                interactable.transform.position,
                interactableType,
                interactable.interactableUsed
            );
            
            interactables.Add(saveData);
        }
        
        Debug.Log($"[MapSave] Saved {interactables.Count} interactables");
    }

    /// <summary>
    /// Restores saved interactables to the scene
    /// </summary>
    public void RestoreInteractables()
    {
        if (interactables == null || interactables.Count == 0)
        {
            Debug.Log("[MapSave] No saved interactables to restore");
            return;
        }

        PrefabCollection prefabCollection = PrefabCollection.instance;
        if (prefabCollection == null)
        {
            Debug.LogError("[MapSave] PrefabCollection not found, cannot restore interactables");
            return;
        }

        GameObject envParent = MapGenHandler.instance?.envParentObj;
        if (envParent == null)
        {
            Debug.LogError("[MapSave] Environment parent object not found");
            return;
        }

        foreach (InteractableSave saveData in interactables)
        {
            // Get the prefab for this interactable type
            GameObject prefab = prefabCollection.GetInteractableByName(saveData.interactableType);
            
            // Instantiate the interactable at the saved position
            GameObject restoredInteractable = UnityEngine.Object.Instantiate(
                prefab, 
                saveData.GetPosition(), 
                Quaternion.identity
            );
            
            // Set the parent to the environment parent
            restoredInteractable.transform.SetParent(envParent.transform);
            
            // Restore the used state
            Interactable interactableComponent = restoredInteractable.GetComponent<Interactable>();
            if (interactableComponent != null)
            {
                interactableComponent.interactableUsed = saveData.isUsed;
                
                // If it was used, update the sprite accordingly
                if (saveData.isUsed && interactableComponent.newSprite != null)
                {
                    SpriteRenderer spriteRenderer = restoredInteractable.GetComponentInParent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sprite = interactableComponent.newSprite;
                    }
                }
            }
        }
        
        Debug.Log($"[MapSave] Restored {interactables.Count} interactables");
    }


    private void CalculateMapLevel()
    {
        
        if (GlobalMap.instance.currentPosition.x > mapScaling || GlobalMap.instance.currentPosition.y > mapScaling)
        {
            mapLevel += 1;
        }
        
    }


}


