using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "Heal", menuName = "Assets/Spells/Heal")]

//Currently Heal - Aura is broken. Fixing it makes no sense, since the "Buff" System should be done in first place. Heal then might become a Buff.
public class Heal : Ability
{
    public float healAmount;

    private float spec1range;

    private float spec2speed;

    float entitieAP;

    public GameObject healAuraEffect;

    public GameObject healAuraTick;

    private StatModifier spec2mod;

    private StatModifier spec3mod;

    private StatModifier spec3ATmod;

    //private var auraEffect;

    #region UseRegion
    public override void UseBase(IEntitie entitie)
    {

        base.UseBase(entitie);



        ///Entitie sollte woanders zwischen gelagert werden, eigene Methode als Return des Transforms verwenden.
        ///
        float entitieAP = entitie.GetStat(EntitieStats.AbilityPower).Value;

        //Debug.Log("entitieAP: " + entitieAP);

        healAmount = healAmount + (entitieAP / 10);
        PlayerManager.instance.player.GetComponent<PlayerStats>().Heal((int)healAmount);

        //Erschaffe ein neues GO für den Healaura Effekt
        Instantiate(healAuraEffect, entitie.GetTransform());

        //Setze Parent des GO für vernünftiges UI Layering
        //auraEffect.transform.SetParent(entitie.transform, false);


    }

