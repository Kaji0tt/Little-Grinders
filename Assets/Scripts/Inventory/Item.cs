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

    public StatModifier
        hp,
        armor,
        attackPower,
        abilityPower,
        movementSpeed,
        attackSpeed;
        
    public int amount;
    public ItemName itemName;
    public string itemType;
    //Stat Integers
    public CharStats charStats;
    //public float attackSpeed;

    //Stat Mods
    //StatModifier mod1, mod2;

    public Item Clone()
    {
       /* Vorerst ausgeklammert, da kein erkennbarer nutzen. 
        this.itemName = itemName;
        this.itemType = itemType;
        this.amount = amount;
        */

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

 
    //Hier würden dann die Stat Modifier auftauchen, ggf. ItemStats überarbeiten.
    public StatModifier ItemStats(Item item)
    {
        
        switch (itemName)
        {
                default:
                //Schuhe
                // To Do: Stack overflow Error beseitigen, MobStats in die BaseValue integrieren
                case ItemName.Einfache_Sandalen:
                armor = new StatModifier(5, StatModType.Flat, this);
                hp = new StatModifier(10, StatModType.Flat, this);
                movementSpeed = (new StatModifier(0.1f, StatModType.PercentMult, this));
                //charStats.armor.AddModifier(new StatModifier(10, StatModType.Flat, this));



                break;


                //Hosen
                case ItemName.Einfache_Hose:
                    armor = new StatModifier(3, StatModType.Flat, this);
                break;



                //Brüste
                case ItemName.Einfache_Brust:
                armor = new StatModifier(5, StatModType.Flat, this);
                break;



                //Köpfe
                case ItemName.Einfacher_Hut:
                armor = new StatModifier(2, StatModType.Flat, this);
                break;



                //Waffen
                case ItemName.Einfaches_Schwert:
                    attackPower = new StatModifier(3, StatModType.Flat, this);
                break;


                //Schmuckstücke
                case ItemName.Anhänger:
                    abilityPower = new StatModifier(2, StatModType.Flat, this);
                break;



        }

        //Hier geschiet ein grob fahrlässiger fehler, wir addieren und geben nicht die einzelnen Schubladen der Itemstats zurück.
        //int itemStats = armor + movementSpeed + attackPower  + abilityPower;
        return ItemStats(item);


    }


}


