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

    private int no_children;
    private Transform child;

    void Start()
    {

        no_children = this.gameObject.transform.childCount;

    }

    void Update()
    {
        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);
            sprite = child.GetComponent<SpriteRenderer>();
            child.LookAt(Camera.main.transform);

            CameraPosition = GameObject.Find("Camera").transform.position;
            DistSelfCamera = (child.position - CameraPosition).sqrMagnitude;
            sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera);
            sprite.sortingLayerName = "Umgebung_col Layer";
        }
        //transform.LookAt(Camera.main.transform);

        /*
        foreach (this.gameObject.GetChild child in transform)
        {
            sprite = child.GetComponent<SpriteRenderer>();
            child.LookAt(Camera.main.transform);

            CameraPosition = GameObject.Find("Camera").transform.position;
            DistSelfCamera = (child.position - CameraPosition).sqrMagnitude;
            sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera);

        }
        */

    }
}