    public override void ApplySpec1Bonus(Talent t)
    {
        activeTime = 1 + spec1Talents[0].currentCount;

        spec1range = 1 + spec1Talents[1].currentCount;


        //Setze die Färbung der Partikel beim Tick
        ParticleSystem.MainModule settingsTick = healAuraTick.GetComponent<ParticleSystem>().main;
        settingsTick.startColor = new Color(1, 0, 1, 1);

        //Setze Färbung der Aura selbst
        ParticleSystem ps = healAuraEffect.GetComponent<ParticleSystem>();
        var col = ps.colorOverLifetime;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.magenta, 0.7f), new GradientColorKey(Color.black, 1.0f) }, 
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f, 0.7f), new GradientAlphaKey(0f, 1.0f) });
        col.color = grad;

    }

    //Möglicherweise reicht eine einzelne Liste von Talenten in Ability, da die Indexes dieser die Referenz für die Values der Spezialisierungen darstellen können.
    public override void OnUseSpec1(IEntitie entitie)
    {
        //Void Spec

        ///Heal erhält eine aktive Zeitspanne von 1 + (Skillpoints of Heiling_Void1) Sekunden, pro Sekunde erhalten Feinde in Range = 1, 4 + 10% AP Schaden.
        ///spec1Talents[0].currentCount; -- Skillspoints of Heilung_Void1
        activeTime = 1 + spec1Talents[0].currentCount;


        ///Erhöht den Radius der Heal-Schadensaura um (Skillpoints of Heilung_Void2)
        ///
        spec1range = 1 + spec1Talents[1].currentCount;


        ///Zu Beginn (und am Ende der aktiven Zeit?) erhalten alle Gegner die verursachte Heilung als Schaden. (x Skillpoints of Heilung_Void3)
        ///
        if(spec1Talents[2].currentCount > 0)
        {
            Collider[] hitColliders = Physics.OverlapSphere(entitie.GetTransform().position, spec1range);

            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.transform.tag == "Enemy")
                {
                    //print("heal amount:" + healAmount);
                    hitCollider.transform.GetComponentInParent<MobStats>().TakeDirectDamage(healAmount, spec1range);
                }
            }
        }

    }

    public override void ApplySpec2Bonus(Talent t)
    {
        activeTime = 1 + spec2Talents[0].currentCount;


        //Setze die Färbung der Partikel beim Tick
        ParticleSystem.MainModule settingsTick = healAuraTick.GetComponent<ParticleSystem>().main;
        settingsTick.startColor = new Color(0, 1, 0, 1); //Green


        //Setze Färbung der Aura selbst
        ParticleSystem ps = healAuraEffect.GetComponent<ParticleSystem>();  
        var col = ps.colorOverLifetime;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.green, 0.7f), new GradientColorKey(Color.black, 1.0f) }, 
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f, 0.7f), new GradientAlphaKey(0f, 1.0f)});

        col.color = grad;

    }
    public override void OnUseSpec2(IEntitie entitie)
    {
        //Heal Spec

        ///Heal erhält eine aktive Zeitspanne von 1+ (Skillpoints of Heilung_Heal1), pro Sekunde heilt der Spieler zusätzlich um 2% max HP + 10% AP
        ///
        activeTime = 1 + spec2Talents[0].currentCount;

        ///Während der aktiven Zeitspanne erhält der Spieler (Skillpoits of Heilung_Heal2) x 5% extra Movementspeed.
        ///
        if(spec2Talents[1].currentCount > 0)
        {
            spec2speed = spec2Talents[1].currentCount * 0.05f;

            spec2mod = new StatModifier(spec2speed, StatModType.PercentAdd);

            entitie.GetStat(EntitieStats.MovementSpeed).AddModifier(spec2mod);

        }

        ///Wenn heal genutzt wird, werden alle Gegner im Umkreis von 1 (+ Skillpoints of Heilung_Heal3) für 2 (x Skillpoints of Heilung_Heal3) Sekunden gestunned.
        ///
        //Stun ist noch nicht implementiert... mist.
        if(spec2Talents[2].currentCount >= 0)
        {
            Collider[] hitColliders = Physics.OverlapSphere(entitie.GetTransform().position, 1 + spec2Talents[2].currentCount);

            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.transform.tag == "Enemy")
                {
                    hitCollider.transform.GetComponentInParent<EnemyController>().StunEnemy(2 * spec2Talents[2].currentCount);
                }
            }
        }
    }


    public override void ApplySpec3Bonus(Talent t)
    {
        activeTime = 1 + spec3Talents[0].currentCount;


        //Setze die Färbung der Partikel beim Tick
        ParticleSystem.MainModule settingsTick = healAuraTick.GetComponent<ParticleSystem>().main;
        settingsTick.startColor = new Color(1, 0.92f, 0.016f, 1);


        //Setze Färbung der Aura selbst
        ParticleSystem ps = healAuraEffect.GetComponent<ParticleSystem>();
        var col = ps.colorOverLifetime;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.yellow, 0.7f), new GradientColorKey(Color.black, 1.0f) }, 
            new GradientAlphaKey[] { new GradientAlphaKey(0.0f, 0.0f), new GradientAlphaKey(1.0f, 0.7f), new GradientAlphaKey(0f, 1.0f) });

        col.color = grad;
    }
    public override void OnUseSpec3(IEntitie entitie)
    {
        //Combat Spec

        ///Heal erhält eine aktive Zeitspanne von (Skillpoints of Heilung_Combat1), während dieser Zeitspannte erhält der Spieler 20% Armor als extra Armor. 
        ///
        activeTime = 1 + spec3Talents[0].currentCount;

        spec3mod = new StatModifier(entitie.GetStat(EntitieStats.Armor).Value * 0.2f, StatModType.PercentAdd);
        entitie.GetStat(EntitieStats.Armor).AddModifier(spec3mod);

        /*
        if (entitie.GetComponent<PlayerStats>() != null)
        {
            if (spec3Talents[0].currentCount > 0)
            {
                PlayerStats pStats = entitie.GetComponent<PlayerStats>();

                spec3mod = new StatModifier(pStats.Armor.Value * 0.2f, StatModType.PercentAdd);

                pStats.Armor.AddModifier(spec3mod);

            }

        }
        else
        {
            print("Spec3 für Mob muss noch eingestellt werden");
        }
        */

        ///Während der aktiven Zeitspanne erhöht sich die Waffenstärke um (Skillpoints of Heilung_Combat2) x 10% ATT des Spielers.
        ///

        //  10             *      3                       *  0.1     = 0.3
        spec3ATmod = new StatModifier(entitie.GetStat(EntitieStats.AttackPower).Value * spec3Talents[1].currentCount * 0.1f, StatModType.PercentAdd);

        /*
        if (entitie.GetComponent<PlayerStats>() != null)
        {
            if (spec3Talents[1].currentCount > 0)
            {
                PlayerStats pStats = entitie.GetComponent<PlayerStats>();
                                                    //  10             *      3                       *  0.1     = 0.3
                spec3ATmod = new StatModifier(pStats.AttackPower.Value * spec3Talents[1].currentCount * 0.1f, StatModType.PercentAdd);

                pStats.AttackPower.AddModifier(spec3ATmod);


            }

        }
        else
        {
            print("Spec3 für Mob muss noch eingestellt werden");
        }
        */

        ///Während der aktiven Zeitspanne von Heal, erhalten Angreifer 10% der Armor des Spielers (x Skillpoints of Heilung_Combat3) als Schaden.
        ///

        //Schon wieder so ein Moment, wo man merkt, wir brauchen DeBuffSystem..
        //In Allen Controllern sollte eine Funktion Apply(Buff, Duration) sein.

        //In diesem Fall, Player.Apply(Dornen, spec3Talents[0].currentCount)
        //Woher weiß die Ability, was Dornen sind?..

    }

    #endregion


    #region TickRegion

    //Ein TickTimer, welcher alle x Sekunden während der Aktiven Zeit ausgeführt wird. Standard tickTimer ist auf 1 - also alle 1 Sekunden.
    public override void OnTickSpec1(IEntitie entitie)
    {
        //print("Spec1Tick got called");
        //Hier gegebenenfalls nochmal ran, für den fall, das mobs diese fähigkeit verwenden
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        float damage = 4 + (playerStats.AbilityPower.Value / 10);

        Collider[] hitColliders = Physics.OverlapSphere(entitie.GetTransform().position, spec1range);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.transform.tag == "Enemy")
            {
                hitCollider.transform.GetComponentInParent<MobStats>().TakeDirectDamage(damage, spec1range);

            }
        }

        //GFX

        if (AudioManager.instance != null)
            AudioManager.instance.Play("Heal_Tick_1");


        GameObject healTick = healAuraTick;

        Instantiate(healTick, new Vector3(entitie.GetTransform().position.x, entitie.GetTransform().position.y, entitie.GetTransform().position.z - 0.3f), Quaternion.identity, entitie.GetTransform());

    }

    public override void OnTickSpec2(IEntitie entitie)
    {
        float entitieAP = entitie.GetStat(EntitieStats.AbilityPower).Value;

        entitie.Heal((int)entitie.Get_maxHp() / 50 + (int)(entitieAP / 10));

        /*
        if (entitie.GetComponent<PlayerStats>() != null)
        {
            PlayerStats pStats = entitie.GetComponent<PlayerStats>();
            entitieAP = entitie.GetComponent<PlayerStats>().AbilityPower.Value;
            PlayerManager.instance.player.GetComponent<PlayerStats>().Heal((int)pStats.Get_maxHp() / 50  + (int)(entitieAP / 10));

        }

        else
        {
            entitieAP = entitie.GetComponent<MobStats>().AbilityPower.Value;
            print("Der Feind braucht noch eine Zuweisung seiner Entitie im Heal Script");
        }
        */

        if(AudioManager.instance != null)
        AudioManager.instance.Play("Heal_Tick_1");


        //Setting Color
        GameObject healTick = healAuraTick;

        Instantiate(healTick, new Vector3(entitie.GetTransform().position.x, entitie.GetTransform().position.y, entitie.GetTransform().position.z - 0.3f), Quaternion.identity, entitie.GetTransform());

    }

    public override void OnTickSpec3(IEntitie entitie)
    {
        //throw new NotImplementedException();
    }

    #endregion


    #region CooldownRegion
    public override void OnCooldown(IEntitie entitie)
    {
        Destroy(entitie.GetTransform().Find("Eff_HealAura(Clone)").gameObject);
    }

    public override void OnCooldownSpec1(IEntitie entitie)
    {
        if (spec1Talents[2].currentCount > 0)
        {
            Collider[] hitColliders = Physics.OverlapSphere(entitie.GetTransform().position, spec1range);

            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.transform.tag == "Enemy")
                {
                    //print("heal amount:" + healAmount);
                    hitCollider.transform.GetComponentInParent<MobStats>().TakeDirectDamage(healAmount, spec1range);
                }
            }
        }

        //entitie.transform.Find("HealAura(Clone)");

        //Destroy(entitie.transform.Find("HealAura(Clone)").gameObject);
        //print("on cooldown got called");
    }

    public override void OnCooldownSpec2(IEntitie entitie)
    {

        entitie.GetStat(EntitieStats.MovementSpeed).RemoveModifier(spec2mod);
        /*
        if (entitie.GetComponent<PlayerStats>() != null)
        {
            PlayerStats pStats = entitie.GetComponent<PlayerStats>();
            pStats.MovementSpeed.RemoveModifier(spec2mod);
        }
        else
        {
            MobStats mStats = entitie.GetComponent<MobStats>();
            mStats.MovementSpeed.RemoveModifier(spec2mod);

        }
        */
        //Destroy(entitie.transform.Find("HealAura(Clone)").gameObject);
    }

    public override void OnCooldownSpec3(IEntitie entitie)
    {
        entitie.GetStat(EntitieStats.Armor).RemoveModifier(spec3mod);
        /*
        if (entitie.GetComponent<PlayerStats>() != null)
        {

            PlayerStats pStats = entitie.GetComponent<PlayerStats>();

            if (spec3Talents[0].currentCount > 0)
            {
                pStats.Armor.RemoveModifier(spec3mod);
            }

            if (spec3Talents[1].currentCount > 0)
            {
                pStats.AttackPower.RemoveModifier(spec3ATmod);
            }



        }
        else
        {
            print("Spec3 für Mob muss noch eingestellt werden");
        }
        */
    }
    #endregion
}
