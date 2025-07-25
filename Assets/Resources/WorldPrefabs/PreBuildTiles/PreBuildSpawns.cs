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
        foreach(Transform spawn in enemiesSpawn)
        {
            if(Random.Range(0,2) == 1)
            {
                GameObject enemyPrefab = enemiesPF[Random.Range(0, enemiesPF.Length)];
                GameObject mob = Instantiate(enemyPrefab, spawn.transform.position, Quaternion.identity);
                mob.name = enemyPrefab.name; // Setze den Namen ohne "(Clone)"
                mob.transform.SetParent(MapGenHandler.instance.mobParentObj.transform);
            }
        }
    }
}
