using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StatTooltips : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string description;


    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(new Vector2(Input.mousePosition.x - 10f, Input.mousePosition.y + 10f), description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
}
