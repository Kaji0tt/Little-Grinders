using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXSortingScript : MonoBehaviour
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
    private ParticleSystemRenderer particleRenderer;

    private int no_children;
    private Transform child;

    void Start()
    {
        no_children = this.gameObject.transform.childCount;

        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);
            
            // Prüfe ob es ein SpriteRenderer ist
            sprite = child.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                if(child.rotation.y == -1 || child.rotation.y == 1)
                {
                    print("found go mit -180 oder 180");
                    child.Rotate(-25, 0, 0);
                }
                else 
                {
                    child.Rotate(25, 0, 0);
                }
            }
            
            // Prüfe ob es ein ParticleSystem ist
            particleRenderer = child.GetComponent<ParticleSystemRenderer>();
        }
    }

    void Update()
    {
        for (int i = 0; i < no_children; i++)
        {
            child = this.gameObject.transform.GetChild(i);
            
            // Handle SpriteRenderer
            sprite = child.GetComponent<SpriteRenderer>();
            if (sprite != null)
            {
                CameraPosition = GameObject.Find("Camera").transform.position;
                DistSelfCamera = (child.position - CameraPosition).sqrMagnitude;
                sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera) + sO_OffSet;
                sprite.sortingLayerName = "Umgebung_col Layer";
            }
            
            // Handle ParticleSystemRenderer
            particleRenderer = child.GetComponent<ParticleSystemRenderer>();
            if (particleRenderer != null)
            {
                CameraPosition = GameObject.Find("Camera").transform.position;
                DistSelfCamera = (child.position - CameraPosition).sqrMagnitude;
                particleRenderer.sortingOrder = (int)(sortingOrderBase - DistSelfCamera) + sO_OffSet;
                particleRenderer.sortingLayerName = "Umgebung_col Layer";
            }
        }
    }
}