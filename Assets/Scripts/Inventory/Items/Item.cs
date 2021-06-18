using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public enum ItemType {Kopf, Brust, Beine, Schuhe, Schmuck, Weapon, Consumable}
//public enum ItemRarity {Unbrauchbar, Gewöhnlich, Ungewöhnlich, Selten, Episch, Legendär}

[CreateAssetMenu(fileName = "Item0000", menuName = "Assets/Item")]
public class Item : ScriptableObject
{
    [Header("Details")]
    public string ItemID;
    public string ItemName;
    [TextArea]
    public string ItemDescription;
    [SerializeField]
    public ItemType itemType;
    [HideInInspector]
    public string itemRarity;   // [Currently gettin Implemented]: https://www.youtube.com/watch?v=dvSYloBxzrU
    public int Range;
    public bool RangedWeapon;
    public Sprite icon;        //scale item.sprite always to correct size, for ItemWorld to Spawn it in according size aswell. either here or in itemworld
    public int percent;

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

    [Space]
    [Header("Consumables")]
    public int c_hp;
    public bool usable;

    [HideInInspector]
    public float c_percent;


    public Spell itemAction;

}

//Erstelle eine ItemInstance, welche Serialized werden kann und aus dem ScriptableObject mit den oben genannten Variabeln ausgelesen wird.
[Serializable]
public class ItemInstance :  IMoveable//Da muss mir nochmal bei
{

    public string ItemID;
    public string ItemName;
    public string ItemDescription;
    public string ItemValueInfo;
    public ItemType itemType;
    public string itemRarity = "Gewöhnlich";   // [Currently gettin Implemented]: https://www.youtube.com/watch?v=dvSYloBxzrU

    public int Range;
    public bool RangedWeapon;
    public Sprite icon { get; private set; }       
    public int percent;


    //Flat Values
    public int hp;
    public int armor;
    public int attackPower;
    public int abilityPower;

    public int[] flatValues = new int[4];


    //Percent Values
    public float p_hp;
    public float p_armor;
    public float p_attackPower;
    public float p_abilityPower;
    public float p_attackSpeed;
    public float p_movementSpeed;

    public float[] percentValues = new float[6];



    public int c_hp;

    public bool useable;

    public Spell itemAction;

    [HideInInspector]
    public float c_percent;

    //Store StatModifiers
    private StatModifier[] flatStatMods = new StatModifier[4];
    private StatModifier[] percentStatMods = new StatModifier[6];

    //Store finalStringInfo
    private string[] modStrings = new string[11];

    [HideInInspector]
    public List<ItemModsData> addedItemMods = new List<ItemModsData>(); //??

    [HideInInspector]
    public int amount = 1;

    private IMoveable MyMoveable;

    private IUseable MyUseAble;

      
    //Wird eigentlich nicht verwendet, sollte aber im Konstruktor gecalled werden.
    //public ItemModsData[] myItemMods; 

