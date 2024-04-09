using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buff/Reflection")]
public class Reflection : Buff
{
    public float damage;

    private IEntitie instanceTarget;

    private void OnEnable()
    {
        //GameEvents.instance.OnPlayerHasAttackedEvent += PlayerAttacked;
        //GameEvents.instance.OnEnemyHasAttackedEvent += EnemyAttacked;
    }

    public override void Activated(IEntitie target, Transform targetTransform)
    {
        //Wenn Reflection aktiviert wird, füge den Listener hinzu, dass ReflectDamage gecalled wird, sobald der Spieler schaden erleidet.
        //GameEvents.current.playerWasAttacked += reflectDamage;
        instanceTarget = target;

        Debug.Log("reflection applied");

    }

    private void reflectDamage(IEntitie target, float damage)
    {
        Debug.Log(damage);

        target.GetStat(EntitieStats.Hp).AddModifier(new StatModifier(damage + this.damage, StatModType.Flat));
    }

    public override void Update()
    {
        //particleEffect.transform.position = targe
    }
    public override void OnTick(IEntitie instanceTarget, IEntitie instanceOrigin)
    {
        


    }

    public override void Expired(IEntitie isntanceTarget, IEntitie instanceOrigin)
    {
        //GameEvents.current.playerWasAttacked -= reflectDamage;
    }

}
