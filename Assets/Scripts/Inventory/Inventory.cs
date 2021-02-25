using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
 
    public event EventHandler OnItemListChanged;
    public List<ItemInstance> itemList;
    private Action<ItemInstance> useItemAction;
    private GameEvents current;



    public Inventory(Action<ItemInstance> useItemAction)
    {
        this.useItemAction = useItemAction;

        itemList = new List<ItemInstance>();
    }

    public void AddItem(ItemInstance item)
    {
        if (itemList.Count <= 15)  // hier nochmal prüfen, aber irgendwie so die inv größe regeln
        {
            itemList.Add(item);
            OnItemListChanged?.Invoke(this, EventArgs.Empty);

        }

    }


    public void RemoveItem(ItemInstance item)
    {
        itemList.Remove(item);
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UseItem(ItemInstance item)
    {
        useItemAction(item);
    }

    public List<ItemInstance> GetItemList()
    {
        return itemList;


    }

}
