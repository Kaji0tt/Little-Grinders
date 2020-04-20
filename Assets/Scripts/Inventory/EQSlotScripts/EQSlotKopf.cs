using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotKopf : MonoBehaviour
{
    private Item storedItem = new Item();
    private string itemType;
    private Inventory inventory;
    private GameObject player;
    private string itemName;

    private void Start()
    {
        GameEvents.current.equipKopf += equip;
    }

    public void equip(Item item)
    {
        itemName = item.itemName.ToString();
        itemType = item.ItemType(item);
        storedItem = item;
        GetComponent<Image>().sprite = item.GetSprite();


    }

    public void dequip()

    {
        print(itemName);
        player = GameObject.Find("Charakter");
        print("Folgendes Item soll ausgezogen werden:" + storedItem.itemName + " es handelt sich um ein " + storedItem);
        inventory = player.GetComponent<IsometricPlayerMovementController>().Inventory;
        inventory.AddItem(storedItem);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        this.storedItem = null;


    }


    public void TaskOnClick()
    {

        dequip();

    }
}
