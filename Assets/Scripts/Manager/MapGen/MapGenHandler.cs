using UnityEngine.AI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

//The MapGenHandler is the main Class in the Second-Scene (Procedual Map-Generation) and is used to coordinate FieldLayouts and Create them.
//It is also managing the Save-Load functionality for explored maps, for yet unknown reasons.
public class MapGenHandler : MonoBehaviour
{
    #region Singleton
    public static MapGenHandler instance;
    private void Awake()
    {
        instance = this;

    }
    #endregion



    public NavMeshSurface navMeshSurface;

    [SerializeField]
    private GameObject[] fieldsPosObj;
    [SerializeField]
    private GameObject fieldPF;

    private int[][] fieldsArray;

    public GameObject envParentObj;
    public GameObject groundParentObj;
    public GameObject mobParentObj;

    public GameObject[] fieldPosSave { get { return fieldsPosObj; } private set { fieldsPosObj = value; } }

    //Flag to indicate if we're loading a map with saved interactables
    public bool isLoadingExistingMap { get; private set; } = false;

    //public FieldType[] saveFieldTypes { private set; get; }


    void Start()
    {
        Debug.Log("=== [MapGenHandler.Start] START ===");
        
        PlayerLoad playerLoad = FindObjectOfType<PlayerLoad>();
        bool isLoadFromMenu = PlayerPrefs.HasKey("Load");
        
        if (isLoadFromMenu)
        {
            Debug.Log("[MapGenHandler.Start] === LOAD-PFAD ===");
            PlayerSave data = SaveSystem.LoadPlayer();
            
            if (data != null)
            {
                // Map-Generierung...
                if (data.currentMap == null)
                {
                    SpawnPoint spawnToUse = (data.lastSpawnpoint != default) ? data.lastSpawnpoint : SpawnPoint.SpawnRight;
                    CreateANewMap(spawnToUse);
                }
                else
                {
                    LoadMap(data.currentMap, data.lastSpawnpoint);
                }
                
                PlayerPrefs.DeleteKey("Load");
                
                // UI Updates...
                if (UI_GlobalMap.instance != null)
                {
                    UI_GlobalMap.instance.CalculateExploredMaps();
                }
                
                if (LogScript.instance != null)
                {
                    LogScript.instance.ShowLog("Game loaded (from menu or tutorial)");
                }
                
                // PLAYER LADEN (ohne Talente)
                Debug.Log("[MapGenHandler.Start] Rufe playerLoad.LoadPlayer() auf");
                playerLoad.LoadPlayer(data);

                // UI Updates NACH dem Laden der Daten
                if (UI_GlobalMap.instance != null)
                {
                    UI_GlobalMap.instance.CalculateExploredMaps();
                }

                if (LogScript.instance != null)
                {
                    LogScript.instance.ShowLog("Game loaded (from menu or tutorial)");
                }

                // TALENTE VERZÖGERT LADEN
                Debug.Log("[MapGenHandler.Start] Starte verzögertes Talent-Laden");
                StartCoroutine(LoadTalentsAfterTreeGeneration(playerLoad, data));
            }
        }
        else
        {
            Debug.Log("[MapGenHandler.Start] === NEUES SPIEL PFAD ===");
            PlayerSave data = SaveSystem.NewSave();
            CreateANewMap(SpawnPoint.SpawnRight);
            
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("New map created (no load flag)");
            }
            
            playerLoad.LoadPlayer(data);
        }
        
