using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Flags]
public enum ItemType
{
    None = 0,
    Kopf = 1 << 0,
    Brust = 1 << 1,
    Beine = 1 << 2,
    Schuhe = 1 << 3,
    Schmuck = 1 << 4,
    Weapon = 1 << 5,
    Consumable = 1 << 6,
    All = ~0 // optional, wenn du "alle Slots erlaubt" brauchst
}
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
    public WeaponCombo weaponCombo;
    //
    //public string itemRarity;   // [Currently gettin Implemented]: https://www.youtube.com/watch?v=dvSYloBxzrU
    [HideInInspector]
    public Rarity itemRarity;
    public int Range;
    public bool RangedWeapon;
    public Sprite icon;        //scale item.sprite always to correct size, for ItemWorld to Spawn it in according size aswell. either here or in itemworld
    public int percent;
    public int baseLevel;
    public bool activeItem;

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

    [Space]
    [Header("Actives")]
    public bool usable;
    public Potion itemPotion;
    //public Ability itemAbility;

    [HideInInspector]
    public float c_percent;


    //wahrscheinlich sollte jedes Item eine bestimmte Nische ausfüllen können.
    //Jede Niesche sollte dann von 2-3 Base Items bestückt sein
    //somit wäre jeder Spielstil grundlegend abgedeckt.
    // auch könnten die verschiedenen Waffen-Combos per zufall rollen
    public int level_needed;


}

//Erstelle eine ItemInstance, welche Serialized werden kann und aus dem ScriptableObject mit den oben genannten Variabeln ausgelesen wird.
[Serializable]
public class ItemInstance :  IMoveable, IUseable
{

    public string ItemID;
    public string ItemName;
    public string ItemDescription;
    public string ItemValueInfo;
    public ItemType itemType;
    public Rarity itemRarity;
    public WeaponCombo weaponCombo;
    //public string itemRarity = "Gewöhnlich";

    public int Range;
    public bool RangedWeapon;
    public Sprite icon { get; private set; }       
    public int percent;
    [Range(1, 100)]
    public int requiredLevel = 1;
    public int baseLevel { get; private set; }


    //Flat Values
    public int hp;
    public int armor;
    public int attackPower;
    public int abilityPower;
    public int reg;

    public int[] flatValues = new int[5];


    //Percent Values
    public float p_hp;
    public float p_armor;
    public float p_attackPower;
    public float p_abilityPower;
    public float p_attackSpeed;
    public float p_movementSpeed;
    public float p_reg;

    public float[] percentValues = new float[7];


    //Actives
    public bool useable;
    public Potion itemPotion;

    [HideInInspector]
    public float c_percent;

    //Store StatModifiers
    private StatModifier[] flatStatMods = new StatModifier[5];
    private StatModifier[] percentStatMods = new StatModifier[7];

    //Store finalStringInfo
    private string[] modStrings = new string[12];

    [HideInInspector]
    public List<ItemMod> addedItemMods = new List<ItemMod>(); //??



    private IMoveable MyMoveable;

    
    public ItemInstance(Item item)
    {
        ItemID = item.ItemID;
        ItemName = item.ItemName;
        ItemDescription = item.ItemDescription;
        itemType = item.itemType;

        weaponCombo = item.weaponCombo;
        //Noch nicht implementiert. Das Interface MyUseAble wird wichtig, wenn wir aktive Fähigkeiten oder Tränke über die ActionBar abrufen können wollen.
        if(itemType == ItemType.Consumable)
        {
            MyMoveable = this;
        }

        if (item.itemPotion != null)
            itemPotion = item.itemPotion;
        
        //ItemRarity wird ausgelassen, da es erst im Roll berechnet wird.

        Range = item.Range;
        RangedWeapon = item.RangedWeapon;
        icon = item.icon;
        percent = item.percent;
        item.baseLevel = GlobalMap.instance != null && GlobalMap.instance.currentMap != null
            ? GlobalMap.instance.currentMap.mapLevel
            : 1;

        //Berechnung der Werte der spezifischen Item Instanz.
        #region Clone&RollItem
        if (item.hp != 0)
        {
            flatValues[0] = Mathf.RoundToInt(RollItemValue(item.hp));
            hp = flatValues[0];
        }

        if (item.armor != 0)
        {
            flatValues[1] = Mathf.RoundToInt(RollItemValue(item.armor));
            armor = flatValues[1];
        }

        if (item.attackPower != 0)
        {
            flatValues[2] = Mathf.RoundToInt(RollItemValue(item.attackPower));
            attackPower = flatValues[2];
        }

        if (item.abilityPower != 0)
        {
            flatValues[3] = Mathf.RoundToInt(RollItemValue(item.abilityPower));
            abilityPower = flatValues[3];
        }

        if (item.reg != 0)
        {
            flatValues[4] = Mathf.RoundToInt(RollItemValue(item.reg));
            reg = flatValues[4];
        }

        // Prozentuale Berechnung des Gegenstands auf 2 Nachkommastellen.
        if (item.p_hp != 0)
        {
            percentValues[0] = Mathf.Round(RollItemValue(item.p_hp) * 100) / 100f;
            p_hp = percentValues[0];
        }

        if (item.p_armor != 0)
        {
            percentValues[1] = Mathf.Round(RollItemValue(item.p_armor) * 100) / 100f;
            p_armor = percentValues[1];
        }

        if (item.p_attackPower != 0)
        {
            percentValues[2] = Mathf.Round(RollItemValue(item.p_attackPower) * 100) / 100f;
            p_attackPower = percentValues[2];
        }

        if (item.p_abilityPower != 0)
        {
            percentValues[3] = Mathf.Round(RollItemValue(item.p_abilityPower) * 100) / 100f;
            p_abilityPower = percentValues[3];
        }

        if (item.p_attackSpeed != 0)
        {
            percentValues[4] = Mathf.Round(RollItemValue(item.p_attackSpeed) * 100) / 100f;
            p_attackSpeed = percentValues[4];
        }

        if (item.p_movementSpeed != 0)
        {
            percentValues[5] = Mathf.Round(RollItemValue(item.p_movementSpeed) * 100) / 100f;
            p_movementSpeed = percentValues[5];
        }

        if (item.p_reg != 0)
        {
            percentValues[6] = Mathf.Round(RollItemValue(item.p_reg) * 100) / 100f;
            p_reg = percentValues[6];
        }


        if (useable)
        {
            useable = true;

        }
        #endregion




        SetValueDescription(this);
        //Die Rolls müssen in der ItemInstance gecalled werden.

 


    }


