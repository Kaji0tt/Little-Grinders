using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeMonkey.Utils;

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

public class Equipment : MonoBehaviour
{
    private static Item item;
    private Inventory inventory;
    private GameObject EQSlot;
    private string itemType;
    private Item[] eqArray = new Item[5];  // static appeared like a solution. problem of static is, that it stores only one variable
                                                  //so if we store the Value of Shoe's, we will lose track of all other Items, considering those
                                                  // are triggered by other game objects.
                                                  //it seems, that we should use "EventHandler" or something for this as a workaround.
                                                  //https://www.codeproject.com/Articles/1474/Events-and-event-handling-in-C
                                                  // https://www.youtube.com/watch?v=gx0Lt4tCDE0
                                                  // #thissucks.
    private GameObject player;


    public void SetEQInventory(Inventory inventory)
    {
        this.inventory = inventory;

    }
    //Anlegen des Items
    public void equip(Item item)
    {
        /*
        //If EQSlot already has Item equipped, swap item
        //Equipment.dequip equipped.item
        //
        //Swap Sprite accordingly to Item & give GO Item.values:
        itemType = this.item.ItemType(item);
        GameObject EQSlot = GameObject.Find("Equipped"+itemType); 
        EQSlot.GetComponent<Image>().sprite = item.GetSprite();
        print(itemType);
        */

        //Store Item Data in own Array:
        switch (item.itemType)
        {
            default:
            case "Schuhe":
                if (eqArray[0] == null) //If != null, dequip eqArray[0], then
                {
                    item = item.Clone();

                    eqArray[0] = item;
                }
                            
                break;

            case "Hose":
                if (eqArray[0] == null) //If != null, dequip eqArray[0], then
                {
                    item = item.Clone();
                    eqArray[0] = item;
                }
                break;

            case "Brust":
                if (eqArray[0] == null) //If != null, dequip eqArray[0], then
                {
                    item = item.Clone();
                    eqArray[0] = item;
                }
                break;

            case "Kopf":
                if (eqArray[0] == null) //If != null, dequip eqArray[0], then
                {
                    item = item.Clone();
                    eqArray[0] = item;
                }
                break;

            case "Weapon":
                if (eqArray[0] == null) //If != null, dequip eqArray[0], then
                {
                    item = item.Clone();
                    eqArray[0] = item;
                    print("ItemName während Equip:" + item.itemName);
                }
                break;

            case "Schmuck":
                if (eqArray[0] == null) //If != null, dequip eqArray[0], then
                {
                    item = item.Clone();
                    eqArray[0] = item;
                }
                break;
        }
        //Setzen des Sprites (ItemType hat wieder ein return Wert)
        print("itemName on Equip:" + item.itemName);
        print("itemType on Equip:" + item.itemType);

        itemType = item.ItemType(item);
        EQSlot = GameObject.Find("Equipped" + item.itemType);
        EQSlot.GetComponent<Image>().sprite = item.GetSprite();

    }



    public void dequip(Item item)
    {
        //EQSlot = GameObject.Find("Equipped" + itemType);
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        player = GameObject.Find("Charakter");
        inventory = player.GetComponent<IsometricPlayerMovementController>().Inventory;
        print("dequip ItemName:" + item.itemName);
        print("dequip ItemType:" + item.itemType);
        inventory.AddItem(item);




    }
    /*public override bool Equals(object obj)
    {
        if (obj == null) return false;
        Item objAsPart = obj as Item;
        if (objAsPart == null) return false;
        else return Equals(objAsPart);
    }*/

    public void TaskOnClick()
    {

        // item = eqList.Find( item => item.itemType.Contains("Schuhe"));
        //item = eqList.Find(item => item.itemName.ToString().Contains("Sandalen"));


        switch (gameObject.name)
        {
            default:
            case "EquippedSchuhe":
                item = eqArray[0];
                print("ToC Item:" + item);
                print("ToC Array[0] Prüfung:" + eqArray[0]);
                dequip(item);

                break;

            case "EquippedHose":
                item = eqArray[1];
                dequip(item);
                break;

            case "EquippedBrust":
                item = eqArray[2];
                dequip(item);
                break;

            case "EquippedKopf":
                item = eqArray[3];
                dequip(item);
                break;

            case "EquippedWeapon":
                item = eqArray[4];
                print("ToC Item:" + item);
                print("ToC Array[4] Prüfung:" + eqArray[4]);
                dequip(item);
                break;

            case "EquippedSchmuck":
                item = eqArray[5];
                dequip(item);
                break;

        }


        //dequip(item);

    }


    //Hier sollten die Attribute des Item Typs auf die PlayerStats raufgerechnet werden.
}
