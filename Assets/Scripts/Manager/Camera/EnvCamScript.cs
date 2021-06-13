using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvCamScript : MonoBehaviour
{
    //Position der Kamera
    private Vector3 CameraPosition;

    //Strecke zwischen dem Spielobjekt und der Kamera
    private float DistSelfCamera;

    //Der Standardvalue der Sorting Order.
    [SerializeField]
    private int sortingOrderBase = 5000;
    public int sO_OffSet;

    private SpriteRenderer sprite;

    private int no_children;
    private Transform child;

    //Collider, also Spielobjekte der Umgebung, welche vom Raycast getroffen werden
    List<RaycastHit> hitsList = new List<RaycastHit>();

    void Start()
    {

        no_children = this.gameObject.transform.childCount;

        for (int i = 0; i < no_children; i++)
        {

            child = this.gameObject.transform.GetChild(i);
            if(child.rotation.y == -1 || child.rotation.y == 1)
            {
                print("found go mit -180 oder 180");
                sprite = child.GetComponent<SpriteRenderer>();
                child.Rotate(-25, 0, 0);
            }
            else 
            {
                sprite = child.GetComponent<SpriteRenderer>();
                child.Rotate(25, 0, 0);
            }


        }
    }

    void Update()
    {
        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);
            sprite = child.GetComponent<SpriteRenderer>();
            //child.LookAt(Camera.main.transform); //LookAt ist fragwürdig, es würde mehr Illusion von 3D erzeugt werden ohne LookAt
            //Rather: Automatische Rotation bis Grad x° für mehr Tiefe.
            //child.Rotate(0, 25, 0);                           

            CameraPosition = GameObject.Find("Camera").transform.position;
            DistSelfCamera = (child.position - CameraPosition).sqrMagnitude;
            sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera) + sO_OffSet;
            sprite.sortingLayerName = "Umgebung_col Layer";
        }



    }
}

