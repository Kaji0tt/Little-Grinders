using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : Ability
{
    public float regValue = 1f;  // Menge an HP pro Tick

    private bool isApplied = false; // Zustand der Heilung

    public override void UseBase(IEntitie entitie)
    {
        if(!isApplied)
        {
            PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

            playerStats.Regeneration.AddModifier(new StatModifier(regValue, StatModType.Flat));



            foreach (TalentNode rootNode in TalentTreeGenerator.instance.allNodes)
            {
                if (rootNode.Depth == 0)
                    rootNode.uiElement.GetComponent<Talent_UI>().Unlock();
            }

            isApplied = true;

            Debug.Log("Reg Value is applied");
        }
        Debug.Log("Regvalue should have been applied.");
    }

    public override void OnTick(IEntitie entitie)
    {

    }

    public override void OnCooldown(IEntitie entitie)
    {

    }
}