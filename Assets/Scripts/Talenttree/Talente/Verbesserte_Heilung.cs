using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Verbesserte_Heilung : Talent, IPointerEnterHandler, IPointerExitHandler
{
    //public Heilung heilung;


    //Dies sollte keinesfalls über Update geschehen! Finde lieber ein Workaround, damit das alles CPU schonender wird.
    public void OnPointerEnter(PointerEventData eventData)
    {

        UI_Manager.instance.ShowTooltip(new Vector2(Input.mousePosition.x - 10f, Input.mousePosition.y + 10f), GetDescription);


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
}
