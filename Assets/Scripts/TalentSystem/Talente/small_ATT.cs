using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class small_ATT : Talent, IPointerEnterHandler, IPointerExitHandler, ISmallTalent
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(GetDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }

    public void PassiveEffect()
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        playerStats.AttackPower.AddModifier(new StatModifier(2, StatModType.Flat));
    }

    void Start()
    {
        SetDescription("+2 ATT");
    }
}