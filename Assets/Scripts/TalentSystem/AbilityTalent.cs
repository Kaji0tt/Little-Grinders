using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityTalent : Talent, IMoveable, IUseable, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
{
    #region TalentTree Stuff


    //CheckForUnlock soll überprüfen, ob im Talenbaum ausreichend Talentpunkte gesammelt wurden, damit das entsprechende Talent freigeschaltet werden kann.
    public void CheckForUnlock()
    {
        //In Abhängigkeit vom Typ der Fähigkeit, wird der Talentbaum auf eine entsprchend ausreichende Anzahl von Spezialisierungspunkten überprüft.
        switch(baseAbility.abilityType)
        {
            case Ability.AbilityType.Combat:
                if (TalentTree.instance.totalCombatSpecPoints >= baseAbility.requiredTypePoints)
                    Unlock();
                break;

            case Ability.AbilityType.Void:
                if (TalentTree.instance.totalVoidSpecPoints >= baseAbility.requiredTypePoints)
                    Unlock();
                break;

            case Ability.AbilityType.Utility:
                if (TalentTree.instance.totalUtilitySpecPoints >= baseAbility.requiredTypePoints)
                    Unlock();
                break;

        }
    }

    #endregion


    #region Abilitiy Stuff


    //Jedes Ability Talent referiert auf eine Ability SO Klasse, welche im Editor ausgewählt werden muss.
    public Ability baseAbility;

    float cooldownTime;

    float activeTime;

    float tickerTimer;


    //Jedes Ability Talent besitzt 3 Stances
    private enum AbilityState { ready, active, cooldown };
    AbilityState state = AbilityState.ready;

    //Ich glaub Unity meckert hier, dass das Image multiple times serialzed wurde.
    public new Sprite icon => baseAbility.icon;

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
        cooldownTime = baseAbility.cooldownTime;
        activeTime = baseAbility.activeTime;

        //print(childTalent.Length);

        //Troubleshoot Spec-Lists, Cleare der Ability Lists, da es sich bei der Ability um ein SO handelt, welches Veränderungen dauerhaft speichert.
        baseAbility.spec1Talents.Clear();
        baseAbility.spec2Talents.Clear();
        baseAbility.spec3Talents.Clear();

        foreach (Talent talent in childTalent)
        {

            RunThroughChildTalents(talent);

        }

    }

    private void RunThroughChildTalents(Talent talent)
    {
        AddSpecializationTalents(talent);
        if(talent.childTalent.Length > 0)
        {
            foreach (Talent childTalent in talent.childTalent)
            {
                RunThroughChildTalents(childTalent);
            }

            
        }
    }

    private void AddSpecializationTalents(Talent childTalent)
    {
        if(!childTalent.passive)
        switch (childTalent.abilitySpecialization)
        {
            case Ability.AbilitySpecialization.Spec1:
                baseAbility.SetSpec1Talent(childTalent);
                    break;

            case Ability.AbilitySpecialization.Spec2:
                baseAbility.SetSpec2Talent(childTalent);
                break;

            case Ability.AbilitySpecialization.Spec3:
                baseAbility.SetSpec3Talent(childTalent);
                break;

        }
    }

    // Update is called once per frame
    void Update()
    {

        //During Active State, reduce the Active Time
        if (state == AbilityState.active)
        {


            if (activeTime > 0)
            {
                activeTime -= Time.deltaTime;


                tickerTimer -= Time.deltaTime;

                if (tickerTimer <= 0)
                {
                    baseAbility.CallAbilityFunctions("tick", PlayerManager.instance.player.GetComponent<PlayerStats>());
                    tickerTimer = baseAbility.tickTimer;
                }
            }


            else
            {
                baseAbility.OnCooldown(PlayerManager.instance.player.GetComponent<PlayerStats>());
                baseAbility.CallAbilityFunctions("cooldown", PlayerManager.instance.player.GetComponent<PlayerStats>());

                state = AbilityState.cooldown;
                activeTime = baseAbility.activeTime;
            }

        }

        //During Cooldown State, reduce the Cooldown Time
        if(state == AbilityState.cooldown)
        {
            //print("ability is on cooldown" + cooldownTime);
            if (cooldownTime > 0)
            {
                cooldownTime -= Time.deltaTime;

                image.color = Color.grey;
            }

            else
            {
                state = AbilityState.ready;

                cooldownTime = baseAbility.cooldownTime;

                image.color = Color.white;
            }

        }



    }


    public void Use()
    {
        //Setzen der Spezialisierungspunkte in der Ability.

        if(state == AbilityState.ready)
        {

            tickerTimer = baseAbility.tickTimer;

            activeTime = baseAbility.activeTime;

            state = AbilityState.active;

            baseAbility.UseBase(PlayerManager.instance.player.GetComponent<PlayerStats>());
            
        }
        
    }
    #endregion

    #region EventSystem Stuff
    public bool IsOnCooldown()
    {
        if (state == AbilityState.cooldown)
            return true;
        else
            return false;
    }

    public float GetCooldown()
    {
        return cooldownTime;
    }

    public float CooldownTimer()
    {
        return cooldownTime;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(baseAbility.description);
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandScript.instance.TakeMoveable(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        HandScript.instance.Put();
    }

    public bool IsActive()
    {

        if (state == AbilityState.active)
            return true;
        else
            return false;
    }

    #endregion
}
