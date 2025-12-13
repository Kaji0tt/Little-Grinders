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
    Gem = 1 << 6,
    All = ~0 // optional, wenn du "alle Slots erlaubt" brauchst
}

[System.Flags]
public enum GemType
{
    None = 0,
    
    [Tooltip("Rubin - Hauptangriffsfähigkeit (Primary Attack Ability)")]
    Ruby = 1 << 0,
    
    [Tooltip("Saphir - Bewegungsfähigkeit (Movement/Dash Ability)")]
    Sapphire = 1 << 1,
    
    [Tooltip("Smaragd - Flächenangriff (AoE/Crowd Control Ability)")]
    Emerald = 1 << 2,
    
    [Tooltip("Amethyst - Finisher/Execute (Finishing Ability)")]
    Amethyst = 1 << 3,
    
    [Tooltip("Topas - Unterstützung/Buff (Support/Buff Ability)")]
    Topaz = 1 << 4
}
//public enum ItemRarity {Unbrauchbar, Gewöhnlich, Ungewöhnlich, Selten, Episch, Legendär}



[CreateAssetMenu(fileName = "Item0000", menuName = "Items/Item")]
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
    
    [Tooltip("Nur relevant für Gems: Welchem Slot-Typ gehört dieses Gem an?")]
    public GemType gemType;
    
    [HideInInspector]
    [Tooltip("Versteckt: Wird zur Laufzeit durch ItemMod-System gesetzt")]
    public AbilityData gemAbility;
    //
    //public string itemRarity;   // [Currently gettin Implemented]: https://www.youtube.com/watch?v=dvSYloBxzrU
    [HideInInspector]
    public Rarity itemRarity;
    public int Range;
    public bool RangedWeapon;
    public Sprite icon;        //scale item.sprite always to correct size, for ItemWorld to Spawn it in according size aswell. either here or in itemworld
    
    [Space]
    [Header("Drop Configuration")]
    [Tooltip("Mindest-Level ab dem dieses Item droppen kann")]
    public int dropLevel = 1;
    
    [Tooltip("Relatives Drop-Gewicht (höher = häufiger). Beispiel: 100 = Common, 50 = Uncommon, 10 = Rare, 1 = Very Rare")]
    public int dropWeight = 100;
    
    [HideInInspector]
    public int calculatedWeight;  // Interner Wert nach Level-Penalty und Modifikatoren
    
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


}

//Erstelle eine ItemInstance, welche Serialized werden kann und aus dem ScriptableObject mit den oben genannten Variabeln ausgelesen wird.
[Serializable]
public class ItemInstance :  IMoveable
{

    public string ItemID;
    public string ItemName;
    public string ItemDescription;
    public string ItemValueInfo;
    public ItemType itemType;
    public Rarity itemRarity;
    public WeaponCombo weaponCombo;
    public GemType gemType;
    
    [HideInInspector]
    public AbilityData gemAbility; // Wird zur Laufzeit durch ItemMod gesetzt
    //public string itemRarity = "Gewöhnlich";

    public int Range;
    public bool RangedWeapon;
    public Sprite icon { get; private set; }       
    public int dropWeight;  // Kopie von Item.dropWeight
    public int itemLevel { get; private set; }  // Tatsächliches Level des gedroppten Items

    //Actives
    public bool useable;
    public Potion itemPotion;

    //Store StatModifiers
    public Dictionary<EntitieStats, int> flatStats = new();
    public Dictionary<EntitieStats, float> percentStats = new();

    private Dictionary<EntitieStats, StatModifier> flatStatMods = new();
    private Dictionary<EntitieStats, StatModifier> percentStatMods = new();

    [HideInInspector]
    public List<ItemMod> addedItemMods = new List<ItemMod>(); //??

