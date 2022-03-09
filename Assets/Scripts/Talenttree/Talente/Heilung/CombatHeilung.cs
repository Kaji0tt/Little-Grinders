using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatHeilung : Talent, IPointerEnterHandler, IPointerExitHandler
{
    Heilung heilung;

    PlayerStats playerStats;

    bool armorgained;
    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(new Vector2(Input.mousePosition.x, Input.mousePosition.y), GetDescription, this.gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }


    void Update()
    {


        StatModifier extraArmor = new StatModifier(currentCount * 10, StatModType.Flat);

        if (currentCount != 0)
        {
            if (heilung.healActivated & !armorgained)
            {
                playerStats.Armor.AddModifier(extraArmor);
                armorgained = true;
            }
            else if (!heilung.healActivated & armorgained)
            {
                playerStats.Armor.RemoveModifier(extraArmor);
                armorgained = false;
            }
        }


    }

    void Start()
    {
        SetDescription("Während die Heilung aktiviert ist,\n erhältst du (10/20/30) extra Armor.");

        heilung = FindObjectOfType<Heilung>();

        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        armorgained = false;
    }

}
