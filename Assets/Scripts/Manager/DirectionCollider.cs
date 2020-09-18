using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionCollider : MonoBehaviour
{

    public IsometricPlayer player;

    void Start()
    {
        player = GetComponent<IsometricPlayer>();
    }


    void Update()
    {
        float horizontalInput = Input.GetAxis("HorizontalKey");
        float verticalInput = Input.GetAxis("VerticalKey");
        int hInput = Mathf.CeilToInt(horizontalInput);
        int vInput = Mathf.CeilToInt(verticalInput);
        Vector2 inputVector = new Vector2(hInput, vInput);

        if (inputVector.x != 0 || inputVector.y != 0)
        transform.position = PlayerManager.instance.player.transform.position + new Vector3(inputVector.x, 0, inputVector.y);


    }
}
