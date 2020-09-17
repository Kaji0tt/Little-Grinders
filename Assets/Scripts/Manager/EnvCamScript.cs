using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvCamScript : MonoBehaviour
{
    private Vector3 CameraPosition;
    private float DistSelfCamera;

    [SerializeField]
    private int sortingOrderBase = 5000;

    private SpriteRenderer sprite;


    void Start()
    {
        


    }

    void Update()
    {
        foreach (Transform child in transform)
        {
            sprite = child.GetComponent<SpriteRenderer>();
            child.LookAt(Camera.main.transform);

            CameraPosition = GameObject.Find("Camera").transform.position;
            DistSelfCamera = (child.position - CameraPosition).sqrMagnitude;
            sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera);

        }
    }
}

