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
    public GameObject[] entitieSpawn; // Public for MapGenHandler access
    [SerializeField] private GameObject[] interactableCollection;

    [Header("Fence Gap Settings")]
    [SerializeField] [Range(0f, 100f)] private float fenceGapChance = 40f; // Chance for a gap to start
    [SerializeField] [Range(1, 5)] private int minGapSize = 2; // Minimum consecutive fence tiles to skip
    [SerializeField] [Range(1, 5)] private int maxGapSize = 3; // Maximum consecutive fence tiles to skip

    private PrefabCollection prefabCollection;

    [HideInInspector]
    public GameObject envParentObj;
    [HideInInspector]
    public GameObject groundParentObj;
    [HideInInspector]
    public GameObject mobParentObj;
    [HideInInspector]
    public bool shouldSpawnEnemies = true; // Controlled by MapGenHandler


    //Stuff for Saving
    private int[] allSpawnPoints;


    void Awake()
    {
        prefabCollection = PrefabCollection.instance;
        
        // These will be set by MapGenHandler.LoadPrefabs()
        if (envParentObj == null)
            envParentObj = MapGenHandler.instance.envParentObj;
        if (groundParentObj == null)
            groundParentObj = MapGenHandler.instance.groundParentObj;
        if (mobParentObj == null)
            mobParentObj = MapGenHandler.instance.mobParentObj;

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
            case FieldType.PreBuildTile:
                LoadPreBuildTile();
                break;
            case FieldType.RoadVertical:
                LoadRoadVerticalField();
                break;
            case FieldType.RoadHorizontal:
                LoadRoadHorizontalField();
                break;
            case FieldType.RoadTJunctionTop:
                LoadRoadTJunctionTopField();
                break;
            case FieldType.RoadTJunctionBot:
                LoadRoadTJunctionBotField();
                break;
            case FieldType.RoadTJunctionLeft:
                LoadRoadTJunctionLeftField();
                break;
            case FieldType.RoadTJunctionRight:
                LoadRoadTJunctionRightField();
                break;
            case FieldType.RoadCrossroad:
                LoadRoadCrossroadField();
                break;
            case FieldType.RoadCurveTopLeft:
                LoadRoadCurveTopLeftField();
                break;
            case FieldType.RoadCurveTopRight:
                LoadRoadCurveTopRightField();
                break;
            case FieldType.RoadCurveBottomLeft:
                LoadRoadCurveBottomLeftField();
                break;
            case FieldType.RoadCurveBottomRight:
                LoadRoadCurveBottomRightField();
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
        
        LoadEnemies(20); // Re-enabled: Standard enemy spawning on roads
    }
    
    public void LoadNoVegField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();

        //LoadRandomGroundTexture();

        LoadEnemies(3); // Re-enabled: Standard enemy spawning

        // Totem/Altar spawning is now handled centrally by MapGenHandler after all fields are loaded
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

    private void LoadPreBuildTile()
    {
       //Currently, the PreBuild Tiles are generated in the MapGenHandler, since only the MapGenHandler knows the transform order of the FieldPositions,
       //which is needed in reference for the PreBuildTile to know where to be placed.
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
        LoadLowCenterVeg();

        LoadLeftFence();
        LoadRightFence();

        exitPos[3].gameObject.SetActive(true);
        //Start from BotSide
    }
    public void LoadOutsideExitBotField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowCenterVeg();

        LoadLeftFence();
        LoadRightFence();

        exitPos[1].gameObject.SetActive(true);
        //Start from TopSide
    }
    public void LoadOutsideExitLeftField()
    {
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();

        LoadTopFence();
        LoadBotFence();

        exitPos[2].gameObject.SetActive(true);
        //Start from RightSide
    }
    public void LoadOutsideExitRightField()
    {
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();

        //if Statement, um zu prüfen, von wo der Charakter kam
        //if(Charakter kam von Rechts)
        //LoadCharakter();

        LoadTopFence();
        LoadBotFence();

        exitPos[0].gameObject.SetActive(true);
        //Start from LeftSide
    }

    // New Directional Road Methods with Fences
    public void LoadRoadVerticalField()
    {
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowCenterVeg();
        LoadLeftFenceWithGaps();
        LoadRightFenceWithGaps();
    }

    public void LoadRoadHorizontalField()
    {
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();
        LoadTopFenceWithGaps();
        LoadBotFenceWithGaps();
    }

    public void LoadRoadTJunctionTopField()
    {
        // T-junction with opening at top (roads go left, right, bottom)
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();
        LoadTopFenceWithGaps();
    }

    public void LoadRoadTJunctionBotField()
    {
        // T-junction with opening at bottom (roads go left, right, top)
        LoadLowTopVeg();
        LoadLowLeftVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();
        LoadBotFenceWithGaps();
    }

    public void LoadRoadTJunctionLeftField()
    {
        // T-junction with opening at left (roads go top, bottom, right)
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();
        LoadLeftFenceWithGaps();
    }

    public void LoadRoadTJunctionRightField()
    {
        // T-junction with opening at right (roads go top, bottom, left)
        LoadLowTopVeg();
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowCenterVeg();
        LoadRightFenceWithGaps();
    }

    public void LoadRoadCrossroadField()
    {
        // Crossroad - only center vegetation, no fences
        LoadLowCenterVeg();
    }

    public void LoadRoadCurveTopLeftField()
    {
        // Curve connecting top and left (roads go up and left)
        // Fences on right and bottom
        LoadLowTopVeg();
        LoadLowLeftVeg();
        LoadLowCenterVeg();
        LoadRightFenceWithGaps();
        LoadBotFenceWithGaps();
    }

    public void LoadRoadCurveTopRightField()
    {
        // Curve connecting top and right (roads go up and right)
        // Fences on left and bottom
        LoadLowTopVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();
        LoadLeftFenceWithGaps();
        LoadBotFenceWithGaps();
    }

    public void LoadRoadCurveBottomLeftField()
    {
        // Curve connecting bottom and left (roads go down and left)
        // Fences on right and top
        LoadLowBotVeg();
        LoadLowLeftVeg();
        LoadLowCenterVeg();
        LoadRightFenceWithGaps();
        LoadTopFenceWithGaps();
    }

    public void LoadRoadCurveBottomRightField()
    {
        // Curve connecting bottom and right (roads go down and right)
        // Fences on left and top
        LoadLowBotVeg();
        LoadLowRightVeg();
        LoadLowCenterVeg();
        LoadLeftFenceWithGaps();
        LoadTopFenceWithGaps();
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
            if (Random.Range(0, 4) == 1)
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
            if (Random.Range(0, 4) == 1)
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
            if (Random.Range(0, 4) == 1)
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
            if (Random.Range(0, 4) == 1)
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
            if (Random.Range(0, 4) == 1)
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
    // Legacy methods for Outside fields (no gaps)
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
        for (int i = 0; i < leftFencePF.Length; i++)
        {
            Instantiate(prefabCollection.GetRandomVFencePFPF(), leftFencePF[i].transform.position, Quaternion.Euler(0, 90, 0)).transform.SetParent(envParentObj.transform);
            Destroy(leftFencePF[i]);
        }
    }
    private void LoadRightFence()
    {
        for (int i = 0; i < rightFencePF.Length; i++)
        {
            Instantiate(prefabCollection.GetRandomVFencePFPF(), rightFencePF[i].transform.position, Quaternion.Euler(0,90,0)).transform.SetParent(envParentObj.transform);
            Destroy(rightFencePF[i]);
        }
    }

    // New methods with gap logic for road fences
    private void LoadTopFenceWithGaps()
    {
        LoadFenceWithGaps(topFencePF, prefabCollection.GetRandomHFencePFPF, Quaternion.identity);
    }
    
    private void LoadBotFenceWithGaps()
    {
        LoadFenceWithGaps(botFencePF, prefabCollection.GetRandomHFencePFPF, Quaternion.identity);
    }
    
    private void LoadLeftFenceWithGaps()
    {
        LoadFenceWithGaps(leftFencePF, prefabCollection.GetRandomVFencePFPF, Quaternion.Euler(0, 90, 0));
    }
    
    private void LoadRightFenceWithGaps()
    {
        LoadFenceWithGaps(rightFencePF, prefabCollection.GetRandomVFencePFPF, Quaternion.Euler(0, 90, 0));
    }

    /// <summary>
    /// Core fence spawning logic with gaps
    /// </summary>
    private void LoadFenceWithGaps(GameObject[] spawnPoints, System.Func<GameObject> getPrefabFunc, Quaternion rotation)
    {
        int i = 0;
        while (i < spawnPoints.Length)
        {
            // Check if we should create a gap
            if (Random.Range(0f, 100f) < fenceGapChance)
            {
                // Create a gap of random size
                int gapSize = Random.Range(minGapSize, maxGapSize + 1);
                
                // Skip 'gapSize' fence positions
                for (int j = 0; j < gapSize && i < spawnPoints.Length; j++)
                {
                    Destroy(spawnPoints[i]);
                    i++;
                }
            }
            else
            {
                // Spawn fence at this position
                if (i < spawnPoints.Length)
                {
                    Instantiate(getPrefabFunc(), spawnPoints[i].transform.position, rotation).transform.SetParent(envParentObj.transform);
                    Destroy(spawnPoints[i]);
                    i++;
                }
            }
        }
    }

    #endregion

    #region EntitieSpawn

    /// <summary>
    /// PUBLIC method to spawn enemies only (called by MapGenHandler after NavMesh baking)
    /// </summary>
    public void SpawnEnemiesOnly(FieldType fieldType)
    {
        shouldSpawnEnemies = true;
        
        switch (fieldType)
        {
            case FieldType.Road:
            case FieldType.RoadVertical:
            case FieldType.RoadHorizontal:
            case FieldType.RoadTJunctionTop:
            case FieldType.RoadTJunctionBot:
            case FieldType.RoadTJunctionLeft:
            case FieldType.RoadTJunctionRight:
            case FieldType.RoadCrossroad:
            case FieldType.RoadCurveTopLeft:
            case FieldType.RoadCurveTopRight:
            case FieldType.RoadCurveBottomLeft:
            case FieldType.RoadCurveBottomRight:
                LoadEnemies(20);
                break;
                
            case FieldType.NoVeg:
                LoadEnemies(3);
                break;
        }
    }

    /// <summary>
    /// Spawns enemy groups instead of individual enemies
    /// Called by MapGenHandler for pack-based spawning
    /// </summary>
    public void SpawnEnemyGroups(int groupCount, FieldType fieldType)
    {
        if (!shouldSpawnEnemies)
        {
            return;
        }
        
        if (entitieSpawn == null || entitieSpawn.Length == 0)
        {
            Debug.LogWarning("[OutsideVegLoader] No entity spawn points available for group spawning");
            return;
        }
        
        // Collect valid spawn positions
        List<GameObject> availableSpawnPoints = new List<GameObject>();
        foreach (GameObject spawnPoint in entitieSpawn)
        {
            if (spawnPoint != null)
            {
                availableSpawnPoints.Add(spawnPoint);
            }
        }
        
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogWarning("[OutsideVegLoader] No valid spawn points found");
            return;
        }
        
        // Ensure groups spawn at least 20 units apart (2 tiles)
        List<Vector3> usedPositions = new List<Vector3>();
        float minDistanceBetweenGroups = 20f;
        
        int groupsSpawned = 0;
        int attempts = 0;
        int maxAttempts = groupCount * 3; // Allow multiple attempts to find valid positions
        
        while (groupsSpawned < groupCount && attempts < maxAttempts)
        {
            attempts++;
            
            // Pick random spawn point
            GameObject spawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
            Vector3 spawnPos = spawnPoint.transform.position;
            
            // Check distance to other groups
            bool tooClose = false;
            foreach (Vector3 usedPos in usedPositions)
            {
                if (Vector3.Distance(spawnPos, usedPos) < minDistanceBetweenGroups)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (tooClose)
            {
                continue; // Try another position
            }
            
            // Create enemy group GameObject
            GameObject groupObj = new GameObject($"EnemyGroup_{fieldType}_{groupsSpawned}");
            groupObj.transform.position = spawnPos;
            groupObj.transform.SetParent(mobParentObj.transform);
            
            EnemyGroup group = groupObj.AddComponent<EnemyGroup>();
            group.isPatrol = false; // Packs are stationary
            
            // Random group size: 3-5 enemies
            int enemyCount = Random.Range(3, 6);
            
            // Spawn the group
            group.SpawnGroup(spawnPos, enemyCount, prefabCollection, mobParentObj.transform);
            
            // Mark position as used
            usedPositions.Add(spawnPos);
            groupsSpawned++;
            
            Debug.Log($"[OutsideVegLoader] Spawned EnemyGroup {groupsSpawned}/{groupCount} with {enemyCount} members at {spawnPos}");
        }
        
        if (groupsSpawned < groupCount)
        {
            Debug.LogWarning($"[OutsideVegLoader] Only spawned {groupsSpawned}/{groupCount} groups due to spacing constraints");
        }
        
        // Cleanup spawn point markers
        foreach (GameObject spawnPoint in entitieSpawn)
        {
            if (spawnPoint != null)
            {
                Destroy(spawnPoint);
            }
        }
    }

    private void LoadEnemies(int chance)
    {
        // Skip enemy spawning if disabled (for NavMesh setup phase)
        if (!shouldSpawnEnemies)
        {
            return; // Silent skip during first phase
        }

        for (int i = 0; i < entitieSpawn.Length; i++)
        {
            if (Random.Range(0, chance) == 1)
            {
                GameObject enemyPrefab = prefabCollection.GetRandomEnemie();
                GameObject mob = Instantiate(enemyPrefab, entitieSpawn[i].transform.position, Quaternion.identity);
                mob.name = enemyPrefab.name;
                
                // Use mobParentObj if set, otherwise fallback to MapGenHandler
                Transform parentTransform = mobParentObj != null ? mobParentObj.transform : MapGenHandler.instance.mobParentObj.transform;
                mob.transform.SetParent(parentTransform);

                // Entferne "(Clone)" aus dem Namen
                if (mob.name.Contains("(Clone)"))
                    mob.name = mob.name.Replace("(Clone)", "").Trim();
            }
            Destroy(entitieSpawn[i]);
        }
    }
    #endregion

    /// <summary>
    /// Spawns exactly 1 Totem and 1 Altar on the map after all fields are loaded.
    /// Called by MapGenHandler after map generation is complete.
    /// FALLBACK: If < 2 NoVeg fields exist, converts High/Medium Veg fields to NoVeg.
    /// </summary>
    public void SpawnTotemAndAltar()
    {
        GameObject[] allFields = MapGenHandler.instance.fieldPosSave;
        List<GameObject> noVegFields = new List<GameObject>();
        
        // Collect all NoVeg fields
        foreach (GameObject field in allFields)
        {
            FieldPos fieldPos = field.GetComponent<FieldPos>();
            if (fieldPos != null && fieldPos.Type == FieldType.NoVeg)
            {
                noVegFields.Add(field);
            }
        }
        
        Debug.Log($"[OutsideVegLoader] SpawnTotemAndAltar: Found {noVegFields.Count} NoVeg fields");
        
        // FALLBACK: If not enough NoVeg fields, convert High/Medium Veg to NoVeg
        if (noVegFields.Count < 2)
        {
            Debug.LogWarning($"[OutsideVegLoader] ⚠️ Only {noVegFields.Count} NoVeg fields! Converting High/Medium Veg fields...");
            
            List<GameObject> vegFields = new List<GameObject>();
            
            // Collect High and Medium Veg fields (prefer HighVeg first)
            foreach (GameObject field in allFields)
            {
                FieldPos fieldPos = field.GetComponent<FieldPos>();
                if (fieldPos != null && (fieldPos.Type == FieldType.HighVeg || fieldPos.Type == FieldType.LowVeg))
                {
                    vegFields.Add(field);
                }
            }
            
            if (vegFields.Count == 0)
            {
                Debug.LogError("[OutsideVegLoader] ❌ CRITICAL: No High/Medium Veg fields available for conversion! Cannot spawn Totem/Altar!");
                return;
            }
            
            // Calculate how many fields we need to convert
            int fieldsNeeded = 2 - noVegFields.Count;
            int fieldsToConvert = Mathf.Min(fieldsNeeded, vegFields.Count);
            
            Debug.Log($"[OutsideVegLoader] Converting {fieldsToConvert} Veg fields to NoVeg...");
            
            // Shuffle and convert fields
            for (int i = 0; i < fieldsToConvert; i++)
            {
                int randomIndex = Random.Range(0, vegFields.Count);
                GameObject fieldToConvert = vegFields[randomIndex];
                FieldPos fieldPos = fieldToConvert.GetComponent<FieldPos>();
                
                FieldType oldType = fieldPos.Type;
                fieldPos.Type = FieldType.NoVeg;
                noVegFields.Add(fieldToConvert);
                vegFields.RemoveAt(randomIndex);
                
                Debug.Log($"[OutsideVegLoader] ✓ Converted field at {fieldToConvert.transform.position} from {oldType} to NoVeg");
                
                // Reload this field's prefabs to reflect NoVeg type
                OutsideVegLoader loader = fieldToConvert.GetComponent<OutsideVegLoader>();
                if (loader != null)
                {
                    // Clear existing vegetation
                    foreach (Transform child in loader.envParentObj.transform)
                    {
                        if (child.gameObject != fieldToConvert)
                        {
                            Destroy(child.gameObject);
                        }
                    }
                    
                    // Reload as NoVeg
                    loader.LoadFieldType(FieldType.NoVeg);
                }
            }
            
            Debug.Log($"[OutsideVegLoader] ✓ Conversion complete! Now have {noVegFields.Count} NoVeg fields");
        }
        
        // Verify we now have enough fields
        if (noVegFields.Count < 2)
        {
            Debug.LogError($"[OutsideVegLoader] ❌ CRITICAL: Still only {noVegFields.Count} NoVeg fields after conversion! Cannot spawn Totem/Altar!");
            return;
        }
        
        // Spawn Totem at random NoVeg field
        GameObject totemField = noVegFields[Random.Range(0, noVegFields.Count)];
        GameObject totemPrefab = prefabCollection.GetTotemPrefab();
        
        if (totemPrefab == null)
        {
            Debug.LogError("[OutsideVegLoader] No Totem prefab found in PrefabCollection!");
            return;
        }
        
        Vector3 totemPos = totemField.transform.position;
        GameObject spawnedTotem = Instantiate(totemPrefab, totemPos, Quaternion.identity);
        spawnedTotem.transform.SetParent(envParentObj.transform);
        
        Debug.Log($"[OutsideVegLoader] Totem spawned at {totemPos}");
        
        // Find valid Altar position (min. 20 units away)
        GameObject altarField = null;
        int attempts = 0;
        int maxAttempts = 50;
        
        while (altarField == null && attempts < maxAttempts)
        {
            GameObject candidate = noVegFields[Random.Range(0, noVegFields.Count)];
            
            if (candidate != totemField)
            {
                float distance = Vector3.Distance(totemPos, candidate.transform.position);
                if (distance >= 20f)
                {
                    altarField = candidate;
                    break;
                }
            }
            
            attempts++;
        }
        
        // Spawn Altar
        if (altarField != null)
        {
            GameObject altarPrefab = prefabCollection.GetAltarPrefab();
            
            if (altarPrefab == null)
            {
                Debug.LogError("[OutsideVegLoader] No Altar prefab found in PrefabCollection!");
                return;
            }
            
            Vector3 altarPos = altarField.transform.position;
            GameObject spawnedAltar = Instantiate(altarPrefab, altarPos, Quaternion.identity);
            spawnedAltar.transform.SetParent(envParentObj.transform);
            
            float finalDistance = Vector3.Distance(totemPos, altarPos);
            Debug.Log($"[OutsideVegLoader] Altar spawned at {altarPos} (distance: {finalDistance:F1} from Totem)");
            
            // Link Totem and Altar
            TotemInteractable totem = spawnedTotem.GetComponent<TotemInteractable>();
            AltarInteractable altar = spawnedAltar.GetComponent<AltarInteractable>();
            
            if (totem != null && altar != null)
            {
                totem.SetLinkedAltar(altar);
                Debug.Log("[OutsideVegLoader] Totem and Altar successfully linked!");
            }
            else
            {
                Debug.LogError("[OutsideVegLoader] Failed to link Totem and Altar - scripts not found!");
            }
        }
        else
        {
            Debug.LogWarning($"[OutsideVegLoader] Could not find valid Altar position (min 20 units) after {attempts} attempts!");
        }
    }

    #region Deprecated Methods
    // OLD METHOD - No longer used, kept for reference
    private void LoadInteractable()
    {
        // This method is deprecated and replaced by SpawnTotemAndAltar()
        // which is called once after all fields are loaded
    }
    #endregion
}
