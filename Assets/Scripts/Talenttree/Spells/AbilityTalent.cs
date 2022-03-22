using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityTalent : Talent, IMoveable, IUseable, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
{
    #region TalentTree Stuff

    public void CheckForUnlock()
    {
        switch(baseAbility.abilityType)
        {
            case Ability.AbilityType.Combat:
                if (TalentTree.instance.totalCombatPoints >= baseAbility.requiredTypePoints)
                    Unlock();
                break;

            case Ability.AbilityType.Void:
                if (TalentTree.instance.totalVoidPoints >= baseAbility.requiredTypePoints)
                    Unlock();
                break;

            case Ability.AbilityType.Utility:
                if (TalentTree.instance.totalUtilityPoints >= baseAbility.requiredTypePoints)
                    Unlock();
                break;

        }
    }

    #endregion


    #region Abilitiy Stuff
    public Ability baseAbility;
    float cooldownTime;
    float activeTime;


    //Stances Enum
    private enum AbilityState { ready, active, cooldown };

    AbilityState state = AbilityState.ready;

    public new Sprite icon => baseAbility.icon;

    private Image image;

    //Integers, welche sich erhöhen, sobald ein entsprechendes Talent erhöht wird.
    [HideInInspector]
    public int voidCounter, combatCounter, utilityCounter;

    void Start()
    {
        image = GetComponent<Image>();
        cooldownTime = baseAbility.cooldownTime;
        activeTime = baseAbility.activeTime;

        /*
        foreach(Talent talent in TalentTree.instance.allTalents)
        {
            if (talent.abilityTalent == abilityTalent)
                abilityTalents.Add(talent);
        }
        */

    }

    // Update is called once per frame
    void Update()
    {
        //During Active State, reduce the Active Time
        if(state ==AbilityState.active)
        {
            print("ability is active");
            if (activeTime > 0)
                activeTime -= Time.deltaTime;
            else
            {
                state = AbilityState.cooldown;
                activeTime = baseAbility.activeTime;
            }

        }

        //During Cooldown State, reduce the Cooldown Time
        if(state == AbilityState.cooldown)
        {
            print("ability is on cooldown" + cooldownTime);
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
            baseAbility.Use(PlayerManager.instance.player);
            state = AbilityState.active;
            activeTime = baseAbility.activeTime;
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


    #endregion
}
