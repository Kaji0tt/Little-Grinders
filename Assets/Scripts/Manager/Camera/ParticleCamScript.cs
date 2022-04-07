using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCamScript : MonoBehaviour
{
    //Agent Stat Values
    //UnityEngine.AI.NavMeshAgent agent;
    //UnityEngine.AI.NavMeshAgent agentDest;
    //private Vector3 playerPosition;


    //Sorting Order Values
    private Vector3 CameraPosition;
    private float DistSelfCamera;
    [SerializeField]
    private int sortingOrderBase = 5000;
    private ParticleSystem particle;


    void Start()
    {

        //agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (GetComponent<ParticleSystem>() != null)
            particle = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        //transform.LookAt(Camera.main.transform);

        CameraPosition = CameraManager.instance.mainCam.transform.position;
        DistSelfCamera = (transform.position - CameraPosition).sqrMagnitude;

        if (particle != null)
            particle.GetComponent<Renderer>().sortingOrder = (int)(sortingOrderBase - DistSelfCamera);


        //11.09
        //Die Folgenden Lines of Code sollten wohl verwendet werden, um Attack Animationen / Kampf zu berechnen,
        //allerdings führen sie aus unbekannten Gründen zu einem Null Error, deshalb erstmal ausgeklammert.

        /*playerPosition = GameObject.Find("Charakter").transform.position;
        agentDest.SetDestination(playerPosition);
        agentDest.SetDestination(GameObject.Find("Charakter").transform.position);

        float remainingDist = agent.remainingDistance;
        if (remainingDist <= 2)
        {

        }
        */
    }


}
