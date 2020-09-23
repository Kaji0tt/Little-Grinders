using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{ 
    public static GameEvents current;

    private void Awake()
    {
        current = this;
    }
    public event Action<Item> equipSchuhe;
    public event Action<Item> equipHose;
    public event Action<Item> equipBrust;
    public event Action<Item> equipKopf;
    public event Action<Item> equipWeapon;
    public event Action<Item> equipSchmuck;

    private IsometricPlayer isometricPlayer;

    
    public void EquipChanged(Item item)
    {
        switch (item.itemType)
        {
            case ItemType.Kopf:
                equipKopf(item);
                break;
            case ItemType.Brust:
                equipBrust(item);
                break;
            case ItemType.Beine:
                equipHose(item);
                break;
            case ItemType.Schuhe:
                equipSchuhe(item);
                break;
            case ItemType.Schmuck:
                equipSchmuck(item);
                break;
            case ItemType.Weapon:
                equipWeapon(item);
                break;
            case ItemType.Consumable:
                //Placeholder. Call for ItemDelte or something.
                break;
              



        }



    }
}