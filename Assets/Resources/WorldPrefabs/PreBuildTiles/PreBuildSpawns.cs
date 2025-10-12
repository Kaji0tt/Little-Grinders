using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreBuildSpawns : MonoBehaviour
{
    [SerializeField]
    private Transform[] enemiesSpawn;

    [SerializeField]
    private GameObject[] enemiesPF;


    private void Start()
    {
        // Reduced spawn chance in camps - main spawning handled by EnemyWaveSpawner
        // Keep some enemies in camps for flavor and as initial threats
        foreach(Transform spawn in enemiesSpawn)
        {
            if(Random.Range(0,4) == 1) // Reduced from 1/2 to 1/4 chance
            {
                GameObject enemyPrefab = enemiesPF[Random.Range(0, enemiesPF.Length)];
                GameObject mob = Instantiate(enemyPrefab, spawn.transform.position, Quaternion.identity);
                mob.name = enemyPrefab.name; // Setze den Namen ohne "(Clone)"
                mob.transform.SetParent(MapGenHandler.instance.mobParentObj.transform);
            }
        }
    }
}
