using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMod", menuName = "Items/Mod Definition")]
public class ItemModDefinition : ScriptableObject
{
    public string modName;
    public string description;

    public EntitieStats targetStat;
    public float baseValue = 1f;

    public ModType modType; // Flat, Percent, etc.
    public RarityScaling[] rarityScalings;

    [Tooltip("Auf welchen Item Typen diese Modifikation verüfgbar sein soll.")]
    public ItemType allowedItemTypes;

    [Header("Skalierung pro Level")]
    [Tooltip("Wie stark der Mod pro Map-Level stärker wird (z. B. 0.05 = +5% pro Level)")]
    [Range(0f, 1f)]
    public float scalingPerLevel = 0.05f;

    public float GetValue(int mapLevel, Rarity rarity)
    {
        float levelMultiplier = 1f + (mapLevel - 1) * scalingPerLevel;
        float rarityMultiplier = GetRarityMultiplier(rarity);
        return baseValue * levelMultiplier * rarityMultiplier;
    }

    private float GetRarityMultiplier(Rarity rarity)
    {
        return rarityScalings.FirstOrDefault(r => r.rarity == rarity)?.multiplier ?? 1f;
    }

}

[System.Serializable]
public class RarityScaling
{
    public Rarity rarity;
    public float multiplier;
    public string displayName; // z. B. "Segen des Hirten" für Legendary
}

public enum ModType { Flat, Percent, FlatFortune, PercentFortune, Aptitude }