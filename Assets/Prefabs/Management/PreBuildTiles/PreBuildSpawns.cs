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
                Instantiate(enemiesPF[Random.Range(0, enemiesPF.Length)], spawn.transform.position, Quaternion.identity).transform.SetParent(MapGenHandler.instance.mobParentObj.transform);
            }
        }
    }
}
