using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotSchuhe : MonoBehaviour
{


    // Equipment hat "Ausrüstungsslots"
    // ___.equip
    // 🗸 Equipment.equip setzt Sprite für die entsprechenden Ausrüstungsslots richtig
    // 🗸 Equipment.equip entfernt InventoryItem (inventory.removeItem) -> gereglt über PlayerController.
    // Equipment.equip setzt PlayerStat Values entsprechend der ausgerüsteten Items
    // 🗸 Equipment.equip speichert die Items im Ausrüstungsslots -> geregelt über eqList
    // Equipment.equip -> ggf. Deequip

    // 
    // ___.deequip
    // Equipment.deequip nimmt Item.Item aus Ausrüstungsslot raus
    // Equipment.deeuip packt das entsprechende Item zurück ins Inventory


    //Aufgabe: Action<Item> useItemAction;

    private Item storedItem;
    private string itemType;
    private Inventory inventory;
    private GameObject player;
    private string itemName;

    private void Start()
    {
        GameEvents.current.equipSchuhe += equip;
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
