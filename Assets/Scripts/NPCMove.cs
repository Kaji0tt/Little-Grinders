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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        /* Das NPC Script brauch einen >>DirectionArray<< um animiert werden zu können.
           Das DirectionArray sollte ausgelesen werden.
           Eine Lösung wäre:

            Fetchen der Kamera Axe:
                            forward = Camera.main.transform.forward;
                            forward.y = 0;
                            forward = Vector3.Normalize(forward);
                            right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

            Right und Forward sind dann die Kamera Axen

            >> Dann muss ermittelt, wo Destination in Bezug zum NPC steht, um die Axenwerte in relation zum NPC zu erhalten. << 
                            Das ist die knackige Aufgabe von der ich kein Plan hab.
                            // vgl. PlayerMovementController
            
            Schließlich kann man dann den Vektor2 ermitteln,
            welcher zur Koordienierung des DirectionArrays verwendet wird.

            (Der DirectionArray wird an ein weiteres Script übergeben, welches die Animation bestimmt)
            // Vgl. IsoCharRenderer


        */

        navMeshAgent = this.GetComponent<NavMeshAgent>();

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
