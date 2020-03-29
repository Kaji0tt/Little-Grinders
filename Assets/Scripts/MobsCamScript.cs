using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobsCamScript : MonoBehaviour
{
    private Vector3 CameraPosition;
    private float DistSelfCamera;    

    [SerializeField]
    private int sortingOrderBase = 5000;

    private SpriteRenderer sprite;


    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();


    }

    void Update()
    {
       transform.LookAt(Camera.main.transform);

        CameraPosition = GameObject.Find("Camera").transform.position;
        DistSelfCamera = (transform.position - CameraPosition).sqrMagnitude;
        sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera);

    }
}
