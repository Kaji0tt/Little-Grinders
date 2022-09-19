using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buff/Reflection")]
public class Reflection : Buff
{
    public float damage;

    public override void Activated(IEntitie target, Transform targetTransform)
    {
        //Wenn Reflection aktiviert wird, füge den Listener hinzu, dass ReflectDamage gecalled wird, sobald der Spieler schaden erleidet.
        //GameEvents.current.playerWasAttacked += reflectDamage;

        Debug.Log("reflection applied");

    }

    private void reflectDamage(IEntitie target, float damage)
    {
        Debug.Log(damage);

        target.AddNewStatModifier(EntitieStats.Hp, new StatModifier(damage + this.damage, StatModType.Flat));
    }

    public override void Update()
    {
        //particleEffect.transform.position = targe
    }
    public override void OnTick(IEntitie target)
    {



    }

    public override void Expired(IEntitie target, Transform targetTransform)
    {
        //GameEvents.current.playerWasAttacked -= reflectDamage;
    }

}
