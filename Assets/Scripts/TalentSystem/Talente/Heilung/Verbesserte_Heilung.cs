using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Verbesserte_Heilung : Talent, IPointerEnterHandler, IPointerExitHandler
{

    void Start()
    {
        SetDescription("Heilung heilt pro Skillpunkt zusätzlich um 2% des maximalen Lebens.");
    }
    public void OnPointerEnter(PointerEventData eventData)
    {

        UI_Manager.instance.ShowTooltip(GetDescription);


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
}
