using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class ItemSave
{
    //Problem könnte daran liegen, dass es sich um Static Values handelt. WorkAround: Konstruktooors.
    //public static string brust, hose, kopf, schmuck, schuhe, weapon; 
    
    

    public static string brust, hose, kopf, schmuck, schuhe, weapon;


    public static List<ItemInstance> equippedItems = new List<ItemInstance>();


    public static List<ItemModsData> modsBrust = new List<ItemModsData>(), modsHose = new List<ItemModsData>(), 
                                     modsKopf = new List<ItemModsData>(), modsSchmuck = new List<ItemModsData>(), 
                                     modsSchuhe = new List<ItemModsData>(), modsWeapon = new List<ItemModsData>();


    public static string brust_r, hose_r, kopf_r, schmuck_r, schuhe_r, weapon_r;

    public static void SaveEquippedItems()
    {
        foreach(ItemInstance item in equippedItems)
        {
            Debug.Log (item.addedItemMods.Count);
            switch (item.itemType)
            {
                case ItemType.Kopf:

                    kopf = item.ItemID;

                    kopf_r = item.itemRarity;

                    foreach (ItemModsData mod in item.addedItemMods)
                    {
                        modsKopf.Add(mod);
                    }


                    break;

                case ItemType.Brust:

                    brust = item.ItemID;

                    brust_r = item.itemRarity;

                    foreach (ItemModsData mod in item.addedItemMods)
                    {

                        modsBrust.Add(mod);
                    }

                    break;

                case ItemType.Beine:

                    hose = item.ItemID;

                    hose_r = item.itemRarity;


                    foreach (ItemModsData mod in item.addedItemMods)
                    {
                        modsHose.Add(mod);
                    }

                    break;

                case ItemType.Schuhe:

                    schuhe = item.ItemID;

                    schuhe_r = item.itemRarity;

                    foreach (ItemModsData mod in item.addedItemMods)
                    {
                        modsBrust.Add(mod);
                    }

                    break;

                case ItemType.Schmuck:

                    schmuck = item.ItemID;

                    schmuck_r = item.itemRarity;

                    foreach (ItemModsData mod in item.addedItemMods)
                    {
                        modsSchmuck.Add(mod);
                    }

                    break;

                case ItemType.Weapon:

                    weapon = item.ItemID;

                    weapon_r = item.itemRarity;

                    foreach (ItemModsData mod in item.addedItemMods)
                    {
                        modsWeapon.Add(mod);
                    }

                    break;
                case ItemType.Consumable:
                    //Placeholder. Call for ItemDelte or something. --> Irgendwie, wird das irgendwo anders schon gemacht.
                    break;

            }
        }
    }




}
