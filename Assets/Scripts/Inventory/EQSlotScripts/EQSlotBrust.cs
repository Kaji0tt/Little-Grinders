using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotBrust : MonoBehaviour
{
    public static ItemInstance brust_Item;

    private Int_SlotBtn int_slotBtn;

    private void Start()
    {
        GameEvents.current.equipBrust += equip;
        int_slotBtn = GetComponent<Int_SlotBtn>();
        //Debug.Log("On Start, i got loaded the following Item: " + brust_Item.ItemName + " with ID: " + brust_Item.ItemID);
    }


    public void equip(ItemInstance item)
    {
        if (brust_Item == null)
        {
            brust_Item = item;

            //PlayerManager.instance.player.equippedItems.Add(item);

            int_slotBtn.StoreItem(item);

            GetComponent<Image>().sprite = item.icon;


        }
        else
        {
            Dequip();

            brust_Item = item;

            int_slotBtn.storedItem = item;

            GetComponent<Image>().sprite = item.icon;

        }
    }

    public void Dequip()
    {


        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.AddItem(brust_Item);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Dequip(brust_Item);

        //ItemSave.equippedItems.Remove(brust_Item);

        brust_Item = null;

        int_slotBtn.storedItem = null;
    }


    public void TaskOnClick()
    {
        if (brust_Item != null)
            Dequip();
    }

    public void LoadItem(ItemInstance item)
    {
        //Find Item by ID?

        brust_Item = item;

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().equippedItems.Add(item);

        item.Equip(PlayerManager.instance.player.GetComponent<PlayerStats>());

        GetComponent<Image>().sprite = item.icon;


        Int_SlotBtn int_slotBtn = gameObject.GetComponentInChildren<Int_SlotBtn>();
        int_slotBtn.storedItem = item;

    }
}
