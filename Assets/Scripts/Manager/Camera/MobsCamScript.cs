using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script to Manage the Sorting-Order of the Sprite in reference to the Camera Distance. Essential for 2.5D visualisation for Mobs.
public class MobsCamScript : MonoBehaviour
{

    //Sorting Order Values
    private Vector3 CameraPosition;

    private float DistSelfCamera;

    [SerializeField] //5000 is Standard value, from which the Distance is being substracted.
    private int sortingOrderBase = 5000;

    //private int originalSortingBase;

    private SpriteRenderer sprite;

    public void ReduceSpriteStartingPoint()
    {
        //originalSortingBase = sortingOrderBase;

        sortingOrderBase = 4800;
    }

    public void ResetSpriteStartingPoint()
    {
        sortingOrderBase = 5000;
    }

    void Start()
    {

        if (GetComponent<SpriteRenderer>() != null)
            sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        CameraPosition = CameraManager.instance.GetCameraPosition();
        DistSelfCamera = (transform.position - CameraPosition).sqrMagnitude;

        if(sprite != null)
        sprite.sortingOrder = (int)(sortingOrderBase - DistSelfCamera);
    }


}
