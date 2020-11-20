using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Heilung : Talent, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    string description;
    /*
    public override bool Click()
    {
        if (base.Click())
        {
            // Give the ability

            return true;
        }

        return false;
    }
    */
    public void OnPointerEnter(PointerEventData eventData)
    {

        UI_Manager.instance.ShowTooltip(new Vector2(Input.mousePosition.x - 10f, Input.mousePosition.y + 10f), description);


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
}
