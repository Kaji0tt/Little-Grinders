using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class ItemSave
{
    public static string brust, hose, kopf, schmuck, schuhe, weapon;
    public static List<Item> equippedItems = new List<Item>();

    public static void SaveEquippedItems()
    {
        foreach(Item item in equippedItems)
        {
            switch (item.itemType)
            {
                case ItemType.Kopf:
                    kopf = item.ItemID;
                    break;
                case ItemType.Brust:
                    brust = item.ItemID;
                    break;
                case ItemType.Beine:
                    hose = item.ItemID;
                    break;
                case ItemType.Schuhe:
                    schuhe = item.ItemID;
                    break;
                case ItemType.Schmuck:
                    schmuck = item.ItemID;
                    break;
                case ItemType.Weapon:
                    weapon = item.ItemID;
                    break;
                case ItemType.Consumable:
                    //Placeholder. Call for ItemDelte or something. --> Irgendwie, wird das irgendwo anders schon gemacht.
                    break;

            }
        }
    }



}