    private IMoveable MyMoveable;

    
    public ItemInstance(Item item, bool skipRolls = false)
    {
        ItemID = item.ItemID;
        ItemName = item.ItemName;
        ItemDescription = item.ItemDescription;
        itemType = item.itemType;
        weaponCombo = item.weaponCombo;
        gemType = item.gemType;
        gemAbility = item.gemAbility;
        // ... weitere Initialisierung ...
        // ... weitere Initialisierung ...

        addedItemMods = new List<ItemMod>();

        Range = item.Range;
        RangedWeapon = item.RangedWeapon;
        icon = item.icon;
        dropWeight = item.dropWeight;

        int mapLevel = GlobalMap.instance != null && GlobalMap.instance.currentMap != null
            ? Mathf.Max(1, GlobalMap.instance.currentMap.mapLevel)
            : 1;

        // Item Level = Map Level ±2
        int minItemLevel = Mathf.Max(1, mapLevel - 2);
        int maxItemLevel = Mathf.Max(minItemLevel, mapLevel + 2);
        itemLevel = UnityEngine.Random.Range(minItemLevel, maxItemLevel + 1);
        item.dropLevel = itemLevel; // Sync zurück (nur für Debugging)

        if (!skipRolls)
        {
            // Flat Stats
            AddRolledFlat(item.hp, EntitieStats.Hp);
            AddRolledFlat(item.armor, EntitieStats.Armor);
            AddRolledFlat(item.attackPower, EntitieStats.AttackPower);
            AddRolledFlat(item.abilityPower, EntitieStats.AbilityPower);
            AddRolledFlat(item.reg, EntitieStats.Regeneration);
            AddRolledFlat(item.critChance, EntitieStats.CriticalChance);
            AddRolledFlat(item.critDamage, EntitieStats.CriticalDamage);

            // Percent Stats
            AddRolledPercent(item.p_hp, EntitieStats.Hp);
            AddRolledPercent(item.p_armor, EntitieStats.Armor);
            AddRolledPercent(item.p_attackPower, EntitieStats.AttackPower);
            AddRolledPercent(item.p_abilityPower, EntitieStats.AbilityPower);
            AddRolledPercent(item.p_attackSpeed, EntitieStats.AttackSpeed);
            AddRolledPercent(item.p_movementSpeed, EntitieStats.MovementSpeed);
            AddRolledPercent(item.p_reg, EntitieStats.Regeneration);
            AddRolledPercent(item.p_critChance, EntitieStats.CriticalChance);
            AddRolledPercent(item.p_critDamage, EntitieStats.CriticalDamage);
        }

        SetValueDescription(this);
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
            float rolled = Mathf.Round(RollItemValue(baseValue, isPercentStat: true) * 100f) / 100f;
            percentStats[stat] = rolled;
        }
    }

    private float RollItemValue(float baseValue, bool isPercentStat = false)
    {
        float variance = (UnityEngine.Random.value * 0.2f) - 0.1f;

        float baseMultiplier = GetItemLevelMultiplier();
        float effectiveMultiplier = isPercentStat
            ? GetPercentMultiplier(baseMultiplier)
            : baseMultiplier;

        return baseValue * (1 + variance) * effectiveMultiplier;
    }

    private float GetItemLevelMultiplier()
    {
        int effectiveItemLevel = Mathf.Max(1, itemLevel);

        float rawLogistic = 1f / (1f + Mathf.Exp(-0.15f * (effectiveItemLevel - 25f)));
        float minLogistic = 1f / (1f + Mathf.Exp(-0.15f * (1f - 25f))); // ≈ logistic at level 1
        float normalized = Mathf.InverseLerp(minLogistic, 1f, rawLogistic);

        const float minMultiplier = 1f;
        const float maxMultiplier = 10f;
        return Mathf.Lerp(minMultiplier, maxMultiplier, normalized);
    }

    private float GetPercentMultiplier(float baseMultiplier)
    {
        const float percentHybridInfluence = 0.4f; // percent stats inherit only part of flat growth
        const float percentExponent = 0.6f; // exponent < 1 dampens high-end growth

        float hybridMultiplier = Mathf.Lerp(1f, baseMultiplier, percentHybridInfluence);
        return Mathf.Pow(hybridMultiplier, percentExponent);
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
            string modText = mod.GetDescription(); // Nutzt die neue Logik aus ItemMod
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

        UpdateItemDescriptionWithMods();
    }

    public string GetName()
    {
        return ItemName;
    }

    public static ItemInstance FromSave(SavedItem savedItem)
    {
        Debug.Log($"=== [ItemInstance.FromSave] START für Item: {savedItem.itemID} ===");
        
        Item baseItem = ItemDatabase.instance.GetItemByID(savedItem.itemID);
        if (baseItem == null)
        {
            Debug.LogError($"Item with ID {savedItem.itemID} not found in database.");
            return null;
        }

        ItemInstance instance = new ItemInstance(baseItem, skipRolls: true);
        
        // Setze gespeicherte Basisdaten
        instance.ItemName = savedItem.itemName ?? baseItem.ItemName;
        instance.ItemDescription = savedItem.itemDescription ?? baseItem.ItemDescription;

        if (Enum.TryParse<Rarity>(savedItem.rarity, out var rarity))
            instance.itemRarity = rarity;

        Debug.Log($"[ItemInstance.FromSave] Basis-Daten gesetzt: {instance.ItemName}, Rarity: {instance.itemRarity}");

        // Lade Flat Stats aus Save
        instance.flatStats.Clear();
        foreach (var kvp in savedItem.flatStats)
        {
            if (Enum.TryParse<EntitieStats>(kvp.Key, out var stat))
            {
                instance.flatStats[stat] = kvp.Value;
                Debug.Log($"[ItemInstance.FromSave] Flat Stat geladen: {stat} = {kvp.Value}");
            }
        }

        // Lade Percent Stats aus Save
        instance.percentStats.Clear();
        foreach (var kvp in savedItem.percentStats)
        {
            if (Enum.TryParse<EntitieStats>(kvp.Key, out var stat))
            {
                instance.percentStats[stat] = kvp.Value;
                Debug.Log($"[ItemInstance.FromSave] Percent Stat geladen: {stat} = {kvp.Value}%");
            }
        }

        // Lade Item Mods
        instance.addedItemMods.Clear();
        int removedAbilityMods = 0;
        foreach (var modSave in savedItem.mods)
        {
            var modDef = ItemDatabase.instance.GetModDefinitionByName(modSave.modName);
            if (modDef == null)
            {
                Debug.LogWarning($"ModDefinition '{modSave.modName}' not found for item '{savedItem.itemID}'");
                continue;
            }

            // MIGRATION: Überspringe veraltete Ability-Mods
            // Diese werden jetzt nur noch über Gems im Talentbaum verwaltet
            // Erkenne sie daran, dass sie früher modAbilityData hatten (jetzt entfernt)
            // Alternativ: Prüfe auf bekannte Ability-Mod-Namen (3_1_, 3_2_, etc.)
            if (modDef.modName.StartsWith("3_") || modDef.description.ToLower().Contains("ability") || modDef.description.ToLower().Contains("fähigkeit"))
            {
                Debug.Log($"[MIGRATION] Entferne veralteten Ability-Mod '{modDef.modName}' von Item '{savedItem.itemID}'");
                removedAbilityMods++;
                continue; // Überspringe diesen Mod
            }

            var mod = new ItemMod();
            mod.definition = modDef;
            mod.rolledRarity = Enum.TryParse<Rarity>(modSave.modRarity, out var modRarity) ? modRarity : Rarity.Common;
            mod.rolledValue = modSave.savedValue;
            instance.addedItemMods.Add(mod);
            Debug.Log($"[ItemInstance.FromSave] Mod geladen: {modDef.modName} = {mod.rolledValue}");
        }

        if (removedAbilityMods > 0)
        {
            Debug.Log($"[MIGRATION] {removedAbilityMods} Ability-Mod(s) entfernt von Item '{instance.ItemName}' (Abilities jetzt nur über Gems)");
        }

        // Item Description mit neuen Stats aufbauen
        instance.SetValueDescription(instance);

        Debug.Log($"[ItemInstance.FromSave] === FERTIG === ItemInstance erstellt für: {instance.ItemName}");
        return instance;
    }

    //Wird gecalled, wenn das Item im Inventar angeklickt wird. Dadurch werden die Stats den playerStats hinzugefügt.
    public void Equip(PlayerStats playerStats)
    {

        Debug.Log($"[ItemInstance.Equip] Item: {ItemName}, flatStats.Count: {flatStats.Count}");
        
        foreach (var kvp in flatStats)
        {
            Debug.Log($"[ItemInstance.Equip] Applying flatStat: {kvp.Key} = {kvp.Value}");
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


    public bool IsOnCooldown()
    {
        // Gems haben keine eigenen Cooldowns (werden über Abilities verwaltet)
        return false;
    }

    public float GetCooldown()
    {
        // Gems haben keine eigenen Cooldowns (werden über Abilities verwaltet)
        return 0;
    }

    public float CooldownTimer()
    {
        // Gems haben keine eigenen Cooldowns (werden über Abilities verwaltet)
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


