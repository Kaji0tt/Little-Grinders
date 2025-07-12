using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a bad name convention and kind of an heritage of old system. "Regenerate Base" would be the correct name, as it is first regeneration point unlocked for the player.#
public class Heal : Ability
{
    public float regValue = 1f;  // Menge an HP pro Tick

    private bool isApplied = false; // Zustand der Heilung

    public override void UseBase(IEntitie entitie)
    {
        if (!isApplied)
        {
            PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

            playerStats.Regeneration.AddModifier(new StatModifier(regValue, StatModType.Flat));



            foreach (TalentNode rootNode in TalentTreeGenerator.instance.allNodes)
            {
                if (rootNode.Depth == 0)
                    rootNode.uiElement.GetComponent<Talent_UI>().Unlock();
            }

            isApplied = true;

            //Debug.Log("Reg Value is applied");
        }
        Debug.Log("Regvalue should have been applied.");
    }

    public override void OnTick(IEntitie entitie)
    {

    }

    public override void OnCooldown(IEntitie entitie)
    {

    }
    
        protected override void ApplyRarityScaling(float rarityScaling)
    {
        // Hier MUSS die Kindklasse den Wert verwenden!
        Debug.Log($"[EssenceDrain] rarityScaling angewendet: {rarityScaling}");
    }
}