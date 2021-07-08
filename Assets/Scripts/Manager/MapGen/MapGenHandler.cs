using UnityEditor.AI;
using UnityEngine;

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
    private GameObject[] fieldsPosObj;
    [SerializeField]
    private GameObject fieldPF;

    private int[][] fieldsArray;

    public GameObject[] fieldPosSave { get { return fieldsPosObj; } private set { fieldsPosObj = value; } }

    //public FieldType[] saveFieldTypes { private set; get; }

    void Start()
    {

        PlayerPrefs.SetInt("MapX", 0);
        PlayerPrefs.SetInt("MapY", 0);

        CreateANewMap("SpawnRight"); //ggf. CreateANewMap(SpielerPosition);

        print(GlobalMap.exploredMaps.Count + " Maps are currently explored. We are moving on " + GlobalMap.GetCurrentMap().mapIndexX + "," + GlobalMap.GetCurrentMap().mapIndexY);


    }

    public void CreateANewMap(string playerSpawn)
    {
        DefineFieldArray();
        CreateMapLayout();
        AssigneFieldType();
        LoadPrefabs(playerSpawn);
        NavMeshBuilder.BuildNavMesh();

        if(!Scene_OnSceneLoad.sceneGotLoaded)
        Scene_OnSceneLoad.LoadScenePlayer(FindObjectOfType<PlayerLoad>());

        SaveThisMap();
        GlobalMap.SetCurrentMap();
    }

    public void ScanForExploredMaps(string spawnPoint)
    {
        if (GlobalMap.GetCurrentMap() != null)
            LoadMap(GlobalMap.GetCurrentMap(), spawnPoint);
        else
            CreateANewMap(spawnPoint);
    }

    public void LoadMap(MapSave map, string spawnPoint)
    {

        for (int i = 0; i < 81; i++)
        {
            fieldsPosObj[i].GetComponent<FieldPos>().Type = map.fieldType[i];
        }

        LoadPrefabs(spawnPoint);



        NavMeshBuilder.BuildNavMesh();

        if (!Scene_OnSceneLoad.sceneGotLoaded)
            Scene_OnSceneLoad.LoadScenePlayer(FindObjectOfType<PlayerLoad>());
        GlobalMap.SetCurrentMap();

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

        Debug.Log("Map Loaded");
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

    private void SaveThisMap()
    {
        print((PlayerPrefs.GetInt("MapX")) + ", " + (PlayerPrefs.GetInt("MapY")));

        new MapSave();
    }

    /*
    private void LoadPlayer()
    {

         PlayerLoad playerload = FindObjectOfType<PlayerLoad>();

         //print(playerload.gameObject.name + " got the playerLoad comp, we should be loading now!");

         PlayerSave data = SaveSystem.LoadPlayer();

         playerload.LoadPlayer(data);

         PlayerPrefs.DeleteKey("SceneLoad");

    }
    */
}
