using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemDatabase : MonoBehaviour
{
    private static ItemDatabase instance;

    //private enum LootTable;
    //Loottable loot;

    public List<Item> tier1;
    public List<Item> tier2;
    public List<Item> tier3;
    public List<Item> tier4;
    public List<Item> tier5;

    private List<List<Item>> totalLoottable;

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

    /*
    public void GetTable(string lootTable)
    {
        List<Item> _lootTable = 
    }
    */

    public void GetDrop(Vector3 position, string lootTable) //hier müsste statt lootTable das Level mitgegeben werden.
    {
        //Item-Drop Calculation
        percentSum = 0;
        totalWeight = 0;
        int roll = Random.Range(0, 101);


        print(tier1[0]);
        //Hässlichste Lösung die überhaupt Möglich ist für Wahl des Loottables.
        //Eher -> Find Variable of Name - vorerst solls reichen.



        //Im Prinzip müssen alle Listen in einer großen Liste liegen. (totalLoottable) check
        //Dann muss entsprechend der Tier und Enemy-Level Stuffe totalWeight erhöht oder verringert werden.

        /*      if(enemy_level = 1)
         * 
         * 
         */

        if (lootTable == "tier1")
        {
            foreach (Item item in tier1)
            {
                totalWeight += item.percent;
            }

            foreach (Item item in tier1)
            {
                //print(item.ItemName + " hat eine % von " + item.percent + ". Bei einem Roll von" + roll +".");
                percentSum += (item.percent * 100) / totalWeight;

                //print("Die kumulative Wahrscheinlichkeit beträgt nun: " + percentSum);
                if (roll < percentSum)
                {

                    position = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                    ItemWorld.SpawnItemWorld(position, item);
                    break;
                }

            }

        }




        #region tierstuff
        if (lootTable == "tier2")
        {
            foreach (Item item in tier2)
            {
                //print(item.ItemName + " hat eine % von " + item.percent + ". Bei einem Roll von" + roll +".");
                percentSum += item.percent;
                //print("Die kumulative Wahrscheinlichkeit beträgt nun: " + percentSum);
                if (roll < percentSum)
                {

                    position = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                    ItemWorld.SpawnItemWorld(position, item);
                    break;
                }


            }




            if (lootTable == "tier3")
            {
                foreach (Item item in tier3)
                {
                    //print(item.ItemName + " hat eine % von " + item.percent + ". Bei einem Roll von" + roll +".");
                    percentSum += item.percent;
                    //print("Die kumulative Wahrscheinlichkeit beträgt nun: " + percentSum);
                    if (roll < percentSum)
                    {

                        position = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                        ItemWorld.SpawnItemWorld(position, item);
                        break;
                    }

                }

            }

            if (lootTable == "tier4")
            {
                foreach (Item item in tier4)
                {
                    //print(item.ItemName + " hat eine % von " + item.percent + ". Bei einem Roll von" + roll +".");
                    percentSum += item.percent;
                    //print("Die kumulative Wahrscheinlichkeit beträgt nun: " + percentSum);
                    if (roll < percentSum)
                    {

                        position = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                        ItemWorld.SpawnItemWorld(position, item);
                        break;
                    }

                }

            }

            if (lootTable == "tier5")
            {
                foreach (Item item in tier5)
                {
                    //print(item.ItemName + " hat eine % von " + item.percent + ". Bei einem Roll von" + roll +".");
                    percentSum += item.percent;
                    //print("Die kumulative Wahrscheinlichkeit beträgt nun: " + percentSum);
                    if (roll < percentSum)
                    {

                        position = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                        ItemWorld.SpawnItemWorld(position, item);
                        break;
                    }

                }

            }
        }
        #endregion



        // Ideenfindung:

        //Problem: Die Lootables werden hier gespeichert, sollten aber über den Mob abrufbar sein
        //foreach (Loottable loot in enemy)

    }

    public void GetWeightDrop(Vector3 position, int level) //hier müsste statt lootTable das Level mitgegeben werden.
    {
        //Item-Drop Calculation
        percentSum = 0;
        totalWeight = 0;
        int roll = Random.Range(0, 101);



        //Hässlichste Lösung die überhaupt Möglich ist für Wahl des Loottables.
        //Eher -> Find Variable of Name - vorerst solls reichen.



        //Im Prinzip müssen alle Listen in einer großen Liste liegen. (totalLoottable) check
        //Dann muss entsprechend der Tier und Enemy-Level Stuffe totalWeight erhöht oder verringert werden.

        /*      if(enemy_level = 1)
         * 
         * 
         */
        for (int i = 0; i < totalLoottable.Count; i++)
        {
            int levelInfluence = 1 / level;

            if (i == level)
            foreach (Item item in totalLoottable[i])
            {
                    totalWeight += item.percent * levelInfluence;
            }
            

            //Tier 2 Drops ab einer Differenz von 5
            if (Mathf.Min(i, level) <= 5 || Mathf.Max(level, i) >= 5)
            foreach (Item item in totalLoottable[i])
            {
                    totalWeight += item.percent * levelInfluence;
            }

            if (Mathf.Min(i, level) == 2 || Mathf.Max(level, i) == 2)
            foreach (Item item in totalLoottable[i])
            {
                    totalWeight += item.percent * levelInfluence;
            }

            if (Mathf.Min(i, level) == 3 || Mathf.Max(level, i) == 3)
            foreach (Item item in totalLoottable[i])
            {
                    totalWeight += item.percent * levelInfluence;
            }

            if (Mathf.Min(i, level) == 4 || Mathf.Max(level, i) == 4)
            foreach (Item item in totalLoottable[i])
            {
                    totalWeight += item.percent * levelInfluence;
            }

            if (Mathf.Min(i, level) == 5 || Mathf.Max(level, i) == 5)
            foreach (Item item in totalLoottable[i])
            {
                    totalWeight += item.percent * levelInfluence;
            }


        }

        //wobei totalLoottable[i] in Abhängigkeit vom int level item.percent skalieren soll.



    }
}