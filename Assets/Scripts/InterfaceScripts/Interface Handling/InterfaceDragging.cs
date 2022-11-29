using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InterfaceDragging : MonoBehaviour,IBeginDragHandler, IDragHandler, IScrollHandler
{
    private Camera _mainCam;

    private Vector3 _dragOffset;

    [SerializeField]
    private float dragSpeed = 400f;

    [SerializeField]
    private float zoomSpeed = 0.1f;

    [SerializeField]
    private float maxZoom = 10f;

    private Vector3 initialScale;

    //private RectTransform rect;


    private void Awake()
    {
        _mainCam = Camera.main;
        //rect = GetComponent<RectTransform>();

        initialScale = transform.localScale;


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

    public void OnScroll(PointerEventData eventData)
    {
        var delta = Vector3.one * (eventData.scrollDelta.y * zoomSpeed);
        var desiredScale = transform.localScale + delta;

        desiredScale = ClampDesiredScale(desiredScale);

        transform.localScale = desiredScale;
    }

    private Vector3 ClampDesiredScale(Vector3 desiredScale)
    {
        desiredScale = Vector3.Max(new Vector3(0.4f, 0.4f, 0.4f), desiredScale);
        desiredScale = Vector3.Min(initialScale, desiredScale);
        return desiredScale;
    }
}
