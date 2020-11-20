using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvLookAt : MonoBehaviour
{
    private Vector3 CameraPosition;
    private float DistSelfCamera;

    [SerializeField]
    private int sortingOrderBase = 5000;
    public int sO_OffSet;

    private SpriteRenderer sprite;

    private int no_children;
    private Transform child;

    void Start()
    {

        no_children = this.gameObject.transform.childCount;

        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);
            sprite = child.GetComponent<SpriteRenderer>();
            child.Rotate(25, 0, 0);

        }
    }

    void Update()
    {
        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);
            sprite = child.GetComponent<SpriteRenderer>();
            child.LookAt(Camera.main.transform); //LookAt ist fragwürdig, es würde mehr Illusion von 3D erzeugt werden ohne LookAt
            //Rather: Automatische Rotation bis Grad x° für mehr Tiefe.
            //child.Rotate(0, 25, 0);                           

            CameraPosition = GameObject.Find("Camera").transform.position;
            DistSelfCamera = (child.position - CameraPosition).sqrMagnitude;
            sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera) + sO_OffSet;
            sprite.sortingLayerName = "Umgebung_col Layer";
        }

    }
    
}
