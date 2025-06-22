using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CodeMonkey.Utils;
using TMPro;

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
        /*
        Int_Inventory = transform.Find("Inventory");
        Int_Slot = Int_Inventory.Find("Slot");
        */
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

        // Zuerst das Dictionary der Consumables durchlaufen
        /*
        foreach (var kvp in inventory.GetConsumableDict())
        {
            if (currentSlot >= maxSlots) break;

            string itemID = kvp.Key;
            int itemAmount = kvp.Value;

            ItemInstance item = new ItemInstance(ItemDatabase.GetItemByID(itemID)); // Hole das Item basierend auf der ID
            CreateInventorySlot(item, itemAmount, ref x, ref y, SlotCellSize);
            currentSlot++;
        }
        */

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
        SlotRectTransform.GetComponent<Int_SlotBtn>().StoreItem(item);

        SlotRectTransform.GetComponent<Button_UI>().ClickFunc = () =>
        {
            if (item.itemType == ItemType.Consumable)
            {
                ItemInstance duplicateItem = new ItemInstance(ItemDatabase.GetItemByID(item.ItemID)); //Trigger 1
                inventory.UseItem(duplicateItem);
                inventory.RemoveItem(duplicateItem);
            }
            else
            {
                inventory.UseItem(item);
                inventory.RemoveItem(item);
            }

            GameEvents.Instance.EquipChanged(item);
            Int_Slot.GetComponent<Int_SlotBtn>().HideItem();
        };

        SlotRectTransform.GetComponent<Button_UI>().MouseRightClickFunc = () =>
        {
            if (item.itemType == ItemType.Consumable)
            {
                ItemInstance duplicateItem = new ItemInstance(ItemDatabase.GetItemByID(item.ItemID));
                inventory.RemoveItem(duplicateItem);
                ItemWorld.DropItem(PlayerManager.instance.player.transform.position, duplicateItem);
            }
            else
            {
                inventory.RemoveItem(item);
                ItemWorld.DropItem(PlayerManager.instance.player.transform.position, item);
            }

            Int_Slot.GetComponent<Int_SlotBtn>().HideItem();
        };

        SlotRectTransform.anchoredPosition = new Vector2(x * SlotCellSize, y * SlotCellSize);

        Image image = SlotRectTransform.Find("image").GetComponent<Image>();
        image.sprite = item.icon;

        // Finde das TMPro-Element und setze den Text für die Menge
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
