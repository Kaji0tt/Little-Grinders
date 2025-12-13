using UnityEngine.AI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MapGenHandler : MonoBehaviour
{
    #region Singleton
    public static MapGenHandler instance;
    private void Awake()
    {
        instance = this;
    }
    #endregion

    // ✅ FIX: Prevent simultaneous map creation/loading
    private static bool isLoadingMap = false;

    public NavMeshSurface navMeshSurface;

    [SerializeField]
    private GameObject[] fieldsPosObj;
    [SerializeField]
    private GameObject fieldPF;

    public GameObject envParentObj;
    public GameObject groundParentObj;
    public GameObject mobParentObj;

    public GameObject[] fieldPosSave { get { return fieldsPosObj; } private set { fieldsPosObj = value; } }
    
    private GameObject enemyWaveSpawnerObj;

    void Start()
    {
        Debug.Log("=== [MapGenHandler.Start] START ===");
        
        PlayerLoad playerLoad = FindFirstObjectByType<PlayerLoad>();
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
        
        // PHASE 1: Lade Environment Prefabs OHNE Enemies
        Debug.Log("[MapGenHandler] === PHASE 1: Loading environment prefabs (no enemies) ===");
        LoadPrefabs(spawnEnemies: false);
        
        // PHASE 2: Configure NavMeshObstacles NACH Prefab-Loading
        Debug.Log("[MapGenHandler] === PHASE 2: Configuring NavMesh Obstacles ===");
        EnvCamNoRot[] envScripts = FindObjectsByType<EnvCamNoRot>(FindObjectsSortMode.None);
        int envScriptCount = envScripts.Length;
        foreach (EnvCamNoRot env in envScripts)
        {
            env.ConfigureObstaclesForProcedularMap();
        }
        Debug.Log($"[MapGenHandler] Configured obstacles for {envScriptCount} environment parent objects");
        
        // PHASE 3: Bake NavMesh MIT Obstacles (damit Carving funktioniert!)
        Debug.Log("[MapGenHandler] === PHASE 3: Baking NavMesh WITH obstacles ===");
        navMeshSurface.BuildNavMesh();
        Debug.Log("[MapGenHandler] NavMesh baked successfully. Vertices: " + NavMesh.CalculateTriangulation().vertices.Length);
        
        // PHASE 4: Spawn Enemies MIT NavMesh
        Debug.Log("[MapGenHandler] === PHASE 4: Spawning enemies WITH NavMesh ===");
        SpawnEnemiesAfterNavMesh();

        // ✅ NEU: PHASE 5: Generate Road Patrols
        Debug.Log("[MapGenHandler] === PHASE 5: Generating Road Patrols ===");
        GenerateRoadPatrols();
                
        // Spawn Totem und Altar NACH dem Laden aller Fields
        OutsideVegLoader outsideVegLoader = FindFirstObjectByType<OutsideVegLoader>();
        if (outsideVegLoader != null)
        {
            outsideVegLoader.SpawnTotemAndAltar();
        }
        else
        {
            Debug.LogError("[MapGenHandler] OutsideVegLoader not found! Cannot spawn Totem and Altar.");
        }
        
        // Setze Player Spawn NACH dem Laden der Prefabs
        StartCoroutine(SetPlayerSpawnDelayed(playerSpawn));
        
        // GlobalMap aktualisieren oder erstellen (verhindert Duplikate mit pre-generierten Maps)
        Debug.Log("[MapGenHandler.CreateNewMap] Calling UpdateOrCreateCurrentMap...");
        GlobalMap.instance.UpdateOrCreateCurrentMap();
        
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
            Debug.Log($"[LoadExistingMap] === Map Data === Position: ({map.mapIndexX}, {map.mapIndexY})");
            Debug.Log($"[LoadExistingMap]   Theme: {map.mapTheme}, Level: {map.mapLevel}");
            Debug.Log($"[LoadExistingMap]   isCleared: {map.isCleared}, isVisited: {map.isVisited}");
            Debug.Log($"[LoadExistingMap]   fieldType array length: {map.fieldType?.Length ?? 0}");
            
            if (map.fieldType != null && map.fieldType.Length >= 5)
            {
                Debug.Log($"[LoadExistingMap]   Sample fieldTypes: [0]={map.fieldType[0]}, [1]={map.fieldType[1]}, [40]={map.fieldType[40]}, [80]={map.fieldType[80]}");
            }
            
            PrefabCollection.instance.PopulatePrefabCollection(map);

            // Weise gespeicherte FieldTypes zu
            Debug.Log("[LoadExistingMap] Restoring FieldTypes to fieldsPosObj...");
            for (int i = 0; i < 81; i++)
            {
                FieldType restoredType = map.fieldType[i];
                fieldsPosObj[i].GetComponent<FieldPos>().Type = restoredType;
                
                // Debug: Erste paar Zuweisungen loggen
                if (i < 5)
                {
                    Debug.Log($"[LoadExistingMap] fieldsPosObj[{i}].Type = {restoredType}");
                }
            }
            Debug.Log("[LoadExistingMap] FieldTypes restored successfully");

            GlobalMap.instance.Set_CurrentMap(map);
            
            // PHASE 1: Load environment WITHOUT enemies
            Debug.Log("[MapGenHandler] === LOAD PHASE 1: Loading environment (no enemies) ===");
            LoadPrefabs(spawnEnemies: false);
            
            // PHASE 2: Configure NavMeshObstacles
            Debug.Log("[MapGenHandler] === LOAD PHASE 2: Configuring NavMesh Obstacles ===");
            EnvCamNoRot[] envScripts = FindObjectsByType<EnvCamNoRot>(FindObjectsSortMode.None);
            foreach (EnvCamNoRot env in envScripts)
            {
                env.ConfigureObstaclesForProcedularMap();
            }
            
            // PHASE 3: Bake NavMesh WITH obstacles
            Debug.Log("[MapGenHandler] === LOAD PHASE 3: Baking NavMesh WITH obstacles ===");
            navMeshSurface.BuildNavMesh();
            Debug.Log("[MapGenHandler] NavMesh baked successfully (LOAD). Vertices: " + NavMesh.CalculateTriangulation().vertices.Length);
            
            // PHASE 4: Spawn enemies WITH NavMesh
            Debug.Log("[MapGenHandler] === LOAD PHASE 4: Spawning enemies ===");
            SpawnEnemiesAfterNavMesh();

            // ✅ NEU: PHASE 5: Generate Road Patrols
            Debug.Log("[MapGenHandler] === PHASE 5: Generating Road Patrols ===");
            GenerateRoadPatrols();
            
            // PHASE 5: Spawn Totem und Altar (wenn noch nicht gecleared)
            Debug.Log("[MapGenHandler] === LOAD PHASE 6: Spawning Totem and Altar ===");
            OutsideVegLoader outsideVegLoader = FindFirstObjectByType<OutsideVegLoader>();
            if (outsideVegLoader != null && !map.isCleared)
            {
                outsideVegLoader.SpawnTotemAndAltar();
                Debug.Log("[MapGenHandler] Totem and Altar spawned for existing map");
            }
            else if (map.isCleared)
            {
                Debug.Log("[MapGenHandler] Map already cleared - skipping Totem/Altar spawn");
            }
            
            LoadPrefabs();
            
            // Setze Player Spawn NACH dem Laden der Prefabs
            StartCoroutine(SetPlayerSpawnDelayed(spawnPoint));

            // NEU: Generiere Nachbarn auch beim Laden existierender Maps
            StartCoroutine(GenerateNeighborMapsDelayed());
            
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
    /// NEU: Lädt oder erstellt eine Map für die aktuelle Position
    /// </summary>
    public void LoadOrCreateMapForCurrentPosition(SpawnPoint playerSpawn)
    {
        Debug.Log($"[MapBug][{Time.time:F2}s] LoadOrCreateMapForCurrentPosition START - Position: {GlobalMap.instance.currentPosition}, SpawnPoint: {playerSpawn}");
        
        // ✅ FIX: Prevent simultaneous map operations
        if (isLoadingMap)
        {
            Debug.LogWarning($"[MapBug][{Time.time:F2}s] Already loading a map - ABORTING to prevent duplicates!");
            return;
        }
        
        isLoadingMap = true;
        Debug.Log($"[MapBug][{Time.time:F2}s] Map loading lock acquired");
        
        // Prüfe ob an der aktuellen Position bereits eine Map existiert
        MapSave existingMap = GlobalMap.instance.GetMapByCords(GlobalMap.instance.currentPosition);
        
        if (existingMap != null)
        {
            Debug.Log($"[MapBug][{Time.time:F2}s] Existierende Map gefunden - lade sie");
            LoadExistingMap(existingMap, playerSpawn);
        }
        else
        {
            Debug.Log($"[MapBug][{Time.time:F2}s] Keine Map gefunden - erstelle neue");
            CreateNewMap(playerSpawn);
        }
        
        // ✅ Release lock after loading is complete
        isLoadingMap = false;
        Debug.Log($"[MapBug][{Time.time:F2}s] Map loading lock released");
        
        Debug.Log($"[MapBug][{Time.time:F2}s] LoadOrCreateMapForCurrentPosition END");
    }
    public static MapSave GenerateMapDataOnly(int mapX, int mapY)
    {
        Debug.Log($"[GenerateMapDataOnly] === START === Position: ({mapX}, {mapY})");
        
        // Berechne Map-Level basierend auf Entfernung vom Zentrum
        int mapLevel = Mathf.Abs(mapX) > Mathf.Abs(mapY) ? Mathf.Abs(mapX) : Mathf.Abs(mapY);
        
        Debug.Log($"[GenerateMapDataOnly] MapLevel calculation: |{mapX}| = {Mathf.Abs(mapX)}, |{mapY}| = {Mathf.Abs(mapY)} → MapLevel = {mapLevel}");
        
        // NEU: Verwende spezifischen Seed für konsistente Generierung
        int mapSeed = GetMapSeed(mapX, mapY);
        Random.State previousState = Random.state;
        Random.InitState(mapSeed);
        Debug.Log($"[GenerateMapDataOnly] Using seed: {mapSeed}");
        
        // Wähle Theme mit Seed
        WorldType theme = (WorldType)Random.Range(0, System.Enum.GetValues(typeof(WorldType)).Length);
        Debug.Log($"[GenerateMapDataOnly] Theme selected: {theme}");
        
        // Generiere Layout mit festem Seed
        int[][] layout = MapLayoutGenerator.GenerateLayout();
        Debug.Log($"[GenerateMapDataOnly] Layout generated: {layout.Length}x{layout[0].Length}");
        
        // ✅ KRITISCH: Konvertiere zu FieldType Array - ORIGINALE Indizierung beibehalten!
        // KI Hinweis: Die Indexierung ist mit Absicht invertiert [x][z]. Nicht überarbeiten bitte.
        FieldType[] fieldTypes = new FieldType[81];
        for (int i = 0; i < 81; i++)
        {
            int x = i % 9;
            int z = i / 9;
            // WICHTIG: Originale Indizierung [x][z] - funktioniert mit MapLayoutGenerator
            fieldTypes[i] = (FieldType)layout[x][z];
            
            // Debug: Erste paar Fields loggen
            if (i < 5)
            {
                Debug.Log($"[GenerateMapDataOnly] fieldTypes[{i}] (x={x}, z={z}): layout[{x}][{z}] = {fieldTypes[i]}");
            }
        }
        
        // Stelle vorherigen Random-State wieder her
        Random.state = previousState;
        
        // ✅ Verwende speziellen Konstruktor der FieldTypes korrekt kopiert
        MapSave newMap = new MapSave(mapX, mapY, mapLevel, theme, fieldTypes);
        
        Debug.Log($"[GenerateMapDataOnly] === ENDE === Map ({mapX}, {mapY}): Theme={newMap.mapTheme}, Level={newMap.mapLevel}, Seed={mapSeed}, fieldType[0]={newMap.fieldType[0]}, fieldType[40]={newMap.fieldType[40]}");
        return newMap;
    }
    
    /// <summary>
    /// NEU: Generiert einen konsistenten Seed basierend auf Map-Koordinaten
    /// </summary>
    private static int GetMapSeed(int mapX, int mapY)
    {
        // Kombiniere X,Y zu einem einzigartigen Seed
        return (mapX * 1000) + mapY + 12345; // Offset für Variation
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

    /// <summary>
    /// Lädt alle Prefabs für die aktuelle Map basierend auf FieldTypes
    /// </summary>
    /// <param name="spawnEnemies">If false, skips enemy spawning (for NavMesh setup)</param>
    private void LoadPrefabs(bool spawnEnemies = true)
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
                vegLoader.envParentObj = envParentObj;
                vegLoader.groundParentObj = groundParentObj;
                vegLoader.mobParentObj = mobParentObj;
                vegLoader.shouldSpawnEnemies = spawnEnemies; // Control enemy spawning
                vegLoader.LoadFieldType(fieldPos.Type);
            }
        }
    }
    

    
    /// <summary>
    /// Spawns enemies AFTER NavMesh is baked. Called in Phase 4 of CreateNewMap().
    /// </summary>
    private void SpawnEnemiesAfterNavMesh()
    {
        Debug.Log("[MapGenHandler] Spawning enemies with NavMesh support...");
        
        // Check if map is cleared - skip spawning if it is
        if (GlobalMap.instance != null && GlobalMap.instance.currentMap != null && GlobalMap.instance.currentMap.isCleared)
        {
            Debug.Log("[MapGenHandler] Map is cleared - skipping enemy spawns");
            return;
        }
        
        // Collect ALL NoVeg fields first
        List<OutsideVegLoader> noVegLoaders = new List<OutsideVegLoader>();
        OutsideVegLoader[] vegLoaders = FindObjectsByType<OutsideVegLoader>(FindObjectsSortMode.None);
        
        foreach (OutsideVegLoader loader in vegLoaders)
        {
            FieldPos fieldPos = loader.GetComponentInParent<FieldPos>();
            if (fieldPos != null && fieldPos.Type == FieldType.NoVeg)
            {
                noVegLoaders.Add(loader);
            }
        }
        
        if (noVegLoaders.Count == 0)
        {
            Debug.LogWarning("[MapGenHandler] No NoVeg fields found for enemy spawning!");
            return;
        }
        
        Debug.Log($"[MapGenHandler] Found {noVegLoaders.Count} NoVeg fields for guaranteed pack spawning");
        
        // Spawn exactly 5 packs across ALL NoVeg fields
        SpawnGuaranteedPacksAcrossFields(noVegLoaders, 5);
    }
    
    /// <summary>
    /// Spawns a guaranteed minimum number of enemy packs across multiple fields
    /// Uses 10-unit minimum spacing with fallback system
    /// </summary>
    private void SpawnGuaranteedPacksAcrossFields(List<OutsideVegLoader> loaders, int minPacksToSpawn)
    {
        if (loaders == null || loaders.Count == 0)
        {
            Debug.LogError("[MapGenHandler] No loaders available for pack spawning!");
            return;
        }
        
        // Collect ALL available spawn points from all NoVeg fields
        List<Vector3> allSpawnPoints = new List<Vector3>();
        Dictionary<Vector3, OutsideVegLoader> spawnPointToLoader = new Dictionary<Vector3, OutsideVegLoader>();
        
        foreach (OutsideVegLoader loader in loaders)
        {
            if (loader.entitieSpawn != null)
            {
                foreach (GameObject sp in loader.entitieSpawn)
                {
                    if (sp != null)
                    {
                        allSpawnPoints.Add(sp.transform.position);
                        spawnPointToLoader[sp.transform.position] = loader;
                    }
                }
            }
        }
        
        if (allSpawnPoints.Count == 0)
        {
            Debug.LogError("[MapGenHandler] No spawn points found across NoVeg fields!");
            return;
        }
        
        Debug.Log($"[MapGenHandler] Collected {allSpawnPoints.Count} total spawn points from {loaders.Count} NoVeg fields");
        
        // Shuffle spawn points for randomness
        for (int i = allSpawnPoints.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Vector3 temp = allSpawnPoints[i];
            allSpawnPoints[i] = allSpawnPoints[randomIndex];
            allSpawnPoints[randomIndex] = temp;
        }
        
        // Spawn packs with distance constraint
        List<Vector3> usedPositions = new List<Vector3>();
        float minDistanceBetweenGroups = 10f; // 10 units = 1 tile apart
        int packsSpawned = 0;
        
        // PHASE 1: Try with 10-unit spacing
        foreach (Vector3 spawnPos in allSpawnPoints)
        {
            if (packsSpawned >= minPacksToSpawn)
                break;
            
            // Check distance to existing groups
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
                continue;
            
            // Get the loader for this spawn point
            if (!spawnPointToLoader.ContainsKey(spawnPos))
                continue;
            
            // Create enemy group
            GameObject groupObj = new GameObject($"EnemyPack_{packsSpawned}");
            groupObj.transform.position = spawnPos;
            groupObj.transform.SetParent(mobParentObj.transform);
            
            EnemyGroup group = groupObj.AddComponent<EnemyGroup>();
            group.isPatrol = false;
            
            // Random group size: 3-5 enemies
            int enemyCount = Random.Range(3, 6);
            
            // Spawn the group
            group.SpawnGroup(spawnPos, enemyCount, PrefabCollection.instance, mobParentObj.transform);
            
            usedPositions.Add(spawnPos);
            packsSpawned++;
            
            Debug.Log($"[MapGenHandler] ✓ Spawned EnemyPack {packsSpawned}/{minPacksToSpawn} with {enemyCount} members at {spawnPos}");
        }
        
        // PHASE 2: FALLBACK - If we couldn't spawn enough packs, reduce distance gradually
        if (packsSpawned < minPacksToSpawn)
        {
            Debug.LogWarning($"[MapGenHandler] Only spawned {packsSpawned}/{minPacksToSpawn} packs with 10-unit spacing. Activating fallback system...");
            
            // Try with 7-unit spacing
            minDistanceBetweenGroups = 7f;
            
            foreach (Vector3 spawnPos in allSpawnPoints)
            {
                if (packsSpawned >= minPacksToSpawn)
                    break;
                
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
                    continue;
                
                if (!spawnPointToLoader.ContainsKey(spawnPos))
                    continue;
                
                GameObject groupObj = new GameObject($"EnemyPack_{packsSpawned}");
                groupObj.transform.position = spawnPos;
                groupObj.transform.SetParent(mobParentObj.transform);
                
                EnemyGroup group = groupObj.AddComponent<EnemyGroup>();
                group.isPatrol = false;
                
                int enemyCount = Random.Range(3, 6);
                group.SpawnGroup(spawnPos, enemyCount, PrefabCollection.instance, mobParentObj.transform);
                
                usedPositions.Add(spawnPos);
                packsSpawned++;
                
                Debug.Log($"[MapGenHandler] ✓ FALLBACK(7u) spawned EnemyPack {packsSpawned}/{minPacksToSpawn}");
            }
        }
        
        // PHASE 3: EMERGENCY FALLBACK - If still not enough, use 5-unit spacing
        if (packsSpawned < minPacksToSpawn)
        {
            Debug.LogWarning($"[MapGenHandler] Still only {packsSpawned}/{minPacksToSpawn} packs. Emergency fallback with 5-unit spacing...");
            
            minDistanceBetweenGroups = 5f;
            
            foreach (Vector3 spawnPos in allSpawnPoints)
            {
                if (packsSpawned >= minPacksToSpawn)
                    break;
                
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
                    continue;
                
                if (!spawnPointToLoader.ContainsKey(spawnPos))
                    continue;
                
                GameObject groupObj = new GameObject($"EnemyPack_{packsSpawned}");
                groupObj.transform.position = spawnPos;
                groupObj.transform.SetParent(mobParentObj.transform);
                
                EnemyGroup group = groupObj.AddComponent<EnemyGroup>();
                group.isPatrol = false;
                
                int enemyCount = Random.Range(3, 6);
                group.SpawnGroup(spawnPos, enemyCount, PrefabCollection.instance, mobParentObj.transform);
                
                usedPositions.Add(spawnPos);
                packsSpawned++;
                
                Debug.Log($"[MapGenHandler] ✓ EMERGENCY FALLBACK(5u) spawned EnemyPack {packsSpawned}/{minPacksToSpawn}");
            }
        }
        
        // Final check
        if (packsSpawned < minPacksToSpawn)
        {
            Debug.LogError($"[MapGenHandler] ⚠️ CRITICAL: Only managed to spawn {packsSpawned}/{minPacksToSpawn} packs even with all fallbacks!");
        }
        else
        {
            Debug.Log($"[MapGenHandler] ✓ SUCCESS: Spawned {packsSpawned} enemy packs (minimum {minPacksToSpawn} guaranteed)");
        }
        
        // Cleanup spawn point markers
        foreach (var kvp in spawnPointToLoader)
        {
            OutsideVegLoader loader = kvp.Value;
            if (loader.entitieSpawn != null)
            {
                foreach (GameObject sp in loader.entitieSpawn)
                {
                    if (sp != null)
                        Destroy(sp);
                }
            }
        }
    }

        /// <summary>
    /// Generates patrol groups on connected road tiles
    /// Called after SpawnEnemiesAfterNavMesh()
    /// </summary>
    private void GenerateRoadPatrols()
    {
        Debug.Log("[MapGenHandler] === Generating Road Patrols ===");
        
        // Check if map is cleared - skip if it is
        if (GlobalMap.instance != null && GlobalMap.instance.currentMap != null && GlobalMap.instance.currentMap.isCleared)
        {
            Debug.Log("[MapGenHandler] Map is cleared - skipping patrol generation");
            return;
        }
        
        // Find all road tiles
        List<FieldPos> roadTiles = new List<FieldPos>();
        
        foreach (GameObject fieldObj in fieldsPosObj)
        {
            if (fieldObj == null) continue;
            
            FieldPos fieldPos = fieldObj.GetComponent<FieldPos>();
            if (fieldPos == null) continue;
            
            // Check if this is a road tile
            if (IsRoadType(fieldPos.Type))
            {
                roadTiles.Add(fieldPos);
            }
        }
        
        if (roadTiles.Count < 4)
        {
            Debug.LogWarning($"[MapGenHandler] Not enough road tiles ({roadTiles.Count}) for patrol generation");
            return;
        }
        
        Debug.Log($"[MapGenHandler] Found {roadTiles.Count} road tiles for patrol paths");
        
        // Generate 2 patrol groups
        int patrolsToGenerate = 2;
        List<Vector3> usedStartPositions = new List<Vector3>();
        float minDistanceBetweenPatrols = 20f; // 2 tiles apart
        
        for (int p = 0; p < patrolsToGenerate; p++)
        {
            // Try to find a valid starting position
            FieldPos startTile = null;
            int attempts = 0;
            int maxAttempts = 20;
            
            while (attempts < maxAttempts)
            {
                attempts++;
                startTile = roadTiles[Random.Range(0, roadTiles.Count)];
                
                // Check distance to other patrols
                bool tooClose = false;
                foreach (Vector3 usedPos in usedStartPositions)
                {
                    if (Vector3.Distance(startTile.transform.position, usedPos) < minDistanceBetweenPatrols)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                {
                    break; // Found valid position
                }
            }
            
            if (startTile == null)
            {
                Debug.LogWarning("[MapGenHandler] Could not find valid start position for patrol");
                continue;
            }
            
            usedStartPositions.Add(startTile.transform.position);
            
            // Build patrol path from connected road tiles
            List<Vector3> patrolPath = BuildPatrolPath(startTile, roadTiles);
            
            if (patrolPath.Count < 2)
            {
                Debug.LogWarning("[MapGenHandler] Could not build valid patrol path");
                continue;
            }
            
            // Create patrol group
            GameObject patrolObj = new GameObject($"EnemyPatrol_{p}");
            patrolObj.transform.position = patrolPath[0];
            patrolObj.transform.SetParent(mobParentObj.transform);
            
            EnemyGroup patrolGroup = patrolObj.AddComponent<EnemyGroup>();
            
            // Random patrol size: 2-4 enemies
            int patrolSize = Random.Range(2, 5);
            
            // Spawn patrol members
            patrolGroup.SpawnGroup(patrolPath[0], patrolSize, PrefabCollection.instance, mobParentObj.transform);
            
            // Setup patrol behavior
            patrolGroup.SetupPatrol(patrolPath);
            
            Debug.Log($"[MapGenHandler] Created patrol {p + 1}/{patrolsToGenerate} with {patrolSize} members and {patrolPath.Count} waypoints");
        }
    }

    /// <summary>
    /// Helper: Check if FieldType is a road variant
    /// </summary>
    private bool IsRoadType(FieldType type)
    {
        return type == FieldType.Road ||
            type == FieldType.RoadVertical ||
            type == FieldType.RoadHorizontal ||
            type == FieldType.RoadTJunctionTop ||
            type == FieldType.RoadTJunctionBot ||
            type == FieldType.RoadTJunctionLeft ||
            type == FieldType.RoadTJunctionRight ||
            type == FieldType.RoadCrossroad ||
            type == FieldType.RoadCurveTopLeft ||
            type == FieldType.RoadCurveTopRight ||
            type == FieldType.RoadCurveBottomLeft ||
            type == FieldType.RoadCurveBottomRight;
    }

    /// <summary>
    /// Builds a patrol path by finding connected road tiles
    /// Returns waypoints with every 2nd tile for smooth movement
    /// </summary>
    private List<Vector3> BuildPatrolPath(FieldPos startTile, List<FieldPos> allRoadTiles)
    {
        List<Vector3> path = new List<Vector3>();
        List<FieldPos> visited = new List<FieldPos>();
        
        FieldPos currentTile = startTile;
        visited.Add(currentTile);
        path.Add(currentTile.transform.position);
        
        // Build path by finding adjacent road tiles
        int maxWaypoints = 6; // Max 6 waypoints per patrol
        int waypointsAdded = 1;
        
        while (waypointsAdded < maxWaypoints)
        {
            // Find adjacent road tiles (within 12 units = 1.2 tiles)
            FieldPos nextTile = null;
            float closestDistance = float.MaxValue;
            
            foreach (FieldPos tile in allRoadTiles)
            {
                if (visited.Contains(tile)) continue;
                
                float distance = Vector3.Distance(currentTile.transform.position, tile.transform.position);
                
                // Adjacent tiles are ~10 units apart
                if (distance < 12f && distance < closestDistance)
                {
                    closestDistance = distance;
                    nextTile = tile;
                }
            }
            
            if (nextTile == null)
            {
                break; // No more connected tiles
            }
            
            visited.Add(nextTile);
            currentTile = nextTile;
            
            // Add waypoint every 2nd tile for smoother movement
            if (waypointsAdded % 2 == 0 || waypointsAdded == maxWaypoints - 1)
            {
                path.Add(currentTile.transform.position);
            }
            
            waypointsAdded++;
        }
        
        // Close the loop if we have enough waypoints
        if (path.Count >= 3)
        {
            // Check if we can connect back to start
            float distanceToStart = Vector3.Distance(currentTile.transform.position, startTile.transform.position);
            
            // If far from start, add start position again to create loop
            if (distanceToStart > 15f)
            {
                path.Add(startTile.transform.position);
            }
        }
        
        return path;
    }

    // Legacy method für Kompatibilität
    public void CreateANewMap(SpawnPoint playerSpawn)
    {
        CreateNewMap(playerSpawn);
    }

    // Legacy method für Kompatibilität  
    public void LoadMap(MapSave map, SpawnPoint spawnPoint)
    {
        if (map != null)
        {
            LoadExistingMap(map, spawnPoint);
        }
        else
        {
            CreateNewMap(spawnPoint);
        }
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

        OutsideVegLoader[] outsideVegPrefabs = FindObjectsByType<OutsideVegLoader>(FindObjectsSortMode.None); 
        foreach(OutsideVegLoader prefab in outsideVegPrefabs)
        {
            Destroy(prefab.gameObject);
        }
        
        // Reset wave spawner when map is reset
        if (EnemyWaveSpawner.instance != null)
        {
            EnemyWaveSpawner.instance.ResetSpawner();
        }
    }

    public void RebuildNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }
}