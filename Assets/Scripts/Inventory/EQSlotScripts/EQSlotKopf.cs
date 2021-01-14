using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotKopf : MonoBehaviour
{
    public Item storedItem;
    private Inventory inventory;
    public IsometricPlayer isometricPlayer;

    private Int_SlotBtn int_slotBtn;

    private void Start()
    {
        GameEvents.current.equipKopf += equip;
        int_slotBtn = GetComponent<Int_SlotBtn>();
    }


    public void equip(Item item)
    {
        if (storedItem == null)
        {
            storedItem = item;
            int_slotBtn.StoreItem(item);
            GetComponent<Image>().sprite = item.icon;


        }
        else
        {
            Dequip();
            storedItem = item;
            int_slotBtn.storedItem = item;
            GetComponent<Image>().sprite = item.icon;

        }
    }

    public void Dequip()
    {


        inventory = PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory;
        inventory.AddItem(storedItem);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        isometricPlayer.Dequip(storedItem);
        this.storedItem = null;
        int_slotBtn.storedItem = null;
    }


    public void TaskOnClick()
    {
        Dequip();
    }

}