    //Klone die vom SO geerbten Daten in die Instanz des Items
    public ItemInstance(Item item)
    {
        ItemID = item.ItemID;
        ItemName = item.ItemName;
        ItemDescription = item.ItemDescription;
        itemType = item.itemType;
        

        //Noch nicht implementiert. Das Interface MyUseAble wird wichtig, wenn wir aktive Fähigkeiten oder Tränke über die ActionBar abrufen können wollen.
        if(itemType == ItemType.Consumable)
        {
            MyMoveable = this;
        }
        
        //ItemRarity wird ausgelassen, da es erst im Roll berechnet wird.

        Range = item.Range;
        RangedWeapon = item.RangedWeapon;
        icon = item.icon;
        percent = item.percent;

        //Derzeit werden nur die Boni von den Mods equipped.
        #region CloneItem
        //Write Array for FlatValues
        if (item.hp != 0)
        {
            flatValues[0] = item.hp;
            hp = flatValues[0];
        }
        if (item.armor != 0)
        {
            flatValues[1] = item.armor;
            armor = flatValues[1];
        }
        if (item.attackPower != 0)
        {
            flatValues[2] = item.attackPower;
            attackPower = flatValues[2];
        }

        if (item.abilityPower != 0)
        {
            flatValues[3] = item.abilityPower;
            abilityPower = flatValues[3];
        }

        //Write Array for PercentValues
        if (item.p_hp != 0)
        {
            percentValues[0] = item.p_hp;
            p_hp = percentValues[0];
        }

        if (item.p_armor != 0)
        {
            percentValues[1] = item.p_armor;
            p_armor = percentValues[1];
        }

        if (item.p_attackPower != 0)
        {
            percentValues[2] = item.p_attackPower;
            p_attackPower = percentValues[2];
        }

        if (item.p_abilityPower != 0)
        {
            percentValues[3] = item.p_abilityPower;
            p_abilityPower = percentValues[3];
        }

        if (item.p_attackSpeed != 0)
        {
            percentValues[4] = item.p_attackSpeed;
            p_attackSpeed = item.p_attackSpeed;
        }

        if (item.p_movementSpeed != 0)
        {
            percentValues[5] = item.p_movementSpeed;
            p_movementSpeed = item.p_movementSpeed;
        }

        if (item.c_hp != 0)
        {
            c_hp = item.c_hp;
        }

        if(useable)
        {
            useable = true;
            itemAction = item.itemAction;
        }
        #endregion

        SetValueDescription(this);


        //Die Rolls müssen in der ItemInstance gecalled werden.




    }

    /* Incoming - Use soll es ermöglichen, aktive Fähigkeiten auf den Items zu besitzen, welche über die ActionBar gecastet werden sollen.
    public void Use()
    {
        
    }
    */

    //Wird gecalled, wenn das Item im Inventar angeklickt wird. Dadurch werden die Stats den playerStats hinzugefügt.
    public void Equip(PlayerStats playerStats)
    {
        //Derzeit werden nur die Boni von den Mods equipped.
        for (int i = 0; i < flatValues.Length; i++)
        {
            //Store Modifiers wird nicht von ItemRolls berührt
            flatStatMods[i] = new StatModifier(flatValues[i], StatModType.Flat, this);
        }


        for (int i = 0; i < percentValues.Length; i++)
        {
            percentStatMods[i] = new StatModifier(percentValues[i], StatModType.PercentAdd, this);
        }

        if (c_hp != 0)
            playerStats.Heal(c_hp);

        if(useable)



        //Add Modifiers to Character
        if (flatStatMods[0] != null) playerStats.Hp.AddModifier(flatStatMods[0]);
        if (flatStatMods[1] != null) playerStats.Armor.AddModifier(flatStatMods[1]);
        if (flatStatMods[2] != null) playerStats.AttackPower.AddModifier(flatStatMods[2]);
        if (flatStatMods[3] != null) playerStats.AbilityPower.AddModifier(flatStatMods[3]);


        if (percentStatMods[0] != null) playerStats.Hp.AddModifier(percentStatMods[0]);
        if (percentStatMods[1] != null) playerStats.Armor.AddModifier(percentStatMods[1]);
        if (percentStatMods[2] != null) playerStats.AttackPower.AddModifier(percentStatMods[2]);
        if (percentStatMods[3] != null) playerStats.AbilityPower.AddModifier(percentStatMods[3]);
        if (percentStatMods[4] != null) playerStats.AttackSpeed.AddModifier(percentStatMods[4]);
        if (percentStatMods[5] != null) playerStats.MovementSpeed.AddModifier(percentStatMods[5]);

        if (Range != 0) playerStats.Range += Range;
        //Implementierung von Special Effekten


    }

