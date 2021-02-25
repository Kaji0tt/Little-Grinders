using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRolls : MonoBehaviour
{
    //private string[] waffenRollsLegendary = new string[] { "- Ikarus Omen", " - Kitava's Qual", " - Blutrabes Erbe", " - Tyraels Gabe", " - Illidans Rache" };

    public ItemMods[] wLegendaryRolls;

    //private string[] waffenRollsEpic = new string[] { "Essenzpeiniger", "Weltenzerstörer", "aus Titanium geschaffen", "von Titanen gerfertigt", "im Seelenfeuer geschmiedet" };

    public ItemMods[] wEpicRolls;

    //private string[] waffenRollsRare = new string[] { "aus Eryx", "von Pandorra", "aus Himmelsrand", "aus Uldaman", "Runenverziert", "mit Edelsteinen besetzt"};

    public ItemMods[] wRareRolls;

    //private string[] waffenRollsUncommon = new string[] { "gefährlich", "magisch", "zwergisch", "geschärft", "gesegnet", "leicht", "pulsierend", "berüchtigt" };

    public ItemMods[] wUncommonRolls;

    //private string[] waffenRollsNegative = new string[] { "rostend", "stumpf", "abgenutzt", "zerbrechlich", "veraltet", "verflucht", "kräfte zehrend", "schwerfällig" };

    public ItemMods[] wUnbrauchbarRolls;

    private static ItemModsData[] allMods;
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

    private void Awake()
    {

    }

    public ItemInstance CalculateRolls(ItemInstance item)
    {
        print(item.ItemName + " is going to get calculated");
        RollRarity(item);

        //Fix Roll
        PickASingleRoll(item, item.itemRarity);


        //Die ROlls fixen wir später.
        /*
        int numberOfRolls = Random.Range(0, 6);

        for (int i = 0; i < numberOfRolls; i++)
        {
            DefineRolls(item);
        }
        */


        return item;
    }

    private ItemInstance RollRarity(ItemInstance item)
    {
        if(item.itemType != ItemType.Consumable)
        {
            rarityChances = new float[] { _usual, _unbrauchbar, _uncommon, _rare, _epic, _legendary };

            int roll = Random.Range(0, 101);

            float rollSum = 0;

            print("rolled " + roll + " for the Item Rarity of " + item.ItemName);

            for (int i = 0; i < rarityChances.Length; i++)
            {

                rollSum += rarityChances[i];

                if (rollSum >= roll)
                {
                    print("rolled item rarity: " + rarity[i] + ", at a value of: " + rollSum + " which picked " + rarityChances[i] + " for the item " + item.ItemName);

                    item.itemRarity = rarity[i];

                    return item;

                }

            }
        }


        Debug.Log("Could not correctly set Rarity.Roll for " + item.ItemName);

        return null;
    }

    private ItemInstance DefineRolls(ItemInstance item)
    {
        switch (item.itemRarity)
        {
            case "Unbrauchbar":

                _rarityChances = new float[] { _unbrauchbar, _usual, _uncommon, _rare, _epic };

                rarity = new string[] { "Unbrauchbar", "Gewöhnlich", "Ungewöhnlich", "Selten", "Episch" };

                PickRarityOfRoll(item, _rarityChances, rarity);

                break;

            case "Gewöhnlich":

                //Since a usual Roll would not roll any Modifiers, skip this Case.

                break;

            case "Ungewöhnlich":

                _rarityChances = new float[] { _unbrauchbar, _usual, _uncommon };

                rarity = new string[] { "Unbrauchbar", "Gewöhnlich", "Ungewöhnlich" };

                PickRarityOfRoll(item, _rarityChances, rarity);

                break;

            case "Selten":

                _rarityChances = new float[] { _unbrauchbar, _usual, _uncommon, _rare };

                rarity = new string[] { "Unbrauchbar", "Gewöhnlich", "Ungewöhnlich", "Selten" };

                PickRarityOfRoll(item, _rarityChances, rarity);

                break;

            case "Episch":

                _rarityChances = new float[] { _unbrauchbar, _usual, _uncommon, _rare, _epic };

                rarity = new string[] { "Unbrauchbar", "Gewöhnlich", "Ungewöhnlich", "Selten", "Episch" };

                PickRarityOfRoll(item, _rarityChances, rarity);

                break;

            case "Legendär":

                _rarityChances = new float[] { _usual, _uncommon, _rare, _epic, _legendary };

                rarity = new string[] { "Gewöhnlich", "Ungewöhnlich", "Selten", "Episch", "Legendär" };

                PickRarityOfRoll(item, _rarityChances, rarity);

                break;

        }

        return (item);

    }

    private ItemInstance PickRarityOfRoll(ItemInstance item, float[] _rarityChances, string[] rarity)
    {
        //In dependency of float[]_rarityChances.Length, calculate the totalWeight of all possible Rolls.

        float totalPossibleRollWeight = 0;

        for (int i = 0; i < _rarityChances.Length; i++)
        {
            totalPossibleRollWeight += _rarityChances[i];
        }

        //Roll the Rarity of given Roll and pick a List/Array to get Roll from

        int roll = Random.Range(0, Mathf.RoundToInt(totalPossibleRollWeight)+1);

        float rollSum = 0;

        for (int i = 0; i < _rarityChances.Length; i++)
        {
            rollSum += _rarityChances[i];           

            if (roll >= totalPossibleRollWeight)
            {

                PickASingleRoll(item, rarity[i]);

                return item;

            }

        }

        Debug.Log("Could not generate a Rarity Roll for item: " + item.ItemName);

        return null;
    }

    private ItemInstance PickASingleRoll(ItemInstance item, string rarityOfRoll)
    {
        if(item.itemType != ItemType.Consumable)
        {
            if (rarityOfRoll == "Legendär")
            {
                int randomRoll = Random.Range(0, wLegendaryRolls.Length);

                AddMods(new ItemModsData(wLegendaryRolls[randomRoll]), item);

                return item;

            }

            if (rarityOfRoll == "Episch")
            {
                int randomRoll = Random.Range(0, wEpicRolls.Length);

                AddMods(new ItemModsData(wEpicRolls[randomRoll]), item);

                return item;

            }

            if (rarityOfRoll == "Selten")
            {
                int randomRoll = Random.Range(0, wRareRolls.Length);

                AddMods(new ItemModsData(wRareRolls[randomRoll]), item);

                return item;

            }

            if (rarityOfRoll == "Ungewöhnlich")
            {
                int randomRoll = Random.Range(0, wUncommonRolls.Length);

                AddMods(new ItemModsData(wUncommonRolls[randomRoll]), item);

                return item;

            }

            if (rarityOfRoll == "Gewöhnlich")
            {
                return item;
            }

            if (rarityOfRoll == "Unbrauchbar")
            {
                int randomRoll = Random.Range(0, wUncommonRolls.Length);

                AddMods(new ItemModsData(wUnbrauchbarRolls[randomRoll]), item);

                return item;

            }
        }



            return item;
        

    }

    private static ItemInstance AddMods(ItemModsData mods, ItemInstance item)
    {
        Debug.Log("Item Rolls hat versucht Mods auf die ItemInstanz " + item.ItemName + " hinzuzufügen");
            

        for(int i = 0; i < mods.flatValues.Length; i++)
            item.flatValues[i] += mods.flatValues[i];
        

        for (int i = 0; i < mods.percentValues.Length; i++)
            item.percentValues[i] += mods.percentValues[i];
        /*
            Berechnung geht, Description fehlt noch.

        */
            item.ItemName += mods.name;

        Debug.Log("Füge den Mod:" + mods.name + " hinzu.");
            item.addedItemMods.Add(mods);
        
            item.SetValueDescription(item);

        return item;
        
    }

    public static ItemInstance SetRarity(ItemInstance item, string rarity)
    {
        item.itemRarity = rarity;

        return item;
    }
    //Lebenstränke können derzeit nicht konsumiert werden.

    //Rarity wird nicht in "AddMods" generiert sondern im Vorfeld. Das generiert in Farbfehler in der Anzeige nach dem laden.
    //Mods werden beim speichern mehrfach geladen, kummulieren also.

    //Tatsächlich werden alle Added-Mods in der itemInstance abgespeichert.. wahrscheinlich liegt der fehler in dem setzen der Display&Rarity Daten.

    public static ItemInstance GetItemStats(string ID, List<ItemModsData> mods, string rarity)
    {

        ItemInstance item = new ItemInstance(ItemDatabase.GetItemID(ID));

        SetRarity(item, rarity);

        //print(mods.Count);
        
        foreach (ItemModsData mod in mods)
        {
            AddMods(mod, item);
        }
        
        return item;
 

        //ItemInstance.AddMods(modsItemDatabase.GetItemID(ID);

        
    }
    

}
