using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(menuName = "Buff/Poison")]
public class Poison : Buff
{
    public float damage;

    //private Transform targetTransform;
    //public IEntitie target;

    public override void Activated(IEntitie target, Transform targetTransform)
    {

    }



    public override void Update()
    {
        //Debug.Log("Original Buff Update is being called.");
        if(active)
        {
            //Debug.Log("Posion is currently active.");
        }
    }
    public override void OnTick(IEntitie target)
    {
        
       target.TakeDirectDamage(damage, 0);  
        
    }

    public override void Expired(IEntitie target, Transform targetTransform)
    {
        //Debug.Log("Poison Expired.");
    }


}
