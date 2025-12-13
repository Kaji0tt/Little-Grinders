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
public class ItemRolls : MonoBehaviour
{
    public float luckModifier = 0f; // Von außen manipulierbar (z. B. per Inspector, aus anderem Script etc.)

    private ItemModDefinition[] allModDefs;

    private readonly Rarity[] rarities = { Rarity.Common, Rarity.Uncommon, Rarity.Rare, Rarity.Epic, Rarity.Legendary };
    private readonly float[] baseRarityWeights = { 0.6f, 0.25f, 0.1f, 0.04f, 0.01f };

    int maxMods = 6;

    public ItemInstance RollItem(ItemInstance itemTemplate, int mapLevel)
    {
        List<ItemMod> mods = new List<ItemMod>();

        while (ShouldRollAnotherMod(mods.Count))
        {
            // Filtere gültige Mods basierend auf ItemType
            // HINWEIS: Aptitude-Mods (Abilities) wurden aus dem System entfernt
            // Abilities werden jetzt nur über Gems im Talentbaum verwaltet
            var validDefs = ItemDatabase.instance.allModDefs
                .Where(def => (def.allowedItemTypes & itemTemplate.itemType) != 0)
                .ToArray();

            if (validDefs.Length == 0) break;

            var def = validDefs[Random.Range(0, validDefs.Length)];
            var rarity = RollRarity();

            float rolledValue = def.GetValue(mapLevel, rarity);

            var mod = new ItemMod
            {
                definition = def,
                rolledRarity = rarity,
                rolledValue = rolledValue
            };

            mods.Add(mod);
        }

        itemTemplate.addedItemMods = mods;
        itemTemplate.ApplyItemMods();

        Rarity highestRarity = mods.Count > 0 ? mods.Max(m => m.rolledRarity) : Rarity.Common;
        itemTemplate.itemRarity = highestRarity;

        return itemTemplate;
    }

    private bool ShouldRollAnotherMod(int currentModCount)
    {
        if (currentModCount >= maxMods) return false;

        float adjustedChance = Mathf.Clamp01(0.3f + luckModifier * 0.2f); // z. B. Luck +1 → 50%, Luck -1 → 10%
        return Random.value < adjustedChance;
    }

    private Rarity RollRarity()
    {
        // Modifiziere die Wahrscheinlichkeiten basierend auf luckModifier
        float[] adjustedWeights = new float[baseRarityWeights.Length];
        for (int i = 0; i < baseRarityWeights.Length; i++)
        {
            float rarityFactor = i / (float)(baseRarityWeights.Length - 1); // Common = 0.0, Legendary = 1.0
            float boost = 1f + (rarityFactor * luckModifier); // z. B. Legendary wird bei luckModifier > 0 mehr geboostet
            adjustedWeights[i] = baseRarityWeights[i] * Mathf.Max(0.1f, boost);
        }

        float total = adjustedWeights.Sum();
        float roll = Random.value * total;

        for (int i = 0; i < adjustedWeights.Length; i++)
        {
            if (roll < adjustedWeights[i]) return rarities[i];
            roll -= adjustedWeights[i];
        }
        return rarities.Last();
    }
}
