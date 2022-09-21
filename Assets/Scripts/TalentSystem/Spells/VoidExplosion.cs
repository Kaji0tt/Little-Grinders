using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidExplosion : Ability
{
    public float baseDamage;

    private float spec1range;

    private float spec1scaling;

    public float spec1Duration;

    private float spec1Intervall;

    float entitieAP;

    public GameObject voidExpEffect;

    //public GameObject healAuraTick;

    private StatModifier spec2mod;

    private StatModifier spec3mod;

    private StatModifier spec3ATmod;


    #region UseRegion
    public override void UseBase(IEntitie entitie)
    {

        base.UseBase(entitie);

        //Calle die Fähigkeit seperat, um sie in den Spezialisierungen unabhängig von der BasAbility (Spec) callen zu können.
        CallVoidExplosion(entitie);






    }

    ///Erschaffe einen Circle um den Spieler in einem Radius von .5 + spec1.Count / 2 und Füge baseDamage + 30% AP Schaden zu.
    ///
    public void CallVoidExplosion(IEntitie entitie)
    {
        //Erhalte die AP Values des Ursprungs
        float entitieAP = entitie.GetStat(EntitieStats.AbilityPower);

        //Erschaffe einen SphereCollider um den Ursprung
        Collider[] hitColliders = Physics.OverlapSphere(entitie.GetTransform().position, .5f + spec1range / 2);

        //Für jeden getroffenen Collider
        foreach (Collider hitCollider in hitColliders)
        {
            //Handelt es sich bei dem Collider um einen Feind und beim Ursprung um den Spieler
            if (entitie.GetTransform().tag == "Player" && hitCollider.transform.tag == "Enemy")
            {
                //Falls der erste Spec nicht geskilled wurde, verwende normale Schadensberechnung für alle getroffenen Feinde.
                if(spec1Talents[0].currentCount == 0)
                hitCollider.transform.GetComponentInParent<IEntitie>().TakeDirectDamage(baseDamage + entitieAP * spec1scaling, spec1range);

                else
                {
                    //Falls der erste Spec1 durch geskilled wurde, verwende zunächst die Standard-Schadensberechnung,
                    if(spec1Intervall == 0)
                    {
                        hitCollider.transform.GetComponentInParent<IEntitie>().TakeDirectDamage(baseDamage + entitieAP * spec1scaling, spec1range);


                    }


                    //ehe der Schaden anschließend durch das Intervall geteilt wird. So erreicht erst der letzte Cast den finalen Schaden.
                    else
                    {
                        hitCollider.transform.GetComponentInParent<IEntitie>().TakeDirectDamage((baseDamage + entitieAP * spec1scaling) / spec1Intervall, spec1range);

                    }

                }


            }

            if (entitie.GetTransform().tag == "Enemy" && hitCollider.transform.tag == "Player")
            {
                //Falls vom Gegener gecalled, füge lediglich dem Spieler Schaden zu.
                print("succesfully called Enemy-Origin, Player Target");                                                //Die Range ist indem fall fix.
                hitCollider.transform.GetComponentInParent<IEntitie>().TakeDirectDamage(baseDamage + entitieAP * 0.3f, 2.5f);
            }

        }

        //Erschaffe ein entsprechenden Partikel Effect.
        Instantiate(voidExpEffect, entitie.GetTransform());

        //Sound
        if (AudioManager.instance != null)
            AudioManager.instance.Play("VoidExplosion");

    }

    //Möglicherweise reicht eine einzelne Liste von Talenten in Ability, da die Indexes dieser die Referenz für die Values der Spezialisierungen darstellen können.

    public override void ApplySpec1Bonus(Talent t)
    {

        spec1range = 1.5f + spec1Talents[0].currentCount;

        spec1scaling = 0.3f + (spec1Talents[1].currentCount / 10);

    }
    public override void OnUseSpec1(IEntitie entitie)
    {
        //Void Spec

        ///_________________First-Follow Talent: 
        ///Void erhält einen erhöhten Radius um 1 pro Punkt in Spec1.
        spec1range = 1.5f + spec1Talents[0].currentCount;



        ///_________________Second-Follow Talent: 
        ///Erhöht die Skalierung von Void-Explosion um 0,1 pro Specpunkt.
        spec1scaling = 0.3f + (spec1Talents[1].currentCount / 10);

        ///_________________Third-Follow Talent: 
        ///Im Intervall einer Lerp-Funktion, calle Void Explosion erneut. Die ersten call werden in radius und schaden durch die Gesamtzahl der Calls geteilt.

        //Falls das dritte Follow Talent geskilled wurde, setze die aktive Zeit der Fähigkeit auf die spec1Duration.
        //Ticks beginnen ab der Hälfte der Duration.
        if(spec1Talents[2].currentCount == 1)
        {
            spec1Intervall = spec1Duration;


            //Setze die aktive Zeit auf die Länge der lerpDuration von Voidexplosion.
            activeTime = spec1Duration;


            //Scheinbar wird lediglich am Ende der ActiveTime nochmal gecalled.
            tickTimer = spec1Duration / 2;


        }


    }

    public override void ApplySpec2Bonus(Talent t)
    {
        activeTime = 1 + spec2Talents[0].currentCount;
    }
    public override void OnUseSpec2(IEntitie entitie)
    {
        //Heal Spec

        ///Heal erhält eine aktive Zeitspanne von 1+ (Skillpoints of Heilung_Heal1), pro Sekunde heilt der Spieler zusätzlich um 2% max HP + 10% AP
        ///
        activeTime = 1 + spec2Talents[0].currentCount;

        ///Während der aktiven Zeitspanne erhält der Spieler (Skillpoits of Heilung_Heal2) x 5% extra Movementspeed.
        ///
        if (spec2Talents[1].currentCount > 0)
        {
            //spec2speed = spec2Talents[1].currentCount * 0.05f;

            //spec2mod = new StatModifier(spec2speed, StatModType.PercentAdd);

            entitie.AddNewStatModifier(EntitieStats.MovementSpeed, spec2mod);

            /*
            if (entitie.GetComponent<PlayerStats>() != null)
            {
                PlayerStats pStats = entitie.GetComponent<PlayerStats>();
                pStats.MovementSpeed.AddModifier(spec2mod);
            }
            else
            {
                MobStats mStats = entitie.GetComponent<MobStats>();
                mStats.MovementSpeed.AddModifier(spec2mod);

            }
            */
        }

        ///Wenn heal genutzt wird, werden alle Gegner im Umkreis von 1 (+ Skillpoints of Heilung_Heal3) für 2 (x Skillpoints of Heilung_Heal3) Sekunden gestunned.
        ///
        //Stun ist noch nicht implementiert... mist.
        if (spec2Talents[2].currentCount >= 0)
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
    }
    public override void OnUseSpec3(IEntitie entitie)
    {
        //Combat Spec

        ///Heal erhält eine aktive Zeitspanne von (Skillpoints of Heilung_Combat1), während dieser Zeitspannte erhält der Spieler 20% Armor als extra Armor. 
        ///
        activeTime = 1 + spec3Talents[0].currentCount;

        spec3mod = new StatModifier(entitie.GetStat(EntitieStats.Armor) * 0.2f, StatModType.PercentAdd);
        entitie.AddNewStatModifier(EntitieStats.Armor, spec3mod);

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
        spec3ATmod = new StatModifier(entitie.GetStat(EntitieStats.AttackPower) * spec3Talents[1].currentCount * 0.1f, StatModType.PercentAdd);

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


        //Das Intervall "Lerpt" in Abhängigkeit der Länge des Spells herunter und called die Explosion erneut.
        if(spec1Intervall >= 0)
        {

            //Reduziere mit jedem Tick die TickZeit um die Hälfte.
            tickTimer = tickTimer / 2;

            //Mit jedem Intervall, call die Explosion
            CallVoidExplosion(entitie);

            spec1Intervall -=  1;
        }
        else if (spec1Intervall == 0)
        {
            
            spec1Intervall = spec1Duration;
        }

    }

    public override void OnTickSpec2(IEntitie entitie)
    {
        float entitieAP = entitie.GetStat(EntitieStats.AbilityPower);

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

        if (AudioManager.instance != null)
            AudioManager.instance.Play("Heal_Tick_1");

        Instantiate(voidExpEffect, entitie.GetTransform());

        Instantiate(voidExpEffect, new Vector3(entitie.GetTransform().position.x, entitie.GetTransform().position.y, entitie.GetTransform().position.z - 0.3f), Quaternion.identity, entitie.GetTransform());

    }

    public override void OnTickSpec3(IEntitie entitie)
    {
        //throw new NotImplementedException();
    }

    #endregion


    #region CooldownRegion
    public override void OnCooldown(IEntitie entitie)
    {
        //Destroy(entitie.GetTransform().Find("Eff_HealAura(Clone)").gameObject);
    }

    public override void OnCooldownSpec1(IEntitie entitie)
    {
        /*
        if (spec1Talents[2].currentCount > 0)
        {
            Collider[] hitColliders = Physics.OverlapSphere(entitie.GetTransform().position, spec1range);

            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.transform.tag == "Enemy")
                {
                    //print("heal amount:" + healAmount);
                    hitCollider.transform.GetComponentInParent<MobStats>().TakeDirectDamage(baseDamage, spec1range);
                }
            }
        }
        */

        //entitie.transform.Find("HealAura(Clone)");

        //Destroy(entitie.transform.Find("HealAura(Clone)").gameObject);
        //print("on cooldown got called");
    }

    public override void OnCooldownSpec2(IEntitie entitie)
    {

        entitie.RemoveStatModifier(EntitieStats.MovementSpeed, spec2mod);
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
        entitie.RemoveStatModifier(EntitieStats.Armor, spec3mod);
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
