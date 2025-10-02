using UnityEngine.AI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

    public GameObject envParentObj;
    public GameObject groundParentObj;
    public GameObject mobParentObj;

    public GameObject[] fieldPosSave { get { return fieldsPosObj; } private set { fieldsPosObj = value; } }

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
                if (data.currentMap == null)
                {
                    SpawnPoint spawnToUse = (data.lastSpawnpoint != default) ? data.lastSpawnpoint : SpawnPoint.SpawnRight;
                    CreateNewMap(spawnToUse);
                }
                else
                {
                    LoadExistingMap(data.currentMap, data.lastSpawnpoint);
                }
                
                PlayerPrefs.DeleteKey("Load");
                
                if (UI_GlobalMap.instance != null)
                {
                    UI_GlobalMap.instance.CalculateExploredMaps();
                }
                
                if (LogScript.instance != null)
                {
                    LogScript.instance.ShowLog("Game loaded (from menu or tutorial)");
                }
                
                playerLoad.LoadPlayer(data);
                StartCoroutine(LoadTalentsAfterTreeGeneration(playerLoad, data));
            }
        }
        else
        {
            Debug.Log("[MapGenHandler.Start] === NEUES SPIEL PFAD ===");
            PlayerSave data = SaveSystem.NewSave();
            CreateNewMap(SpawnPoint.SpawnRight);
            
            if (LogScript.instance != null)
            {
                LogScript.instance.ShowLog("New map created (no load flag)");
            }
            
            playerLoad.LoadPlayer(data);
        }
        
        // NEU: Generiere Nachbar-Maps nach der Initialisierung
        StartCoroutine(GenerateNeighborMapsDelayed());
        
        Debug.Log("=== [MapGenHandler.Start] ENDE ===");
    }

    /// <summary>
    /// Erstellt eine neue Map von Grund auf
    /// </summary>
    public void CreateNewMap(SpawnPoint playerSpawn)
    {
        Debug.Log($"=== [CreateNewMap] START mit SpawnPoint: {playerSpawn} ===");
        
        // Setup
        PrefabCollection.instance.PopulatePrefabCollection();
        
        // Generiere Layout
        int[][] layout = MapLayoutGenerator.GenerateLayout();
        
        // Weise FieldTypes zu
        AssignFieldTypes(layout);
        
        // Lade Prefabs
        LoadPrefabs();
        
        // Setze Player Spawn NACH dem Laden der Prefabs
        StartCoroutine(SetPlayerSpawnDelayed(playerSpawn));
        
        // NavMesh und GlobalMap
        navMeshSurface.BuildNavMesh();
        GlobalMap.instance.CreateAndSaveNewMap();
        
        Debug.Log("=== [CreateNewMap] ENDE ===");
    }

    /// <summary>
    /// Lädt eine existierende Map
    /// </summary>
    public void LoadExistingMap(MapSave map, SpawnPoint spawnPoint)
    {
        Debug.Log($"=== [LoadExistingMap] START mit Map: {map?.ToString() ?? "null"}, SpawnPoint: {spawnPoint} ===");
        
        if(map != null)
        {
            Debug.Log($"[LoadExistingMap] Map gefunden - Theme: {map.mapTheme}");
            
            PrefabCollection.instance.PopulatePrefabCollection(map);

            // Weise gespeicherte FieldTypes zu
            for (int i = 0; i < 81; i++)
            {
                fieldsPosObj[i].GetComponent<FieldPos>().Type = map.fieldType[i];
            }

            GlobalMap.instance.Set_CurrentMap(map);
            LoadPrefabs();
            
            // Setze Player Spawn NACH dem Laden der Prefabs
            StartCoroutine(SetPlayerSpawnDelayed(spawnPoint));
            
            navMeshSurface.BuildNavMesh();

            Debug.Log("[LoadExistingMap] Map erfolgreich aus Save Data geladen");
        }
        else
        {
            Debug.Log("[LoadExistingMap] Map ist null - erstelle neue Map");
            CreateNewMap(spawnPoint);
        }
        
        Debug.Log("=== [LoadExistingMap] ENDE ===");
    }

    /// <summary>
    /// Verzögertes Spawning um sicherzustellen, dass alle Prefabs geladen sind
    /// </summary>
    private IEnumerator SetPlayerSpawnDelayed(SpawnPoint spawnPoint)
    {
        yield return new WaitForEndOfFrame(); 
        yield return new WaitForEndOfFrame(); 
        
        // Debug: Logge alle Exit-Fields vor dem Spawning
        MapSpawnHandler.DebugLogAllExitFields(fieldsPosObj);
        
        MapSpawnHandler.SetPlayerSpawn(spawnPoint, fieldsPosObj);
    }

    /// <summary>
    /// NEU: Generiert nur das Layout einer Map ohne GameObjects zu erstellen
    /// </summary>
    public static MapSave GenerateMapDataOnly(int mapX, int mapY)
    {
        Debug.Log($"[MapGenHandler] Generiere Map-Daten für Position ({mapX}, {mapY})");
        
        MapSave newMap = new MapSave();
        newMap.mapIndexX = mapX;
        newMap.mapIndexY = mapY;
        
        // Berechne Map-Level basierend auf Entfernung vom Zentrum
        newMap.mapLevel = Mathf.Abs(mapX) > Mathf.Abs(mapY) ? Mathf.Abs(mapX) : Mathf.Abs(mapY);
        
        // Wähle zufälliges Theme
        newMap.mapTheme = (WorldType)Random.Range(0, System.Enum.GetValues(typeof(WorldType)).Length);
        
        // Generiere Layout ohne GameObjects
        int[][] layout = MapLayoutGenerator.GenerateLayout();
        
        // Konvertiere zu FieldType Array
        for (int i = 0; i < 81; i++)
        {
            int x = i % 9;
            int z = i / 9;
            newMap.fieldType[i] = (FieldType)layout[x][z];
        }
        
        Debug.Log($"[MapGenHandler] Map-Daten generiert für ({mapX}, {mapY}) - Theme: {newMap.mapTheme}, Level: {newMap.mapLevel}");
        return newMap;
    }

    private void AssignFieldTypes(int[][] layout)
    {
        for (int i = 0; i < 81; i++)
        {
            var fieldPos = fieldsPosObj[i].GetComponent<FieldPos>();
            int x = fieldPos.ArrayPosX;
            int z = fieldPos.ArrayPosZ;
            fieldPos.Type = (FieldType)layout[z][x];
        }
    }

    private void LoadPrefabs()
    {
        for (int i = 0; i < fieldsPosObj.Length; i++)
        {
            var fieldObject = Instantiate(fieldPF, fieldsPosObj[i].transform.position, Quaternion.identity);
            var fieldPos = fieldsPosObj[i].GetComponent<FieldPos>();

            // Handle PreBuildTiles - MIT NULL-CHECK
            if (Random.Range(0, 3) == 1 && fieldPos.Type == FieldType.LowVeg)
            {
                // ✅ Prüfe erst auf null, dann auf Length
                if (PrefabCollection.instance.preBuildTiles != null && 
                    PrefabCollection.instance.preBuildTiles.Length > 0)
                {
                    var preBuildTile = PrefabCollection.instance.GetRandomPreBuildTile();
                    if (preBuildTile != null) // Zusätzlicher Schutz
                    {
                        Instantiate(preBuildTile, 
                                fieldsPosObj[i].transform.position, 
                                Quaternion.identity).transform.SetParent(envParentObj.transform);
                        fieldPos.Type = FieldType.PreBuildTile;
                    }
                }
            }
            
            // Lade Field Type
            var vegLoader = fieldObject.GetComponent<OutsideVegLoader>();
            if (vegLoader != null)
            {
                vegLoader.LoadFieldType(fieldPos.Type);
            }
        }
    }

    // Legacy method für Kompatibilität
    public void CreateANewMap(SpawnPoint playerSpawn)
    {
        CreateNewMap(playerSpawn);
    }

    // Legacy method für Kompatibilität  
    public void LoadMap(MapSave map, SpawnPoint spawnPoint)
    {
        LoadExistingMap(map, spawnPoint);
    }

    // Restliche Methoden bleiben unverändert...
    private IEnumerator GenerateNeighborMapsDelayed()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log("[MapGenHandler] Starte Generierung von Nachbar-Maps");
        
        if (GlobalMap.instance != null)
        {
            GlobalMap.instance.GenerateNeighborMaps(1);
        }
        
        if (UI_GlobalMap.instance != null)
        {
            UI_GlobalMap.instance.CalculateExploredMaps();
        }
        
        Debug.Log("[MapGenHandler] Nachbar-Maps-Generierung abgeschlossen");
    }

    private IEnumerator LoadTalentsAfterTreeGeneration(PlayerLoad playerLoad, PlayerSave data)
    {
        Debug.Log("[LoadTalentsAfterTreeGeneration] Warte auf TalentTree-Generierung...");
        
        while (TalentTreeGenerator.instance == null || 
               TalentTreeGenerator.instance.allNodes == null || 
               TalentTreeGenerator.instance.allNodes.Count == 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        yield return new WaitForEndOfFrame();
        
        Debug.Log("[LoadTalentsAfterTreeGeneration] TalentTree bereit - lade Talente");
        playerLoad.LoadTalentsDelayed(data);
    }

    public void ResetThisMap()
    {
        GameObject envParentObj = GameObject.Find("EnvParent");
        for (int i = 0; i < envParentObj.transform.childCount; i++)
        {
            Destroy(envParentObj.transform.GetChild(i).gameObject);
        }

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

        OutsideVegLoader[] outsideVegPrefabs = FindObjectsOfType<OutsideVegLoader>(); 
        foreach(OutsideVegLoader prefab in outsideVegPrefabs)
        {
            Destroy(prefab.gameObject);
        }
    }

    public void RebuildNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
}