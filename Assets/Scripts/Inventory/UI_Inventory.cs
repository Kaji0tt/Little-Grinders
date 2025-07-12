using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CodeMonkey.Utils;
using TMPro;
using System.Linq;

public class UI_Inventory : MonoBehaviour //, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Transform Int_Inventory;
    [SerializeField]
    private Transform Int_Slot;

    private Inventory inventory;

    private IsometricPlayer charakter;

    //private EQSlots eqSlots;


    private void Awake()
    {
    }


    public void SetCharakter (IsometricPlayer charakter)
    {
        this.charakter = charakter; 
    }

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;

        inventory.OnItemListChanged += Inventory_OnItemListChanged;
        RefreshInventoryItems();
    }



    private void Inventory_OnItemListChanged(object sender, System.EventArgs e)
    {
        RefreshInventoryItems();
    }

    
    private void RefreshInventoryItems()
    {
        foreach (Transform child in Int_Inventory)
        {
            if (child == Int_Slot) continue;
            Destroy(child.gameObject);
        }

        int x = 0;
        int y = 0;
        float SlotCellSize = 50f;
        int maxSlots = 15;
        int currentSlot = 0;

        // Dann die Liste der Nicht-Consumables durchlaufen
        foreach (ItemInstance item in inventory.GetItemList())
        {
            if (currentSlot >= maxSlots) break;

            CreateInventorySlot(item, 1, ref x, ref y, SlotCellSize);
            currentSlot++;
        }
    }


    private void CreateInventorySlot(ItemInstance item, int amount, ref int x, ref int y, float SlotCellSize)
    {
        RectTransform SlotRectTransform = Instantiate(Int_Slot, Int_Inventory).GetComponent<RectTransform>();
        SlotRectTransform.gameObject.SetActive(true);
        Int_SlotBtn intSlotBtn = SlotRectTransform.GetComponent<Int_SlotBtn>();

        intSlotBtn.StoreItem(item);

        SlotRectTransform.GetComponent<Button_UI>().ClickFunc = () =>
        {
            Debug.Log($"[UI_Inventory] Button clicked! SlotType: {intSlotBtn.slotType}, StoredItem: {(intSlotBtn.storedItem != null ? intSlotBtn.storedItem.GetName() : "null")}, InventoryCount: {inventory.GetItemList().Count}");

            // Ausrüsten: Wenn Inventar-Slot angeklickt wird
            if (intSlotBtn.slotType == ItemType.None && item != null)
            {
                // Suche Equip-Slot mit passendem ItemType
                var allSlots = GameObject.FindObjectsOfType<Int_SlotBtn>();
                var equipSlot = allSlots.FirstOrDefault(slot => slot.slotType == item.itemType);

                if (equipSlot != null)
                {
                    // Falls im Equip-Slot schon ein Item liegt, ins Inventar zurücklegen
                    if (equipSlot.storedItem.ItemName != "")
                    {
                        Debug.Log($"[UI_Inventory] Tausche Item: {equipSlot.storedItem.GetName()} zurück ins Inventar!");
                        inventory.AddItem(equipSlot.storedItem, 1);
                    }

                    // Jetzt Item ausrüsten
                    equipSlot.StoreItem(item);
                    inventory.RemoveItem(item);
                }
                else
                {
                    Debug.LogWarning($"[UI_Inventory] Kein Equip-Slot für ItemType {item.itemType} gefunden!");
                }
            }

            Int_Slot.GetComponent<Int_SlotBtn>().HideItem();
        };

        SlotRectTransform.anchoredPosition = new Vector2(x * SlotCellSize, y * SlotCellSize);

        Image image = SlotRectTransform.Find("image").GetComponent<Image>();
        image.sprite = item.icon;

        TextMeshProUGUI uiText = SlotRectTransform.Find("amount").GetComponent<TextMeshProUGUI>();
        if (amount > 1)
            uiText.SetText(amount.ToString());
        else
            uiText.SetText("");

        x++;
        if (x > 5)
        {
            x = 0;
            y++;
        }
    }

}