    //Wird gecalled, wenn die ausgerüsteten Items angeklickt werden. Zuständig hierfür sind die Klassen #EQSlot[Kopf, Schuhe, ...], welche in den entsprechenden InterfaceObjekten im Canvas liegen.
    public void Unequip(PlayerStats playerStats)
    {

        if (flatStatMods[0] != null) playerStats.Hp.RemoveModifier(flatStatMods[0]);
        if (flatStatMods[1] != null) playerStats.Armor.RemoveModifier(flatStatMods[1]);
        if (flatStatMods[2] != null) playerStats.AttackPower.RemoveModifier(flatStatMods[2]);
        if (flatStatMods[3] != null) playerStats.AbilityPower.RemoveModifier(flatStatMods[3]);


        if (percentStatMods[0] != null) playerStats.Hp.RemoveModifier(percentStatMods[0]);
        if (percentStatMods[1] != null) playerStats.Armor.RemoveModifier(percentStatMods[1]);
        if (percentStatMods[2] != null) playerStats.AttackPower.RemoveModifier(percentStatMods[2]);
        if (percentStatMods[3] != null) playerStats.AbilityPower.RemoveModifier(percentStatMods[3]);
        if (percentStatMods[4] != null) playerStats.AttackSpeed.RemoveModifier(percentStatMods[4]);
        if (percentStatMods[5] != null) playerStats.MovementSpeed.RemoveModifier(percentStatMods[5]);

        if (Range != 0) playerStats.Range -= Range;
        //Implementierung von Special Effekten
    }

    //Schreibe die Beschreibung für den Tooltip
    public void SetValueDescription(ItemInstance item)
    {
        if (item.flatValues[0] != 0) item.modStrings[0] = "\nHp: " +                                item.flatValues[0]; else item.modStrings[0] = "";
        if (item.flatValues[1] != 0) item.modStrings[1] = "\nArmor: " +                             item.flatValues[1]; else item.modStrings[1] = "";
        if (item.flatValues[2] != 0) item.modStrings[2] = "\nAttack Power: " +                      item.flatValues[2]; else item.modStrings[2] = "";
        if (item.flatValues[3] != 0) item.modStrings[3] = "\nAbility Power: " +                     item.flatValues[3]; else item.modStrings[3] = "";
        if (item.percentValues[0] != 0) item.modStrings[4] = "\nErhöht HP um " +                    item.percentValues[0] * 100 + "%"; else item.modStrings[4] = "";
        if (item.percentValues[1] != 0) item.modStrings[5] = "\nErhöht Armor um " +                 item.percentValues[1] * 100 + "%"; else item.modStrings[5] = "";
        if (item.percentValues[2] != 0) item.modStrings[6] = "\nErhöht Attack Power um " +          item.percentValues[2] * 100 + "%"; else item.modStrings[6] = "";
        if (item.percentValues[3] != 0) item.modStrings[7] = "\nErhöht Ability Power um " +         item.percentValues[3] * 100 + "%"; else item.modStrings[7] = "";
        if (item.percentValues[4] != 0) item.modStrings[8] = "\nErhöht Attack Speed um " +          item.percentValues[4] * 100 + "%"; else item.modStrings[8] = "";
        if (item.percentValues[5] != 0) item.modStrings[9] = "\nErhöht deinen Movementspeed um " +  item.percentValues[5] * 100 + "%"; else item.modStrings[9] = "";

        if (item.c_hp != 0) item.modStrings[10] = "\nHeilt den Spieler um " +                       item.c_hp  + " Lebenspunkte"; else item.modStrings[10] = "";
        string finalString;
        finalString = modStrings[0] + modStrings[1] + modStrings[2] + modStrings[3] + modStrings[4] + modStrings[5] + modStrings[6] + modStrings[7] + modStrings[8] + modStrings[9] + modStrings[10];

        this.ItemValueInfo = finalString;
        //return finalString;
    }

    //Serialize information about Item and its Mods (not used currently)
    public ItemModsData[] SaveItemMods(ItemInstance item)
    {
        ItemModsData[] myArray = item.addedItemMods.ToArray();

        return myArray;

    }

}


