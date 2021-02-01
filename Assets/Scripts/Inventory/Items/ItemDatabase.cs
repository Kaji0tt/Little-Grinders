using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance;

    //private enum LootTable;
    //Loottable loot;

    public List<Item> tier1;
    public List<Item> tier2;
    public List<Item> tier3;
    public List<Item> tier4;
    public List<Item> tier5;


    private static List<List<Item>> totalLoottable = new List<List<Item>>();

    List<Item> currentDropTable = new List<Item>();

    int itemsTotalCount;

    int totalWeight;

    private int percentSum = 0;

    private void Awake()
    {
        instance = this;

        itemsTotalCount = tier1.Count + tier2.Count + tier3.Count + tier4.Count + tier5.Count;

        totalLoottable.Add(tier1);
        totalLoottable.Add(tier2);
        totalLoottable.Add(tier3);
        totalLoottable.Add(tier4);
        totalLoottable.Add(tier5);

    }


    public void GetWeightDrop(Vector3 position)
    {


        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        int level = playerStats.Get_level();

        currentDropTable.Clear();

        totalWeight = 0;

        int dropRange = 2;

        int dropTable = 0;

            if (level <= dropRange)
            {
                calculateTotalWeight(dropTable);
                dropTable = 0;
            }

            else if (level <= dropRange * 2)
            {
                calculateTotalWeight(dropTable);
                dropTable =  1;
            }

            else if (level <= dropRange * 3)
            {
                calculateTotalWeight(dropTable);
                dropTable = 2;
            }

            else if (level <= dropRange * 4)
            {
                calculateTotalWeight(dropTable);
                dropTable = 3;
            }

            else if (level <= dropRange * 5)
            {
                calculateTotalWeight(dropTable);
                dropTable = 4;
            }



        //print(totalWeight);
        // Berechnung des Rolls
        percentSum = 0;
        int roll = Random.Range(0, 101);

         foreach (Item item in currentDropTable)
         {                    
              percentSum += ((int)item.c_percent * 100) / totalWeight;
                    
              if (roll < percentSum)
              {
                    position = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));

                    //ItemRolls.CalculateRolls(item);

                    ItemWorld.SpawnItemWorld(position, item);

                    break;
              }
         }

    }

    int calculateTotalWeight(int lootTable)
    {

        foreach (Item item in totalLoottable[lootTable])
        {
            item.c_percent = item.percent; //Nochmal nachsehen. item.c_percent scheint falsch weiter gegeben zu werden (e.g. 10% = 8% im Roll, wtf)
            totalWeight += item.percent;

            currentDropTable.Add(item);


        }


        if (totalLoottable[lootTable +1] != null)
            foreach (Item item in totalLoottable[lootTable +1])
            {
                item.c_percent = item.percent / 2;

                totalWeight += item.percent / 2;

                currentDropTable.Add(item);

            }

        if (totalLoottable[lootTable + 2] != null)
            foreach (Item item in totalLoottable[lootTable + 2])
            {
                item.c_percent = item.percent / 3;
                totalWeight += item.percent / 3;

                currentDropTable.Add(item);

            }

        if (lootTable >= 1)
            foreach (Item item in totalLoottable[lootTable -1])
            {
                item.c_percent = item.percent / 2;
                totalWeight += item.percent / 2;

                currentDropTable.Add(item);

            }

        if (lootTable >= 2)
            foreach (Item item in totalLoottable[lootTable - 2])
            {
                item.c_percent = item.percent / 3;

                totalWeight += item.percent / 3;

                currentDropTable.Add(item);

            }

        return totalWeight;
    }

    public float PosDiff(float nr1, float nr2)
    {
        float result = Mathf.Abs(nr1 - nr2);
        if (result <= 0)
            result = result * -1;
        return result;
    }

    public static Item GetItemID(string ID)
    {
        Debug.Log("Searching for item with ID: " + ID);

        for(int i = 0; i < totalLoottable.Count; i++)
        {
            
            foreach(Item item in totalLoottable[i])
            {

                if (item.ItemID == ID)
                {
                    Item foundItem = item;

                    print("Found item: " + foundItem.ItemName);

                    return foundItem;
                }

            }

        }

        Debug.Log("No item with ID: " + ID + " found.");

        return null;
    }

}