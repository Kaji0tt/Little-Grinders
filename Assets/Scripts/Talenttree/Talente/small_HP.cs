using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class small_HP : Talent, IPointerEnterHandler, IPointerExitHandler, ISmallTalent
{
    float hpIncrease = 3;

    StatModifier mod;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(new Vector2(Input.mousePosition.x - 10f, Input.mousePosition.y + 10f), GetDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }

    public void PassiveEffect()
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        playerStats.Hp.AddModifier(new StatModifier(hpIncrease, StatModType.Flat));
    }

    void Start()
    {
        SetDescription("+3 HP");
    }


}