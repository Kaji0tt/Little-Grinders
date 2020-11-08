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


    private int percentSum = 0;

    private void Awake()
    {
        instance = this;
    }

    /*
    public void GetTable(string lootTable)
    {
        List<Item> _lootTable = 
    }
    */

    public void GetDrop(Vector3 position, string lootTable)
    {
        //Item-Drop Calculation
        percentSum = 0;
        int roll = Random.Range(0, 101);


        //Hässlichste Lösung die überhaupt Möglich ist für Wahl des Loottables.
        //Eher -> Find Variable of Name - vorerst solls reichen.



        if (lootTable =="tier1")
        {
            foreach (Item item in tier1)
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
                else
                    print("nothing dropped");

            }
        }



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
                else
                    print("nothing dropped");

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
                    else
                        print("nothing dropped");

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
                    else
                        print("nothing dropped");

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
                    else
                        print("nothing dropped");

                }

            }
        }



        // Ideenfindung:

        //Problem: Die Lootables werden hier gespeichert, sollten aber über den Mob abrufbar sein
        //foreach (Loottable loot in enemy)

    }
}
