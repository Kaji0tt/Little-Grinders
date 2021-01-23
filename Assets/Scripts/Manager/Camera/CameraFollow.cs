using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    public float minZoom = 5f;
    public float maxZoom = 15f;
    public float zoomSpeed = 4f;
    private float currentZoom = 10f;

    private void Update()
    {
        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * -zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        target = PlayerManager.instance.player.transform;


    }

    private void LateUpdate()
    {
        //transform.position = target.position + offset;
    }
}
