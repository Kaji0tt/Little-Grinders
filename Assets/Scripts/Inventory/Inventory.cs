using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public event EventHandler OnItemListChanged;
    private Action<ItemInstance> useItemAction;

    // Dictionary speichert die Consumable ItemID (String) und die zugehörige Anzahl
    private Dictionary<string, int> consumableInventory;

    // Liste speichert alle Nicht-Consumables individuell
    public List<ItemInstance> itemList;

    public Inventory(List<ItemInstance> itemList)
    {
        this.itemList = itemList;
    }

    public Inventory(Action<ItemInstance> useItemAction)
    {
        this.useItemAction = useItemAction;
        consumableInventory = new Dictionary<string, int>();
        itemList = new List<ItemInstance>();
    }

    public void AddItem(ItemInstance item, int amount = 1)
    {
        if (item.itemType == ItemType.Consumable)  // Prüfe, ob das Item ein Consumable ist
        {
            // Stapelbare Items (Consumables) werden im Dictionary gestapelt
            if (consumableInventory.ContainsKey(item.ItemID))
            {
                consumableInventory[item.ItemID] += amount;  // Erhöhe die Anzahl, wenn bereits vorhanden
            }
            else
            {
                consumableInventory[item.ItemID] = amount;  // Füge das Item hinzu, wenn es neu ist
            }
        }
        else
        {
            // Nicht-Consumables werden einzeln in die Liste aufgenommen
            itemList.Add(item);
        }

        // Benachrichtige Listener, dass sich das Inventar geändert hat
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItem(ItemInstance item, int amount = 1)
    {
        if (item.itemType == ItemType.Consumable)
        {
            if (consumableInventory.ContainsKey(item.ItemID))
            {
                consumableInventory[item.ItemID] -= amount;

                // Entferne das Item, wenn die Menge <= 0 ist
                if (consumableInventory[item.ItemID] <= 0)
                {
                    consumableInventory.Remove(item.ItemID);
                }
            }
        }
        else
        {
            // Entferne das Item aus der Liste der Nicht-Consumables
            itemList.Remove(item);
        }

        // Benachrichtige Listener, dass sich das Inventar geändert hat
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    // Gibt die Anzahl eines bestimmten Consumable-Items im Inventar zurück
    public int GetItemAmount(ItemInstance item)
    {
        if (item.itemType == ItemType.Consumable && consumableInventory.ContainsKey(item.ItemID))
        {
            return consumableInventory[item.ItemID];
        }
        return 0; // Item nicht im Inventar oder kein Consumable
    }

    public void UseItem(ItemInstance item)
    {
        useItemAction(item);
    }

    public Dictionary<string, int> GetConsumableDict()
    {
        return consumableInventory;
    }

    public List<ItemInstance> GetItemList()
    {
        return itemList;
    }
}