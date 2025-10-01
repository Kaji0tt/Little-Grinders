using UnityEngine;

public static class XP_OrbSpawner
{
    // Lädt das Prefab aus Resources/VFX/VFX_XP-Orb.prefab
    public static GameObject xpOrbPrefab = Resources.Load<GameObject>("VFX/VFX_XP-Orb");
    
    [Header("Spawn Settings")]
    public static float minDistanceBetweenOrbs = 0.03f;
    public static int maxSpawnAttempts = 10;
    
    public static void SpawnXPOrbs(Vector3 spawnPosition, int totalXP)
    {
        if (xpOrbPrefab == null)
        {
            return;
        }

        var orbCounts = CalculateOrbDistribution(totalXP);

        // Liste aller gespawnten Orb-Positionen
        var spawnedPositions = new System.Collections.Generic.List<Vector3>();

        foreach (var orbData in orbCounts)
        {
            for (int i = 0; i < orbData.count; i++)
            {
                Vector3 validPosition = FindValidSpawnPosition(spawnPosition, spawnedPositions);
                spawnedPositions.Add(validPosition);
                SpawnSingleOrb(validPosition, orbData.xpValue);
            }
        }
    }
    
    static Vector3 FindValidSpawnPosition(Vector3 basePosition, System.Collections.Generic.List<Vector3> existingPositions)
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Kreisförmige Verteilung um die Basis-Position
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = Random.Range(0.2f, 1.0f);
            
            Vector3 candidatePosition = basePosition + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            
            // Prüfe Distanz zu allen existierenden Orbs
            bool validPosition = true;
            foreach (Vector3 existingPos in existingPositions)
            {
                if (Vector3.Distance(candidatePosition, existingPos) < minDistanceBetweenOrbs)
                {
                    validPosition = false;
                    break;
                }
            }
            
            if (validPosition)
            {
                return candidatePosition;
            }
        }
        
        // Fallback: Verwende zufällige Position auch wenn sie zu nah ist
        return basePosition + new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)
        );
    }
    
    static void SpawnSingleOrb(Vector3 spawnPosition, int xpValue)
    {
        GameObject orb = Object.Instantiate(xpOrbPrefab, spawnPosition, Quaternion.identity);
        VFX_XPOrb orbScript = orb.GetComponent<VFX_XPOrb>();
        
        if (orbScript != null)
        {
            orbScript.Initialize(xpValue);
        }
    }
    
    static (int xpValue, int count)[] CalculateOrbDistribution(int totalXP)
    {
        var result = new System.Collections.Generic.List<(int, int)>();
        
        // Greedy Algorithm: Größte Orbs zuerst
        int[] orbValues = { 50, 20, 10, 5, 2, 1 };
        
        foreach (int orbValue in orbValues)
        {
            int count = totalXP / orbValue;
            if (count > 0)
            {
                result.Add((orbValue, count));
                totalXP -= count * orbValue;
            }
        }
        
        return result.ToArray();
    }
}
