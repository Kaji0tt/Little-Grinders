using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public Canvas canvas;

    private RectTransform rect;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }
    public void Update()
    {
        if(IsFullyOnScreen())
            rect.pivot = new Vector2(0, 1);
        else
            rect.pivot = new Vector2(1, 0);

    }

    public bool IsFullyOnScreen()
    {

        RectTransform CanvasRect = canvas.GetComponent<RectTransform>();

        if ((rect.anchoredPosition.x * -1) + rect.sizeDelta.x >= (CanvasRect.sizeDelta.x / 2))
        {

            return true;
        }

        return false;
    }
}
