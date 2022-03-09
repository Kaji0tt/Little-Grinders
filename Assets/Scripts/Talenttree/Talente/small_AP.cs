using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class small_AP : Talent, IPointerEnterHandler, IPointerExitHandler, ISmallTalent
{
    float apIncrease = 2;

    StatModifier mod;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(new Vector2(Input.mousePosition.x, Input.mousePosition.y), GetDescription, gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }

    public void PassiveEffect()
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        playerStats.AbilityPower.AddModifier(new StatModifier(apIncrease, StatModType.Flat));
    }

    void Start()
    {
        SetDescription("+2 AP");
    }
}
