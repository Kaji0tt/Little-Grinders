using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabCollection : MonoBehaviour
{
    [SerializeField]
    private GameObject[] smalGreenPF;
    [SerializeField]
    private GameObject[] midGreenPF;
    [SerializeField]
    private GameObject[] highGreenPF;
    [SerializeField]
    private GameObject[] horizntalFencePF;
    [SerializeField]
    private GameObject[] verticalFencePF;
    [SerializeField]
    private GameObject[] enemiesPF;
    [SerializeField]
    public GameObject[] groundTexture;
    [SerializeField]
    public GameObject[] exitColliders;
    [SerializeField]
    private GameObject[] interactablesPF;



    List<GameObject> possibleMobs = new List<GameObject>();

    public GameObject GetRandomSmalGreenPF()
    {
        return smalGreenPF[Random.Range(0, smalGreenPF.Length)];
    }
    public GameObject GetRandomMidGreenPF()
    {
        return midGreenPF[Random.Range(0, midGreenPF.Length)];
    }
    public GameObject GetRandomHighGreenPF()
    {
        return highGreenPF[Random.Range(0, highGreenPF.Length)];
    }
    public GameObject GetRandomHFencePFPF()
    {
        return horizntalFencePF[Random.Range(0, horizntalFencePF.Length)];
    }
    public GameObject GetRandomVFencePFPF()
    {
        return verticalFencePF[Random.Range(0, verticalFencePF.Length)];
    }
    public GameObject GetRandomEnemie()
    {
        possibleMobs.Clear();

        foreach(GameObject mob in enemiesPF)
        {
            if(mob.GetComponent<EnemyController>().level <= GlobalMap.instance.currentMap.mapLevel+1)
            {
                possibleMobs.Add(mob);
            }
        }

        return possibleMobs[Random.Range(0, possibleMobs.Count)];
    }
    public GameObject GetRandomGroundTexture()
    {
        return groundTexture[Random.Range(0, groundTexture.Length)];
    }

    public GameObject GetRandomInteractable()
    {
        return interactablesPF[Random.Range(0, interactablesPF.Length)];
    }

}
