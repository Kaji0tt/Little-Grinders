using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvCamNoRot : MonoBehaviour
{
    private Vector3 CameraPosition;
    //private float distSelfCamera;

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
            //child.Rotate(25, 0, 0);

            if (sprite)
                //Setze das Tag, damit CameraFollow.cs weiß, welche GO's zwischen Spieler und Kamera liegen für Transparenz.
                child.gameObject.tag = "Env";

        }
    }
    
    void Update()
    {
        no_children = this.gameObject.transform.childCount;

        //Folgender Abschnitt soll für sämtliche Child-GameObjekte angewandt werden.
        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);



            sprite = child.GetComponent<SpriteRenderer>();
            if(sprite)
            {
                //Enable Shadow Casting for the Sprite
                sprite.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

                //---  LookAt wurde vorerst Disabled, macht Perspektivisch wenig Sinn ---
                //child.LookAt(Camera.main.transform); 
                //child.Rotate(0, 25, 0);                           

                if(CameraManager.instance.mainCamGO.activeSelf)
                {
                    //Finde Position der Kamera
                    CameraPosition = CameraManager.instance.mainCamGO.transform.position;

                    //Berechne Abstand zur Kamera
                    float distSelfCamera = (child.position - CameraPosition).sqrMagnitude;

                    //Verändere die Sorting-Order entsprechend zum Abstand. Damit Layern die Sprite's schließlich automatisch korrekt.
                    sprite.sortingOrder = (int)(sortingOrderBase - distSelfCamera) + sO_OffSet;
                    sprite.sortingLayerName = "Umgebung_col Layer";
                }

            }

        }

    }



    //LayerSprites war ein Ansatz für das Layering im Aufbau von ProcedualMap Generation
    /*
    public void LayerSprites()
    {
        no_children = this.gameObject.transform.childCount;

        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);
            sprite = child.GetComponent<SpriteRenderer>();
            //child.LookAt(Camera.main.transform); //LookAt ist fragwürdig, es würde mehr Illusion von 3D erzeugt werden ohne LookAt
            //Rather: Automatische Rotation bis Grad x° für mehr Tiefe.
            //child.Rotate(0, 25, 0);                           

            CameraPosition = GameObject.FindGameObjectWithTag("MainCamera").transform.position;
            DistSelfCamera = (child.position - CameraPosition).sqrMagnitude;
            sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera) + sO_OffSet;
            sprite.sortingLayerName = "Umgebung_col Layer";
        }

    }
    */
}

