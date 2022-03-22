using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VoidHeilung : Talent, IPointerEnterHandler, IPointerExitHandler
{

    private float radius = 0.5f;

    VoidHeilungMax heilungVoidMax;

    PlayerStats playerStats;

    void Start()
    {

        heilungVoidMax = FindObjectOfType<VoidHeilungMax>();

        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        SetDescription("Solange der Spieler heilt, erhalten Gegner in der Umgebung (ein drittel/Die h‰lfte/die gesamte) Heilung als Schaden \n (0.5/1.5/2.5 Radius)");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(GetDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }


    internal void ActivateTick(Heilung heilung)
    {
        Collider[] hitColliders = Physics.OverlapSphere(PlayerManager.instance.player.transform.position, radius + currentCount);

        foreach (Collider hitCollider in hitColliders)
        {
            //Mobs heilen derzeit. Auﬂerdem sollte das pro Tick invoked werden.
            if (hitCollider.transform.tag == "Enemy")
            {
                float damage = (heilung.healAmount + (playerStats.AbilityPower.Value / 10)) / (4 - currentCount);

                if (heilungVoidMax.currentCount == 1)
                    damage = damage * 2;

                hitCollider.transform.GetComponentInParent<EnemyController>().TakeDirectDamage((int)(damage), radius + currentCount);


            }

        }
    }
}
