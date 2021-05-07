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

    private string[] rarity = new string[] { "Unbrauchbar", "Gewöhnlich", "Ungewöhnlich", "Selten", "Episch", "Legendär" };

    private int[] rarityChances = new int[5];

    private float[] _rarityChances;

    #region New Concept for Item-Rools
    ///--- Rarity of Item ---
    ///
    /// RollRarity()
    ///By a Chance of (15% * Level-Modifier * Map-Modifier)  Item will drop as Uncommon,
    ///By a Chance of (10% * Level-Modifier * Map-Modifier)  Uncommon Item will become a Rare,
    ///By a Chance of (5% * Level-Modifier * Map Modifier)   Rare Item will become an Epic,
    ///By a Chance of (1% * Level-Modifier * Map-Modifier)   Epic Item will become a Legendary
    ///
    /// -> This System will allow to add a global Modifier to percentage, scaling by Player-Level or Map-Level
    ///    E.g.: If player is level 10, these scales will improve by 1,1. If player is Level 20, this scale will improve by 1,2

    ///--- Quantity of Rolls ---
    ///
    /// As an Item rolls its Rarity, it will have 1 guaranteed Mod of Type of rolled Rarity.
    /// It then should roll how many additonal Rolls there might be. 
    /// 
    /// RollQuantity()
    ///By a Chance of       (20% * Level-Modifier * Map-Modifier) Item will have an additional Roll, resulting in:        2 Rolls
    ///then, by a Chance of (15% * Level-Modifier * Map-Modifier) Item will have another additional Roll, resultin in:    3 Rolls
    ///then, by a Chance of (10% * Level-Modifier * Map Modifier) Item will have another additional Roll, resulting in:   4 Rolls
    ///then, by a Chance of (5%  * Level-Modifier * Map Modifier)  Item will have another additional Roll,resulting in:   5 Rolls
    ///then, by a Chance of (1%  * Level-Modifier * Map Modifier)  Item will have another additional Roll,resulting in:   6 Rolls, which should be the highest possible Amount of Rolls.
    ///
    /// Every single Roll should then RollRarity(), where the additional Rolls may only Roll below the Rarity of Item, //including BadRolls.
    #endregion


    //private float _usual = 400, _unbrauchbar = 250, _uncommon = 150, _rare = 100, _epic = 80, _legendary = 20;
    private float uncommon = 200, rare = 100, epic = 50, legendary = 10;

    private float badroll = 155;

    [HideInInspector]
    public float levelModifier, worldModifier, bossModifier;

    public ItemInstance RollItem(ItemInstance item)
    {
        //If the Item is not a Consumable
        if (item.itemType != ItemType.Consumable)
        {
            //Roll the rarity of the Item with no dependency (0) so it might get every rarity and safe it to 
            int currentItemRarity = RollRarity(5);

            print("Es wurde die Seltenheitsstufe: " + currentItemRarity + " ausgwählt.");

            item.itemRarity = rarity[currentItemRarity];

            PickASingleRoll(currentItemRarity);

            int numberOfRolls = RollQuantity();

            ItemMods[] allRolledMods = new ItemMods[numberOfRolls];

            print(allRolledMods.Length);

            foreach(ItemMods mods in allRolledMods)
            {

                AddMods(PickASingleRoll(RollRarity(currentItemRarity)), item);
            }

        }



        //Set the Rarity of the Item, if its not a Consumable
        //if(item.itemType != ItemType.Consumable)
        //item.itemRarity = rarity[RollRarity(item)];

        //Fix Roll
        //PickASingleRoll(item, item.itemRarity);


        return item;
    }

    private string SetRarity()
    {
        
        return "hi";
    }

    //Problem: Der Roll soll anwendbar sowohl auf Items, als auch Mods sein.
    //Wenn die Item
    
    private int RollRarity(int fixedRarity)
    {
        //Bestimme den Level-Modifier anhand des Spieler-Levels
        levelModifier = 1 + 1 * (PlayerManager.instance.player.GetComponent<PlayerStats>().level / 10);

        //Bestimme den Map-Modifier anhand des Welt-Levels (Maps not implemented yet.)
        // worldModifier = 1 + 1 * (MapLevel / 10)

        //Da wir ein neues Item mit einer fixedRarity von 5 in den Roll geben, kann als itemRarity nicht ein "Unbrauchbaren" Standardwert gesetzt werden. Somit bleibt diese Option nur für Mods.
        if (Random.Range(0, 1001) <= badroll - 10 * levelModifier && fixedRarity != 5 && fixedRarity > 1)
        {
            //Wenn das Item ungewöhnlich ist, darf kein Roll diese Raritätsstufe überschreiten.
            if (Random.Range(0, 1001) <= uncommon * levelModifier && fixedRarity > 1) // Falls ( Random <= Uncommon && Seltenheitsstufe nicht über Uncommon)
            {
                //Wenn das Item rare ist, darf kein Roll diese Raritätsstufe überschreiten.
                if (Random.Range(0, 1001) <= rare * levelModifier && fixedRarity > 2)
                {
                    //Wenn das Item episch ist, darf kein Roll diese Raritätsstufe überschreiten.
                    if (Random.Range(0, 1001) <= epic * levelModifier && fixedRarity > 3)
                    {
                        //Ein Item, das Legendär ist, darf keine weitere Legendären Rolls erhalten, und darf diese Raritätsstufe nicht erneut überschreiten. 
                        if (Random.Range(0, 1001) <= legendary * levelModifier && fixedRarity > 4)
                        {
                            return 5; //Denkfehler, ein Item das die Rarity Ungewöhnlich erhalten hat, wird stets all Rolls bekommen können, da es im Umkehrschluss auch stets unter dem Schwellenwert liegt.
                        }
                        else
                            return 4;
                    }
                    else
                        return 3;
                }
                else
                    return 2;
            }
            else
                return 1;
        }
        else
            return 0;
    }

    //Geht sicher noch hübscher, do dis.
    private int RollQuantity()
    {
        //Würfel eine Raritätsstufe.
        if (Random.Range(0, 1001) <= uncommon * levelModifier)
        {

            if (Random.Range(0, 1001) <= uncommon * levelModifier)
            {

                if (Random.Range(0, 1001) <= rare * levelModifier)
                {

                    if (Random.Range(0, 1001) <= rare * levelModifier)
                    {

                        if(Random.Range(0, 10001) <= epic * levelModifier)
                        {
                            return 6;
                        }
                        else
                        return 5;
                    }
                    else
                    return 4;
                }
                else
                return 3;
            }
            else
            return 2;

        }
        else
        return 1;      
    }


