using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;

    public ItemRolls itemRolls;
    //private enum LootTable;
    //Loottable loot;

    public List<Item> tier1;
    public List<Item> tier2;
    public List<Item> tier3;
    public List<Item> tier4;
    public List<Item> tier5;

    private static List<List<Item>> totalLoottable = new List<List<Item>>();


    //Sollte längerfristig automatisch initialisiert werden über Ressource Folder
    List<Item> currentDropTable = new List<Item>();

    List<Item> allItems = new List<Item>();
    public ItemModDefinition[] allModDefs { get; private set; }

    int totalWeight;

    private int percentSum = 0;



    private void Awake()
    {
        instance = this;

        allModDefs = Resources.LoadAll<ItemModDefinition>("Mods");

        allItems.AddRange(Resources.LoadAll<Item>("Items"));
    }


    public void GetWeightDrop(Vector3 position)
    {
        Debug.Log($"GetWeightDrop called at {position}");

        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        int level = playerStats.Get_level();

        currentDropTable.Clear();

        totalWeight = 0;

        CalculateTotalWeight(playerStats);

        currentDropTable.Sort(CompareItemPercents);

        // Berechnung des Rolls
        percentSum = 0;
        int roll = Random.Range(0, totalWeight);

        foreach (Item item in currentDropTable)
        {
            percentSum += (int)item.c_percent;//((int)item.c_percent * 100) / totalWeight;

            if (roll < percentSum)
            {
                position = position + new Vector3(Random.Range(-.5f, .5f), .1f, Random.Range(-.5f, .5f));


                ItemInstance newItem = new ItemInstance(item);

                //itemRolls -> neues Item providen
                ItemWorld.SpawnItemWorld(position, itemRolls.RollItem(newItem, 1));

                Debug.Log($"Item {item.ItemName} with ID {item.ItemID} dropped at position {position} with roll {roll} and percentSum {percentSum}");

                //newItem.AppendModNamesToItemName();
                
                //newItem.UpdateItemDescriptionWithMods();

                break;
            }
        }


    }
    int CalculateTotalWeight(PlayerStats playerStats)
    {
        int mapLevel = playerStats.level;

        currentDropTable.Clear();
        totalWeight = 0;

        foreach (Item item in allItems)
        {
            // Leveldifferenz zwischen Maplevel und Item-Level
            float levelDiff = Mathf.Abs(item.baseLevel - mapLevel);

            // Je näher das Item-Level am Maplevel, desto höher das Gewicht
            // z. B. Exponentialer Abfall: je weiter weg, desto drastischer der Drop in Chance
            float weightFactor = 1f / (1f + Mathf.Pow(levelDiff, 1.5f));

            // Skalieren mit Originalgewicht (z. B. 100) → du kannst auch ein separates Feld benutzen
            item.c_percent = item.percent * weightFactor;

            totalWeight += (int)item.c_percent;
            currentDropTable.Add(item);
        }

        // Optional: Chance dass gar nichts droppt
        totalWeight = (totalWeight / 100) * 300;

        return totalWeight;
    }

    public int CompareItemPercents(Item item1, Item item2)
    {
        return item2.c_percent.CompareTo(item1.c_percent);
    }


    public float PosDiff(float nr1, float nr2)
    {
        float result = Mathf.Abs(nr1 - nr2);
        if (result <= 0)
            result = result * -1;
        return result;
    }

    public static Item GetItemByID(string ID)
    {
        //Debug.Log("Searching for item with ID: " + ID);

        for(int i = 0; i < totalLoottable.Count; i++)
        {
            
            foreach(Item item in totalLoottable[i])
            {

                if (item.ItemID == ID)
                {
                    Item foundItem = item;

                    //Es sollten nun Instanzen von Items geladen werden. Mit dieser Loop werden die Items der SO gefunden, nicht jedoch die Instanzen.
                    //print("Found item: " + foundItem.ItemName);

                    return foundItem;
                }

            }

        }

        Debug.Log("No item with ID: " + ID + " found.");

        return null;
    }

    public static Item GetItemByName(string name)
    {
        //Debug.Log("Searching for item with ID: " + ID);

        for (int i = 0; i < totalLoottable.Count; i++)
        {

            foreach (Item item in totalLoottable[i])
            {

                if (item.ItemName == name)
                {
                    Item foundItem = item;

                    //Es sollten nun Instanzen von Items geladen werden. Mit dieser Loop werden die Items der SO gefunden, nicht jedoch die Instanzen.
                    //print("Found item: " + foundItem.ItemName);

                    return foundItem;
                }

            }

        }

        Debug.Log("No item with ID: " + name + " found.");

        return null;
    }

}