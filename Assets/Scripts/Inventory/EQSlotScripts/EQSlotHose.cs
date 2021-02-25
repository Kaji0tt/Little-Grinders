using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotHose : MonoBehaviour
{
    public static ItemInstance hose_Item;
    private Inventory inventory;
    public IsometricPlayer isometricPlayer;

    private Int_SlotBtn int_slotBtn;

    private void Start()
    {
        GameEvents.current.equipHose += equip;
        int_slotBtn = GetComponent<Int_SlotBtn>();
    }


    public void equip(ItemInstance item)
    {
        if (hose_Item == null)
        {
            hose_Item = item;

            ItemSave.equippedItems.Add(item);

            int_slotBtn.StoreItem(item);

            GetComponent<Image>().sprite = item.icon;

        }
        else
        {
            Dequip();

            hose_Item = item;

            int_slotBtn.storedItem = item;

            GetComponent<Image>().sprite = item.icon;

        }
    }

    public void Dequip()
    {

        inventory = PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory;

        inventory.AddItem(hose_Item);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Dequip(hose_Item);

        ItemSave.equippedItems.Remove(hose_Item);

        hose_Item = null;

        int_slotBtn.storedItem = null;

    }


    public void TaskOnClick()
    {
        if (hose_Item != null)
            Dequip();
    }

    public void LoadItem(ItemInstance item)
    {
        hose_Item = item;

        ItemSave.equippedItems.Add(item);

        item.Equip(PlayerManager.instance.player.GetComponent<PlayerStats>());

        GetComponent<Image>().sprite = item.icon;

        Int_SlotBtn int_slotBtn = gameObject.GetComponentInChildren<Int_SlotBtn>();
        int_slotBtn.storedItem = item;

    }

}
