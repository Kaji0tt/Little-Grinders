using System.Linq;
using UnityEngine;


//ItemTypes that use Active Abilities: Kopf, Brust, Weapon, Beine, Schuhe. Schmuck soll eine Passiv Fähigkeit werden.
[CreateAssetMenu(fileName = "NewMod", menuName = "Items/Mod Definition")]
public class ItemModDefinition : ScriptableObject
{
    public string modName;
    public string description;

    public EntitieStats targetStat;
    public float baseValue = 1f;

    public ModType modType; // Flat, Percent, etc.
    public RarityScaling[] rarityScalings;

    [Tooltip("Wenn der Mod eine Fähigkeit sein soll, verweise hier auf die entsprechende Ability")]
    public AbilityData modAbilityData;

    [Tooltip("Auf welchen Item Typen diese Modifikation verfügbar sein soll.")]
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

    public float GetRarityMultiplier(Rarity rarity)
    {
        return rarityScalings.FirstOrDefault(r => r.rarity == rarity)?.multiplier ?? 1f;
    }

}


//Mit der rollRarity kann in definition der Anzaigename nachgesehen werden. Eine Funktion "GetName" welche den passend zur rarityVerfügbaren Namen anzeigt wäre sinnvoll.
[System.Serializable]
public class ItemMod
{
    public ItemModDefinition definition;
    public Rarity rolledRarity;
    public float rolledValue;
    public bool IsPercent => definition != null &&
    (definition.modType == ModType.Percent || definition.modType == ModType.PercentFortune);

    // Initialisiert den Mod basierend auf Kartenlevel und Rarity
    public void Initialize(int mapLevel)
    {
        if (definition != null)
        {
            rolledValue = definition.GetValue(mapLevel, rolledRarity);
        }
    }

    // Gibt den dynamischen Anzeigenamen zurück basierend auf der Rarity
    public string GetName()
    {
        if (definition == null || definition.rarityScalings == null)
            return "";

        var scaling = definition.rarityScalings
            .FirstOrDefault(r => r.rarity == rolledRarity);

        return scaling?.displayName ?? definition.modName;
    }

    // Gibt die Beschreibung mit Wert, Zielstat und statischer Textbeschreibung zurück
    public string GetDescription()
    {
        if (definition == null) return "";

        // Spezialbehandlung für Aptitude Mods
        if (definition.modType == ModType.Aptitude)
        {
            return $"Fügt dem Spieler die Fähigkeit {definition.modName} hinzu.";
        }

        // Standard-Beschreibung für andere Mod-Typen
        string valueStr = definition.modType == ModType.Percent || definition.modType == ModType.PercentFortune
            ? $"+{rolledValue * 100f:F1}%"
            : $"+{rolledValue:F1}";

        return $"{valueStr} {definition.targetStat}\n{definition.description}";
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