    private float RollItemValue(float baseValue)
    {
        float variance = (UnityEngine.Random.value * 0.2f) - 0.1f;

        int mapLevel = GlobalMap.instance != null && GlobalMap.instance.currentMap != null
            ? GlobalMap.instance.currentMap.mapLevel
            : 1;

        // Leveldifferenz-Faktor, max ±20% Skalierung
        float levelFactor = Mathf.Clamp((float)mapLevel / (float)baseLevel, 0.5f, 1.5f);

        // Beispiel: 10% zufällige Varianz + Levelanpassung
        return baseValue * (1 + variance) * levelFactor;
    }

    public void AppendModNamesToItemName()
    {
        if (addedItemMods == null || addedItemMods.Count == 0)
            return;

        string modSuffixes = "";

        foreach (var mod in addedItemMods)
        {
            // Hole rarity-spezifischen Displaynamen
            string suffix = mod.GetName();

            if (!string.IsNullOrEmpty(suffix))
                modSuffixes += " " + suffix;
        }

        ItemName += modSuffixes;
    }

    public void UpdateItemDescriptionWithMods()
    {
        if (addedItemMods == null || addedItemMods.Count == 0)
            return;

        string modDescriptions = "";

        foreach (var mod in addedItemMods)
        {
            string modText = mod.GetDescription(); // z.B. "+5% Crit Chance"
            if (!string.IsNullOrEmpty(modText))
                modDescriptions += "\n" + modText;
        }

        ItemDescription += modDescriptions;
    }

    public void ApplyItemMods()
    {
        foreach (var mod in addedItemMods)
        {
            if (mod == null) continue;

            switch (mod.definition.targetStat)
            {
                case EntitieStats.Hp:
                    if (mod.isPercent) p_hp += mod.value;
                    else hp += Mathf.RoundToInt(mod.value);
                    break;

                case EntitieStats.Armor:
                    if (mod.isPercent) p_armor += mod.value;
                    else armor += Mathf.RoundToInt(mod.value);
                    break;

                case EntitieStats.AttackPower:
                    if (mod.isPercent) p_attackPower += mod.value;
                    else attackPower += Mathf.RoundToInt(mod.value);
                    break;

                case EntitieStats.AbilityPower:
                    if (mod.isPercent) p_abilityPower += mod.value;
                    else abilityPower += Mathf.RoundToInt(mod.value);
                    break;

                case EntitieStats.AttackSpeed:
                    if (mod.isPercent) p_attackSpeed += mod.value;
                    break;

                case EntitieStats.MovementSpeed:
                    if (mod.isPercent) p_movementSpeed += mod.value;
                    break;

                case EntitieStats.Reg:
                    if (mod.isPercent) p_reg += mod.value;
                    else reg += Mathf.RoundToInt(mod.value);
                    break;


            }
        }
    }

    public string GetName()
    {
        return ItemName;
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

        //Setze die Beschreibung auf die Beschreibung des Scriptable Objects des Trankes.
        if (item.useable)
            if (item.itemPotion != null)
                item.modStrings[10] = item.itemPotion.descr;

        string finalString;
        finalString = modStrings[0] + modStrings[1] + modStrings[2] + modStrings[3] + modStrings[4] + modStrings[5] + modStrings[6] + modStrings[7] + modStrings[8] + modStrings[9] + modStrings[10];

        this.ItemValueInfo = finalString;
        //return finalString;
    }

    //Serialize information about Item and its Mods (not used currently)
    /*
    public ItemModsData[] SaveItemMods(ItemInstance item)
    {
        ItemModsData[] myArray = item.addedItemMods.ToArray();

        return myArray;

    }
    */
    public void Use()
    {
        //Inventory inventory = PlayerManager.instance.player.Inventory;

        itemPotion.Use();
        PlayerManager.instance.player.Inventory.RemoveItem(this);
        //An dieser Stelle sollte die Referenz zu einem bestimmten Spell geschehen. So bleibt sicher gestellt, dass jedes individuelle Item
        //unterschiedliche Spells abrufen kann.
        /*
        Debug.Log("Item is beeing used.");

        if (itemType == ItemType.Consumable)
        {
            Debug.Log("Item is a consumable");
            if (inventory.itemList.Contains(this))
            {
                Debug.Log("Inventory contains this consumable");
                PlayerManager.instance.player.Inventory.RemoveItem(this);
                Use();
            };

        };

        */
    }

    public bool IsOnCooldown()
    {
        if (this.itemType == ItemType.Consumable)
        {
            return false;
        };

        return false;
    }

    public float GetCooldown()
    {
        if (this.itemType == ItemType.Consumable)
        {
            return 0;
        };

        return 0;
    }

    public float CooldownTimer()
    {
        if (this.itemType == ItemType.Consumable)
        {
            return 0;
        };

        return 0;
    }

    public bool IsActive()
    {
        //Do Sepcial Stuff which is called like Update.
        if ( GetCooldown()== 0)
            return false;
        else
        return true;
    }


}


