using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Zur Animation / Bewegung des Spielsobjekts auf dem die Waffe liegt.
public class WeaponController : MonoBehaviour
{

    Vector3 forward, right;

    void Start()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;


    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.anyKey)
        {
            animator.enabled = true;
            Move();
        }
        else
            animator.enabled = false;
        */

    }

    void Move()
    {

        Vector3 direction = new Vector3(Input.GetAxis("HorizontalKey"), 0, Input.GetAxis("VerticalKey"));
        Vector3 rightMovement = right * Input.GetAxis("HorizontalKey");
        Vector3 upMovement = forward * Input.GetAxis("VerticalKey");

        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);



    }
}