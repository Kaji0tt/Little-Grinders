using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

public class UI_Inventory : MonoBehaviour
{
    private Transform Int_Inventory;
    private Transform Int_Slot;

    private Inventory inventory;
    private IsometricPlayerMovementController charakter;
    private Equipment equipment;


    private void Awake()
    {

        Int_Inventory = transform.Find("Inventory");
        Int_Slot = Int_Inventory.Find("Slot");
    }


    public void SetCharakter (IsometricPlayerMovementController charakter)
    {
        this.charakter = charakter; 
    }

    public void SetInventory(Inventory inventory)
    {
        this.inventory = inventory;

        inventory.OnItemListChanged += Inventory_OnItemListChanged;
        RefreshInventoryItems();
    }

    public void SetEquipment(Equipment equipment)
    {
        this.equipment = equipment;
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
        foreach (Item item in inventory.GetItemList())
        {

            RectTransform SlotRectTransform = Instantiate(Int_Slot, Int_Inventory).GetComponent<RectTransform>();
            SlotRectTransform.gameObject.SetActive(true);

            SlotRectTransform.GetComponent<Button_UI>().ClickFunc = () =>
            {
                inventory.UseItem(item);
                //equipment.equip(item);
                inventory.RemoveItem(item);
                GameEvents.current.EquipChanged(item);

            };
            SlotRectTransform.GetComponent<Button_UI>().MouseRightClickFunc = () =>
            {
                inventory.RemoveItem(item);
                ItemWorld.DropItem(charakter.GetPosition(), item);
            };

            SlotRectTransform.anchoredPosition = new Vector2(x * SlotCellSize, y * SlotCellSize);

            Image image = SlotRectTransform.Find("image").GetComponent<Image>();
            image.sprite = item.GetSprite();
            x++;
            if (x > 5)
            {
                x = 0;
                y++;
            }

        }
    }




}
