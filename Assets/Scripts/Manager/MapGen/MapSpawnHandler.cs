using UnityEngine;

public static class MapSpawnHandler
{
    public static void SetPlayerSpawn(SpawnPoint spawnPoint, GameObject[] fieldObjects)
    {
        Debug.Log($"[MapSpawnHandler] Setze Player Spawn auf: {spawnPoint}");
        
        // Debug: Zeige alle verfügbaren Exits
        DebugLogAllExitFields(fieldObjects);
        
        Vector3 spawnPosition = Vector3.zero;
        bool spawnFound = false;
        
        // Versuche den spezifischen Exit-Typ zu finden
        FieldType targetFieldType = GetTargetFieldType(spawnPoint);
        spawnPosition = FindSpawnPosition(fieldObjects, targetFieldType);
        
        if (spawnPosition != Vector3.zero)
        {
            spawnFound = true;
            Debug.Log($"[MapSpawnHandler] ✅ Gewünschter Spawn ({spawnPoint}) gefunden bei: {spawnPosition}");
        }
        else
        {
            Debug.LogWarning($"[MapSpawnHandler] ❌ Gewünschter Spawn ({spawnPoint}) nicht gefunden!");
            
            // Fallback: Nimm IRGENDEINEN verfügbaren Exit
            FieldType[] allExitTypes = {
                FieldType.OutsideExitRight,
                FieldType.OutsideExitLeft,
                FieldType.OutsideExitTop,
                FieldType.OutsideExitBot
            };
            
            foreach (var exitType in allExitTypes)
            {
                spawnPosition = FindSpawnPosition(fieldObjects, exitType);
                if (spawnPosition != Vector3.zero)
                {
                    spawnFound = true;
                    Debug.Log($"[MapSpawnHandler] 🔄 Fallback-Spawn gefunden: {exitType} bei {spawnPosition}");
                    break;
                }
            }
        }
        
        if (spawnFound)
        {
            PlayerManager.instance.player.transform.position = spawnPosition;
            Debug.Log($"[MapSpawnHandler] ✅ Player gespawnt bei: {spawnPosition}");
        }
        else
        {
            Debug.LogError($"[MapSpawnHandler] ❌ KEIN Exit gefunden! Center-Fallback.");
            PlayerManager.instance.player.transform.position = new Vector3(0, 1, 0);
        }
    }
    
    private static Vector3 FindSpawnPosition(GameObject[] fieldObjects, FieldType targetType)
    {
        for (int i = 0; i < fieldObjects.Length; i++)
        {
            var fieldPos = fieldObjects[i].GetComponent<FieldPos>();
            
            if (fieldPos != null && fieldPos.Type == targetType)
            {
                Vector3 fieldPosition = fieldObjects[i].transform.position;
                Vector3 spawnPos = CalculateSpawnPositionFromField(fieldPosition, targetType);
                
                Debug.Log($"[MapSpawnHandler] 🎯 Field {targetType} gefunden bei Index {i}, Field-Pos: {fieldPosition}, Spawn-Pos: {spawnPos}");
                return spawnPos;
            }
        }
        
        return Vector3.zero;
    }
    
    private static Vector3 CalculateSpawnPositionFromField(Vector3 fieldPosition, FieldType exitType)
    {
        // Spawn-Position für Exit-Fields (leicht erhöht für Kollision)
        Vector3 offset = new Vector3(0, 0.5f, 0);
        return fieldPosition + offset;
    }
    
    private static FieldType GetTargetFieldType(SpawnPoint spawnPoint)
    {
        return spawnPoint switch
        {
            SpawnPoint.SpawnRight => FieldType.OutsideExitRight,
            SpawnPoint.SpawnLeft => FieldType.OutsideExitLeft,
            SpawnPoint.SpawnTop => FieldType.OutsideExitTop,
            SpawnPoint.SpawnBot => FieldType.OutsideExitBot,
            _ => FieldType.OutsideExitRight
        };
    }
    
    public static void DebugLogAllExitFields(GameObject[] fieldObjects)
    {
        Debug.Log("[MapSpawnHandler] === ALLE EXIT-FIELDS ===");
        
        int exitCount = 0;
        for (int i = 0; i < fieldObjects.Length; i++)
        {
            var fieldPos = fieldObjects[i].GetComponent<FieldPos>();
            if (fieldPos != null)
            {
                FieldType type = fieldPos.Type;
                if (type == FieldType.OutsideExitRight || type == FieldType.OutsideExitLeft || 
                    type == FieldType.OutsideExitTop || type == FieldType.OutsideExitBot)
                {
                    Debug.Log($"  🚪 Exit {type} bei Index {i}, Pos: {fieldObjects[i].transform.position}, ArrayPos: [{fieldPos.ArrayPosX},{fieldPos.ArrayPosZ}]");
                    exitCount++;
                }
            }
        }
        
        Debug.Log($"[MapSpawnHandler] Insgesamt {exitCount} Exits gefunden");
    }
}