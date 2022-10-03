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
///Immer wenn TakeDamage gecalled wird, muss 'damage / 10' als TakeDamage auf die Origin der Klasse zurück gecalled werden.. irgendwie so.
///Wenn wir nicht in die TakeDamage Funktion eingreifen wollen, kann dies lediglich über das Event-System geschehen.



public class BuffInstance : Buff, IDescribable
{
    public string buffName;

    public float MyDuration;

    public float MyBaseDamage;

    public IEntitie MyTargetEntitie;

    //Lediglich die BuffInstance kann von der Origin Wissen, ansonsten werden die Scriptable Objects persistent überwiesen.
    //
    public IEntitie MyOriginEntitie { get; set; }

    private Transform MyTargetTransform;

    private float MyTickTimer;

    public float tick;

    //public float damage;

    public bool stackable;

    private int MyStacks;

    public string MyDescription { get; set; }



    //Der StatModifier sollte in der geerbten Klasse definiert werden.
    //Er könnte sich auf das MovementSpeed ebenso auswirken, wie auf die HP!
    //Ergo liegt in der Klasse Buff lediglich eine Methode, welcher der entsprechende Stat mitgegeben wird.
    //public StatModifier mod;

    //public string statType;

    public GameObject particleEffect;

    public Sprite icon;

    public Buff originalBuff;

    public BuffInstance(Buff buff)
    {
        
        
        buffName = buff.buffName;
        MyDuration = buff.MyDuration;

        stackable = buff.stackable;

        particleEffect = buff.particleEffect;

        icon = buff.icon;

        tick = buff.tickIntervall;

        MyBaseDamage = buff.MyBaseDamage;
        //If you'd like to acces certain values, which not the original Buff but the 
        //Scriptable Version of it (e.g. LifePoison), refert to the original Buff and use its values.
        //Don't change the values so! Changes to SO's are permanent!
        originalBuff = buff;

        MyDescription = buff.MyDescription;


    }

    //BuffInstance wird mittels ApplyBuff und einer entsprechenden Entitie zugewiesen.
    //Über die Entitie lassen sich alle Stats und Values verändern.
    //Innerhalb der Entittie (MobStat / PlayerStat) wird der Buff einer Liste hinzugefügt.
    public void ApplyBuff(IEntitie myTargetEntitie, IEntitie myOriginEntititie)
    {
        //Die Entitie wird in der BuffInstance gespeichert.
        MyTargetEntitie = myTargetEntitie;

        //Das TickIntervall wird übernommen, so dass der Buff in dem im SO gespeicherten Intervall tickt.
        MyTickTimer = tick;

        MyOriginEntitie = myOriginEntititie;

        //Prüfe ob der Buff Stackable ist und füge die BuffInstanz der Entitie hinzu.
        myTargetEntitie.ApplyBuff(this);

        //Speicher die Transform der Entitie. Relevant für Positionierung von Particel-Effekten.
        MyTargetTransform = myTargetEntitie.GetTransform();
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
        MyTargetEntitie.RemoveBuff(this);
    }

    public override void Expired(IEntitie instanceTarget, IEntitie instanceOrigin)
    {
        originalBuff.Expired(instanceTarget, instanceOrigin);
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

        //Debug.Log("Current Duration: " + MyDuration);

        originalBuff.Update();
    }


    public override void OnTick(IEntitie instanceTarget, IEntitie instanceOrigin)
    {
        originalBuff.OnTick(MyTargetEntitie, MyOriginEntitie);
    }

    public void CalculateTickers()
    {
        //Debug.Log("Tick:" + tick);
        tick -= Time.deltaTime;

        if (tick <= 0)
        {
            tick = MyTickTimer;

            //Debug.Log("Cast Tick on " + MyTargetEntitie.GetTransform().name + " which originated from " + MyOriginEntitie.GetTransform().name );
            OnTick(MyTargetEntitie, MyOriginEntitie);
        }


    }

    public string GetDescription()
    {
        return MyDescription;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(GetDescription());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
}


public abstract class Buff : ScriptableObject
{
    public string buffName;

    public float MyDuration;

    public float MyBaseDamage;

    public float ProcChance;

    public float tickIntervall;

    public float tickTimer { get; set; }

    public bool stackable;

    [HideInInspector]
    public bool active = false;

    public GameObject particleEffect;

    public Sprite icon;

    [TextArea]
    public string MyDescription;

    public abstract void Activated(IEntitie target, Transform targetTransform);

    public abstract void Update();

    public abstract void Expired(IEntitie instanceTarget, IEntitie instanceOrigin);

    public abstract void OnTick(IEntitie instanceTarget, IEntitie instanceOrigin);


}
