using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatHeilung_Max : Talent, IPointerEnterHandler, IPointerExitHandler
{

    PlayerStats playerStats;



    bool damageApplied;
    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(GetDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
    void Start()
    {

        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        SetDescription("Wenn der Heilungseffekt von Heilung ausläuft, verursachst du deinen doppelten Rüstungswert als Schaden. Radius: 1");

        damageApplied = false;
    }

    internal void ActivateArmorDamage(Heilung heilung)
    {

            
        Collider[] hitColliders = Physics.OverlapSphere(PlayerManager.instance.player.transform.position, 1);

        foreach (Collider hitCollider in hitColliders)
        {

            if (hitCollider.transform.tag == "Enemy")
            {
                float damage = playerStats.Armor.Value * 2;


                hitCollider.transform.GetComponentInParent<MobStats>().TakeDirectDamage((int)(damage), 1);

                damageApplied = true;

            }

        }

    }
}
