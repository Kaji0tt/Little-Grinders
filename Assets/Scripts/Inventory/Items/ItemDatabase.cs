﻿using System.Collections;
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


    private List<List<Item>> totalLoottable = new List<List<Item>>();

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

    /*
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


        if (lootTable == "tier1")
        {
            foreach (Item item in tier1)
            {
                totalWeight += item.percent;
            }

            foreach (Item item in tier1)
            {

                percentSum += (item.percent * 100) / totalWeight;


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

                percentSum += item.percent;

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

                    percentSum += item.percent;

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

                    percentSum += item.percent;

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

                    percentSum += item.percent;

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

    }
    */

    public void GetWeightDrop(Vector3 position) //hier müsste statt lootTable das Level mitgegeben werden.
    {
        //Item-Drop Calculation

        //totalWeight = 0;

        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        int level = playerStats.Get_level();


        //Hässlichste Lösung die überhaupt Möglich ist für Wahl des Loottables.
        //Eher -> Find Variable of Name - vorerst solls reichen.



        //Im Prinzip müssen alle Listen in einer großen Liste liegen. (totalLoottable) check
        //Dann muss entsprechend der Tier und Enemy-Level Stuffe totalWeight erhöht oder verringert werden.


        bool foundItem = false;

        //Im finalen Roll orientiere ich i am Player.Level.. 

        //Fragen die zum Fixxen relevant sind: Wofür steht i genau? 
        //i ist eine unabhängige Variabel, welche sich an meinem Level orientieren sollte
        for (int i = 0; i < totalLoottable.Count; i++)      
        {
            int levelInfluence = 1 / level;

            //if (i+1 == level) //das würde bedeuten, dass es eine Spannweite < i > geben sollte, welche die erste capsule definiert.

            //definieren wir einfach eine variabel für die Drop-Range, also die Levelgröße der Loottables.
            int dropRange = 5;

            //und definieren wir das total weight orientiert an der drop range
            if (i + 1 <= level + dropRange && i + 1 >= level - dropRange) //Logikfehler - eins von beiden tritt aufjedenfall ein. && stattdessen
                                                                         // weiterer Logikfehler: solange die level variabel im spiel ist, wird sich niemals was daran ändern.
                                                                         //conclusion: die Loottables sollten eine eigene Klasse besitzen, aus denen die entsprechenden drop ranges ausgelesen werden können - und alle informationen die nötig sind.
                                                                         // to do für morgen: schreibe ein ScriptableObject Script für einzelne Loottables.
                                                                         // notwendige Parameter: Range Parameter // levelspannweite
                                                                         // ggf. enemy type's oder sowas..
                foreach (Item item in totalLoottable[i])
                {
                    totalWeight += item.percent;
                }


            else if (PosDiff(i+1, level) <= 2*dropRange || PosDiff(i + 1, level) >= 2*dropRange)
                foreach (Item item in totalLoottable[i])
                {
                    totalWeight += item.percent * 1 / ((int)PosDiff(i + 1, level) + 1);
                }

            if (PosDiff(i + 1, level) <= 3 * dropRange || PosDiff(i + 1, level) >= 3 * dropRange)
                foreach (Item item in totalLoottable[i])
                {
                    totalWeight += item.percent * 1 / ((int)PosDiff(i + 1, level) + 1);
                }

            if (PosDiff(i + 1, level) <= 4 * dropRange || PosDiff(i + 1, level) >= 4 * dropRange)
                foreach (Item item in totalLoottable[i])
                {
                    totalWeight += item.percent * 1 / ((int)PosDiff(i + 1, level) + 1);
                }

            if (PosDiff(i + 1, level) <= 5 * dropRange || PosDiff(i + 1, level) >= 5 * dropRange)
                foreach (Item item in totalLoottable[i])
                {
                    totalWeight += item.percent * 1 / ((int)PosDiff(i + 1, level) + 1);
                }

            if (PosDiff(i + 1, level) <= 6 * dropRange || PosDiff(i + 1, level) >= 6 * dropRange)
                foreach (Item item in totalLoottable[i - 1])
                {
                    totalWeight += item.percent * 1 / ((int)PosDiff(i + 1, level) + 1);
                }
        }

        print(totalWeight);
        // Berechnung des Rolls
        percentSum = 0;
        int roll = Random.Range(0, 101);
        print("Der Roll beträgt:" + roll);

        //Runterzählen der Loot-Tier Listen bis zu einem Grenzbereich von 3
        
        for (int i = level; i > level - 4; i--)
        {
            print("i im niedrigschwelligen Kasten: " + i);
            if (totalLoottable[i - 1] != null)
            {
                foreach (Item item in totalLoottable[i - 1])
                {
                    percentSum += (item.percent * 100) / totalWeight;
                    print("Kleiner Kasten: " +percentSum + " wurde von dem Item " + item.ItemName + " hinzugefügt.");

                    if (roll < percentSum)
                    {
                        position = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                        ItemWorld.SpawnItemWorld(position, item);
                        foundItem = true;
                        break;
                    }
                }
            }

            if(i <= level || foundItem == true)
            break;


        }
        

        //Hochzählen der Loot-Tier Listen bis zu einem Grenzbereich von 3
        for (int i = level +1; i < level + 3; i++)
        {
            if(foundItem == false && totalLoottable[i] != null)
            {
                foreach (Item item in totalLoottable[i])
                {
                    percentSum += (item.percent * 100) / totalWeight;
                    print("Großer Kasten: " + percentSum + " wurde von dem Item " + item.ItemName + " hinzugefügt.");

                    if (roll < percentSum)
                    {

                        position = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                        ItemWorld.SpawnItemWorld(position, item);
                        foundItem = true;
                        break;
                    }
                }

            }

        }

    }


    public float PosDiff(float nr1, float nr2)
    {
        float result = Mathf.Abs(nr1 - nr2);
        if (result <= 0)
            result = result * -1;
        return result;
    }

}