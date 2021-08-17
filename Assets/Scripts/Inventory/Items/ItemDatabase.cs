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

        //int dropRange = 2;

        //int dropTable = (int)Mathf.Floor(level / WorldModifiers.instance.dropTableTierRange);

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

                //print("Item: " + item.ItemName + " was choosen.");

                ItemWorld.SpawnItemWorld(position, itemRolls.RollItem(new ItemInstance(item)));

                break;
            }
        }

    }

    int CalculateTotalWeight(PlayerStats playerStats)
    {
        foreach (List<Item> itemList in totalLoottable)
        {
            foreach (Item item in itemList)
            {
                //Berechne den Einfluss des Spielerleves in Abhängigkeit vom Baselevel des Items
                float levelInfluence = Mathf.Clamp01((float)playerStats.level / (float)item.baseLevel);

                //An dieser Stelle sollte zu einem späteren Zeitpunkt weitere Modifier errechnet werden, oder dies geschieht in WorldModifiers direkt.
                //float mapInfluence

                //Speichere das errechnete Gewicht in neuer Variabel, um das original Item nicht zu modifizieren. Da wir mit int's arbeiten, 100 um mehr Varianz zu erhalten.
                item.c_percent = item.percent  * levelInfluence;

                //Füge das errechnete Gewicht des Items zum komplett Gewicht hinzu
                totalWeight += (int)item.c_percent;

                currentDropTable.Add(item);
            }
        }

        //Errechne eine 66% Chance, das nichts gedropped wird.
        totalWeight = ((totalWeight / 100) * 300);


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

    public static Item GetItemID(string ID)
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

}