using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
/*
{
    public enum ItemType
    {
        Schuhe,
        Hose,
        Brust,
        Kopf,
        Weapon,
        Schmuck

    }
    public ItemType itemType;
    public int amount;
    public string ItemName;


    public Sprite GetSprite()
    {
        switch (itemType)
        {
            default:
            case ItemType.Schuhe:       return ItemAssets.Instance.SchuheSprite;
            case ItemType.Hose:         return ItemAssets.Instance.HoseSprite;
            case ItemType.Brust:        return ItemAssets.Instance.BrustSprite;
            case ItemType.Kopf:         return ItemAssets.Instance.KopfSprite;
            case ItemType.Weapon:       return ItemAssets.Instance.WeaponSprite;
            case ItemType.Schmuck:      return ItemAssets.Instance.SchmuckSprite;
        }
    }

}
*/


{
    public enum ItemName
    {
        Einfache_Sandalen,
        Einfache_Hose,
        Einfache_Brust,
        Einfacher_Hut,
        Einfaches_Schwert,
        Anhänger

    }
    public int amount;
    public ItemName itemName;
    public string itemType;
    //Stat Integers
    public int attackPower, abillityPower, attackSpeed, armor;
    public float movementSpeed, attackSped;

    public Item Clone()
    {
        this.itemName = itemName;
        this.itemType = itemType;
        this.amount = amount;

        return this;
    }

    public Sprite GetSprite()
    {
        switch (itemName)
        {
            default:
            case ItemName.Einfache_Sandalen: return ItemAssets.Instance.SchuheSprite;
            case ItemName.Einfache_Hose: return ItemAssets.Instance.HoseSprite;
            case ItemName.Einfache_Brust: return ItemAssets.Instance.BrustSprite;
            case ItemName.Einfacher_Hut: return ItemAssets.Instance.KopfSprite;
            case ItemName.Einfaches_Schwert: return ItemAssets.Instance.WeaponSprite;
            case ItemName.Anhänger: return ItemAssets.Instance.SchmuckSprite;
        }
    }

    public string ItemType(Item item)
    {
        switch (itemName)
        {
            default:
            //Schuhe
            case ItemName.Einfache_Sandalen: itemType = "Schuhe";
                break;


            //Hosen
            case ItemName.Einfache_Hose: itemType = "Hose";
                break;



            //Brüste
            case ItemName.Einfache_Brust: itemType = "Brust";
                break;



            //Köpfe
            case ItemName.Einfacher_Hut: itemType = "Kopf";
                break;



            //Waffen
            case ItemName.Einfaches_Schwert: itemType = "Weapon";
                break;


            //Schmuckstücke
            case ItemName.Anhänger: itemType = "Schmuck";
                break;



        }
        return itemType;
    }

 

    public string ItemStats(Item item)
    {
        switch (itemName)
        {
                default:
                //Schuhe
                case ItemName.Einfache_Sandalen:
                    armor = 1;
                    movementSpeed = 5;
                    break;


                //Hosen
                case ItemName.Einfache_Hose:
                    armor = 3;
                    break;



                //Brüste
                case ItemName.Einfache_Brust:
                    armor = 5;
                    break;



                //Köpfe
                case ItemName.Einfacher_Hut:
                    armor = 2;
                    break;



                //Waffen
                case ItemName.Einfaches_Schwert:
                    attackPower = 3;
                    break;


                //Schmuckstücke
                case ItemName.Anhänger:
                    abillityPower = 2;
                    break;



        }

        string itemStats = "Armor:" + armor + "Movementspeed" + movementSpeed + "Attack Power" + attackPower + "Abillity Power" + abillityPower;
            return itemStats;


    }
}


