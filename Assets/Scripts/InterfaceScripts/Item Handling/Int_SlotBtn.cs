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

    public void HideItem()
    {
        UI_Manager.instance.HideTooltip();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Falls der Inventar-Slot ein Item trägt,
        if (storedItem != null)

        UI_Manager.instance.ShowItemTooltip(new Vector2(transform.position.x, transform.position.y), storedItem);

        else
        {
            UI_Manager.instance.ShowTooltip("Du findest sicher noch einen Gegenstand für diese Stelle!");
        }


    }

    public void OnPointerExit(PointerEventData eventData)
    {

        HideItem();
    }
    


}
