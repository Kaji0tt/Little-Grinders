using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CodeMonkey.Utils;
using TMPro;
using System.Linq;

public class UI_Inventory : MonoBehaviour
{
    [SerializeField]
    private Transform slotParent;
    [SerializeField]
    private Transform slotTransform;

    public Inventory inventory; // Jetzt public, damit Inspector-Zuweisung möglich ist

    private IsometricPlayer charakter;

    public List<Int_SlotBtn> inventorySlots = new List<Int_SlotBtn>();

    /// Singleton Pattern
    public static UI_Inventory instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        int maxSlots = 15;

        for (int i = 0; i < maxSlots; i++)
        {
            var slotGO = Instantiate(slotTransform, slotParent);
            slotGO.gameObject.SetActive(true);
            var slotBtn = slotGO.GetComponent<Int_SlotBtn>();
            slotBtn.slotIndex = i;

            slotBtn.slotImage = slotGO.GetComponentInChildren<Image>(); // oder: slotGO.GetComponentInChildren<Image>();

            inventorySlots.Add(slotBtn);
        }

        // Setze die Referenz
        if (inventory != null)
            inventory.uiInventory = this;

        // Initiales Füllen der Slots aus dem Inventory-Dictionary
        RefreshAllSlotsFromInventory();
    }

    public void SetCharakter(IsometricPlayer charakter)
    {
        this.charakter = charakter;
    }

    public void RefreshAllSlotsFromInventory()
    {
        // Gehe alle Slots durch und hole das Item direkt aus dem Inventory-Dictionary
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            ItemInstance item = null;
            if (inventory != null && inventory.itemDict.TryGetValue(i, out item))
            {
                inventorySlots[i].StoreItem(item);
            }
            else
            {
                inventorySlots[i].ClearSlot();
            }
        }
    }

    public void OnInventorySlotChanged(int slotIndex, ItemInstance item)
    {
        if (slotIndex >= 0 && slotIndex < inventorySlots.Count)
        {
            // Blockiere Click-Events auf diesem Slot während des Updates
            inventorySlots[slotIndex].BlockClicksTemporarily();
            
            inventorySlots[slotIndex].StoreItem(item);
            // Synchronisiere das Dictionary direkt
            if (inventory != null)
                inventory.itemDict[slotIndex] = item;
        }
    }
}
