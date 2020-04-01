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
    private GameObject Schuhe, Hose, Brust, Kopf, Weapon, Schmuck;
    private Item item;
    private Inventory inventory;
    private Equipped equipped;

    public enum Equipped
    {
        SlotSchuhe,
        SlotHose,
        SlotBrust,
        SlotKopf,
        SlotWeapon,
        SlotSchmuck
    }

    public void equip(Item item)
    {

        //Store Item Data in own ENUM:



        GameObject EQSlot = GameObject.Find(item.itemType+"Img");
        EQSlot.GetComponent<Image>().sprite = item.GetSprite();

        equipslot(item);
        //das Item sollte vorher im entsprechenden Slot gespeichert werden


        //Schuhe.GetComponent<Image>().sprite = ItemAssets.Instance.SchuheSprite;
        //inventory.RemoveItem(item);  
    }

    private Sprite equipslot(Item item)
    {
        switch (equipped)
        {
            default:
            case Equipped.SlotSchuhe:   return item.GetSprite();
            case Equipped.SlotHose:     return item.GetSprite();
            case Equipped.SlotBrust:    return item.GetSprite();
            case Equipped.SlotKopf:     return item.GetSprite();
            case Equipped.SlotWeapon:   return item.GetSprite();
            case Equipped.SlotSchmuck:  return item.GetSprite();
        }
    }


    public void dequip(string item)
    {
 
        
    }

    

    //Hier sollten die Attribute des Item Typs auf die PlayerStats raufgerechnet werden.
}
