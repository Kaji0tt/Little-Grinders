using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingOrderCanvas : MonoBehaviour
{
    private Vector3 CameraPosition;
    private float DistSelfCamera;
    [SerializeField]
    public int sortingOrderBase = 5000;

    private Canvas canvas;
    private GameObject go;


    void Start()
    {
        go = this.gameObject;
        canvas = go.GetComponent<Canvas>();
        canvas.sortingLayerName = "Umgebung_col Layer";


    }

    void Update()
    {
        // Blick in Kamera Richtung
        //transform.LookAt(Camera.main.transform);

        CameraPosition = GameObject.Find("Camera").transform.position;
        DistSelfCamera = (transform.position - CameraPosition).sqrMagnitude;
        canvas.sortingOrder = (int)(sortingOrderBase - DistSelfCamera);


    }
}
