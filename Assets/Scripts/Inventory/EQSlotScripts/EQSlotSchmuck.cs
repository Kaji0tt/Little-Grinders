﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotSchmuck : MonoBehaviour
{
    public static ItemInstance schmuck_Item;

    private Int_SlotBtn int_slotBtn;



    private void OnEnable()
    {
        GameEvents.instance.equipSchmuck += equip;

        int_slotBtn = GetComponent<Int_SlotBtn>();

    }

    private void OnDisable()
    {
        GameEvents.instance.equipSchmuck -= equip;
    }


    public void equip(ItemInstance item)
    {
        if (schmuck_Item == null)
        {
            schmuck_Item = item;

            //ItemSave.equippedItems.Add(item);

            int_slotBtn.StoreItem(item);

            GetComponent<Image>().sprite = item.icon;


        }
        else
        {
            Dequip();

            schmuck_Item = item;

            int_slotBtn.storedItem = item;

            GetComponent<Image>().sprite = item.icon;

        }
    }

    public void Dequip()
    {
        

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.AddItem(schmuck_Item);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Dequip(schmuck_Item);

        //ItemSave.equippedItems.Remove(schmuck_Item);

        schmuck_Item = null;

        int_slotBtn.storedItem = null;

    }


    public void TaskOnClick()
    {
        if(schmuck_Item != null)
        Dequip();
    }
    public void LoadItem(ItemInstance item)
    {
        schmuck_Item = item;

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().equippedItems.Add(item);

        item.Equip(PlayerManager.instance.player.GetComponent<PlayerStats>());

        GetComponent<Image>().sprite = item.icon;

        Int_SlotBtn int_slotBtn = gameObject.GetComponentInChildren<Int_SlotBtn>();
        int_slotBtn.storedItem = item;

    }

}
