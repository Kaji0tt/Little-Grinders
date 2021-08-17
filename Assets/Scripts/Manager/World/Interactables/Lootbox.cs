using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lootbox : Interactable
{
    public override void Use()
    {
        //Generiere den Loot
        ItemDatabase.instance.GetWeightDrop(gameObject.transform.position);
    }
}
