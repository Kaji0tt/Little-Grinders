using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;
    public float smoothSpeed = 0.125f;
    //public Vector3 offset;

    [SerializeField]
    private float min = 10f;
    [SerializeField]
    private float max = 20f;

    private float zoom;


    //public float zoomSpeed = 4f;
    //private float currentZoom = 10f;
    /*
    private bool perspectiveView;

    private Action[] toggleCamPos = new Action[4];

    private int activeCam = 0;
    */
    void Awake()
    {
        //toggleCamPos[0] = CamPos1;
        //toggleCamPos[1] = CamPos2;
        //toggleCamPos[2] = CamPos3;
        //toggleCamPos[3] = CamPos4;
    }

    private void Update()
    {

        if (Input.mouseScrollDelta.y != 0 && Time.timeScale == 1f)
            Zoom();

        //if (Input.GetKeyDown(UI_Manager.instance.toggleCam))
        //    ToggleCam();


    }


    void Zoom()
    {
        //Ggf. sollten die Mausrad zoomies invertiert werden.
        //float _zoom = Camera.main.fieldOfView;

        //float max, min;
        max = 20.0f;
        min = 10.0f;

        if (Input.mouseScrollDelta.y > 0 && zoom > min)
        {
            Camera.main.fieldOfView = Camera.main.fieldOfView - Input.mouseScrollDelta.y;
        }

        if (Input.mouseScrollDelta.y < 0 && zoom < max)
        {
            Camera.main.fieldOfView = Camera.main.fieldOfView - Input.mouseScrollDelta.y;
        }
        zoom = Camera.main.fieldOfView;

    }

}
