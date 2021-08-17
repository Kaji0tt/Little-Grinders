using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inter_HealFountain : Interactable
{

    public override void Use()
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        playerStats.Heal((int)playerStats.Get_maxHp());

        if (gameObject.GetComponentInChildren<Light>())
        {
            gameObject.GetComponentInChildren<Light>().intensity = 0;
        }
    }
}
