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

    //Idle Time Check
    private int time = 0;

    private float rotationSpeed = -10;

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

        if (Input.mouseScrollDelta.y != 0)
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





    /*
    void ToggleCam()
    {
        if (activeCam <= 3)
            toggleCamPos[activeCam + 1]();
        else
            toggleCamPos[0]();
    }


    void CamPos1()
    {
        Vector3 Pos1 = new Vector3(0.0f, -0.0f, -10f);

        transform.position = PlayerManager.instance.player.transform.position + Pos1;

        Vector3 Rot1 = new Vector3(0.00f, 0f, 0f);

        Quaternion qRot1 = Quaternion.Euler(Rot1);

        transform.rotation = qRot1;

        perspectiveView = false;





        activeCam = 0;
    }

    void CamPos2()
    {
        Vector3 Pos2 = new Vector3(-0.02f, -1.98f, -3.843f);

        transform.position = PlayerManager.instance.player.transform.position + Pos2;

        Vector3 Rot2 = new Vector3(-35f, -1.98f, -3.843f);

        Quaternion qRot2 = Quaternion.Euler(Rot2);

        transform.rotation = qRot2;

        //fieldofView = 40
        Camera.main.fieldOfView = 40;

        perspectiveView = true;




        activeCam = 1;
    }

    void CamPos3()
    {
        Vector3 Pos3 = new Vector3(0.0f, 10.0f, 0f);

        transform.position = PlayerManager.instance.player.transform.position + Pos3;

        Vector3 Rot3 = new Vector3(90.00f, -180f, 0f);

        Quaternion qRot3 = Quaternion.Euler(Rot3);

        transform.rotation = qRot3;

        perspectiveView = false;




        activeCam = 2;
    }

    void CamPos4()
    {
        Vector3 Pos4 = new Vector3(-0.0f, 3f, 2f);

        transform.position = PlayerManager.instance.player.transform.position + Pos4;

        Vector3 Rot4 = new Vector3(45f, -180f, 0f);

        Quaternion qRot4 = Quaternion.Euler(Rot4);

        transform.rotation = qRot4;




        activeCam = 3;
    }
    */
}
