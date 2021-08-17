using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InterfaceDragging : MonoBehaviour,IBeginDragHandler, IDragHandler
{
    private Camera _mainCam;

    private Vector3 _dragOffset;

    [SerializeField]
    private float dragSpeed = 100f;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    Vector3 GetMousePos()
    {

        var mousePos = _mainCam.ScreenToViewportPoint(Input.mousePosition);

        mousePos.z = 0;

        mousePos *= dragSpeed;

        return mousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {

        transform.position = GetMousePos() + _dragOffset;

    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragOffset = transform.position - GetMousePos();
    }

}
