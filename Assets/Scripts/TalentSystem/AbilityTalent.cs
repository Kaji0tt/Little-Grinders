using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


//Ability Talent ist das Script, welches sich im Editor innerhalb des Canvas auf dem GameObject befinden sollte, damit der Spieler das Talent "bewegen" kann.
public class AbilityTalent : Talent, IMoveable, IUseable, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
{
    #region TalentTree Stuff


    //CheckForUnlock soll �berpr�fen, ob im Talenbaum ausreichend Talentpunkte gesammelt wurden, damit das entsprechende Talent freigeschaltet werden kann.
    public void CheckForUnlock()
    {
        //In Abh�ngigkeit vom Typ der F�higkeit, wird der Talentbaum auf eine entsprchend ausreichende Anzahl von Spezialisierungspunkten �berpr�ft.
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

    internal void ApplySpecialization(Talent talent)
    {
    
        switch (talent.abilitySpecialization)
        {
            case Ability.AbilitySpecialization.Spec1:
                //Debug.Log(talent.talentName + " soll auf spec1 geskilled werden.");
                baseAbility.ApplySpec1Bonus(talent);
                break;

            case Ability.AbilitySpecialization.Spec2:
                baseAbility.ApplySpec2Bonus(talent);
                break;

            case Ability.AbilitySpecialization.Spec3:
                baseAbility.ApplySpec3Bonus(talent);
                break;

        }
        
    }

    #endregion


    #region Abilitiy Stuff


    //Jedes Ability Talent referiert auf eine Ability SO Klasse, welche im Editor ausgew�hlt werden muss.
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


        /*
        -> Troubleshoot Spec-Lists, Cleare der Ability Lists, da es sich bei der Ability um ein SO handelt, welches Ver�nderungen dauerhaft speichert.
        F�r den Moment suche ich nach einer L�sung f�r einen Ansatz der SO geschichte - aber ob der SO Ansatz �berhaupt richtig ist?
        Ich finde es fragw�rdig, dass ich eine Klasse erstelle und anschlie�end daraus ein SO abwandel, wenn ich die Ability als MonoBehaviour selbst
        im Editor verwende..
         */
        baseAbility.spec1Talents.Clear();
        baseAbility.spec2Talents.Clear();
        baseAbility.spec3Talents.Clear();

        //Durchlaufe jedes Childtalent
        foreach (Talent talent in childTalent)
        {

            RunThroughChildTalents(talent);

        }

    }

    private void RunThroughChildTalents(Talent talent)
    {
        //Und f�ge dieses der entsprechenden Spezialisierungsliste hinzu. (Spec1 / Spec2 / Spec3)
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
                baseAbility.AddSpec1TalentsToList(childTalent);
                    break;

            case Ability.AbilitySpecialization.Spec2:
                baseAbility.AddSpec2TalentsToList(childTalent);
                break;

            case Ability.AbilitySpecialization.Spec3:
                baseAbility.AddSpec3TalentsToList(childTalent);
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
        UI_Manager.instance.ShowTooltip(baseAbility.description + '\n' + GetRequirementsOfTalent(this));
    }

    public string GetRequirementsOfTalent(AbilityTalent ability)
    {
        string info = null;
        switch (ability.baseAbility.abilityType)
        {
            case Ability.AbilityType.Combat:
                if (TalentTree.instance.totalCombatSpecPoints <= baseAbility.requiredTypePoints)
                    info = "<color=#b27d90>Ben�tigte Combat-Punkte:<b> " + ability.baseAbility.requiredTypePoints + "</b></color>";
                    return info;

            case Ability.AbilityType.Void:
                if (TalentTree.instance.totalVoidSpecPoints <= baseAbility.requiredTypePoints)
                    info = "<color=#b27d90>Ben�tigte Void-Punkte:<b> " + ability.baseAbility.requiredTypePoints + "</b></color>";
                return info;
                

            case Ability.AbilityType.Utility:
                if (TalentTree.instance.totalUtilitySpecPoints <= baseAbility.requiredTypePoints)
                    info = "<color=#b27d90>Ben�tigte Utility-Punkte:<b> " + ability.baseAbility.requiredTypePoints + "</b></color>";
                return info;
                

            default: return info;

        }
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
