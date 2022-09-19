using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


///Buffsystem oder StatusEffekt?
///StatusEffekt -> Buff -> Dornen

///Buffs could be an easy foundation for Boss-Spell or special Abilities used by Mobs aswell.

/// Rather Simple Buffs: 
/// Improved / Reduced Armor,
/// Double HP,
/// Increased AttackDamage,
/// Increased / Decreased Movement,
/// Increased / Decreased AttackSpeed,
/// Increased / Reduced AbilityPower,
/// Regenerating Health / Heal
/// 
/// 
/// More Complex Buffs:
/// Thorns,
/// Reflecting Damage,
/// Group Effects,
/// SpellVamp / Lifeleech
/// 
///Immer wenn TakeDamage gecalled wird, muss 'damage / 10' als TakeDamage auf die Origin der Klasse zur�ck gecalled werden.. irgendwie so.
///Wenn wir nicht in die TakeDamage Funktion eingreifen wollen, kann dies lediglich �ber das Event-System geschehen.



public class BuffInstance : Buff
{
    public string buffName;

    public float MyDuration;

    public IEntitie target;

    private Transform targetTransform;

    private float tickTimer;

    public float tick;

    //public float damage;

    public bool stackable;

    //Der StatModifier sollte in der geerbten Klasse definiert werden.
    //Er k�nnte sich auf das MovementSpeed ebenso auswirken, wie auf die HP!
    //Ergo liegt in der Klasse Buff lediglich eine Methode, welcher der entsprechende Stat mitgegeben wird.
    //public StatModifier mod;

    //public string statType;

    public GameObject particleEffect;

    public Sprite icon;

    private Buff originalBuff;

    public BuffInstance(Buff buff)
    {
        
        buffName = buff.buffName;
        MyDuration = buff.MyDuration;

        stackable = buff.stackable;

        particleEffect = buff.particleEffect;

        icon = buff.icon;

        originalBuff = buff;

        tick = buff.tickIntervall;
        

    }

    //BuffInstance wird mittels ApplyBuff und einer entsprechenden Entitie zugewiesen.
    //�ber die Entitie lassen sich alle Stats und Values ver�ndern.
    //Innerhalb der Entittie (MobStat / PlayerStat) wird der Buff einer Liste hinzugef�gt.
    public void ApplyBuff(IEntitie entitie)
    {
        //Die Entitie wird in der BuffInstance gespeichert.
        target = entitie;

        //Das TickIntervall wird �bernommen, so dass der Buff in dem im SO gespeicherten Intervall tickt.
        tickTimer = tick;

        //Pr�fe ob der Buff Stackable ist und f�ge die BuffInstanz der Entitie hinzu.
        entitie.ApplyBuff(this);

        //Speicher die Transform der Entitie. Relevant f�r Positionierung von Particel-Effekten.
        targetTransform = entitie.GetTransform();
    }

    public override void Activated(IEntitie target, Transform targetTransform)
    {
        originalBuff.Activated(target, targetTransform);
        Debug.Log(targetTransform.gameObject.name);

        if (originalBuff.particleEffect != null)
            Instantiate(originalBuff.particleEffect).transform.SetParent(targetTransform);
    }


    public virtual void Remove()
    {
        target.RemoveBuff(this);
    }

    public override void Expired(IEntitie target, Transform targetTransform)
    {
        originalBuff.Expired(target, targetTransform);
    }
    public override void Update()
    {           

        MyDuration -= Time.deltaTime;

        if (MyDuration >= 0)
        {
            CalculateTickers();
        }
        else
            Remove();

        Debug.Log("Current Duration: " + MyDuration);

        originalBuff.Update();
    }


    public override void OnTick(IEntitie target)
    {
        originalBuff.OnTick(target);
    }

    public void CalculateTickers()
    {
        Debug.Log("Tick:" + tick);
        tick -= Time.deltaTime;

        if (tick <= 0)
        {
            tick = tickTimer;

            Debug.Log(MyDuration + "duration");
            OnTick(target);
        }


    }

}


public abstract class Buff : ScriptableObject
{
    public string buffName;

    public float MyDuration;

    public float ProcChance;

    public float tickIntervall;

    public float tickTimer { get; set; }

    public bool stackable;

    [HideInInspector]
    public bool active = false;

    public GameObject particleEffect;

    public Sprite icon;

    public abstract void Activated(IEntitie target, Transform targetTransform);

    public abstract void Update();

    public abstract void Expired(IEntitie target, Transform targetTransform);

    public abstract void OnTick(IEntitie target);


}
