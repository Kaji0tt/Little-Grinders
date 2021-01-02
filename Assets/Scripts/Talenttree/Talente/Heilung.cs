using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class Heilung : Spell, IUseable
{
    [Header("Werte dieses Spells")]
    public float healAmount;


    PlayerStats playerStats;

    //If its a Spell, which instantiate Prefabs like Bullets / Fireball, it should be called on Isometric Player with "player.CastSpell(this);"
    public void Use()
    {

        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        if (!onCoolDown && currentCount >= 1)
        {


            playerStats.Heal((int)healAmount);
            

            onCoolDown = true;
        }





    }


}
