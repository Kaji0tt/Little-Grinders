using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutsideVegLoader : MonoBehaviour
{
    [Header("Object Spawn")]
    [SerializeField] private GameObject[] topVegPF;
    [SerializeField] private GameObject[] botVegPF;
    [SerializeField] private GameObject[] leftVegPF;
    [SerializeField] private GameObject[] rightVegPF;
    [SerializeField] private GameObject[] centerVegPF;
    
    [SerializeField] private GameObject[] topFencePF;
    [SerializeField] private GameObject[] botFencePF;
    [SerializeField] private GameObject[] leftFencePF;
    [SerializeField] private GameObject[] rightFencePF;

    [Header("GroundTexture")]
    [SerializeField] private GameObject[] groundTextureSpawn;
    [SerializeField] private GameObject[] exitPos;

    [Header("Entities")]
    public GameObject characterSpawn;
    [SerializeField] private GameObject[] entitieSpawn;
    [SerializeField] private GameObject[] interactableCollection;


    [SerializeField] private PrefabCollection prefabCollection;



    private GameObject envParentObj;
    private GameObject groundParentObj;


    //Stuff for Saving
    private int[] allSpawnPoints;


    void Awake()
    {
        prefabCollection = GameObject.Find("PrefabCollection").GetComponent<PrefabCollection>();
        envParentObj = GameObject.Find("EnvParent");
        groundParentObj = GameObject.Find("GroundParent");

    }

    #region Generate Fields in Dependency of FieldType
    public void LoadFieldType(FieldType type)
    {
        switch (type)
        {
            case FieldType.Road:
                LoadRoadField();
                break;
            case FieldType.NoVeg:
                LoadNoVegField();
                break;
            case FieldType.LowVeg:
                LoadLowVegField();
                break;
            case FieldType.HighVeg:
                LoadHighVegField();
                break;
            case FieldType.ExitBot:
                LoadRoadField();
                break;
            case FieldType.ExitTop:
                LoadRoadField();
                break;
            case FieldType.ExitLeft:
                LoadRoadField();
                break;
            case FieldType.ExitRight:
                LoadRoadField();
                break;
            case FieldType.OutsideExitTop:
                LoadOutsideExitTopField();
                break;
            case FieldType.OutsideExitBot:
                LoadOutsideExitBotField();
                break;
            case FieldType.OutsideExitLeft:
                LoadOutsideExitLeftField();
                break;
            case FieldType.OutsideExitRight:
                LoadOutsideExitRightField();
                break;
            case FieldType.OutsideTop:
                LoadOutsideTopField();
                break;
            case FieldType.OutsideBot:
                LoadOutsideBotField();
                break;
            case FieldType.OutsideLeft:
                LoadOutsideLeftField();
                break;
            case FieldType.OutsideRight:
                LoadOutsideRightField();
                break;
            case FieldType.OutsideCorner:
                LoadHighVegField();
                break;
            default:
                break;
        }

    }
    #endregion

    /// <summary>
    /// Generating Vegetation in Dependency of Fieldtype
    /// </summary>
    /// Each Field consists of 5 Areas, the Border + Center. In dependency of which fieldtype is supposed to be generated, we
    /// may change the vegetation of those 5 areas.
    /// We here may declare, wether we'd like to spawn Small/Low Perfabs or Gian/Big Prefabs in those areas manually

    public void LoadRoadField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();

        //LoadRoadTexture();
        //LoadRandomGroundTexture();
        LoadEnemies(20);
    }
    
    public void LoadNoVegField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();

        //LoadRandomGroundTexture();

        LoadEnemies(3);


        LoadInteractable();
    }



    public void LoadLowVegField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();


        
        LoadMidTopVeg();
        LoadMidBotVeg();
        LoadMidLeftVeg();
        LoadMidRightVeg();
        LoadMidCenterVeg();

        //LoadRandomGroundTexture();
    }
    public void LoadHighVegField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();
        
        LoadMidTopVeg();
        LoadMidBotVeg();
        LoadMidLeftVeg();
        LoadMidRightVeg();
        LoadMidCenterVeg();
        
        LoadHighTopVeg();
        LoadHighBotVeg();
        LoadHighLeftVeg();
        LoadHighRightVeg();
        LoadHighCenterVeg();

        //LoadRandomGroundTexture();
    }

    public void LoadOutsideTopField()
    {
        LoadHighVegField();
        LoadBotFence();
    }
    public void LoadOutsideBotField()
    {
        LoadHighVegField();
        LoadTopFence();
    }
    public void LoadOutsideLeftField()
    {
        LoadHighVegField();
        LoadRightFence();
    }
    public void LoadOutsideRightField()
    {
        LoadHighVegField();
        LoadLeftFence();
    }

    public void LoadOutsideExitTopField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();
        
        LoadMidLeftVeg();
        LoadMidRightVeg();

        LoadLeftFence();
        LoadRightFence();

        exitPos[3].gameObject.SetActive(true);
        //Start from BotSide
    }
    public void LoadOutsideExitBotField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();
        
        LoadMidLeftVeg();
        LoadMidRightVeg();

        LoadLeftFence();
        LoadRightFence();

        exitPos[1].gameObject.SetActive(true);
        //Start from TopSide
    }
    public void LoadOutsideExitLeftField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();
        
        LoadMidTopVeg();
        LoadMidBotVeg();

        LoadTopFence();
        LoadBotFence();

        exitPos[2].gameObject.SetActive(true);
        //Start from RightSide
    }
    public void LoadOutsideExitRightField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();

        //if Statement, um zu prüfen, von wo der Charakter kam
        //if(Charakter kam von Rechts)
        //LoadCharakter();
        
        LoadMidTopVeg();
        LoadMidBotVeg();

        LoadTopFence();
        LoadBotFence();

        exitPos[0].gameObject.SetActive(true);
        //Start from LeftSide
    }

    /// <summary>
    /// Vegetation Section
    /// </summary>

    #region High Vegetation SpawnPoints
    private void LoadHighTopVeg()
    {
        //For each Spawnpoint, setted for th upper border
        for (int i = 0; i < topVegPF.Length; i++)
        {
            //roll the dice. if we hit 1..
            if (Random.Range(0, 2) == 1)
            {
                //..roll again, wether to spawn low, medium or large prefabs at the spawnpoint of the upper border.
                switch (Random.Range(0,3))
                                {
                                    case 0:
                                        Instantiate(prefabCollection.GetRandomSmalGreenPF(), topVegPF[i].transform.position, Quaternion.identity).transform.SetParent(envParentObj.transform);                                  
                                        break;
                                    case 1:
                                        Instantiate(prefabCollection.GetRandomMidGreenPF(), topVegPF[i].transform.position, Quaternion.identity).transform.SetParent(envParentObj.transform);
                                        break;
                                    case 2:
                                        Instantiate(prefabCollection.GetRandomHighGreenPF(), topVegPF[i].transform.position, Quaternion.identity).transform.SetParent(envParentObj.transform);
                                        break;
                                }
            }
            
            //destroy this spawnpoint.
            //Destroy(topVegPF[i]);

            //we may want to store the information of the spawned prefab on a serializable variable for saving / loading
            //consider passing information about the spawned object on prefabCollection.cs
        }
    }
    private void LoadHighBotVeg()
    {
        for (int i = 0; i < botVegPF.Length; i++)
        {
            if (Random.Range(0, 2) == 1)
            {
                switch (Random.Range(0, 3))
                {
                    case 0:
                        Instantiate(prefabCollection.GetRandomSmalGreenPF(), botVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 1:
                        Instantiate(prefabCollection.GetRandomMidGreenPF(), botVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 2:
                        Instantiate(prefabCollection.GetRandomHighGreenPF(), botVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                }
            }
            Destroy(botVegPF[i]);
        }
    }
    private void LoadHighLeftVeg()
    {
        for (int i = 0; i < leftVegPF.Length; i++)
        {
            if (Random.Range(0, 2) == 1)
            {
                switch (Random.Range(0, 3))
                {
                    case 0:
                        Instantiate(prefabCollection.GetRandomSmalGreenPF(), leftVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 1:
                        Instantiate(prefabCollection.GetRandomMidGreenPF(), leftVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 2:
                        Instantiate(prefabCollection.GetRandomHighGreenPF(), leftVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                }
            }
            Destroy(leftVegPF[i]);
        }
    }
    private void LoadHighRightVeg()
    {
        for (int i = 0; i < rightVegPF.Length; i++)
        {
            if (Random.Range(0, 2) == 1)
            {
                switch (Random.Range(0, 3))
                {
                    case 0:
                        Instantiate(prefabCollection.GetRandomSmalGreenPF(), rightVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 1:
                        Instantiate(prefabCollection.GetRandomMidGreenPF(), rightVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 2:
                        Instantiate(prefabCollection.GetRandomHighGreenPF(), rightVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                }
            }
            Destroy(rightVegPF[i]);
        }
    }
    private void LoadHighCenterVeg()
    {
        for (int i = 0; i < centerVegPF.Length; i++)
        {
            if (Random.Range(0, 2) == 1)
            {
                switch (Random.Range(0, 3))
                {
                    case 0:
                        Instantiate(prefabCollection.GetRandomSmalGreenPF(), centerVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 1:
                        Instantiate(prefabCollection.GetRandomMidGreenPF(), centerVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 2:
                        Instantiate(prefabCollection.GetRandomHighGreenPF(), centerVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                }
            }

            //SaveSpawnPointForSerialization(centerVegPF[i]);
            Destroy(centerVegPF[i]);
        }
    }
    #endregion

    #region Mid Vegetation SpawnPoints
    private void LoadMidTopVeg()
    {
        for (int i = 0; i < topVegPF.Length; i++)
        {
            if (Random.Range(0, 4) == 1)
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        Instantiate(prefabCollection.GetRandomSmalGreenPF(), topVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 1:
                        Instantiate(prefabCollection.GetRandomMidGreenPF(), topVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                }
            }
            Destroy(topVegPF[i]);
        }
    }
    private void LoadMidBotVeg()
    {
        for (int i = 0; i < botVegPF.Length; i++)
        {
            if (Random.Range(0, 4) == 1)
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        Instantiate(prefabCollection.GetRandomSmalGreenPF(), botVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 1:
                        Instantiate(prefabCollection.GetRandomMidGreenPF(), botVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                }
            }
            Destroy(botVegPF[i]);
        }
    }
    private void LoadMidLeftVeg()
    {
        for (int i = 0; i < leftVegPF.Length; i++)
        {
            if (Random.Range(0, 4) == 1)
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        Instantiate(prefabCollection.GetRandomSmalGreenPF(), leftVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 1:
                        Instantiate(prefabCollection.GetRandomMidGreenPF(), leftVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                }
            }
            Destroy(leftVegPF[i]);
        }
    }
    private void LoadMidRightVeg()
    {
        for (int i = 0; i < rightVegPF.Length; i++)
        {
            if (Random.Range(0, 4) == 1)
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        Instantiate(prefabCollection.GetRandomSmalGreenPF(), rightVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 1:
                        Instantiate(prefabCollection.GetRandomMidGreenPF(), rightVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                }
            }
            Destroy(rightVegPF[i]);
        }
    }
    private void LoadMidCenterVeg()
    {
        for (int i = 0; i < centerVegPF.Length; i++)
        {
            if (Random.Range(0, 4) == 1)
            {
                switch (Random.Range(0, 2))
                {
                    case 0:
                        Instantiate(prefabCollection.GetRandomSmalGreenPF(), centerVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                    case 1:
                        Instantiate(prefabCollection.GetRandomMidGreenPF(), centerVegPF[i].transform.position,
                            Quaternion.identity).transform.SetParent(envParentObj.transform);
                        break;
                }
            }
            Destroy(centerVegPF[i]);
        }
    }
    #endregion

    #region Low Vegetation SpawnPoints
    private void LoadLowTopVeg()
    {
        for (int i = 0; i < topVegPF.Length; i++)
        {
            if (Random.Range(0,3)==1)
            {
                Instantiate(prefabCollection.GetRandomSmalGreenPF(), topVegPF[i].transform.position, Quaternion.identity).transform.SetParent(envParentObj.transform);
            }
            Destroy(topVegPF[i]); 
        }
    }
    private void LoadLowBotVeg()
    {
        
            for (int i = 0; i < botVegPF.Length; i++)
            {
                if (Random.Range(0, 3) == 1)
                {
                    Instantiate(prefabCollection.GetRandomSmalGreenPF(), botVegPF[i].transform.position,
                        Quaternion.identity).transform.SetParent(envParentObj.transform);
                }
                Destroy(botVegPF[i]);
            }
    }
    private void LoadLowLeftVeg()
    {
        for (int i = 0; i < leftVegPF.Length; i++)
        {
            if (Random.Range(0, 3) == 1)
            {
                Instantiate(prefabCollection.GetRandomSmalGreenPF(), leftVegPF[i].transform.position,
                    Quaternion.identity).transform.SetParent(envParentObj.transform);
            }
            Destroy(leftVegPF[i]);
        }
    }
    private void LoadLowRightVeg()
    {
        for (int i = 0; i < rightVegPF.Length; i++)
        {
            if (Random.Range(0, 3) == 1)
            {
                Instantiate(prefabCollection.GetRandomSmalGreenPF(), rightVegPF[i].transform.position,
                    Quaternion.identity).transform.SetParent(envParentObj.transform);
            }
            Destroy(rightVegPF[i]);
        }
    }
    private void LoadLowCenterVeg()
    {
        for (int i = 0; i < centerVegPF.Length; i++)
        {
            if (Random.Range(0, 3) == 1)
            {
                Instantiate(prefabCollection.GetRandomSmalGreenPF(), centerVegPF[i].transform.position,
                    Quaternion.identity).transform.SetParent(envParentObj.transform);
            }
            Destroy(centerVegPF[i]);
        }
    }
    #endregion

    #region GroundTexture

    void LoadRandomGroundTexture()
    {
        foreach (GameObject spawnpoint in groundTextureSpawn)
        {
            Instantiate(prefabCollection.GetRandomGroundTexture(), spawnpoint.transform.position, Quaternion.Euler(90, 0, 0)).transform.SetParent(groundParentObj.transform);

            Destroy(spawnpoint);
        }
    }

    void LoadRoadTexture()
    {
        foreach (GameObject spawnpoint in groundTextureSpawn)
        {
            Instantiate(prefabCollection.GetRandomGroundTexture(), spawnpoint.transform.position, Quaternion.Euler(90, 0, 0)).transform.SetParent(groundParentObj.transform);

            Destroy(spawnpoint);
        }
    }

    void LoadNoVegTexture()
    {
        foreach(GameObject spawnpoint in groundTextureSpawn)
        {
            Instantiate(prefabCollection.groundTexture[1], spawnpoint.transform.position, Quaternion.Euler(90, 0, 0)).transform.SetParent(groundParentObj.transform);

            Destroy(spawnpoint);
        }

    }

    void LoadMediumVegTexture()
    {
        foreach (GameObject spawnpoint in groundTextureSpawn)
        {
            Instantiate(prefabCollection.groundTexture[2], spawnpoint.transform.position, Quaternion.Euler(90, 0, 0)).transform.SetParent(groundParentObj.transform);

            Destroy(spawnpoint);
        }
    }

    void LoadHighVegTexture()
    {
        foreach (GameObject spawnpoint in groundTextureSpawn)
        {
            Instantiate(prefabCollection.groundTexture[3], spawnpoint.transform.position, Quaternion.Euler(90, 0, 0)).transform.SetParent(groundParentObj.transform);

            Destroy(spawnpoint);
        }
    }

    #endregion

    /// <summary>
    /// Border / Fence Section
    /// </summary>

    #region Select Fences for Border at Random
    private void LoadTopFence()
    {
        for (int i = 0; i < topFencePF.Length; i++)
        {
            Instantiate(prefabCollection.GetRandomHFencePFPF(), topFencePF[i].transform.position, Quaternion.identity).transform.SetParent(envParentObj.transform);
            Destroy(topFencePF[i]);
        }
    }
    private void LoadBotFence()
    {
        for (int i = 0; i < botFencePF.Length; i++)
        {
            Instantiate(prefabCollection.GetRandomHFencePFPF(), botFencePF[i].transform.position, Quaternion.identity).transform.SetParent(envParentObj.transform);
            Destroy(botFencePF[i]);
        }
    }
    private void LoadLeftFence()
    {
        for (int i = 0; i < botFencePF.Length; i++)
        {
            Instantiate(prefabCollection.GetRandomVFencePFPF(), leftFencePF[i].transform.position, Quaternion.Euler(0, 90, 0)).transform.SetParent(envParentObj.transform);
            Destroy(leftFencePF[i]);
        }
    }
    private void LoadRightFence()
    {
        for (int i = 0; i < botFencePF.Length; i++)
        {
            Instantiate(prefabCollection.GetRandomVFencePFPF(), rightFencePF[i].transform.position, Quaternion.Euler(0,90,0)).transform.SetParent(envParentObj.transform);
            Destroy(rightFencePF[i]);
        }
    }

    #endregion

    #region EntitieSpawn


    private void LoadEnemies(int chance)
    {
        GameObject parentObj = GameObject.Find("MobParent");

        for (int i = 0; i < entitieSpawn.Length; i++)
        {
            if (Random.Range(0, chance) == 1)
            {
                Instantiate(prefabCollection.GetRandomEnemie(), entitieSpawn[i].transform.position,
                    Quaternion.identity).transform.SetParent(parentObj.transform);
            }
            Destroy(entitieSpawn[i]);
        }
    }
    #endregion

    private void LoadInteractable()
    {
        for (int i = 0; i < interactableCollection.Length; i++)
        {
            if(Random.Range(0, 15) == 1)
            {
                Instantiate(prefabCollection.GetRandomInteractable(), interactableCollection[i].transform.position,
                    Quaternion.identity).transform.SetParent(envParentObj.transform);
            }
        }
    }
}
