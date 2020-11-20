using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Int_SlotBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Item storedItem;


    private void Update()
    {
        //if (storedItem != null)
        //print("storedItem: " + storedItem.ItemName + " derived from gameObject: " + gameObject.name);
    }
    public void StoreItem(Item item)
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
