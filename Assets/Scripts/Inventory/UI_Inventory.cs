using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CodeMonkey.Utils;
using TMPro;

public class UI_Inventory : MonoBehaviour//, IPointerEnterHandler, IPointerExitHandler
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
        foreach (ItemInstance item in inventory.GetItemList())
        {

            RectTransform SlotRectTransform = Instantiate(Int_Slot, Int_Inventory).GetComponent<RectTransform>();
            SlotRectTransform.gameObject.SetActive(true);
            SlotRectTransform.GetComponent<Int_SlotBtn>().StoreItem(item); //funktioniert

            SlotRectTransform.GetComponent<Button_UI>().ClickFunc = () =>
            {
                inventory.UseItem(item);
                inventory.RemoveItem(item);
                GameEvents.current.EquipChanged(item);
                //eqSlots.equip(item);
                Int_Slot.GetComponent<Int_SlotBtn>().HideItem();

            };
            SlotRectTransform.GetComponent<Button_UI>().MouseRightClickFunc = () =>
            {
                //print("we passed this");
                ItemInstance duplicateItem = new ItemInstance(ItemDatabase.GetItemID(item.ItemID));
                inventory.RemoveItem(item);
                ItemWorld.DropItem(PlayerManager.instance.player.transform.position, duplicateItem);
                Int_Slot.GetComponent<Int_SlotBtn>().HideItem();
                //print("successfull");
            };

            SlotRectTransform.anchoredPosition = new Vector2(x * SlotCellSize, y * SlotCellSize);

            Image image = SlotRectTransform.Find("image").GetComponent<Image>();
            image.sprite = item.icon;

            //Finde das TMPro Element und dessen Text um den Amount anzuzeigen.
            TextMeshProUGUI uiText = SlotRectTransform.Find("amount").GetComponent<TextMeshProUGUI>();

            if (item.amount > 1)
                uiText.SetText(item.amount.ToString());
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


}
