using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Voidexplosion : Spell, IUseable
{
    public float radius = 2.5f;

    [SerializeField]
    private float damage = 8;

    imp_Voidexplosion improved;

    IsometricPlayer player;

    void Start()
    {
        player = PlayerManager.instance.player.GetComponent<IsometricPlayer>();

        SetDescription("Verursacht im Radius von " + radius + " um den Spieler herum \n" + damage + player.playerStats.AbilityPower.Value * 0.7f + " Schaden. (8 + 70% AP)");
    }

    public override void Use()
    {


        if (!onCoolDown && currentCount >= 1)
        {
            Collider[] hitColliders = Physics.OverlapSphere(PlayerManager.instance.player.transform.position, radius);

            foreach (Collider hitCollider in hitColliders)
            {

                if (hitCollider.transform.tag == "Enemy")
                {

                    hitCollider.transform.GetComponentInParent<MobStats>().TakeDirectDamage(damage + player.playerStats.AbilityPower.Value * 0.7f, radius + improved.currentCount);


                }

            }

            onCoolDown = true;
        }
    }

    public override bool IsOnCooldown()
    {
        return onCoolDown;
    }

    public override float GetCooldown()
    {
        return GetSpellCoolDown;
    }

    public override float CooldownTimer()
    {
        return coolDownTimer;
    }

}

