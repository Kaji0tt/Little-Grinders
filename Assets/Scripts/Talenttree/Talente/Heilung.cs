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

    Verbesserte_Heilung verbesserte_Heilung;

    //If its a Spell, which instantiate Prefabs like Bullets / Fireball, it should be called on Isometric Player with "player.CastSpell(this);"

    void Start()
    {
        verbesserte_Heilung = FindObjectOfType<Verbesserte_Heilung>();
    }
    public void Use()
    {
        
        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        if (!onCoolDown && currentCount >= 1)
        {

            if(verbesserte_Heilung.currentCount != 0)
            {
                playerStats.Heal((int)healAmount + Mathf.RoundToInt((playerStats.Hp.Value / 100) * verbesserte_Heilung.currentCount));
            }
            else
            playerStats.Heal((int)healAmount);
            
            onCoolDown = true;
        }

    }

    public bool IsOnCooldown()
    {
        return onCoolDown;
    }

    public float CooldownTimer()
    {
        return coolDownTimer;
    }

    public float GetCooldown()
    {
        return GetSpellCoolDown;
    }

}
