using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public abstract class Ability : MonoBehaviour, IMoveable, IUseable, IDragHandler, IEndDragHandler
{
    public string abilityName;
    [TextArea]
    public string description;
    [Space]
    public float cooldownTime;
    private float _cooldown;

    public float activeTime;
    private float _activeTime;

    public bool isPersistent; // Falls true, bleibt die Fähigkeit aktiv

    float tickerTimer;

    [HideInInspector]
    public float tickTimer = 1;

    public Sprite image;
    public Sprite icon => image;


    #region Abilitiy Stuff


    //Ggf. sollten States in einer anderen Klasse / einem Manager reguliert werden?
    [HideInInspector]
    public enum AbilityState { ready, active, cooldown };
    AbilityState state = AbilityState.ready;

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
                tickerTimer -= Time.deltaTime;

                if (tickerTimer <= 0)
                {
                    CallAbilityFunctions("tick", PlayerManager.instance.player.GetComponent<PlayerStats>());
                    tickerTimer = tickTimer;
                }
            }
            else
            {
                if (_activeTime > 0)
                {
                    _activeTime -= Time.deltaTime;
                    tickerTimer -= Time.deltaTime;

                    if (tickerTimer <= 0)
                    {
                        CallAbilityFunctions("tick", PlayerManager.instance.player.GetComponent<PlayerStats>());
                        tickerTimer = tickTimer;
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


