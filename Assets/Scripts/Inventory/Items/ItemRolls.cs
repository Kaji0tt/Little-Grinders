using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRolls : MonoBehaviour
{
    //Ggf. könnte man einzelne Rolls erstellen - und diese sind, je nach roll, intensiver.

    public ItemMods[] wLegendaryRolls; //{ get; set; }

    public ItemMods[] wEpicRolls; //{ get; set; }

    public ItemMods[] wRareRolls; //{ get; set; }

    public ItemMods[] wUncommonRolls; //{ get; set; }

    public ItemMods[] wUnbrauchbarRolls; //{ get; set; }

    public ItemModsData[] modsOnItem;
    //----Gedanke, bzw. logische Struktur - umgangsprachlich:

    private string[] rarity = new string[] { "Unbrauchbar", "Gewöhnlich", "Ungewöhnlich", "Selten", "Episch", "Legendär" };

    private int[] rarityChances = new int[6];

    private float[] _rarityChances;

    #region Modifier Concepts
    /// <summary>
    /// (Of Flat)
    /// +Add Flatt      (je Type)             wUncommon -> +2 auf AD
    ///                                       wRare     -> +5 auf AD
    ///                                       wEpic     -> +10 auf AD
    ///                                       wLegendary-> +15 auf AD                                   
    /// (Of Percent)
    /// +Add Percent      (je Type)           wUncommon -> +2%   auf AD
    ///                                       wRare     -> +5%   auf AD
    ///                                       wEpic     -> +10%  auf AD
    ///                                       wLegendary-> +15%  auf AD                                   
    /// (Of Flat Fortune)
    /// +Add Range of item      (je Type)     wUncommon -> +/-2  auf AD
    ///                                       wRare     -> +/-5  auf AD
    ///                                       wEpic     -> +/-10 auf AD
    ///                                       wLegendary-> +/-15 auf AD                                   
    /// (Of Percent Fortune)
    /// +Add Range of item      (je Type)     wUncommon -> +/-2%  auf AD
    ///                                       wRare     -> +/-5%  auf AD
    ///                                       wEpic     -> +/-10% auf AD
    ///                                       wLegendary-> +/-15% auf AD                                
    /// (Of Aptitude)
    /// +Add Ability                          wUncommon -> Base      1+3
    ///                                       wRare     -> Enhanced  4+6 
    ///                                       wEpic     -> Ultimate  7+9
    ///                                       wLegendary-> Unique    10+
    ///                                       
    /// 
    /// </summary>
    #endregion

    #region New Concept for Item-Rolls
    ///--- Rarity of Item ---
    ///
    /// RollRarity()
    ///By a Chance of (20% * Level-Modifier * Map-Modifier)  Item will drop as Uncommon,
    ///By a Chance of (15% * Level-Modifier * Map-Modifier)  Uncommon Item will become a Rare,
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
    private float uncommon = 200, rare = 150, epic = 50, legendary = 10;

    private float badroll = 155; 

    [HideInInspector]
    public float levelModifier, worldModifier, bossModifier;

    private void Start()
    {

    }

    public ItemInstance RollItem(ItemInstance item)
    {
        //print(wEpicRolls.Length);
        //If the Item is not a Consumable
        if (item.itemType != ItemType.Consumable)
        {
            //Roll the rarity of the Item with no dependency (5) so it might get every rarity.
            int currentItemRarity = RollRarity(6, "item");

            //Set the Rarity of the item.
            item.itemRarity = rarity[currentItemRarity];

            //If item is not of Common-Rarity, pick a Single Roll of itemRarity Type and add it to the item.
            if (currentItemRarity != 1)
            AddMods(PickASingleRoll(currentItemRarity), item); 

            //Roll how many Mods the Item might have, with the Modifier of the currentItemRarity. 
            //Note that in RollQuantity, item will only roll additional Mods, if its above "Common".
            int numberOfRolls = RollQuantity(currentItemRarity);

            print("The item rolled with: " + numberOfRolls + " additional Mods, by a rarity of " + currentItemRarity);

            //Populate Mods-Array by the rolled Quantity of Mods
            ItemModsData[] allMods = new ItemModsData[numberOfRolls];

            //Downscale the possible rarity of Mods by 1 of ItemRarity
            if (currentItemRarity > 0)
                currentItemRarity  -= 1;

            //Create a Mod for each ItemModsData in Array allMods
            for (int i = 0; i <= allMods.Length -1; i++ )
            {
                print("We are in the int:i loop for all Mods, which has a length of " + allMods.Length);
                //Roll the Rarity of each Mod, capped at below the rarity of the Item
                allMods[i] = PickASingleRoll(RollRarity(currentItemRarity, "mod"));
            }

            foreach(ItemModsData mod in allMods)
            {
                //Foreach Mod, add according Values to the Item, if its not null. Null-Mods may occur, if the itemRarity is of type "Uncommon"
                if (mod != null) 
                    AddMods(mod, item);
            }


        }




        return item;
    }

    //Problem: Der Roll soll anwendbar sowohl auf Items, als auch Mods sein.

    //Wenn die Item
    
    private int RollRarity(int fixedRarity, string type)
    {
        //Bestimme den Level-Modifier anhand des Spieler-Levels
        //levelModifier = 1 + 1 * (PlayerManager.instance.player.GetComponent<PlayerStats>().level / 1000);

        //Bestimme den Roll-Modifier anhand des Welt-Levels (Replaced with Level-Modifier 23.09.22)
        if (GlobalMap.instance != null)
            worldModifier = 1 + 1 * (GlobalMap.instance.currentMap.mapLevel / 10);
        else worldModifier = 1;


        //Da wir ein neues Item mit einer fixedRarity von 5 in den Roll geben, kann als itemRarity nicht ein "Unbrauchbaren" Standardwert gesetzt werden. Somit bleibt diese Option nur für Mods.
        if ((Random.Range(1, 1000) >= (badroll - 10 * worldModifier)) && (fixedRarity > 0)) 
        {
            if (type != "mod")
                print("Passed the Bad Rarity, rolling for an Uncommon! /n Rarity is currently Common.");
            else
                print("Items only of type Uncommon can't roll additional mods but badrolls, resultung in Null;");

            //Wenn das Item gewöhnlich ist, darf kein Roll diese Raritätsstufe überschreiten. Falls es sich um ein Mod handelt, der gewürfelt wird, wird die Abfrage geskipped.
            if ((Random.Range(1, 1000) <= uncommon * worldModifier || type == "mod") && (fixedRarity > 1))
            {
                print("Passed the Uncommon Rarity, rolling for a Rare! /n Rarity is currently Uncommon.");

                //Wenn das Item rare ist, darf kein Roll diese Raritätsstufe überschreiten.
                if ((Random.Range(1, 1000) <= (rare * worldModifier)) && (fixedRarity > 2))
                {
                    print("Passed the Rare Rarity, rolling for an Epic! /n Rarity is currently Rare.");
                    //Wenn das Item episch ist, darf kein Roll diese Raritätsstufe überschreiten.

                    if ((Random.Range(1, 1000) <= (epic * worldModifier)) && (fixedRarity > 3))
                    {
                        print("Passed the Epic Rarity, rolling for a Legendary! /n Rarity is currently Epic.");
                        //Ein Item, das Legendär ist, darf keine weitere Legendären Rolls erhalten, und darf diese Raritätsstufe nicht erneut überschreiten. 

                        if ((Random.Range(1, 1000) <= (legendary * worldModifier) || type != "mod") && (fixedRarity > 4))
                        {
                            print("Congrats! Rolled a Legendary!");
                            //rarity[5]=Legendary
                            return 5; 
                        }
                        //Ansonsten: rarity[4]=Epic
                        else
                            return 4;
                    }
                    //Ansonsten: rarity[3]=Rare
                    else
                        return 3;
                }
                //Ansonsten: rarity[2]=Uncommon 
                else
                    return 2;
            }
            //Ansonsten: rarity[1]=Gewöhnlich // No Mods
            else
                return 1;
        }
        //Ansonsten: rarity[0]=Unbrauchbar
        else
            return 0;
    }

    //Die Menge der Rolls wird nicht nur über die Standardwerte berechnet, sondern skaliert mit der Raritätsstufe des Items.
    private int RollQuantity(int rarityModifier)
    {
        //Das Item darf nur zusätzliche Mods erhalten, wenn es nicht von der Seltenheitsstuffe "Unbrauchbar", "Gewöhnlich" oder "Ungewöhnlich" ist.
        if (rarityModifier > 2)

            //Würfel die Menge der zusätzlichen Modifier.
            if (Random.Range(0, 1001) <= uncommon * worldModifier * rarityModifier / 2)
            {

                //Beispiel: Chance Minimum 2 zusätzliche Rolls zu erhalten, bei Spielerstufe 100 bei einem Item der Rarität Episch.
                //          150 * 2 * 3 / 2 = 450
                //          Mit 45% Wahrscheinlichkeit, würde ein episches Item auf Level 100 mit 2 oder mehr zusätzlichen Rolls generiert werden.
                if (Random.Range(1, 1000) <= rare * worldModifier * rarityModifier / 2)
                {

                    if (Random.Range(1, 1000) <= rare * worldModifier * rarityModifier / 2)
                    {

                        if (Random.Range(1, 1000) <= epic * worldModifier * rarityModifier / 2)
                        {

                            if (Random.Range(1, 10000) <= legendary * levelModifier * rarityModifier / 2)
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
        else return 0;
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

                int randomRoll = Random.Range(0, wUnbrauchbarRolls.Length);

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
        ItemInstance item = new ItemInstance(ItemDatabase.GetItemByID(ID));

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
