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
        
        int roll = Random.Range(0, 101);
        foreach (Item item in tier1)
        {

            percentSum += item.percent;
            if(roll < percentSum)
            {
                position = position + new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
                ItemWorld.SpawnItemWorld(position, item);
            }                 
        }




    }
}
