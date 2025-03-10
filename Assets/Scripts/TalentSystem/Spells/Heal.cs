using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : Ability
{
    public float healAmount = 1f;  // Menge an HP pro Tick
    public GameObject healAuraEffect; // Effekt für die Heilung
    public GameObject healAuraTick; // Effekt für Tick-Heilung


    private GameObject activeHealEffect;
    private bool isHealing = false; // Zustand der Heilung

    public override void UseBase(IEntitie entitie)
    {
        if (!isHealing)
        {
            isHealing = true;
            Debug.Log("Healing activated!");

            if (healAuraEffect != null && activeHealEffect == null)
            {
                activeHealEffect = Instantiate(healAuraEffect, entitie.GetTransform().position, Quaternion.identity);
                activeHealEffect.transform.SetParent(entitie.GetTransform());
            }
        }
        else
        {
            isHealing = false;
            Debug.Log("Healing deactivated, going into cooldown.");

            OnCooldown(entitie);
            CallAbilityFunctions("cooldown", entitie);
            //SetState(AbilityState.cooldown); // Hier die neue Methode verwenden
        }
    }

    public override void OnTick(IEntitie entitie)
    {
        if (isHealing)
        {
            Debug.Log("Healing " + healAmount + " HP per second!");
            entitie.Heal(Mathf.RoundToInt(healAmount)); // Spieler heilen

            if (healAuraTick != null)
            {
                Instantiate(healAuraTick, entitie.GetTransform().position, Quaternion.identity);
            }
        }
    }

    public override void OnCooldown(IEntitie entitie)
    {
        if (activeHealEffect != null)
        {
            Destroy(activeHealEffect); // Heal-Aura entfernen
            activeHealEffect = null;
        }
    }
}