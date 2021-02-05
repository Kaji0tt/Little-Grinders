using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRolls : MonoBehaviour
{
    private string[] waffenRollsLegendary = new string[] { "- Ikarus Omen", " - Kitava's Qual", " - Blutrabes Erbe", " - Tyraels Gabe", " - Illidans Rache" };

    public ItemMods[] wLegendaryRolls;

    private string[] waffenRollsEpic = new string[] { "Essenzpeiniger", "Weltenzerstörer", "aus Titanium geschaffen", "von Titanen gerfertigt", "im Seelenfeuer geschmiedet" };

    public ItemMods[] wEpicRolls;

    private string[] waffenRollsRare = new string[] { "aus Eryx", "von Pandorra", "aus Himmelsrand", "aus Uldaman", "Runenverziert", "mit Edelsteinen besetzt"};

    public ItemMods[] wRareRolls;

    private string[] waffenRollsUncommon = new string[] { "gefährlich", "magisch", "zwergisch", "geschärft", "gesegnet", "leicht", "pulsierend", "berüchtigt" };

    public ItemMods[] wUncommonRolls;

    private string[] waffenRollsNegative = new string[] { "rostend", "stumpf", "abgenutzt", "zerbrechlich", "veraltet", "verflucht", "kräfte zehrend", "schwerfällig" };

    public ItemMods[] wUnbrauchbarRolls;


    //----Gedanke, bzw. logische Struktur - umgangsprachlich:

    // --- item.Rarity:                 Die Rarity wird ausgewürfelt, sie bestimmen die Wahrscheinlichkeiten der Rolls 
    // 20% Chance - Crap-Rarity,        
    // 33% Chance - Keine Rarity,
    // 26% Chance - Ungewöhnlich,
    // 15% Chance - Selten,
    // 5%  Chance - Episch,
    // 1%  Chance - Legendär.

    
    [HideInInspector]
    public float _unbrauchbar = 20, _usual = 33, _uncommon = 26, _rare = 15, _epic = 5, _legendary = 1;

    private string[] rarity = new string[] { "Unbrauchbar", "Gewöhnlich", "Ungewöhnlich", "Selten", "Episch", "Legendär" };

    private float[] rarityChances = new float[6];

    private float[] _rarityChances;

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

        RollRarity(item);

        int numberOfRolls = Random.Range(0, 6);


        DefineRolls(item, numberOfRolls);



        return item;
    }

    private void DefineRolls(Item item, int numberOfRolls)
    {
        switch (item.itemRarity)
        {
            case "Unbrauchbar":
                _rarityChances = new float[] { _unbrauchbar, _usual, _uncommon, _rare, _epic };
                break;
            case "Gewöhnlich":
                _rarityChances = new float[] { _unbrauchbar, _usual, _uncommon, _rare, _epic, _legendary };
                break;
            case "Ungewöhnlich":
                _rarityChances = new float[] { _unbrauchbar, _usual, _uncommon, _rare, _epic, _legendary };
                break;
            case "Selten":
                _rarityChances = new float[] { _unbrauchbar, _usual, _uncommon, _rare, _epic, _legendary };
                break;
            case "Episch":
                _rarityChances = new float[] { _unbrauchbar, _usual, _uncommon, _rare, _epic, _legendary };
                break;
            case "Legendär":
                _rarityChances = new float[] { _usual, _uncommon, _rare, _epic, _legendary };
                break;

        }

        for(int i = 0; i < numberOfRolls; i++)
        {

            PickRarityOfRoll(_rarityChances);
        }
    }

    private void PickRarityOfRoll(float[] _rarityChances)
    {
        //In Abhängigkeit von der float[] länge, soll der Gesamtwert der Floats berechnet werden, um ein variable "totalPossibleRolls" zu errechnen.

        float totalPossibleRollWeight = 0;

        for (int i = 0; i < _rarityChances.Length; i++)
        {
            totalPossibleRollWeight += _rarityChances[i];
        }
        //Anschließend soll ein Würfel von 0 bis "totalPossibleRolls" geworfen werden und die übliche Pick-Prozedur abgehalten werden. (Hier wäre allmählich eine eigene Klasse sinnig)

        int roll = Random.Range(0, Mathf.RoundToInt(totalPossibleRollWeight)+1);
        //
        float rollSum = 0;

        for (int i = 0; i < _rarityChances.Length; i++)
        {
            rollSum += _rarityChances[i];

            if (rollSum >= totalPossibleRollWeight)
            {
                //Pick According String-Array

                //Oh Gott, jetzt wirds krebsig. Nice Spaghettis right there.



                //return item;

            }

        }
    }

    private Item RollRarity(Item item)
    {
        rarityChances = new float[] { _unbrauchbar, _usual, _uncommon, _rare, _epic, _legendary };

        int roll = Random.Range(0, 101);

        float rollSum = 0;

        //int totalWeight = 100;

        //Funktion und Aufbau der Niere

        for(int i = 0; i < rarityChances.Length; i++)
        {
            rollSum += rarityChances[i];

            if(rollSum >= roll)
            {
                //string r = rarity[i];
                //item.itemRarity = ItemRarity.name();
                item.itemRarity = rarity[i];

                return item;

            }

        }

        Debug.Log("Could not correctly set Rarity.Roll for " + item.ItemName );

        return null;
    }


}