        Debug.Log("=== [MapGenHandler.Start] ENDE ===");
    }

    private IEnumerator LoadTalentsAfterTreeGeneration(PlayerLoad playerLoad, PlayerSave data)
    {
        Debug.Log("[LoadTalentsAfterTreeGeneration] Warte auf TalentTree-Generierung...");
        
        // Warte bis TalentTreeGenerator existiert und Tree generiert ist
        while (TalentTreeGenerator.instance == null || 
               TalentTreeGenerator.instance.allNodes == null || 
               TalentTreeGenerator.instance.allNodes.Count == 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        // Zusätzliche Sicherheit: Ein Frame warten nach der Generierung
        yield return new WaitForEndOfFrame();
        
        Debug.Log("[LoadTalentsAfterTreeGeneration] TalentTree bereit - lade Talente");
        playerLoad.LoadTalentsDelayed(data);
    }

    public void CreateANewMap(SpawnPoint playerSpawn)
    {
        Debug.Log($"=== [CreateANewMap] START mit SpawnPoint: {playerSpawn} ===");
        
        //Roll the Theme of the Map and Populate the PrefabCollection accordingly.
        Debug.Log("[CreateANewMap] PopulatePrefabCollection...");
        PrefabCollection.instance.PopulatePrefabCollection();

        //Create a new Map Layout
        Debug.Log("[CreateANewMap] DefineFieldArray...");
        DefineFieldArray();
        Debug.Log("[CreateANewMap] CreateMapLayout...");
        CreateMapLayout();
        Debug.Log("[CreateANewMap] AssigneFieldType...");
        AssigneFieldType();
        Debug.Log("[CreateANewMap] LoadPrefabs...");
        LoadPrefabs(playerSpawn);

        //Build NavMesh for Enemys
        Debug.Log("[CreateANewMap] BuildNavMesh...");
        navMeshSurface.BuildNavMesh();

        //tell global map about the newly created map
        Debug.Log("[CreateANewMap] CreateAndSaveNewMap...");
        GlobalMap.instance.CreateAndSaveNewMap();
        
        Debug.Log("=== [CreateANewMap] ENDE ===");
    }

    public void LoadMap(MapSave map, SpawnPoint spawnPoint)
    {
        Debug.Log($"=== [LoadMap] START mit Map: {map?.ToString() ?? "null"}, SpawnPoint: {spawnPoint} ===");
        
        if(map != null)
        {
            Debug.Log($"[LoadMap] Map gefunden - Theme: {map.mapTheme}");
            
            // Set flag to indicate we're loading an existing map
            isLoadingExistingMap = true;
            
            //Load the according theme to Prefab-Collection
            Debug.Log("[LoadMap] PopulatePrefabCollection mit Map...");
            PrefabCollection.instance.PopulatePrefabCollection(map);

            //Load existing Map Layout from Save file
            Debug.Log("[LoadMap] Lade Map Layout aus Save Datei...");
            for (int i = 0; i < 81; i++)
            {
                fieldsPosObj[i].GetComponent<FieldPos>().Type = map.fieldType[i];
            }
            Debug.Log("[LoadMap] Map Layout geladen");

            Debug.Log("[LoadMap] Set_CurrentMap...");
            GlobalMap.instance.Set_CurrentMap(map);

            Debug.Log("[LoadMap] LoadPrefabs...");
            LoadPrefabs(spawnPoint);

            //Restore saved interactables
            Debug.Log("[LoadMap] RestoreInteractables...");
            map.RestoreInteractables();

            //Build NavMesh for Enemys
            Debug.Log("[LoadMap] BuildNavMesh...");
            navMeshSurface.BuildNavMesh();

            Debug.Log("[LoadMap] Map erfolgreich aus Save Data geladen");
            
            // Reset flag after loading is complete
            isLoadingExistingMap = false;
        }
        else
        {
            Debug.Log("[LoadMap] Map ist null - erstelle neue Map");
            CreateANewMap(spawnPoint);
        }
        
        Debug.Log("=== [LoadMap] ENDE ===");
    }

    public void ResetThisMap()
    {

        //Destroy all loaded Env Objects
        GameObject envParentObj = GameObject.Find("EnvParent");

        for (int i = 0; i < envParentObj.transform.childCount; i++)
        {
            Destroy(envParentObj.transform.GetChild(i).gameObject);
        }

        //Destroy all loaded Mobs
        GameObject mobParentObj = GameObject.Find("MobParent");

        for (int i = 0; i < mobParentObj.transform.childCount; i++)
        {
            Destroy(mobParentObj.transform.GetChild(i).gameObject);
        }

        if(GameObject.FindGameObjectsWithTag("SpawnedItems") != null)
        {
            GameObject[] spawnedItems = GameObject.FindGameObjectsWithTag("SpawnedItems");

            for (int i = 0; i < spawnedItems.Length; i++)
            {
            Destroy(spawnedItems[i]);
            }
            
        }





        //Destroy all OutSideVegLoader Prefab Instances
        OutsideVegLoader[] outsideVegPrefabs = FindObjectsOfType<OutsideVegLoader>(); 

        foreach(OutsideVegLoader prefab in outsideVegPrefabs)
        {
            Destroy(prefab.gameObject);
        }


    }



    private void DefineFieldArray()
    {
        fieldsArray = new int[9][];
        for (int i = 0; i < 9; i++)
        {
            fieldsArray[i] = new int[9];
        }
    }

    private void CreateMapLayout()
    {
        SelectOutsideFields();
        SelectExitPoints();
        FillEmptyFields();
    }

    private void SelectOutsideFields()
    {
        for (int x = 0; x < fieldsArray.Length; x++)
        {
            for (int z = 0; z < fieldsArray[x].Length; z++)
            {
                if (x == 0 && z == 0 || x == 8 && z == 0 || x == 0 && z == 8 || x == 8 && z == 8)
                {
                    fieldsArray[x][z] = 19;
                }
                else if (x == 0)
                {
                    fieldsArray[x][z] = 17;
                }
                else if (x == 8)
                {
                    fieldsArray[x][z] = 18;
                }
                else if (z == 0)
                {
                    fieldsArray[x][z] = 16;
                }
                else if (z == 8)
                {
                    fieldsArray[x][z] = 15;
                }
            }
        }
    }

    private void SelectExitPoints()
    {
        // Sets Random Bot Exit Pos & Outside Exit Bot
        int rand = Random.Range(1, 8);
        fieldsArray[rand][1] = 8;
        fieldsArray[rand][0] = 12;
        // save Start Pos
        int xPosStart = rand;

        // Sets Random Top Exit Pos & Outside Exit Top
        rand = Random.Range(1, 8);
        fieldsArray[rand][7] = 7;
        fieldsArray[rand][8] = 11;
        //save Exit Pos
        int xPosExit = rand;

        // Sets Random Left Exit Pos & Outside Exit Left
        rand = Random.Range(2, 7);
        fieldsArray[1][rand] = 9;
        fieldsArray[0][rand] = 13;
        int yPosLeftStart = rand;

        // Sets Random Right Exit Pos & Outside Exit Right
        rand = Random.Range(2, 7);
        fieldsArray[7][rand] = 10;
        fieldsArray[8][rand] = 14;
        int yPosRightStart = rand;


        CreateRoadLayout(xPosStart, xPosExit);
        ConnectSideRoad(yPosLeftStart, yPosRightStart);
    }

    private void ConnectSideRoad(int yPosLeft, int yPosRight)
    {
        int xPosLeft = 1;
        int xPosRigth = 7;
        while (IsFieldEmpty(xPosLeft + 1, yPosLeft))
        {
            fieldsArray[xPosLeft + 1][yPosLeft] = 1;
            xPosLeft++;
        }
        while (IsFieldEmpty(xPosRigth - 1, yPosRight))
        {
            fieldsArray[xPosRigth - 1][yPosRight] = 1;
            xPosRigth--;
        }
    }

    private void CreateRoadLayout(int xPosStart, int xPosExit)
    {
        bool isNextBlockExit = false;
        int xPos = xPosStart;
        int zPos = 1;
        int counter = 0;
        do
        {
            switch (Random.Range(1, 4))
            {
                // Left
                case 1:
                    {

                        // Break if Exit in different dir
                        if (zPos == 7 && xPosExit > xPos)
                        {
                            break;
                        }
                        // Is the next Field Empty, ExitLeft or ExitTop?
                        if (IsFieldEmpty(xPos - 1, zPos) || fieldsArray[xPos - 1][zPos] == 9 || fieldsArray[xPos - 1][zPos] == 7)
                        {
                            // Replace the last Field
                            if (IsFieldEmpty(xPos, zPos))
                            {
                                fieldsArray[xPos][zPos] = 1;
                            }
                            xPos -= 1;
                        }
                        break;
                    }
                // Top
                case 2:
                    {
                        if (IsFieldEmpty(xPos, zPos + 1) || fieldsArray[xPos][zPos + 1] == 10 || fieldsArray[xPos][zPos + 1] == 9 || fieldsArray[xPos][zPos + 1] == 7)
                        {
                            // Replace the last Field
                            if (IsFieldEmpty(xPos, zPos))
                            {
                                fieldsArray[xPos][zPos] = 1;
                            }
                            zPos += 1;
                        }
                        break;
                    }
                // Right
                case 3:
                    {
                        // Break if Exit in different dir
                        if (zPos == 7 && xPosExit < xPos)
                        {
                            break;
                        }
                        if (IsFieldEmpty(xPos + 1, zPos) || fieldsArray[xPos + 1][zPos] == 10 || fieldsArray[xPos + 1][zPos] == 7)
                        {
                            // Replace the last Field
                            if (IsFieldEmpty(xPos, zPos))
                            {
                                fieldsArray[xPos][zPos] = 1;
                            }
                            xPos += 1;
                        }
                        break;
                    }
                default:
                    break;
            }
            if (fieldsArray[xPos][zPos] == 7)
            {
                isNextBlockExit = true;
            }

            counter++;
            if (counter == 200)
            {
                Debug.Log("Counter bei 1000" + xPos + zPos);
                isNextBlockExit = true;
            }
        } while (!isNextBlockExit);


    }

    private bool IsFieldEmpty(int xPos, int zPos)
    {
        if (fieldsArray[xPos][zPos] == 0)
        {
            return true;
        }
        return false;
    }

    private void FillEmptyFields()
    {
        for (int x = 0; x < fieldsArray.Length; x++)
        {
            for (int z = 0; z < fieldsArray[x].Length; z++)
            {
                if (fieldsArray[x][z] == 0)
                {
                    fieldsArray[x][z] = Random.Range(4, 7);
                }
            }
        }

        //Debug.Log("Map-Layout Generated");
    }

    private void AssigneFieldType()
    {
        for (int i = 0; i < 81; i++)
        {
            fieldsPosObj[i].GetComponent<FieldPos>().Type = (FieldType)(fieldsArray[fieldsPosObj[i].GetComponent<FieldPos>().ArrayPosX][fieldsPosObj[i].GetComponent<FieldPos>().ArrayPosZ]);

        }
    }

    private void LoadPrefabs(SpawnPoint playerSpawn)
    {
        for (int i = 0; i < fieldsPosObj.Length; i++)
        {

            var field = Instantiate(fieldPF, fieldsPosObj[i].transform.position, Quaternion.identity);


            //Falls es sich um ein Low-Veg Field handelt, fülle dieses bei gegebener Chance mit einem vorgebauten Tile
            if (Random.Range(0, 3) == 1 && fieldsPosObj[i].GetComponent<FieldPos>().Type == FieldType.LowVeg)
            {
                //Prüfe ob es für das entsprechende Theme überhaupt PreBuildTiles gibt, ansonsten Skip.
                if (PrefabCollection.instance.preBuildTiles.Length > 0)
                {
                    Instantiate(PrefabCollection.instance.GetRandomPreBuildTile(), fieldsPosObj[i].transform.position, Quaternion.identity).transform.SetParent(envParentObj.transform);
                    
                    //Setze den Type, damit dieser abgespeichert werden kann.
                    fieldsPosObj[i].GetComponent<FieldPos>().Type = FieldType.PreBuildTile;
                }

            }

            else
                //Ansonsten fülle die Field-Pos in Abhängigkeit ihres Typs mit zufälligen Prefabs. 
                field.GetComponent<OutsideVegLoader>().LoadFieldType(fieldsPosObj[i].GetComponent<FieldPos>().Type);

            //Debug.Log("Those Types are Loaded: " + fieldsPosObj[i].GetComponent<FieldPos>().Type);
            //Set the Player Position in dependency of FieldPos.Type and field.characterSpawn position.
            switch (playerSpawn)
            {
                case SpawnPoint.SpawnRight:
                    if (fieldsPosObj[i].GetComponent<FieldPos>().Type == FieldType.OutsideExitRight)
                        PlayerManager.instance.player.transform.position = field.GetComponent<OutsideVegLoader>().characterSpawn.transform.position;
                    break;

                case SpawnPoint.SpawnLeft:
                    if (fieldsPosObj[i].GetComponent<FieldPos>().Type == FieldType.OutsideExitLeft)
                        PlayerManager.instance.player.transform.position = field.GetComponent<OutsideVegLoader>().characterSpawn.transform.position;
                    break;

                case SpawnPoint.SpawnTop:
                    if (fieldsPosObj[i].GetComponent<FieldPos>().Type == FieldType.OutsideExitTop)
                        PlayerManager.instance.player.transform.position = field.GetComponent<OutsideVegLoader>().characterSpawn.transform.position;
                    break;

                case SpawnPoint.SpawnBot:
                    if (fieldsPosObj[i].GetComponent<FieldPos>().Type == FieldType.OutsideExitBot)
                        PlayerManager.instance.player.transform.position = field.GetComponent<OutsideVegLoader>().characterSpawn.transform.position;
                    break;

                default: break;
            }

        }

    }

    //Define Entities on Current Map
    /*
    private void SaveLevelObjects()
    {
        DeclareLevelObject[] objectsToSave = GameObject.FindObjectsOfType<DeclareLevelObject>();

        foreach (DeclareLevelObject levelObject in objectsToSave)
        {
            levelObject.obj_v3x = levelObject.gameObject.transform.position.x;
            levelObject.obj_v3y = levelObject.gameObject.transform.position.y;
            levelObject.obj_v3z = levelObject.gameObject.transform.position.z;
        }
    }
    */

    public void RebuildNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
    
}
