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
    public event Action<ItemInstance> equipSchuhe;
    public event Action<ItemInstance> equipHose;
    public event Action<ItemInstance> equipBrust;
    public event Action<ItemInstance> equipKopf;
    public event Action<ItemInstance> equipWeapon;
    public event Action<ItemInstance> equipSchmuck;

    public event Action<float, GameObject> entitieAttacked;

    public event Action<string> playSound;
    //private IsometricPlayer isometricPlayer;

    
    public void EquipChanged(ItemInstance item)
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
                //Placeholder. Call for ItemDelte or something. --> Irgendwie, wird das irgendwo anders schon gemacht.
                break;

        }

    }

    public void OnEntitieAttack(float damage, GameObject entitie)
    {
        //Nothing yet.
        if(entitieAttacked != null)
        {

        }

    }


}