using UnityEngine;
using System;
using System.Collections.Generic;

public enum ItemType {Kopf, Brust, Beine, Schuhe, Schmuck, Weapon, Consumable}
public enum ItemRarity {Standard, Selten, Rar, Episch, Einzigartig}

[CreateAssetMenu(fileName = "Item0000", menuName = "Assets/Item")]
public class Item : ScriptableObject, IDescribable
{
    [Header("Details")]
    public string ItemID;
    public string ItemName;
    [TextArea]
    public string ItemDescription;
    [SerializeField]
    public ItemType itemType;
    public ItemRarity itemRarity;   // Not used yet. For further Information to implement the infleunce of itemRarity check: https://www.youtube.com/watch?v=dvSYloBxzrU
    public int Range;
    public Sprite GetSprite;        //scale item.sprite always to correct size, for ItemWorld to Spawn it in according size aswell. either here or in itemworld
    public int percent;

    [Space]
    [Header("Flat-Werte")]
    public int hp;
    public int armor;
    public int attackPower;
    public int abilityPower;

    [Space]
    [Header("Prozent-Werte")]
    public int p_hp;
    public int p_armor;
    public int p_attackPower;
    public int p_abilityPower;
    public int p_attackSpeed;
    public int p_movementSpeed;

    [Space]
    [Header("Consumables")]
    public int c_hp;

    private string[] modStrings = new string[] {"Hp" ,"Armor" , "Attack Power", "Ability Power", "p_hp", "p_armor", "p_attackPower", "p_abilityPower", "p_attackSpeed", "p_movemenSpeed" };


    //private List<StatModifier> itemModifiers = new List<StatModifier>(); //Mit einer Liste wäre bestimmt entspannter, dafür bin ich aber zu dumm.
    //Kein Bock mich jetzt nochmal mit der Syntax zu befassen, wenn das Ziel so nah ist.
    private StatModifier m1, m2, m3, m4, m5, m1p, m2p, m3p, m4p, m5p, m6p, m1c;

    public void Equip(PlayerStats playerStats)
    {
        //Store Modifiers
        if (hp !=0)             {m1 = new StatModifier(hp, StatModType.Flat, this);} //Add HP Only if the Item is an Consumeable. -> Rather create CurrentHP=Hp.Value variable for HP Management.
        if (armor != 0)         {m2 = new StatModifier(armor, StatModType.Flat, this);}
        if (attackPower != 0)   {m3 = new StatModifier(attackPower, StatModType.Flat, this);}
        if (abilityPower != 0)  {m4 = new StatModifier(abilityPower, StatModType.Flat, this);}



        if (p_hp != 0)              {m1p = new StatModifier(p_attackPower, StatModType.PercentAdd, this);}
        if (p_armor != 0)           {m2p = new StatModifier(p_abilityPower, StatModType.PercentAdd, this);}
        if (p_attackPower != 0)     {m3p = new StatModifier(p_attackPower, StatModType.PercentAdd, this);}
        if (p_abilityPower != 0)    {m4p = new StatModifier(p_abilityPower, StatModType.PercentAdd, this);}
        if (p_attackSpeed != 0)     {m5p = new StatModifier(p_attackSpeed, StatModType.PercentAdd, this);} //Ich glaub damit würde was nicht stimmen, da AttackSpeed als Flat int durch 1 addiert wird.
        if (p_movementSpeed != 0)   {m6p = new StatModifier(p_movementSpeed, StatModType.PercentAdd, this);}

        if (c_hp != 0)
            playerStats.Heal(c_hp); 


        //Add Modifiers to Character
        if (m1 != null)playerStats.Hp.AddModifier(m1);
        if (m2 != null)playerStats.Armor.AddModifier(m2);
        if (m3 != null)playerStats.AttackPower.AddModifier(m3);
        if (m4 != null)playerStats.AbilityPower.AddModifier(m4);


        if (m1p != null) playerStats.Hp.AddModifier(m1p);
        if (m2p != null) playerStats.Armor.AddModifier(m2p);
        if (m3p != null) playerStats.AttackPower.AddModifier(m3p);
        if (m4p != null) playerStats.AbilityPower.AddModifier(m4p);
        if (m5p != null) playerStats.AttackSpeed.AddModifier(m5p);
        if (m6p != null) playerStats.MovementSpeed.AddModifier(m6p);






    }

    public void Unequip (PlayerStats playerStats, Item item)
    {

        if (m1 != null) playerStats.Hp.RemoveModifier(m1);
        if (m2 != null) playerStats.Armor.RemoveModifier(m2);
        if (m3 != null) playerStats.AttackPower.RemoveModifier(m3);
        if (m4 != null) playerStats.AbilityPower.RemoveModifier(m4);


        if (m1p != null) playerStats.Hp.RemoveModifier(m1p);
        if (m2p != null) playerStats.Armor.RemoveModifier(m2p);
        if (m3p != null) playerStats.AttackPower.RemoveModifier(m3p);
        if (m4p != null) playerStats.AbilityPower.RemoveModifier(m4p);
        if (m5p != null) playerStats.AttackSpeed.RemoveModifier(m5p);
        if (m6p != null) playerStats.MovementSpeed.RemoveModifier(m6p);

    }

    public string GetValueDescription()
    {

        if (hp != 0) modStrings[0] = "\nHp: " + hp; else modStrings[0] = "";
        if (armor != 0) modStrings[1] = "\nArmor: " + armor; else modStrings[1] = "";
        if (attackPower != 0) modStrings[2] = "\nAttack Power: " + attackPower; else modStrings[2] = "";
        if (abilityPower != 0) modStrings[3] = "\nAbility Power: " + abilityPower; else modStrings[3] = "";
        if (p_hp != 0) modStrings[4] = "\nErhöht HP um " + p_hp + "%"; else modStrings[4] = "";
        if (p_armor != 0) modStrings[5] = "\nErhöht Armor um " + p_armor + "%"; else modStrings[5] = "";
        if (p_attackPower != 0) modStrings[6] = "\nErhöht Attack Power um " + p_attackPower + "%"; else modStrings[6] = "";
        if (p_abilityPower != 0) modStrings[7] = "\nErhöht Ability Power um " + p_abilityPower + "%"; else modStrings[7] = "";
        if (p_attackSpeed != 0) modStrings[8] = "\nErhöht Attack Speed um " + p_attackSpeed + "%"; else modStrings[8] = "";
        if (p_movementSpeed != 0) modStrings[9] = "\nErhöht deinen Movementspeed um " + p_movementSpeed + "%"; else modStrings[9] = "";

        string finalString;
        finalString = modStrings[0] + modStrings[1] + modStrings[2] + modStrings[3] + modStrings[4] + modStrings[5] + modStrings[6] + modStrings[7] + modStrings[8] + modStrings[9];

        return finalString;
    }

    public string GetDescription()
    {
        return "Über GetDescription soll Rarität ausgewertet werden.";
    }
}


