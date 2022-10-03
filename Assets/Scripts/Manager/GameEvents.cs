using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    #region Singleton
    public static GameEvents instance;
    void Awake()
    {
        //Singleton Anweisung, zur globalen Reference AudioManager.instance
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    #endregion

    public event Action<ItemInstance> equipSchuhe;
    public event Action<ItemInstance> equipHose;
    public event Action<ItemInstance> equipBrust;
    public event Action<ItemInstance> equipKopf;
    public event Action<ItemInstance> equipWeapon;
    public event Action<ItemInstance> equipSchmuck;

    public event Action<float> OnPlayerHasAttackedEvent;
    //public event Action<float> OnPlayerWasAttackedEvent;

    public event Action<float> OnEnemyHasAttackedEvent;
    public event Action<float> OnEnemyWasAttackedEvent;
    //public event Action<IEntitie, float> playerWasAttacked;

    public static event Action<string> playSound;
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

    
    public void PlayerHasAttacked(float damage)
    {
        OnPlayerHasAttackedEvent(damage);
    }
    public void PlayerWasAttacked(float damage)
    {
        //OnPlayerWasAttackedEvent(damage);
    }

    public void EnemyHasAttacked(float damage)
    {
        OnEnemyHasAttackedEvent(damage);
    }

    public void EnemyWasAttacked(float damage)
    {
        OnEnemyHasAttackedEvent(damage);
    }


}