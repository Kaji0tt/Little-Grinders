using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public abstract class Ability : MonoBehaviour, IMoveable, IUseable, IDragHandler, IEndDragHandler
{
    private string abilityName;

    public string description;

    private float cooldownTime;
    private float _cooldown;

    private float activeTime;
    private float _activeTime;

    private bool isPersistent; // Falls true, bleibt die Fähigkeit aktiv

    private float tickTimer;


    private float _tickTimer;

    public Sprite image;
    public Sprite icon => image;


    #region Abilitiy Stuff


    //Ggf. sollten States in einer anderen Klasse / einem Manager reguliert werden?
    [HideInInspector]
    public enum AbilityState { ready, active, cooldown };
    AbilityState state = AbilityState.ready;

    public void Initialize(AbilityData abilityData, PlayerStats playerStats)
    {
        ///Initialize all Variables provided by Scriptable Object;
        abilityName = abilityData.abilityName;
        description = abilityData.description;
        cooldownTime = abilityData.cooldownTime;
        _cooldown = cooldownTime;
        isPersistent = abilityData.isPersistent;
        tickTimer = abilityData.tickTimer;
        _tickTimer = tickTimer;
        image = abilityData.icon;

    }


    void Start()
    {
        _activeTime = activeTime;
        _cooldown = cooldownTime;
    }

    // Update is called once per frame
    void Update()
    {

        if (state == AbilityState.active)
        {
            if (isPersistent)
            {
                // Permanente Fähigkeiten ignorieren activeTime und ticken einfach weiter
                tickTimer -= Time.deltaTime;

                if (tickTimer <= 0)
                {
                    CallAbilityFunctions("tick", PlayerManager.instance.player.GetComponent<PlayerStats>());
                    tickTimer = _tickTimer;
                }
            }
            else
            {
                if (_activeTime > 0)
                {
                    _activeTime -= Time.deltaTime;
                    tickTimer -= Time.deltaTime;

                    if (tickTimer <= 0)
                    {
                        CallAbilityFunctions("tick", PlayerManager.instance.player.GetComponent<PlayerStats>());
                        tickTimer = _tickTimer;
                    }
                }
                else
                {
                    OnCooldown(PlayerManager.instance.player.GetComponent<PlayerStats>());
                    CallAbilityFunctions("cooldown", PlayerManager.instance.player.GetComponent<PlayerStats>());
                    state = AbilityState.cooldown;
                    _activeTime = activeTime;
                }
            }
        }

        //During Cooldown State, reduce the Cooldown Time
        if (state == AbilityState.cooldown)
        {
            //print("ability is on cooldown" + cooldownTime);
            if (cooldownTime > 0)
            {
                cooldownTime -= Time.deltaTime;

                //image.color = Color.grey;
            }

            else
            {
                state = AbilityState.ready;

                cooldownTime = _cooldown;

                //image.color = Color.white;
            }

        }



    }


    public void Use()
    {
        if (state == AbilityState.ready)
        {

            state = AbilityState.active;


            UseBase(PlayerManager.instance.player.GetComponent<PlayerStats>());
        }

    }
    #endregion


    public void SetState(AbilityState newState)
    {
        state = newState;
    }


    public virtual void CallAbilityFunctions(string action, IEntitie entitie)
    {
        if (action == "use")
            UseBase(entitie);

        if (action == "tick")
            OnTick(entitie);

        if (action == "cooldown")
            OnCooldown(entitie);      
    }

    public abstract void UseBase(IEntitie entitie);


    public abstract void OnTick(IEntitie entitie);


    public abstract void OnCooldown(IEntitie entitie);





    public string GetName()
    {
        return abilityName;
    }

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

    public bool IsActive()
    {
        if (state == AbilityState.active)
            return true;
        else
            return false;
    }

    public void OnDrag(PointerEventData eventData)
    {
            HandScript.instance.TakeMoveable(this);
    }


    // Diese Methode wird ausgeführt, wenn das Ziehen des Objekts endet
    public void OnEndDrag(PointerEventData eventData)
    {

        // Liste für die Raycast-Ergebnisse
        List<RaycastResult> results = new List<RaycastResult>();

        // Raycast durchführen und Ergebnisse sammeln
        EventSystem.current.RaycastAll(eventData, results);

        // Ergebnisse überprüfen
        foreach (RaycastResult result in results)
        {
            ActionButton actionButton = result.gameObject.GetComponent<ActionButton>();
            if (actionButton != null)
            {
                // SetUseable aufrufen, wenn ActionButton gefunden wird
                actionButton.SetUseable(this);
                Debug.Log("Item dropped onto ActionButton, SetUseable called.");
                return;
            }
        }


    }

}


