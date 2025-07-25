using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // Visuelles Inventar
    public UI_Inventory uiInventory;

    // Dictionary: SlotIndex -> ItemInstance (statisches Inventar mit festen Plätzen)
    public Dictionary<int, ItemInstance> itemDict = new Dictionary<int, ItemInstance>();

    // Optional: Consumables, falls benötigt
    private Dictionary<string, int> consumableInventory = new Dictionary<string, int>();

    // Fügt ein Item an einem bestimmten Slot hinzu
    public void AddItemAtIndex(ItemInstance item, int slotIndex)
    {
        itemDict[slotIndex] = item;
        Debug.Log($"[Inventory] AddItemAtIndex: Slot {slotIndex} bekommt Item '{item?.GetName()}'");
        if (uiInventory != null)
            uiInventory.OnInventorySlotChanged(slotIndex, item);
    }

    // Entfernt das Item aus einem bestimmten Slot
    public void RemoveItemAtIndex(int slotIndex)
    {
        if (itemDict.ContainsKey(slotIndex))
        {
            Debug.Log($"[Inventory] RemoveItemAtIndex: Entferne Item '{itemDict[slotIndex]?.GetName()}' aus Slot {slotIndex}");
            itemDict[slotIndex] = null;
            if (uiInventory != null)
                uiInventory.OnInventorySlotChanged(slotIndex, null);
        }
    }

    // Gibt das Item an einem bestimmten Slot zurück
    public ItemInstance GetItemAtIndex(int slotIndex)
    {
        itemDict.TryGetValue(slotIndex, out var item);
        return item;
    }

    // Gibt alle Items als Liste zurück (z.B. für UI)
    public List<ItemInstance> GetItemList()
    {
        // Gibt die Items in Slot-Reihenfolge zurück
        return itemDict.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList();
    }

    // Gibt das gesamte Dictionary zurück (z.B. für UI)
    public Dictionary<int, ItemInstance> GetItemDict()
    {
        return itemDict;
    }

    public bool AddItemToFirstFreeSlot(ItemInstance item)
    {
        // Gehe alle möglichen Indizes durch (z.B. 0 bis 14 für 15 Slots)
        for (int i = 0; i < uiInventory.inventorySlots.Count; i++)
        {
            if (!itemDict.ContainsKey(i) || itemDict[i] == null)
            {
                AddItemAtIndex(item, i);
                return true;
            }
        }
        Debug.LogWarning("[Inventory] Kein freier Slot gefunden!");
        return false;
    }
}