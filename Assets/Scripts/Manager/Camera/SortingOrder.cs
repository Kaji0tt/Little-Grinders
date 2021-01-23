using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingOrder : MonoBehaviour
{
    private Vector3 CameraPosition;
    private float DistSelfCamera;
    [SerializeField]
    public int sortingOrderBase = 5000;

    private SpriteRenderer sprite;


    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();


    }

    void Update()
    {
        // Blick in Kamera Richtung
        //transform.LookAt(Camera.main.transform);

        CameraPosition = GameObject.FindGameObjectWithTag("MainCamera").transform.position;
        DistSelfCamera = (transform.position - CameraPosition).sqrMagnitude;
        sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera);


    }
}

