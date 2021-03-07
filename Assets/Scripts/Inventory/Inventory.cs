using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
 
    public event EventHandler OnItemListChanged; //Würde man für Equip nicht brauchen, da einzelne GO
    public List<ItemInstance> itemList;
    private Action<ItemInstance> useItemAction; //Würde man brauchen, falls man aktive Fähigkeiten auf den Items haben möchte.
    private GameEvents current;



    public Inventory(Action<ItemInstance> useItemAction)
    {
        this.useItemAction = useItemAction;

        itemList = new List<ItemInstance>();
    }

    public void AddItem(ItemInstance item)
    {
        if (itemList.Count <= 14)  // InvGröße: das war schlau von MonkeyDev, lediglich das Interface anpassen
        {
            /*
            if(item.itemType == ItemType.Consumable) //Ggf. gleiches für Currency hinzufügen, außerhalb von Inventory.
            {
                bool itemAlreadyInInventory = false;
                foreach (ItemInstance itemInInventory in itemList)
                {
                    if (itemInInventory.ItemID == item.ItemID)
                    {
                        itemInInventory.amount += item.amount;
                    }
                }
            }
            */
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
