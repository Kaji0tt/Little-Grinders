using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Equipment hat "Ausrüstungsslots"
// ___.equip
// Equipment.equip setzt Sprite für die entsprechenden Ausrüstungsslots richtig
// Equipment.equip entfernt InventoryItem (inventory.removeItem) -> Braucht acces zu Inventory
// Equipment.equip setzt PlayerStat Values entsprechend der ausgerüsteten Items
// Equipment.equip speichert die Items im Ausrüstungsslots -> return funktion
// Equipment.equip -> ggf. Deequip

// 
// ___.deequip
// Equipment.deequip nimmt Item.Item aus Ausrüstungsslot raus
// Equipment.deeuip

public class Equipment : MonoBehaviour
{
    //private GameObject Schuhe, Hose, Brust, Kopf, Weapon, Schmuck;
    private Item item;
    private Inventory inventory;
    public List<Item> eqlist;
    private GameObject EQSlot;
    private string itemType;



    //Anlegen des Items
    public void equip(Item item)
    {

        //Store Item Data in own List:
        eqlist = new List<Item>(); // möglicherweise sollte nicht jedesmal eine neue liste erstellt werden
        AddItem(new Item { itemName = item.itemName, amount = 1 });

        //Swap Sprite accordingly to Item:
        itemType = item.ItemType(item);
        GameObject EQSlot = GameObject.Find(itemType+"Img"); // <- elegantere Lösung finden
        EQSlot.GetComponent<Image>().sprite = item.GetSprite();
        //das Item sollte vorher im entsprechenden Slot gespeichert werden
        print(eqlist);


    }
    public void AddItem(Item item)
    {
        eqlist.Add(item);

    }


    public void dequip()
    {
        EQSlot = gameObject;
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
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
    
    public void RemoveItem(Item item)
    {
        eqlist.Remove(item);
    }

    //Hier sollten die Attribute des Item Typs auf die PlayerStats raufgerechnet werden.
}
