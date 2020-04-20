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

    //EQSlots
    EQSlotSchuhe Schuhe;

    public void EquipChanged(Item item)
    {
            switch (item.itemType)
            {
                default:
                case "Schuhe":
                equipSchuhe(item);

                    break;

                case "Hose":
                equipHose(item);
                break;

                case "Brust":
                equipBrust(item);
                break;

                case "Kopf":
                equipKopf(item);
                break;

                case "Weapon":
                equipWeapon(item);
                break;

                case "Schmuck":
                equipSchmuck(item);
                break;
            }
        
    }


}