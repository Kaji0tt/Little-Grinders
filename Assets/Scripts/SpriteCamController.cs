using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteCamController : MonoBehaviour
{
    private Vector3 CharPosition;
    private Vector3 CameraPosition;
    private float DistCharCamera;
    private float DistSelfCamera;
    private SpriteRenderer sprite;
    // Start is called before the first frame update
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        CharPosition = GameObject.Find("Charakter").transform.position;
        //transform.LookAt(Camera.main.transform);
        CameraPosition = GameObject.Find("Camera").transform.position;


        //Vector3 offset = CharPosition - transform.position;
        DistCharCamera = (CharPosition - CameraPosition).sqrMagnitude;
        DistSelfCamera = (transform.position - CameraPosition).sqrMagnitude;
        //print(DistSelfCamera);

        if (DistCharCamera < DistSelfCamera)

        {
            sprite.sortingOrder = 0;

        }

        else
        {
            sprite.sortingOrder = 10;

        }
                //Falls 


    }
}
