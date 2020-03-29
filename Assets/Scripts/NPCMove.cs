using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCMove : MonoBehaviour
{
    [SerializeField]
    Transform destination;

    NavMeshAgent navMeshAgent;
    IsometricCharacterRenderer isoRenderer;

    Vector3 forward, right;
    private float xInputVector, zInputVector;
    // Start is called before the first frame update
    void Start()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Die Direction berechnet sich noch aus den Welt
        Vector3 Direction = destination.transform.position - transform.position;
        
        Vector2 inputVector = new Vector2(Direction.x * -1, Direction.z); 
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
  
        isoRenderer.SetNPCDirection(inputVector);


        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent == null)
        {
            Debug.LogError("The Nav Mesh agent Component is not attached to " + gameObject.name);
        }
        else
        {
            SetDestination();
        }
    }

    private void SetDestination()
    {
        if (destination != null)
        {
            Vector3 targetVector = destination.transform.position;
            navMeshAgent.SetDestination(targetVector);
        }
    }
}
