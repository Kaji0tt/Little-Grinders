using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemDatabase : MonoBehaviour
{
    private static ItemDatabase instance;
    public List<Item> tier1;
    public List<Item> tier2;

    private int percentSum = 0;

    private void Awake()
    {
        instance = this;
    }

    public void GetDrop(Vector3 position)
    {
        //position = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        //print("Hi there, im called at: " + position);
        //Item-Drop Calculation
        percentSum = 0;
        int roll = Random.Range(0, 101);
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
}
