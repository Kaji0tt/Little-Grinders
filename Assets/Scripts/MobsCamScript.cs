using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobsCamScript : MonoBehaviour
{
    //Agent Stat Values
    UnityEngine.AI.NavMeshAgent agent;
    UnityEngine.AI.NavMeshAgent agentDest;
    private Vector3 playerPosition;

    private GameObject player;

    //Sorting Order Values
    private Vector3 CameraPosition;
    private float DistSelfCamera;
    [SerializeField]
    private int sortingOrderBase = 5000;
    private SpriteRenderer sprite;


    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.Find("Charakter");
        //player = GetComponent<PlayerStats>();




    }

    void Update()
    {
       transform.LookAt(Camera.main.transform);

        CameraPosition = GameObject.Find("Camera").transform.position;
        DistSelfCamera = (transform.position - CameraPosition).sqrMagnitude;
        sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera);

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
