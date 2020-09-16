using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Item0000", menuName = "Assets/Item")]
public class Item : ScriptableObject
{
    [Header("Details")]
    public string ItemID;
    public string ItemName;
    [TextArea]
    public string ItemDescription;
    public string ItemType;
    public Sprite GetSprite;
    [Space]
    [Header("Flat-Werte")]
    public int hp;
    public int armor;
    public int attackPower;
    public int abilityPower;
    public int attackSpeed;
    public int movementSpeed;
    [Space]
    [Header("Prozent-Werte")]
    public int p_hp;
    public int p_armor;
    public int p_attackPower;
    public int p_abilityPower;
    public int p_attackSpeed;

    //private List<StatModifier> itemModifiers = new List<StatModifier>(); //Mit einer Liste wäre bestimmt entspannter, dafür bin ich aber zu dumm.
    //Kein Bock mich jetzt nochmal mit der Syntax zu befassen, wenn das Ziel so nah ist.
    private StatModifier m1, m2, m3, m4, m5, m6, m1p, m2p, m3p, m4p, m5p;

    public void Equip(IsometricPlayer isometricPlayer)
    {
        //Store Modifiers
        if (hp !=0)             {m1 = new StatModifier(hp, StatModType.Flat, this);}
        if (armor != 0)         {m2 = new StatModifier(armor, StatModType.Flat, this);}
        if (attackPower != 0)   {m3 = new StatModifier(attackPower, StatModType.Flat, this);}
        if (abilityPower != 0)  {m4 = new StatModifier(abilityPower, StatModType.Flat, this);}
        if (attackSpeed != 0)   {m5 = new StatModifier(attackSpeed, StatModType.Flat, this);}
        if (movementSpeed != 0) {m6 = new StatModifier(movementSpeed, StatModType.PercentMult, this);}


        if (p_hp != 0)          {m1p = new StatModifier(attackPower, StatModType.PercentMult, this);}
        if (p_armor != 0)       {m2p = new StatModifier(abilityPower, StatModType.PercentMult, this);}
        if (p_attackPower != 0) {m3p = new StatModifier(attackPower, StatModType.PercentMult, this);}
        if (abilityPower != 0)  {m4p = new StatModifier(abilityPower, StatModType.PercentMult, this);}
        if (attackSpeed != 0)   {m5p = new StatModifier(attackSpeed, StatModType.PercentMult, this);}

        //Add Modifiers to Character
        if (m1 != null)isometricPlayer.Hp.AddModifier(m1);
        if (m2 != null)isometricPlayer.Armor.AddModifier(m2);
        if (m3 != null)isometricPlayer.AttackPower.AddModifier(m3);
        if (m4 != null)isometricPlayer.AbilityPower.AddModifier(m4);
        if (m5 != null)isometricPlayer.AttackSpeed.AddModifier(m5);
        if (m6 != null)isometricPlayer.MovementSpeed.AddModifier(m6);

        if (m1p != null) isometricPlayer.Hp.AddModifier(m1p);
        if (m2p != null) isometricPlayer.Armor.AddModifier(m2p);
        if (m3p != null) isometricPlayer.AttackPower.AddModifier(m3p);
        if (m4p != null) isometricPlayer.AbilityPower.AddModifier(m4p);
        if (m5p != null) isometricPlayer.AttackSpeed.AddModifier(m5p);







    }

    public void Unequip (IsometricPlayer isometricPlayer, Item item)
    {

        if (m1 != null) isometricPlayer.Hp.RemoveModifier(m1);
        if (m2 != null) isometricPlayer.Armor.RemoveModifier(m2);
        if (m3 != null) isometricPlayer.AttackPower.RemoveModifier(m3);
        if (m4 != null) isometricPlayer.AbilityPower.RemoveModifier(m4);
        if (m5 != null) isometricPlayer.AttackSpeed.RemoveModifier(m5);
        if (m6 != null) isometricPlayer.MovementSpeed.RemoveModifier(m6);

        if (m1p != null) isometricPlayer.Hp.RemoveModifier(m1p);
        if (m2p != null) isometricPlayer.Armor.RemoveModifier(m2p);
        if (m3p != null) isometricPlayer.AttackPower.RemoveModifier(m3p);
        if (m4p != null) isometricPlayer.AbilityPower.RemoveModifier(m4p);
        if (m5p != null) isometricPlayer.AttackSpeed.RemoveModifier(m5p);

    }



    // Altes Item-Management.
    /*
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
    //public CharStats charStats;
    //public float attackSpeed;

    //Stat Mods
    //StatModifier mod1, mod2;


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


    public StatModifier[] itemValues;

    
    


    private CharStats charStats;

    public StatModifier[] ItemStats(Item item)
    {

        StatModifier[] itemValues = new StatModifier[2];
        switch (itemName)
        {
                default:
                //Schuhe
                // To Do: Stack overflow Error beseitigen, MobStats in die BaseValue integrieren
                case ItemName.Einfache_Sandalen:
                itemValues[0] = new StatModifier(10, StatModType.Flat, this);
                itemValues[1] = new StatModifier(5, StatModType.Flat, this);

                break;


                //Hosen
                case ItemName.Einfache_Hose:
                    //armor = new StatModifier(3, StatModType.Flat, this);
                break;



                //Brüste
                case ItemName.Einfache_Brust:
                //armor = new StatModifier(5, StatModType.Flat, this);
                break;



                //Köpfe
                case ItemName.Einfacher_Hut:
                //armor = new StatModifier(2, StatModType.Flat, this);
                break;



                //Waffen
                case ItemName.Einfaches_Schwert:
                itemValues[1] = new StatModifier(10, StatModType.Flat, this);
                break;


                //Schmuckstücke
                case ItemName.Anhänger:
                    //abilityPower = new StatModifier(2, StatModType.Flat, this);
                break;



        }



        //Hier geschiet ein grob fahrlässiger fehler, wir addieren und geben nicht die einzelnen Schubladen der Itemstats zurück.
        //int itemStats = armor + movementSpeed + attackPower  + abilityPower;
        //Debug.Log(i_hp);
        //float itemStats = Hp;
        //Debug.Log(itemValues[1]);
        return itemValues;


    }
    */
}


