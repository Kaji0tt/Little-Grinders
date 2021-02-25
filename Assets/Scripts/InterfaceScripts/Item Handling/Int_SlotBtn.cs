using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


//Int_SlotBtn sollte mit IMoveable und IUseable ebenfalls arbeiten.
public class Int_SlotBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ItemInstance storedItem;

    public void StoreItem(ItemInstance item)
    {
        storedItem = item;

    }

    public void ShowItem()
    {                                                                                                         
        UI_Manager.instance.ShowItemTooltip(new Vector2(Input.mousePosition.x - 10f, Input.mousePosition.y + 10f), storedItem);
    }

    public void HideItem()
    {
        UI_Manager.instance.HideTooltip();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (storedItem != null)
        ShowItem();

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideItem();

    }
    


}
