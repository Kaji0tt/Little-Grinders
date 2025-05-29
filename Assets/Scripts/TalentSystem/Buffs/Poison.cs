using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Buff/Poison")]
public class Poison : Buff
{
    public float MyTickDamage;

    //private Transform targetTransform;
    //public IEntitie target;

    public override void Activated(IEntitie instanceTarget, Transform targetTransform)
    {
        //instanceTarget.TakeDirectDamage(MyBaseDamage, 0);
    }



    public override void Update()
    {
        //Debug.Log("Original Buff Update is being called.");
        if(active)
        {
            Debug.Log("Posion is currently active.");
        }
    }
    public override void OnTick(IEntitie instanceTarget, IEntitie instanceOrigin)
    {
        
       //instanceTarget.TakeDirectDamage(MyTickDamage, 0);
        Instantiate(particleEffect, PlayerManager.instance.player.transform);
        
    }

    public override void Expired(IEntitie instanceTarget, IEntitie instanceOrigin)
    {
        //Debug.Log("Poison Expired.");
    }


}
