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
    //private GameObject Schuhe, Hose, Brust, Kopf, Weapon, Schmuck;
    private Item item;
    private Inventory inventory;
    public List<Item> eqList;
    private GameObject EQSlot;
    private string itemType;
    //public Item item;


    public Equipment()
    {
        eqList = new List<Item>();
    }
    //Anlegen des Items
    public void equip(Item item)
    {

        //Store Item Data in own List:
        eqList.Add(new Item { itemName = item.itemName, itemType=item.itemType});
        item = new Item { itemName = item.itemName };

        //If EQSlot already has Item equipped, swap item
        //Equipment.dequip equipped.item
        //
        //Swap Sprite accordingly to Item & give GO Item.values:
        itemType = item.ItemType(item);
        GameObject EQSlot = GameObject.Find("Equipped"+itemType); 
        EQSlot.GetComponent<Image>().sprite = item.GetSprite();
        print(itemType);




    }



    public void dequip(Item item)
    {
        //EQSlot = GameObject.Find("Equipped" + itemType);
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        inventory.AddItem(item);


        /*  switch(Item.ItemType)
          {
              default:
              case EQSlot.name = GameObject.Find("SchuheImg"):         return Item.itemType.Schuhe;
              case gameObject.name = "HoseImg";           return Item.ItemType.Hose;
              case Item.ItemType.Brust:           return ItemAssets.Instance.BrustSprite;
              case Item.ItemType.Kopf:            return ItemAssets.Instance.KopfSprite;
              case Item.ItemType.Weapon:          return ItemAssets.Instance.WeaponSprite;
              case Item.ItemType.Schmuck:         return ItemAssets.Instance.SchmuckSprite;
          }
          */


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

        item = eqList.Find(item => item.itemType.Contains("Schuhe"));
        print(item);

        dequip(item);
        print(itemType);
    }

    //Hier sollten die Attribute des Item Typs auf die PlayerStats raufgerechnet werden.
}
