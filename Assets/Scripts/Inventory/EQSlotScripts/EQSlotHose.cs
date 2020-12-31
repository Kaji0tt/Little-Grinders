using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotHose : MonoBehaviour
{

    public Item storedItem;
    private Inventory inventory;
    private GameObject player;
    public IsometricPlayer isometricPlayer;
    private Int_SlotBtn int_slotBtn;

    private void Start()
    {
        GameEvents.current.equipHose += equip;
        int_slotBtn = GetComponent<Int_SlotBtn>();
    }


    public void equip(Item item)
    {
        if (storedItem == null)
        {
            storedItem = item;
            int_slotBtn.StoreItem(item);
            GetComponent<Image>().sprite = item.icon;
            player = GameObject.Find("Charakter");

        }
        else
        {
            Dequip();
            storedItem = item;
            int_slotBtn.storedItem = item;
            GetComponent<Image>().sprite = item.icon;
            player = GameObject.Find("Charakter");
        }
    }

    public void Dequip()
    {
        player = GameObject.Find("Charakter");

        inventory = player.GetComponent<IsometricPlayer>().Inventory;
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
