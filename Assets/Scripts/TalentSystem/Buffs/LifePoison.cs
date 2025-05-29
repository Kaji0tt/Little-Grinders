using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buff/LifePoison")]
public class LifePoison : Buff
{

    //public float damage;
    public float percentLifeDamage;

    //public float baseDamage;

    //private Transform targetT;
    //private Transform targetTransform;
    //public IEntitie target;



    public override void Activated(IEntitie target, Transform targetTransform)
    {


        /* The Idea to make a Buff stackable only a certain number of times.
         * Not working yet tho.
        List<BuffInstance> result = target.GetBuffs().FindAll(x => x.buffName == this.buffName);
        Debug.Log(result.Count);
        if (result.Count >= 5)
            target.RemoveBuff(target.GetBuffs().Find(x => x.buffName == this.buffName));
        */


    }



    public override void Update()
    {
        //Debug.Log("Original Buff Update is being called.");
        if (active)
        {
            /*
            foreach(BuffInstance activeBuff in targetE.GetBuffs())
            {
                if(activeBuff.buffName == buffName)

            }
            */
            //Debug.Log("Posion is currently active.");
        }
    }
    public override void OnTick(IEntitie instanceTarget, IEntitie instanceOrigin)
    {

        //PlayerMaxHP / 
        float lifeDamage = instanceOrigin.Get_maxHp() * percentLifeDamage;
        float apDamage = instanceOrigin.GetStat(EntitieStats.AbilityPower).Value / 100 * 2;

        float totalDamage = MyBaseDamage + lifeDamage + apDamage;
        Debug.Log("Ticking for " + MyBaseDamage + " and " + lifeDamage + " lifeDamage and " + apDamage + " apDamage. So in total:" + totalDamage);
        //instanceTarget.TakeDirectDamage(MyBaseDamage + lifeDamage + apDamage, 900);

    }

    public override void Expired(IEntitie instanceTarget, IEntitie instanceOrigin)
    {
        Debug.Log("Life Poison Expired on " + instanceTarget.GetTransform().gameObject.name);
    }


}
