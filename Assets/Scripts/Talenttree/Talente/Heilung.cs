using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class Heilung : Spell, IUseable
{
    [Header("Werte dieses Spells")]
    private float healAmount = 2;

    private string descriptionOverride = "Heilt den Spieler im Verlauf von 20 Sekunden um 10 Punkte. Heilt zusätzlich pro Sekunde um 10% des AP Wertes.";

    PlayerStats playerStats;

    //Timer to calculate time between spell use and healDuration (max time spawn for it to be activated)
    private float healTimer = 0;

    //timer to calculate every x seconds, a certein action is done (in this case, the heal is applied)
    private float healTickDuration = 1;

    //the maximum duration of this over time spell
    private float healDuration = 10;

    Verbesserte_Heilung verbesserte_Heilung;

    private bool skillActivated = false;


    //If its a Spell, which instantiate Prefabs like Bullets / Fireball, it should be called on Isometric Player with "player.CastSpell(this);"

    void Start()
    {
        verbesserte_Heilung = FindObjectOfType<Verbesserte_Heilung>();

        SetDescription(descriptionOverride);
    }


    public override void Use()
    {
        
        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        if (!onCoolDown && currentCount >= 1)
        {
            if(AudioManager.instance != null)
            AudioManager.instance.Play("HealSpell");

            skillActivated = true;

            onCoolDown = true;


        }

    }
    
    void Update()
    {
        //print("update got called");
        if (skillActivated)
        {

            //print("skillActivated");
            healTimer += Time.deltaTime;
            healDuration -= Time.deltaTime;
            //coolDownTimer -= Time.deltaTime;
            if (healTimer > healTickDuration && healDuration >= 0)
            {
                ActivateHotTick();

                healTimer = 0;

            }
            else if (healDuration <= 0)
            {
                healDuration = 10;

                skillActivated = false;

                onCoolDown = false;

            }

        }
    }
    
    //Wichtig für die Hot Ticks
    private void ActivateHotTick()
    {
        if (verbesserte_Heilung.currentCount == 0)
        {
            playerStats.Heal((int)healAmount + ((int)playerStats.AbilityPower.Value / 10));
        }
        else
        {
           playerStats.Heal((int)healAmount + ((int)playerStats.AbilityPower.Value / 10) + Mathf.RoundToInt((playerStats.Hp.Value / 50) * verbesserte_Heilung.currentCount));
        }
    }


    public override bool IsOnCooldown()
    {
        return onCoolDown;
    }

    public override float CooldownTimer()
    {
        return (-healDuration +20);
    }

    public override float GetCooldown()
    {
        return GetSpellCoolDown;
    }

}