//Es wäre eigentlich schlauer, wenn PickASingleRoll nicht ein Item zurück gibt, sondern ItemModsData. Dann könnte oben der Array aus ItemMods entsprechende ItemModsData generieren.
    private ItemModsData PickASingleRoll(int rarityOfRoll)
    {

            if (rarityOfRoll == 5)
            {
                int randomRoll = Random.Range(0, wLegendaryRolls.Length);

                return new ItemModsData(wLegendaryRolls[randomRoll]);

            }

            if (rarityOfRoll == 4)
            {
                int randomRoll = Random.Range(0, wEpicRolls.Length);

                return new ItemModsData(wEpicRolls[randomRoll]);

            }

            if (rarityOfRoll == 3)
            {
                int randomRoll = Random.Range(0, wRareRolls.Length);

                return new ItemModsData(wRareRolls[randomRoll]);

            }

            if (rarityOfRoll == 2)
            {
                int randomRoll = Random.Range(0, wUncommonRolls.Length);

                return new ItemModsData(wUncommonRolls[randomRoll]);

            }

            if (rarityOfRoll == 1)
            {

                return null;
            }

            if (rarityOfRoll == 0)
            {
                int randomRoll = Random.Range(0, wUncommonRolls.Length);

                return new ItemModsData(wUnbrauchbarRolls[randomRoll]);

            }
        

            return null;        

    }

    private static ItemInstance AddMods(ItemModsData mods, ItemInstance item)
    {
        //Schreibe Flat und Percent Values der Mods auf das Item

        for(int i = 0; i < mods.flatValues.Length; i++)
            item.flatValues[i] += mods.flatValues[i];
        

        for (int i = 0; i < mods.percentValues.Length; i++)
            item.percentValues[i] += mods.percentValues[i];


        //Schreibe den Namen des Items.
            item.ItemName += mods.name;

        //Füge den Mod:" + mods.name + " hinzu.
            item.addedItemMods.Add(mods);
        
            item.SetValueDescription(item);

        //Gebe das Item zurück.
        return item;
        
    }

    public static ItemInstance SetRarity(ItemInstance item, string rarity)
    {
        item.itemRarity = rarity;

        return item;
    }

    //Ist vorallem für das Laden wichtig, damit das "Flat-Item" mit den entsprechend abgespeicherten Mods generiert wird.
    public static ItemInstance GetItemStats(string ID, List<ItemModsData> mods, string rarity)
    {
        //Erstelle eine ItemInstance, welche aus der Datenbank anhand der ID ausgelesen wird.
        ItemInstance item = new ItemInstance(ItemDatabase.GetItemID(ID));

        //Setze die Rarität des Items entsprechend des gespeicherten Strings.
        SetRarity(item, rarity);

        //Füge die entsprechenden Mods, welche abgespeichert wurden, dem Item hinzu.
        foreach (ItemModsData mod in mods)
        {
            AddMods(mod, item);
        }
        
        //Gebe das Item zurück.
        return item;

        
    }
    

}
