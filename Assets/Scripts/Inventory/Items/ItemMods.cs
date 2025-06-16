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
    public int reg;

    [Space]
    [Header("Prozent-Werte")]
    public float p_hp;
    public float p_armor;
    public float p_attackPower;
    public float p_abilityPower;
    public float p_attackSpeed;
    public float p_movementSpeed;
    public float p_reg;

    public Ability modAbility;

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
    public int reg;

    public int[] flatValues;

    public float p_hp;
    public float p_armor;
    public float p_attackPower;
    public float p_abilityPower;
    public float p_attackSpeed;
    public float p_movementSpeed;
    public float p_reg;

    public float[] percentValues;

    public Ability modAbility;


    /// <summary>
    /// ###Remodel ItemMod Data###
    /// Unterschiedliche Mods mit einzigartigen Effekten, Prefix 1, Prefix 2, Prefix 3, Item, Suffix 1, Suffix 2, Suffix 3
    /// Was ich zu deklarieren versuche:
    /// ___ Item_______ Lvl
    /// |Flatwerte = +#
    /// |Erhöhung vorhandener Itemwerte um #%
    /// |Prozentwerte des Spielers = +#%
    /// |Spezifische Fähigkeiten
    /// |Mögliche Steigerungen dieser Fähigkeiten
    /// _______________________
    /// 
    /// </summary>
    /// <param name="itemMod"></param>
    public ItemModsData(ItemMods itemMod) 
    {
        name = itemMod.ModName;
        id = itemMod.ModID;

        hp = itemMod.hp;
        armor = itemMod.armor;
        attackPower = itemMod.attackPower;
        abilityPower = itemMod.abilityPower;
        reg = itemMod.reg;

        flatValues = new int[5] { hp, armor, attackPower, abilityPower, reg };

        p_hp = itemMod.p_hp;
        p_armor = itemMod.p_armor;
        p_attackPower = itemMod.p_attackPower;
        p_abilityPower = itemMod.p_abilityPower;
        p_attackSpeed = itemMod.p_attackSpeed;
        p_movementSpeed = itemMod.p_movementSpeed;
        p_reg = itemMod.p_reg;

        percentValues = new float[7] { p_hp, p_armor, p_attackPower, p_abilityPower, p_attackSpeed, p_movementSpeed, p_reg };

        modAbility = itemMod.modAbility;

    }

}
