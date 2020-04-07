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

    //EQSlots
    EQSlotSchuhe Schuhe;

    public void EquipChanged(Item item)
    {
            switch (item.itemType)
            {
                default:
                case "Schuhe":
                print("Wo bin ich");
                equipSchuhe(item);

                    break;

                case "Hose":
                    break;

                case "Brust":
                    break;

                case "Kopf":
                    break;

                case "Weapon":
                    break;

                case "Schmuck":
                    break;
            }
        
    }


}