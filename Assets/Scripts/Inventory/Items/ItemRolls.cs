using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRolls : MonoBehaviour
{
    private string[] waffenRollsLegendary = new string[] { "- Ikarus Omen", " - Kitava's Qual", " - Blutrabes Erbe", " - Tyraels Gabe", " - Illidans Rache" };

    private string[] waffenRollsEpic = new string[] { "Essenzpeiniger", "der Weltenzerstörer", "aus Mithril geschaffen", "von Titanen gerfertigt", "im Seelenfeuer geschmiedet" };

    private string[] waffenRollsRare = new string[] { "aus Eryx", "von Pandorra", "aus Himmelsrand", "aus Uldaman", "Runenverziert", "mit Edelsteinen besetzt"};

    private string[] waffenRollsUncommon = new string[] { "gefährlich", "magisch", "zwergisch", "geschärft", "gesegnet", "leicht", "pulsierend", "berüchtigt" };

    private string[] waffenRollsNegative = new string[] { "rostend", "stumpf", "abgenutzt", "zerbrechlich", "veraltet", "verflucht", "kräfte zehrend", "schwerfällig" };


    //----Gedanke, bzw. logische Struktur - umgangsprachlich:

    // --- item.Rarity:                 Die Rarity wird ausgewürfelt, sie bestimmen die Wahrscheinlichkeiten der Rolls 
    // 18% Chance - Crap-Rarity,        
    // 33% Chance - Keine Rarity,
    // 25% Chance - Ungewöhnlich,
    // 15% Chance - Selten,
    // 6%  Chance - Episch,
    // 2%  Chance - Legendär.

    // -- item.Rolls:                  Anschließend werden die Rolls in %-Abhängigkeit von der Rarity ausgewürfelt -> es wird (bis zu) 5x gewürfelt?
    // Crap-Rarity:     80% Negative, 14% Uncommon, 5% Rare, 1% Epic, 0% Legendary
    // No-Rarity:       No Rolls.
    // Ungewöhnlich:    20% Negative, 60%.. da muss ich irgendwie nochmal drüber nachdenken.

    //Next Konzept:

    //Der Wurf von Item.Rarity bestimmt einen gesicherten Roll der Kategorie,

    //Es wird zusätzlich gewürfelt, ohne Influence, wie viele zusätzliche Rolls das Item erhalten kann (0-5) -> No-Rarity müsste in dieser Instanz rausgerechnet werdne.

    //Ein Crap Item kann keine Legendären Rolls enthalten <-> ein Legendäres Items keine Crap-Rolls

    public Item CalculateRolls(Item item)
    {
        //switch (item.itemType)

        return item;
    }
}
