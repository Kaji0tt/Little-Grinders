using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidExplosion : Ability
{
    public float baseDamage;

    public GameObject voidExpEffect;

    /// <summary>
    /// Spec 1 Variables
    /// </summary>
    private float spec1range;

    private float spec1scaling;

    public float spec1Duration;

    private float spec1Intervall;

    /// <summary>
    /// Spec 2 Variables
    /// </summary>

    public Buff lifeDot;

    public float spec2extraDamage;

    private float spec2finalDamage;

    public GameObject spec2VoidEplo;

    //public GameObject healAuraTick;

    /// <summary>
    /// Spec 3 Variables
    /// </summary>

    public Buff weakArmor;

    private void Start()
    {
        GameEvents.instance.OnPlayerHasAttackedEvent += PlayerAttacked; 

    }

    private void OnDisable()
    {
        //GameEvents.instance.OnPlayerHasAttackedEvent -= PlayerAttacked;
    }

    private void PlayerAttacked(float playerDamage)
    {
        if(spec3Talents[2].currentCount > 0 && this.GetComponent<AbilityTalent>().IsActive())
        {
            CallVoidExplosion(PlayerManager.instance.player.GetComponent<PlayerStats>(), PlayerManager.instance.player.transform.position);
        }
    }

    #region UseRegion
    public override void UseBase(IEntitie entitie)
    {

        base.UseBase(entitie);

        //Calle die Fähigkeit seperat, um sie in den Spezialisierungen unabhängig von der BasAbility (Spec) callen zu können.
        CallVoidExplosion(entitie, entitie.GetTransform().position);

    }

    ///Erschaffe einen Circle um den Spieler in einem Radius von .5 + spec1.Count / 2 und Füge baseDamage + 30% AP Schaden zu.
    ///
    public void CallVoidExplosion(IEntitie entitie, Vector3 position)
    {
        //Erhalte die AP Values des Ursprungs
        float entitieAP = entitie.GetStat(EntitieStats.AbilityPower).Value;

        //Erschaffe einen SphereCollider um den Ursprung
        Collider[] hitColliders = Physics.OverlapSphere(position, 1f + spec1range);

        //Für jeden getroffenen Collider
        foreach (Collider hitCollider in hitColliders)
        {
            //Handelt es sich bei dem Collider um einen Feind und beim Ursprung um den Spieler
            if (entitie.GetTransform().tag == "Player" && hitCollider.transform.tag == "Enemy")
            {
                //Falls der erste Spec nicht geskilled wurde, verwende normale Schadensberechnung für alle getroffenen Feinde.
                if(spec1Talents[0].currentCount == 0)
                hitCollider.transform.GetComponentInParent<IEntitie>().TakeDirectDamage(baseDamage + entitieAP * (0.3f + spec1scaling) + spec2extraDamage, 1f + spec1range);

                else
                {
                    //Falls der erste Spec1 durch geskilled wurde, verwende zunächst die Standard-Schadensberechnung,
                    if(spec1Intervall == 0)
                    {
                        hitCollider.transform.GetComponentInParent<IEntitie>().TakeDirectDamage(baseDamage + entitieAP * (0.3f + spec1scaling), 1f + spec1range);

                    }


                    //ehe der Schaden anschließend durch das Intervall geteilt wird. So erreicht erst der letzte Cast den finalen Schaden.
                    else
                    {
                        hitCollider.transform.GetComponentInParent<IEntitie>().TakeDirectDamage((baseDamage + (entitieAP * 0.3f) * spec1scaling) / spec1Intervall, 1f + spec1range);

                    }

                }


            }

            if (entitie.GetTransform().tag == "Enemy" && hitCollider.transform.tag == "Player")
            {
                //Falls vom Gegener gecalled, füge lediglich dem Spieler Schaden zu.
                print("succesfully called Enemy-Origin, Player Target");                                                //Die Range ist indem fall fix.
                hitCollider.transform.GetComponentInParent<IEntitie>().TakeDirectDamage(baseDamage + entitieAP * 0.3f, 1.5f);
            }

        }

        //Erschaffe ein entsprechenden Partikel Effect.
        Instantiate(voidExpEffect, entitie.GetTransform());

        //Sound
        if (AudioManager.instance != null)
            AudioManager.instance.Play("VoidExplosion");

    }

    //Möglicherweise reicht eine einzelne Liste von Talenten in Ability, da die Indexes dieser die Referenz für die Values der Spezialisierungen darstellen können.

    //De we actually need to give Information of the clicked Talent? Not sure.
    public override void ApplySpec1Bonus(Talent t)
    {

        //Standard reichweite beträgt 1. 
        //Ist die erste Fähigkeit von Voidexplosion durch geskilled, beträgt die reichweite 1 +  (5 / 5 = 1) = 2
        spec1range = spec1Talents[0].currentCount / 5;

        //Erhöht den Verursachten schaden pro skillpunkt um zusätzlich 10% AP
        spec1scaling = (spec1Talents[1].currentCount / 10);

    }
    public override void OnUseSpec1(IEntitie entitie)
    {
        //Void Spec

        ///_________________First-Follow Talent: 
        ///Void erhält einen erhöhten Radius um 1 pro Punkt in Spec1.
        spec1range = spec1Talents[0].currentCount / 5;



        ///_________________Second-Follow Talent: 
        ///Erhöht die Skalierung von Void-Explosion um 0,1 pro Specpunkt.
        spec1scaling = spec1Talents[1].currentCount / 10;

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
        spec2extraDamage = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_maxHp() / 100 * (5 * spec2Talents[1].currentCount);

        if (spec2Talents[2].currentCount > 0)
        {
            activeTime = 5;
        }

        if (spec2Talents[2].currentCount > 0)
        {
            spec2finalDamage = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_maxHp() / 100 * 20;
        }
    }
    public override void OnUseSpec2(IEntitie entitie)
    {
        //Utility Spec


        ///Void-Explosion fügt je Skillpunkt einen Dot hinzu, welcher über 5 Sekunden je Skillpunkt 1 + 2% AP + 2% PlayerMaxLife Schaden verursacht.  
        //activeTime = 1 + spec2Talents[0].currentCount;

        //for(int i = 0; i < spec2Talents[0].currentCount; i++)
        //{
        Collider[] hitColliders = Physics.OverlapSphere(entitie.GetTransform().position, 1f);

        ApplySpec2Dots(entitie, hitColliders);
        //}




        ///Der Schaden von Void-Explosion wird je Skillpunkt um 5% des Spieler Lebens erhöht.
        if (spec2Talents[1].currentCount > 0)
        {
            spec2extraDamage = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_maxHp() / 100 * (5 * spec2Talents[1].currentCount);
            Debug.Log(spec2extraDamage);
        }

        ///Void Explosion wird nach 5 Sekunden an der Stelle des Cursors ein zusätzliches Mal gecasted und verursacht dabei 20% des Spieler Lebens als zusätzlichen Schaden.
        ///
        //Stun ist noch nicht implementiert... mist.
        if (spec2Talents[2].currentCount > 0)
        {
            spec2finalDamage = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_maxHp() / 100 * 20;


        }
    }

    void ApplySpec2Dots(IEntitie entitie, Collider[] hitColliders)
    {


        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.transform.tag == "Enemy")
            {
                /*Certain Problem about Buffs - the BuffInstance does not know about certain values of the Buff. While Damage Buffs might have a Damage value
                 * thats subject to Change from other Classes (e.g. Abilities), other Buffs like Reflection or Thorns might not deal with base damage Values.
                 * LF for a way, that Specific Buffs might be edited after they've been constructed.
                 */
                BuffInstance buffInstance = new BuffInstance(lifeDot);

                //You could change the MyBaseDamage Value here, however we are applying the Debuff currently for the number of SpecPoints invested which makes more fun and sense.
                //buffInstance.MyBaseDamage = buffInstance.MyBaseDamage * 

                //print(hitCollider.gameObject.transform.parent.GetComponent<MobStats>());
                buffInstance.ApplyBuff(hitCollider.gameObject.transform.parent.GetComponent<MobStats>(), PlayerManager.instance.player.GetComponent<PlayerStats>());

            }
        }
    }

    public override void ApplySpec3Bonus(Talent t)
    {
        if(spec3Talents[2].currentCount > 0)
        activeTime = 5;
    }
    public override void OnUseSpec3(IEntitie entitie)
    {
        //Combat Spec

        ///Void Explosion reduziert über einen Debuff die Rüstung der Gegner um jeweils 5% für 5 Sekunden. -> Buff, Rüstungszerreißen
        Collider[] hitColliders = Physics.OverlapSphere(entitie.GetTransform().position, 1f + spec1range);

        foreach(Collider collider in hitColliders)
        {
            if(collider.gameObject.tag == "Enemy")
            {
                BuffInstance buffInstance = new BuffInstance(weakArmor);

                //You could change the MyBaseDamage Value here, however we are applying the Debuff currently for the number of SpecPoints invested which makes more fun and sense.
                //buffInstance.MyBaseDamage = buffInstance.MyBaseDamage * 

                //print(hitCollider.gameObject.transform.parent.GetComponent<MobStats>());
                buffInstance.ApplyBuff(collider.gameObject.transform.parent.GetComponent<MobStats>(), PlayerManager.instance.player.GetComponent<PlayerStats>());
            }
        }





        ///Void-Explosion gibt dir einen Attack-Power von jeweils 5% für 5 Sekunden.





        ///Void-Explosion bleibt 5 Sekunden aktiv - während der aktiven Zeit, castest du Void-Explosion während deiner Angriffe.
        //Should work.


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
            CallVoidExplosion(entitie, entitie.GetTransform().position);

            spec1Intervall -=  1;
        }
        else if (spec1Intervall == 0)
        {
            
            spec1Intervall = spec1Duration;
        }

    }

    public override void OnTickSpec2(IEntitie entitie)
    {

        //Spec 2 aint got no Tick dependent on Spell but uses Buff System.
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
        float entitieAP = entitie.GetStat(EntitieStats.AbilityPower).Value;

        //Get MousePos.
        Ray ray = CameraManager.instance.mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, LayerMask.NameToLayer("Floor")))
        {
            Collider[] hitColliders = Physics.OverlapSphere(raycastHit.point, 1f + spec1range);

            ApplySpec2Dots(entitie, hitColliders);

            //Für jeden getroffenen Collider
            foreach (Collider hitCollider in hitColliders)
            {
                //Handelt es sich bei dem Collider um einen Feind und beim Ursprung um den Spieler
                if (entitie.GetTransform().tag == "Player" && hitCollider.transform.tag == "Enemy")
                {
                    //Falls der erste Spec nicht geskilled wurde, verwende normale Schadensberechnung für alle getroffenen Feinde.

                    hitCollider.transform.GetComponentInParent<IEntitie>().TakeDirectDamage(baseDamage + entitieAP * (0.3f + spec1scaling) + spec2extraDamage + spec2finalDamage, float.MaxValue);
                }

            }

            Instantiate(spec2VoidEplo, raycastHit.point, Quaternion.identity);
        }

        //Erschaffe einen SphereCollider um den Ursprung




    }


    public override void OnCooldownSpec3(IEntitie entitie)
    {
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
