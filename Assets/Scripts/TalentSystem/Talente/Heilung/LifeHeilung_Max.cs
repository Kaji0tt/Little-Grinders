using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LifeHeilung_Max : Talent, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {

        UI_Manager.instance.ShowTooltip(GetDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetDescription("Verdoppelt die Heilungsdauer von Heilung.");
    }

}