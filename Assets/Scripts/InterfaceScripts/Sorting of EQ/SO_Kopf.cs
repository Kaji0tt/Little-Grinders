using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SO_Kopf : MonoBehaviour
{
    private int sortingOrderBase;
    private GameObject Charakter;
    private SpriteRenderer Sprite;
    private float DistCharCamera;
    // Start is called before the first frame update
    void Start()
    {
        Charakter = GameObject.Find("Charakter");
        sortingOrderBase = 5000;
        Sprite = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        Charakter.GetComponent<Transform>();
        Vector3 CameraPosition = GameObject.Find("Camera").transform.position;
        DistCharCamera = (Charakter.transform.position - CameraPosition).sqrMagnitude;
        Sprite.sortingOrder = (int)(sortingOrderBase - DistCharCamera + 4);
    }
}
