using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;

    public ItemRolls itemRolls;
    //private enum LootTable;
    //Loottable loot;

    //private static List<List<Item>> totalLoottable = new List<List<Item>>();


    //Temporär für die Dropchance
    List<Item> currentDropTable = new List<Item>();

    List<Item> allItems = new List<Item>();
    public ItemModDefinition[] allModDefs { get; private set; }

    int totalWeight;

    private int percentSum = 0;



    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // DontDestroyOnLoad hinzufügen
        DontDestroyOnLoad(gameObject);

        allModDefs = Resources.LoadAll<ItemModDefinition>("Mods");
        allItems.AddRange(Resources.LoadAll<Item>("Items"));

        Debug.Log("ItemDatabase initialized.");
    }


    public void GetWeightDrop(Vector3 position)
    {
        Debug.Log($"GetWeightDrop called at {position}");

        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        int level = playerStats.Get_level();

        currentDropTable.Clear();

        totalWeight = 0;

        CalculateTotalWeight(playerStats);

        currentDropTable.Sort(CompareItemWeights);

        // Weighted Random Roll
        int roll = Random.Range(0, totalWeight);
        int currentSum = 0;

        foreach (Item item in currentDropTable)
        {
            currentSum += item.calculatedWeight;

            if (roll < currentSum)
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
        int playerLevel = playerStats.level;
        const float MIN_WEIGHT_FACTOR = 0.1f;  // Mindestens 10% vom Base-Weight
        const float GEM_WEIGHT_MULTIPLIER = 0.5f;  // Gems sind halb so häufig wie normale Items

        currentDropTable.Clear();
        totalWeight = 0;

        foreach (Item item in allItems)
        {
            // 1. Level-Penalty: Kann das Item überhaupt droppen?
            int levelDiff = item.dropLevel - playerLevel;
            float levelPenalty;

            if (levelDiff <= 0) 
            {
                // Item ist für Spieler geeignet
                levelPenalty = 1f;
            }
            else 
            {
                // Item ist zu hoch-levelig → stark reduzierte Chance
                levelPenalty = Mathf.Max(MIN_WEIGHT_FACTOR, 1f / (1f + Mathf.Pow(levelDiff, 1.5f)));
            }

            // 2. Typ-Modifier: Gems seltener machen (optional)
            float typeModifier = (item.itemType == ItemType.Gem) ? GEM_WEIGHT_MULTIPLIER : 1f;

            // 3. Finaler Weight = BaseWeight * LevelPenalty * TypeModifier
            item.calculatedWeight = Mathf.RoundToInt(item.dropWeight * levelPenalty * typeModifier);
            
            totalWeight += item.calculatedWeight;
            currentDropTable.Add(item);
        }

        return totalWeight;
    }

    public int CompareItemWeights(Item item1, Item item2)
    {
        return item2.calculatedWeight.CompareTo(item1.calculatedWeight);
    }


    public float PosDiff(float nr1, float nr2)
    {
        float result = Mathf.Abs(nr1 - nr2);
        if (result <= 0)
            result = result * -1;
        return result;
    }

    public Item GetItemByID(string ID)
    {
        foreach (Item item in allItems)
        {
            if (item.ItemID == ID)
            {
                return item;
            }
        }
        Debug.LogWarning($"No item with ID: {ID} found in allItems.");
        return null;
    }

    public Item GetItemByName(string name)
    {
        foreach (Item item in allItems)
        {
            if (item.ItemName == name)
            {
                return item;
            }
        }
        Debug.LogWarning($"No item with name: {name} found in allItems.");
        return null;
    }
    
    public ItemModDefinition GetModDefinitionByName(string modName)
    {
        if (allModDefs == null) return null;

        foreach (var modDef in allModDefs)
        {
            if (modDef.modName == modName)
                return modDef;
        }

        Debug.LogWarning($"No ItemModDefinition found with name: {modName}");
        return null;
    }

}