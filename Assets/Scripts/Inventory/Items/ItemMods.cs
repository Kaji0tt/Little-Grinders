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
    //with a SpecialEffect Class, it should be possible to add SpecialEffects to a Mod, e.g. shortened CD of Teleport

}

[System.Serializable]
public class ItemModsData
{
    public string name;
    public string id;

    public int hp;
    public int armor;
    public int attackPower;
    public int abilityPower;

    public int[] flatValues;

    public float p_hp;
    public float p_armor;
    public float p_attackPower;
    public float p_abilityPower;
    public float p_attackSpeed;
    public float p_movementSpeed;

    public float[] percentValues;


    public ItemModsData(ItemMods itemMod) 
    {
        name = itemMod.ModName;
        id = itemMod.ModID;

        hp = itemMod.hp;
        armor = itemMod.armor;
        attackPower = itemMod.attackPower;
        abilityPower = itemMod.abilityPower;

        flatValues = new int[4] { hp, armor, attackPower, abilityPower };

        p_hp = itemMod.p_hp;
        p_armor = itemMod.p_armor;
        p_attackPower = itemMod.p_attackPower;
        p_abilityPower = itemMod.p_abilityPower;
        p_attackSpeed = itemMod.p_attackSpeed;
        p_movementSpeed = itemMod.p_movementSpeed;

        percentValues = new float[6] { p_hp, p_armor, p_attackPower, p_abilityPower, p_attackSpeed, p_movementSpeed };

    }

}
