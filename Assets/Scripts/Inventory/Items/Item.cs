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
    public int attackSpeed;

    [Space]
    [Header("Prozent-Werte")]
    public int p_hp;
    public int p_armor;
    public int p_attackPower;
    public int p_abilityPower;
    public int p_attackSpeed;
    public int p_movementSpeed;


    //private List<StatModifier> itemModifiers = new List<StatModifier>(); //Mit einer Liste wäre bestimmt entspannter, dafür bin ich aber zu dumm.
    //Kein Bock mich jetzt nochmal mit der Syntax zu befassen, wenn das Ziel so nah ist.
    private StatModifier m1, m2, m3, m4, m5, m1p, m2p, m3p, m4p, m5p, m6p;

    public void Equip(IsometricPlayer isometricPlayer)
    {
        //Store Modifiers
        if (hp !=0)             {m1 = new StatModifier(hp, StatModType.Flat, this);}
        if (armor != 0)         {m2 = new StatModifier(armor, StatModType.Flat, this);}
        if (attackPower != 0)   {m3 = new StatModifier(attackPower, StatModType.Flat, this);}
        if (abilityPower != 0)  {m4 = new StatModifier(abilityPower, StatModType.Flat, this);}



        if (p_hp != 0)              {m1p = new StatModifier(p_attackPower, StatModType.PercentMult, this);}
        if (p_armor != 0)           {m2p = new StatModifier(p_abilityPower, StatModType.PercentMult, this);}
        if (p_attackPower != 0)     {m3p = new StatModifier(p_attackPower, StatModType.PercentMult, this);}
        if (p_abilityPower != 0)    {m4p = new StatModifier(p_abilityPower, StatModType.PercentMult, this);}
        if (p_attackSpeed != 0)     {m5p = new StatModifier(p_attackSpeed, StatModType.PercentMult, this);}
        if (p_movementSpeed != 0)   {m6p = new StatModifier(p_movementSpeed, StatModType.PercentMult, this);}


        //Add Modifiers to Character
        if (m1 != null)isometricPlayer.Hp.AddModifier(m1);
        if (m2 != null)isometricPlayer.Armor.AddModifier(m2);
        if (m3 != null)isometricPlayer.AttackPower.AddModifier(m3);
        if (m4 != null)isometricPlayer.AbilityPower.AddModifier(m4);


        if (m1p != null) isometricPlayer.Hp.AddModifier(m1p);
        if (m2p != null) isometricPlayer.Armor.AddModifier(m2p);
        if (m3p != null) isometricPlayer.AttackPower.AddModifier(m3p);
        if (m4p != null) isometricPlayer.AbilityPower.AddModifier(m4p);
        if (m5p != null) isometricPlayer.AttackSpeed.AddModifier(m5p);
        if (m6p != null) isometricPlayer.MovementSpeed.AddModifier(m6p);






    }

    public void Unequip (IsometricPlayer isometricPlayer, Item item)
    {

        if (m1 != null) isometricPlayer.Hp.RemoveModifier(m1);
        if (m2 != null) isometricPlayer.Armor.RemoveModifier(m2);
        if (m3 != null) isometricPlayer.AttackPower.RemoveModifier(m3);
        if (m4 != null) isometricPlayer.AbilityPower.RemoveModifier(m4);


        if (m1p != null) isometricPlayer.Hp.RemoveModifier(m1p);
        if (m2p != null) isometricPlayer.Armor.RemoveModifier(m2p);
        if (m3p != null) isometricPlayer.AttackPower.RemoveModifier(m3p);
        if (m4p != null) isometricPlayer.AbilityPower.RemoveModifier(m4p);
        if (m5p != null) isometricPlayer.AttackSpeed.RemoveModifier(m5p);
        if (m6p != null) isometricPlayer.MovementSpeed.RemoveModifier(m6p);

    }

    public string GetDescription()
    {
        return "I'm an Item";
    }


}


