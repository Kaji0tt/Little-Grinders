using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public float smoothSpeed = 0.125f;


    [SerializeField]
    private float min = 10f;
    [SerializeField]
    private float max = 20f;

    private float zoom;

    //List<RaycastHit> hitsList = new List<RaycastHit>();

    RaycastHit[] raycastHits;

    //List<SpriteRenderer> lowAlphaSprites = new List<SpriteRenderer>();

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

    void Start()
    {
        raycastHits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), Mathf.Infinity);
    }

    private void Update()
    {

        if (Input.mouseScrollDelta.y != 0 && Time.timeScale == 1f)
            Zoom();

        //Vector3 CameraPosition = GameObject.FindGameObjectWithTag("MainCamera").transform.position;

        //Berechne Abstand zur Kamera
        float distSelfCamera = (PlayerManager.instance.player.transform.position - CameraManager.instance.activeCam.transform.position).sqrMagnitude;

        

        foreach (RaycastHit hit in raycastHits.ToList())
        {


            if(hit.collider == null)
            {
                //Work-Around für hit.collier == null - im Prinzip, fährt er mit dem Code dann fort. Scheint zu gehen.
                raycastHits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), distSelfCamera);
            }
            else
            {
                if (hit.collider.transform.GetComponent<SpriteRenderer>() != null)
                {
                    SpriteRenderer spriterend = hit.collider.transform.GetComponent<SpriteRenderer>();

                    if (spriterend && hit.collider.transform.gameObject.tag == "Env") //&& 
                    {
                        spriterend.color = new Color(1, 1, 1, 1);

                        //lowAlphaSprites.Add(hit.collider.transform.GetComponent<SpriteRenderer>());
                    }
                }
            }


        }

        //Populate die Variabel anhand von Kameraausrichtung neu.
        raycastHits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), distSelfCamera);
        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward), Color.green, distSelfCamera);

        //print(distSelfCamera);

        if (raycastHits.ToList().Count != 0)
            foreach (RaycastHit hit in raycastHits.ToList())
            {
                SpriteRenderer spriterend = hit.collider.transform.GetComponent<SpriteRenderer>();

                if (spriterend && hit.collider.transform.gameObject.tag == "Env") //&& 
                {
                    spriterend.color = new Color(1, 1, 1, 0.7f);

                    //lowAlphaSprites.Add(hit.collider.transform.GetComponent<SpriteRenderer>());
                }
            }

        
        if(raycastHits.Length != 0)
        for (int i = 0; i < raycastHits.Length; i++)
        {
            RaycastHit hit = raycastHits[i];

            SpriteRenderer spriterend = hit.collider.transform.GetComponent<SpriteRenderer>();

            if(spriterend && hit.collider.transform.gameObject.tag == "Env") //&& 
            {
                spriterend.color = new Color(1, 1, 1, 0.7f);

                //lowAlphaSprites.Add(hit.collider.transform.GetComponent<SpriteRenderer>());
            }

        }          
        
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
            CameraManager.instance.mainCam.fieldOfView = CameraManager.instance.mainCam.fieldOfView - Input.mouseScrollDelta.y;
            //PlayerManager.instance.player.GetComponent<IsometricPlayer>().userFOV = CameraManager.instance.mainCam.fieldOfView;
        }

        if (Input.mouseScrollDelta.y < 0 && zoom < max)
        {
            CameraManager.instance.mainCam.fieldOfView = CameraManager.instance.mainCam.fieldOfView - Input.mouseScrollDelta.y;
            //PlayerManager.instance.player.GetComponent<IsometricPlayer>().userFOV = CameraManager.instance.mainCam.fieldOfView;
        }
        zoom = CameraManager.instance.mainCam.fieldOfView;

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().userFOV = zoom;

    }



}
