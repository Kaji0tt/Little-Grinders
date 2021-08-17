using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldModifiers : MonoBehaviour
{
    public static WorldModifiers instance;

    private void Awake()
    {
        instance = this;
        PlayerStats.eventLevelUp += UpdateModifiers;
    }


    //Every 2 Levels, the percentage drop Chance of the according DropTierTable is higher.
    public int dropTableTierRange = 2;


    private void UpdateModifiers()
    {
        PlayerStats player = PlayerManager.instance.player.GetComponent<PlayerStats>();

        if(player.level >= dropTableTierRange)
        {
            dropTableTierRange += dropTableTierRange;
        }
    }

}
