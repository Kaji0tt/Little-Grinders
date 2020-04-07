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

    private Item item = new Item();
    private string itemType;
    private Inventory inventory;
    private GameObject player;

    private void Start()
    {
        GameEvents.current.equipSchuhe += equip;
    }

    public void equip(Item item)
    {

        itemType = item.ItemType(item);
        GetComponent<Image>().sprite = item.GetSprite();


    }

    public void dequip(Item item)

    {
        player = GameObject.Find("Charakter");
        print("Folgendes Item soll ausgezogen werden:" + item.itemName + " es handelt sich um ein " + item);
        inventory = player.GetComponent<IsometricPlayerMovementController>().Inventory;
        inventory.AddItem(item);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        this.item = null;


    }


    public void TaskOnClick()
    {

        dequip(item);

    }



}
