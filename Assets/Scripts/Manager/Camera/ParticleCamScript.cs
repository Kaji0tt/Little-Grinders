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
    [SerializeField]
    private string sortingLayerName = "Umgebung_col Layer";
    private ParticleSystem particle;
    private TrailRenderer trailRenderer;


    void Start()
    {

        //agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        if (GetComponent<ParticleSystem>() != null)
        {
            particle = GetComponent<ParticleSystem>();
            particle.GetComponent<Renderer>().sortingLayerName = sortingLayerName;
        }
        
        if (GetComponent<TrailRenderer>() != null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
            trailRenderer.sortingLayerName = sortingLayerName;
        }
    }

    void Update()
    {
        //transform.LookAt(Camera.main.transform);

        CameraPosition = CameraManager.instance.GetCameraPosition();
        DistSelfCamera = (transform.position - CameraPosition).sqrMagnitude;

        if (particle != null)
        {
            var renderer = particle.GetComponent<Renderer>();
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = (int)(sortingOrderBase - DistSelfCamera);
        }
        
        if (trailRenderer != null)
        {
            trailRenderer.sortingLayerName = sortingLayerName;
            trailRenderer.sortingOrder = (int)(sortingOrderBase - DistSelfCamera);
        }

        //11.09
        //Die Folgenden Lines of Code sollten wohl verwendet werden, um Attack Animationen / Kampf zu berechnen,
        //allerdings f�hren sie aus unbekannten Gr�nden zu einem Null Error, deshalb erstmal ausgeklammert.

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
