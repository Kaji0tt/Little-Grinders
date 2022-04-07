using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class Heilung : Spell, IUseable
{
    [Header("Werte dieses Spells")]


    private string descriptionOverride = "Heilt den Spieler im Verlauf von 20 Sekunden um 10 Punkte. Heilt zusätzlich pro Sekunde um 10% des AP Wertes.";

    PlayerStats playerStats;

    //Timer to calculate time between spell use and healDuration (max time spawn for it to be activated)
    private float healTimer = 0;

    //timer to calculate every x seconds, a certein action is done (in this case, the heal is applied)
    private float healTickDuration = 1;

    //the maximum duration of this over time spell
    public float healDuration { get; private set; }

    Verbesserte_Heilung verbesserte_Heilung;

    [SerializeField]
    VoidHeilung voidHeilung;

    [SerializeField]
    CombatHeilung_Max combatHeilungMax;

    LifeHeilung_Max lifeHeilungMax;

    public bool healActivated { get; private set;  }

    public float healAmount { get; private set; }

    private float healCoolDown;


    //If its a Spell, which instantiate Prefabs like Bullets / Fireball, it should be called on Isometric Player with "player.CastSpell(this);"

    void Start()
    {
        healAmount = 2;

        healDuration = 10;

        healActivated = false;

        //Genau diesen Rotz wollen wir umgehen.
        lifeHeilungMax = FindObjectOfType<LifeHeilung_Max>();

        //So ein mist auch.
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

            healActivated = true;

            onCoolDown = true;

            healCoolDown = GetSpellCoolDown;
        }

    }
    
    void Update()
    {
        //print("update got called");
        if (healActivated)
        {
            //print(healTimer);

            if (lifeHeilungMax.currentCount != 0)
                healDuration = healDuration * 2;

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

                //Falls Combat-Ultimate geskilled wurde, wird dieses beim Auslaufen der Heilung hier aktiviert.
                if (combatHeilungMax.currentCount != 0)
                    combatHeilungMax.ActivateArmorDamage(this);

                healActivated = false;

                healTimer = 0;

                //onCoolDown = false;

            }

        }

        if(onCoolDown)
        {
            healCoolDown -= Time.deltaTime;

            if (healCoolDown <= 0)
                onCoolDown = false;
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

        if (voidHeilung.currentCount != 0)
            voidHeilung.ActivateTick(this);

        
    }


    public override bool IsOnCooldown()
    {
        return onCoolDown;
    }

    public override float CooldownTimer()
    {
        return healCoolDown;
        //return GetSpellCoolDown;
        //return (-healDuration + GetSpellCoolDown);
    }

    public override float GetCooldown()
    {
        return coolDownTimer;
    }

}
