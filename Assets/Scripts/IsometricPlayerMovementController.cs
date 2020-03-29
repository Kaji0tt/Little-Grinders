using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricPlayerMovementController : MonoBehaviour
{

    public float movementSpeed = 30f;
    public float SlowFactor;
    IsometricCharacterRenderer isoRenderer;

    Rigidbody rbody;

    Vector3 forward, right;


    private void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();

        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
    }


    void FixedUpdate()
    {

        if (Input.anyKey)
        { 
            Move();
        }

        //Vector2 currentPos = rbody.position;
        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        //print(inputVector + "Peter stink");
        isoRenderer.SetDirection(inputVector);
    }
    void Move()
    {

        Vector3 ActualSpeed = right * Input.GetAxis("HorizontalKey") + forward * Input.GetAxis("VerticalKey");
        ActualSpeed = Vector3.ClampMagnitude(ActualSpeed, 1);
        rbody.AddForce(ActualSpeed * movementSpeed, ForceMode.Force);



    }



    //Bei Collision mit Busch soll das Movementspeed reduziert werden. 
    //"Other" sollte ersetzt werden um entsprechende Objektvariabel. 
    //Movementspeed sollte runter multipliziert werden, aber unter 1 mag float nicht.
    private void OnTriggerEnter(Collider other)
    {
        //movementSpeed = 15;
        rbody.velocity = rbody.velocity * SlowFactor * Time.deltaTime;
    }

    private void OnTriggerExit(Collider other)
    {
        //movementSpeed = 30;

    }
}