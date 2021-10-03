using UnityEngine.AI;
using UnityEngine;
using UnityEngine.SceneManagement;

//The MapGenHandler is the may Class in the Second-Scene (Procedual Map-Generation) and is used to coordinate FieldLayouts and Create them.
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


    [SerializeField]
    NavMeshSurface navMeshSurface;

    [SerializeField]
    private GameObject[] fieldsPosObj;
    [SerializeField]
    private GameObject fieldPF;

    private int[][] fieldsArray;

    public GameObject[] fieldPosSave { get { return fieldsPosObj; } private set { fieldsPosObj = value; } }

    //public FieldType[] saveFieldTypes { private set; get; }


    void Start()
    {

        //If the Player enters the World-Map Scene, it should load its current save.
        if (!PlayerPrefs.HasKey("Load"))
        {
            PlayerLoad playerLoad = FindObjectOfType<PlayerLoad>();

            PlayerSave data = SaveSystem.LoadPlayer();

            CreateANewMap("SpawnRight");

            playerLoad.LoadPlayer(data);

            LogScript.instance.ShowLog("Loaded playerLoad automaticly and created a new Map, \n from within the first Scene (No PlayerPref Key: Load found)");
        }

        //else it should load the data.
        else
        {

            PlayerLoad playerLoad = FindObjectOfType<PlayerLoad>();

            PlayerSave data = SaveSystem.LoadPlayer();

            playerLoad.LoadPlayer(data);

            if (data.currentMap == null)
            {
                LogScript.instance.ShowLog("creating a new Map, \n since data.currentMap == null");
                CreateANewMap("SpawnRight");
            }

            else
            {
                //Load the Map from data. Load the last Spawnpoint from data - it is from within the second scene only used, to teleport the character transform.
                //see LoadPrefabs for this function, since im to stupid to call it from elsewhere.
                LoadMap(data.currentMap, data.lastSpawnpoint);

            }

            PlayerPrefs.DeleteKey("Load");

            UI_GlobalMap.instance.CalculateExploredMaps();

            LogScript.instance.ShowLog("Loaded with Key: Load (Should be called, when loaded from menu )");

        }

        //print(GlobalMap.exploredMaps.Count + " Maps are currently explored. We are moving on " + GlobalMap.GetNextMap().mapIndexX + "," + GlobalMap.GetNextMap().mapIndexY);

    }

    public void CreateANewMap(string playerSpawn)
    {
        //Roll the Theme of the Map and Populate the PrefabCollection accordingly.
        PrefabCollection.instance.PopulatePrefabCollection(playerSpawn);


    }

    public void CreateANewMap2(string playerSpawn)
    {
        //Create a new Map Layout
        DefineFieldArray();
        CreateMapLayout();
        AssigneFieldType();
        LoadPrefabs(playerSpawn);

        //Build NavMesh for Enemys
        navMeshSurface.BuildNavMesh();

        //tell global map about the newly created map
        GlobalMap.instance.CreateAndSaveNewMap();
    }

    public void LoadMap(MapSave map, string spawnPoint)
    {
        if(map != null)
        {
            //Load the according theme to Prefab-Collection
            PrefabCollection.instance.LoadPrefabCollection(map);

            //Load existing Map Layout from Save file
            for (int i = 0; i < 81; i++)
            {
                fieldsPosObj[i].GetComponent<FieldPos>().Type = map.fieldType[i];
            }

            GlobalMap.instance.Set_CurrentMap(map);

            LoadPrefabs(spawnPoint);

            //Build NavMesh for Enemys
            navMeshSurface.BuildNavMesh();

            Debug.Log("Map loaded from Save Data or GlobalMap");
        }

        else
        {
            Debug.Log("Creating new Map, since it was not yet explored.");

            CreateANewMap(spawnPoint);
        }





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

    private void LoadPrefabs(string playerSpawn)
    {
        for (int i = 0; i < fieldsPosObj.Length; i++)
        {

            var field = Instantiate(fieldPF, fieldsPosObj[i].transform.position, Quaternion.identity);
            

            //if (fieldPosObj[i].Type = LowVeg)
            // dann, if(Random.Range (0, 3) == 0)
            //          PrefabCollection.PreBuildArea(Random.Range(0, PreBuildArea.Length))
            field.GetComponent<OutsideVegLoader>().LoadFieldType(fieldsPosObj[i].GetComponent<FieldPos>().Type);

            //Debug.Log("Those Types are Loaded: " + fieldsPosObj[i].GetComponent<FieldPos>().Type);
            //Set the Player Position in dependency of FieldPos.Type and field.characterSpawn position.
            switch (playerSpawn)
            {
                case "SpawnRight":
                    if (fieldsPosObj[i].GetComponent<FieldPos>().Type == FieldType.OutsideExitRight)
                        PlayerManager.instance.player.transform.position = field.GetComponent<OutsideVegLoader>().characterSpawn.transform.position;

                    break;

                case "SpawnLeft":
                    if (fieldsPosObj[i].GetComponent<FieldPos>().Type == FieldType.OutsideExitLeft)
                        PlayerManager.instance.player.transform.position = field.GetComponent<OutsideVegLoader>().characterSpawn.transform.position;

                    break;

                case "SpawnTop":
                    if (fieldsPosObj[i].GetComponent<FieldPos>().Type == FieldType.OutsideExitTop)
                        PlayerManager.instance.player.transform.position = field.GetComponent<OutsideVegLoader>().characterSpawn.transform.position;

                    break;

                case "SpawnBot":
                    if (fieldsPosObj[i].GetComponent<FieldPos>().Type == FieldType.OutsideExitBot)
                        PlayerManager.instance.player.transform.position = field.GetComponent<OutsideVegLoader>().characterSpawn.transform.position;

                    break;

                default: break;
            }

        }

    }

    public void RebuildNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
    
}
