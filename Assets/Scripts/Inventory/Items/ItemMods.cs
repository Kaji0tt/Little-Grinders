using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ModRarity { Unbrauchbar, Ungewöhnlich, Selten, Episch, Legendär }

[CreateAssetMenu(fileName = "ItemType_Mod0000", menuName = "Assets/ItemMods")]
public class ItemMods : ScriptableObject
{
    [Header("Details")]
    public ModRarity modRarity;
    //public string modRarity;
    public string ModName;
    public string ModID;

    [Space]
    [Header("Flat-Werte")]
    public int hp;
    public int armor;
    public int attackPower;
    public int abilityPower;

    [Space]
    [Header("Prozent-Werte")]
    public float p_hp;
    public float p_armor;
    public float p_attackPower;
    public float p_abilityPower;
    public float p_attackSpeed;
    public float p_movementSpeed;

    //public Item_SpecialEffect special

    //Müsste über Item.Equip() aktivierbar sein! Ansonsten geerbt von ItemMods

    public void GenerateRandomValue()
    {
        // Soll eine RNG Range implementiert werden?
        //Mit dem derzeitigen System wäre +-10% sinnvoll.

        if(modRarity != ModRarity.Legendär)
        { }
    }
}
