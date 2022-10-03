using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Buff/WeakArmor")]
public class WeakArmor : Buff
{

    //public float damage;
    private float amountArmorReduce;

    StatModifier armorReductionStat;
    //public float baseDamage;

    //private Transform targetT;
    //private Transform targetTransform;
    //public IEntitie target;



    public override void Activated(IEntitie instanceTarget, Transform targetTransform)
    {

        amountArmorReduce = instanceTarget.GetStat(EntitieStats.Armor).BaseValue / 100 * 5;

        armorReductionStat = new StatModifier(-amountArmorReduce, StatModType.PercentAdd);

        instanceTarget.GetStat(EntitieStats.Armor).AddModifier(armorReductionStat);

    }



    public override void Update()
    {
        //Debug.Log("Original Buff Update is being called.");
        if (active)
        {

            //target.AddNewStatModifier(EntitieStats.Armor, armorReduction);
        }
    }
    public override void OnTick(IEntitie instanceTarget, IEntitie instanceOrigin)
    {

    }

    public override void Expired(IEntitie instanceTarget, IEntitie instanceOrigin)
    {
        instanceTarget.GetStat(EntitieStats.Armor).RemoveModifier(armorReductionStat);
    }


}
