using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombatHeilung_Max : Talent, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    Heilung heilung;

    PlayerStats playerStats;

    bool damageApplied;
    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(new Vector2(Input.mousePosition.x - 10f, Input.mousePosition.y + 10f), GetDescription);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
    void Start()
    {

        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        SetDescription("Wenn der Heilungseffekt von Heilung ausläuft, verursachst du deinen doppelten Rüstungswert als Schaden. Radius: 1");
    }

    // Update is called once per frame
    void Update()
    {
        if(currentCount != 0)
        {
            if (heilung.healDuration >= 0 && !damageApplied)
            {
                Collider[] hitColliders = Physics.OverlapSphere(PlayerManager.instance.player.transform.position, 1);

                foreach (Collider hitCollider in hitColliders)
                {

                    if (hitCollider.transform.tag == "Enemy")
                    {
                        float damage = playerStats.Armor.Value * 2;


                        hitCollider.transform.GetComponentInParent<EnemyController>().TakeDirectDamage((int)(damage), 1);

                        damageApplied = true;

                    }

                }
            }
            else if (heilung.healDuration <= 0)
                damageApplied = false;
        }

    }
}
