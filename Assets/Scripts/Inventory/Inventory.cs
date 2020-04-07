using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public event EventHandler OnItemListChanged;
    public List<Item> itemList;
    private Action<Item> useItemAction;
    private GameEvents current;
    //public string itemType;

    public Inventory(Action<Item> useItemAction)
    {
        this.useItemAction = useItemAction;

        itemList = new List<Item>();


        AddItem(new Item { itemName = Item.ItemName.Einfache_Sandalen, itemType = "Schuhe" });


        AddItem(new Item { itemName = Item.ItemName.Einfaches_Schwert, itemType = "Weapon" });


    }

    public void AddItem(Item item)
    {
        if (itemList.Count <= 15)  // hier nochmal prüfen, aber irgendwie so die inv größe regeln
        {
            itemList.Add(item);
            OnItemListChanged?.Invoke(this, EventArgs.Empty);
        }

    }


    public void RemoveItem(Item item)
    {
        itemList.Remove(item);
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void UseItem(Item item)
    {
        useItemAction(item);
    }

    public List<Item> GetItemList()
    {
        return itemList;

    }
}
