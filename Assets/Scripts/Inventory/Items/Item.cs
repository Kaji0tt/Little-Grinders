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
    public int critChance;
    public int critDamage;



    [Space]
    [Header("Prozent-Werte")]
    public float p_hp;
    public float p_armor;
    public float p_attackPower;
    public float p_abilityPower;
    public float p_attackSpeed;
    public float p_movementSpeed;
    public float p_reg;
    public float p_critChance;
    public float p_critDamage;

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

    //Actives
    public bool useable;
    public Potion itemPotion;

    [HideInInspector]
    public float c_percent;

    //Store StatModifiers
    public Dictionary<EntitieStats, int> flatStats = new();
    public Dictionary<EntitieStats, float> percentStats = new();

    private Dictionary<EntitieStats, StatModifier> flatStatMods = new();
    private Dictionary<EntitieStats, StatModifier> percentStatMods = new();

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
        /*
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

        if (item.critChance != 0)
        {
            flatValues[5] = Mathf.RoundToInt(RollItemValue(item.critChance));
            critChance = flatValues[5];
        }

        if (item.critDamage != 0)
        {
            flatValues[6] = Mathf.RoundToInt(RollItemValue(item.critDamage));
            critDamage = flatValues[6];
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

        if (item.p_critChance != 0)
        {
            percentValues[7] = Mathf.Round(RollItemValue(item.p_critChance) * 100) / 100f;
            p_critChance = percentValues[7];
        }

        if (item.p_critDamage != 0)
        {
            percentValues[8] = Mathf.Round(RollItemValue(item.p_critDamage) * 100) / 100f;
            p_critDamage = percentValues[8];
        }


        if (useable)
        {
            useable = true;

        }
        */
        #endregion


        // Flat Stats
        AddRolledFlat(item.hp, EntitieStats.Hp);
        AddRolledFlat(item.armor, EntitieStats.Armor);
        AddRolledFlat(item.attackPower, EntitieStats.AttackPower);
        AddRolledFlat(item.abilityPower, EntitieStats.AbilityPower);
        AddRolledFlat(item.reg, EntitieStats.Regeneration);
        AddRolledFlat(item.critChance, EntitieStats.CriticalChance);
        AddRolledFlat(item.critDamage, EntitieStats.CritcalDamage);

        // Percent Stats
        AddRolledPercent(item.p_hp, EntitieStats.Hp);
        AddRolledPercent(item.p_armor, EntitieStats.Armor);
        AddRolledPercent(item.p_attackPower, EntitieStats.AttackPower);
        AddRolledPercent(item.p_abilityPower, EntitieStats.AbilityPower);
        AddRolledPercent(item.p_attackSpeed, EntitieStats.AttackSpeed);
        AddRolledPercent(item.p_movementSpeed, EntitieStats.MovementSpeed);
        AddRolledPercent(item.p_reg, EntitieStats.Regeneration);
        AddRolledPercent(item.p_critChance, EntitieStats.CriticalChance);
        AddRolledPercent(item.p_critDamage, EntitieStats.CritcalDamage);

        SetValueDescription(this);
        //Die Rolls müssen in der ItemInstance gecalled werden.

    }

    private void AddRolledFlat(int baseValue, EntitieStats stat)
    {
        if (baseValue != 0)
        {
            int rolled = Mathf.RoundToInt(RollItemValue(baseValue));
            flatStats[stat] = rolled;
        }
    }

    private void AddRolledPercent(float baseValue, EntitieStats stat)
    {
        if (baseValue != 0)
        {
            float rolled = Mathf.Round(RollItemValue(baseValue) * 100f) / 100f;
            percentStats[stat] = rolled;
        }
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
        
    // Wendet alle Modifikatoren aus addedItemMods auf die flatStats und percentStats an
    public void ApplyItemMods()
    {
        foreach (var mod in addedItemMods)
        {
            if (mod == null || mod.definition == null) continue;

            var stat = mod.definition.targetStat;

            if (mod.IsPercent)
            {
                // Wenn bereits ein Prozentwert existiert, addiere dazu
                if (percentStats.ContainsKey(stat))
                    percentStats[stat] += mod.rolledValue;
                else
                    percentStats[stat] = mod.rolledValue;
            }
            else
            {
                int intValue = Mathf.RoundToInt(mod.rolledValue);

                // Wenn bereits ein Flatwert existiert, addiere dazu
                if (flatStats.ContainsKey(stat))
                    flatStats[stat] += intValue;
                else
                    flatStats[stat] = intValue;
            }
        }

        // Aktualisiere Name und Beschreibung
        AppendModNamesToItemName();
        //UpdateItemDescriptionWithMods();

        // Tooltip-Text neu aufbauen
        SetValueDescription(this);
    }

    public string GetName()
    {
        return ItemName;
    }

    //Wird gecalled, wenn das Item im Inventar angeklickt wird. Dadurch werden die Stats den playerStats hinzugefügt.
    public void Equip(PlayerStats playerStats)
    {
        foreach (var kvp in flatStats)
        {
            var mod = new StatModifier(kvp.Value, StatModType.Flat, this);
            flatStatMods[kvp.Key] = mod;
            playerStats.GetStat(kvp.Key).AddModifier(mod);
        }

        foreach (var kvp in percentStats)
        {
            var mod = new StatModifier(kvp.Value, StatModType.PercentAdd, this);
            percentStatMods[kvp.Key] = mod;
            playerStats.GetStat(kvp.Key).AddModifier(mod);
        }

        if (Range != 0) playerStats.Range += Range;
        //Implementierung von Special Effekten
    }

    //Wird gecalled, wenn die ausgerüsteten Items angeklickt werden. Zuständig hierfür sind die Klassen #EQSlot[Kopf, Schuhe, ...], welche in den entsprechenden InterfaceObjekten im Canvas liegen.
    public void Unequip(PlayerStats playerStats)
    {

        foreach (var kvp in flatStatMods)
        {
            playerStats.GetStat(kvp.Key).RemoveModifier(kvp.Value);
        }

        foreach (var kvp in percentStatMods)
        {
            playerStats.GetStat(kvp.Key).RemoveModifier(kvp.Value);
        }

        if (Range != 0) playerStats.Range -= Range;
        //Implementierung von Special Effekten
    }

    //Schreibe die Beschreibung für den Tooltip
    public void SetValueDescription(ItemInstance item)
    {
        item.ItemValueInfo = "";

        foreach (var kvp in flatStats)
        {
            if (kvp.Value != 0)
                item.ItemValueInfo += $"\n{kvp.Key}: {kvp.Value}";
        }

        foreach (var kvp in percentStats)
        {
            if (kvp.Value != 0)
                item.ItemValueInfo += $"\nErhöht {kvp.Key} um {kvp.Value * 100}%";
        }

    }


    public void Use()
    {
        //Inventory inventory = PlayerManager.instance.player.Inventory;

        itemPotion.Use();
        PlayerManager.instance.player.Inventory.RemoveItem(this);
        //An dieser Stelle sollte die Referenz zu einem bestimmten Spell geschehen. So bleibt sicher gestellt, dass jedes individuelle Item
        //unterschiedliche Spells abrufen kann.
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


