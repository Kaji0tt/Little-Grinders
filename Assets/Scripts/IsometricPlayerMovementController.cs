using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsometricPlayerMovementController : MonoBehaviour
{

    public float movementSpeed = 1f;
    IsometricCharacterRenderer isoRenderer;

    Rigidbody2D rbody;
    //BoxCollider2D rbody;

    Vector3 forward, right;

    bool Trigger;

    private void Awake()
    {
        rbody = GetComponent<Rigidbody2D>();
        //rbody = GetComponent<BoxCollider2D>();
        isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();

        // **MOVE SPLASH** // Hier Splash ich das Movement aus dem anderen Tutorial in den Code mit dem 2D Movement Animator
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        // ** MOVE SPLASH ** //
        if (Input.anyKey)
        { 
            Move();
        }

 

        //Vector2 currentPos = rbody.position;
        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        //inputVector = Vector2.ClampMagnitude(inputVector, 1);
        //Vector2 movement = inputVector * movementSpeed;
        //Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
        isoRenderer.SetDirection(inputVector);
        //rbody.MovePosition(newPos);

        print(Trigger);
    }
    // ** MOVE SPLASH METHODE ** //
    void Move()
    {
        Vector3 direction = new Vector3(Input.GetAxis("HorizontalKey"), 0, Input.GetAxis("VerticalKey"));
        Vector3 rightMovement = right * movementSpeed * Time.deltaTime * Input.GetAxis("HorizontalKey");
        Vector3 upMovement = forward * movementSpeed * Time.deltaTime * Input.GetAxis("VerticalKey");

        //Vector3 heading = Vector3.Normalize(rightMovement + upMovement);



        //transform.Translate(rightMovement * movementSpeed);
        //transform.Translate(upMovement * movementSpeed);

        // Was wir eigentlich wollen, ist das Objekt über AddForce zu bewegen, weil wir sonst die Physik von Unity umgehen (Collider funktionieren nicht)

        transform.position += rightMovement;
        transform.position += upMovement;


    }

    private void OnTriggerEnter(Collider other)
    {
        Trigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        Trigger = false;
    }
}