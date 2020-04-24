using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    float moveSpeed = 0f;

    Vector3 forward, right;
    Animator animator;

    void Start()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);
        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;

        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKey)
            Move();
    }

    void Move()
    {

        Vector3 direction = new Vector3(Input.GetAxis("HorizontalKey"), 0, Input.GetAxis("VerticalKey"));
        Vector3 rightMovement = right * Input.GetAxis("HorizontalKey");
        Vector3 upMovement = forward * Input.GetAxis("VerticalKey");

        Vector3 heading = Vector3.Normalize(rightMovement + upMovement);

        transform.forward = heading;
        //transform.position += rightMovement;
        //transform.position += upMovement;

        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
        inputVector = Vector2.ClampMagnitude(inputVector, 1);
        print(inputVector);

        if (inputVector.x == 0 && inputVector.y > 0)
        {
        }
    }
}
