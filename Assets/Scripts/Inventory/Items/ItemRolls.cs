using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public class ItemMod
{
    public ItemModDefinition definition;
    public Rarity rollRarity;
    public float rolledValue;

    public void Initialize(int mapLevel)
    {
        rolledValue = definition.GetValue(mapLevel, rollRarity);
    }

    public string GetDisplayText()
    {
        return $"{definition.modName}: +{rolledValue} {definition.targetStat}";
    }
}

public class ItemRolls : MonoBehaviour
{
    private ItemModDefinition[] allModDefs;

    // Mod-Roll-Chance (z. B. 50% Wahrscheinlichkeit, dass ein weiterer Mod gerollt wird)
    private const float modRollChance = 0.4f;

    // Raritätenverteilung (fix / hardcoded)
    private readonly Rarity[] rarities = { Rarity.Common, Rarity.Uncommon, Rarity.Rare, Rarity.Epic, Rarity.Legendary };
    private readonly float[] rarityWeights = { 0.6f, 0.25f, 0.1f, 0.04f, 0.01f };

    int maxMods = 6;
    public ItemInstance RollItem(ItemInstance itemTemplate, int mapLevel)
    {
        //ItemInstance instance = new ItemInstance(itemTemplate);


        List<ItemMod> mods = new List<ItemMod>();

        while (Random.value < modRollChance && mods.Count < maxMods)
        {
            var validDefs = ItemDatabase.instance.allModDefs.Where(def => (def.allowedItemTypes & itemTemplate.itemType) != 0).ToArray();
            if (validDefs.Length == 0) break;

            var def = validDefs[UnityEngine.Random.Range(0, validDefs.Length)];
            var rarity = RollRarity();

            float rolledValue = def.GetValue(mapLevel, rarity);

            var mod = new ItemMod
            {
                definition = def,
                rollRarity = rarity,
                rolledValue = rolledValue
            };

            mods.Add(mod);
        }

        //Setze die ItemRarity zur höchsten Rarity der verfügbaren Rolls.
        Rarity highestRarity = mods.Count > 0 ? mods.Max(m => m.rollRarity) : Rarity.Common;
        itemTemplate.itemRarity = highestRarity;

        //instance = GetItemStats(itemTemplate.ID, modDataList, highestRarity);
        return itemTemplate;
    }

    private Rarity RollRarity()
    {
        float total = rarityWeights.Sum();
        float roll = Random.value * total;
        for (int i = 0; i < rarityWeights.Length; i++)
        {
            if (roll < rarityWeights[i]) return rarities[i];
            roll -= rarityWeights[i];
        }
        return rarities.Last();
    }
}

    


