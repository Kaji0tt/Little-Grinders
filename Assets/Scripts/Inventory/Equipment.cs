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
    private Item item;
    private Inventory inventory;
    //public List<Item> eqList = new List<Item>();
    private GameObject EQSlot;
    private string itemType;
    private Item[] eqArray = new Item[5];
    //public Item item;


    public Equipment()
    {

    }
    //Anlegen des Items
    public void equip(Item item)
    {
        /*
        //Store Item Data in own List:
        eqList.Add(new Item { itemName = item.itemName, itemType=item.itemType});
        this.item = new Item { itemName = item.itemName };

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
            case "Schuhe": if(eqArray[0] == null) eqArray[0] = item;        //If != null, dequip eqArray[0], then
                print(item);
                print(eqArray[0]);
                break;

            case "Hose": if (eqArray[1] == null) eqArray[1] = item;

                break;

            case "Brust":
                if (eqArray[2] == null) eqArray[2] = item;

                break;

            case "Kopf":
                if (eqArray[3] == null) eqArray[3] = item;

                break;

            case "Weapon":
                if (eqArray[4] == null) eqArray[4] = item;

                break;

            case "Schmuck":
                if (eqArray[5] == null) eqArray[5] = item;

                break;
        }
        //Setzen des Sprites (ItemType hat wieder ein return Wert)
        //print(eqArray[0]);
        itemType = item.ItemType(item);
        EQSlot = GameObject.Find("Equipped" + item.itemType);
        EQSlot.GetComponent<Image>().sprite = item.GetSprite();

    }



    public void dequip(Item item)
    {
        //EQSlot = GameObject.Find("Equipped" + itemType);
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
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
                Item item2 = eqArray[0];
                print(eqArray[0]);
                print(item2);
                
                dequip(item2);
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
