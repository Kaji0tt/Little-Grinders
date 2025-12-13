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



public class BuffInstance : Buff, IDescribable
{


    public new string buffName;

    public new float MyDuration;

    public new float MyBaseDamage;

    public IEntitie MyTargetEntitie;

    //Lediglich die BuffInstance kann von der Origin Wissen, ansonsten werden die Scriptable Objects persistent �berwiesen.
    //
    public IEntitie MyOriginEntitie { get; set; }

    private Transform MyTargetTransform;

    private float MyTickTimer;

    public float tick;

    //public float damage;

    public new bool stackable;

    private int MyStacks;

    public new string MyDescription { get; set; }

    // ✅ KONZEPT 1: Instanzspezifische Buff-Daten (für SlowDebuff, Poison, etc.)
    // Jede BuffInstance hält ihre eigenen Daten, kein Dictionary im ScriptableObject nötig
    private Dictionary<string, object> instanceData = new Dictionary<string, object>();

    /// <summary>
    /// Speichert instanzspezifische Daten für diese BuffInstance
    /// </summary>
    public void SetInstanceData(string key, object value)
    {
        // ✅ Lazy Initialization - falls Dictionary noch nicht existiert
        if (instanceData == null)
            instanceData = new Dictionary<string, object>();
            
        instanceData[key] = value;
    }

    /// <summary>
    /// Holt instanzspezifische Daten für diese BuffInstance
    /// </summary>
    public T GetInstanceData<T>(string key, T defaultValue = default(T))
    {
        // ✅ Lazy Initialization
        if (instanceData == null)
            instanceData = new Dictionary<string, object>();
            
        if (instanceData.ContainsKey(key))
            return (T)instanceData[key];
        return defaultValue;
    }

    /// <summary>
    /// Prüft ob instanzspezifische Daten vorhanden sind
    /// </summary>
    public bool HasInstanceData(string key)
    {
        // ✅ Lazy Initialization
        if (instanceData == null)
            instanceData = new Dictionary<string, object>();
            
        return instanceData.ContainsKey(key);
    }

    //Der StatModifier sollte in der geerbten Klasse definiert werden.
    //Er k�nnte sich auf das MovementSpeed ebenso auswirken, wie auf die HP!
    //Ergo liegt in der Klasse Buff lediglich eine Methode, welcher der entsprechende Stat mitgegeben wird.
    //public StatModifier mod;

    //public string statType;

    public GameObject particleEffect;

    public Sprite icon;

    public Buff originalBuff;

    public BuffInstance(Buff buff)
    {       
        // ✅ Initialisiere instanceData Dictionary beim Konstruktor
        instanceData = new Dictionary<string, object>();
        
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
    //�ber die Entitie lassen sich alle Stats und Values ver�ndern.
    //Innerhalb der Entittie (MobStat / PlayerStat) wird der Buff einer Liste hinzugef�gt.
    public void ApplyBuff(IEntitie myTargetEntitie, IEntitie myOriginEntititie)
    {
        //Die Entitie wird in der BuffInstance gespeichert.
        MyTargetEntitie = myTargetEntitie;

        //Das TickIntervall wird �bernommen, so dass der Buff in dem im SO gespeicherten Intervall tickt.
        MyTickTimer = tick;

        MyOriginEntitie = myOriginEntititie;

        //Pr�fe ob der Buff Stackable ist und f�ge die BuffInstanz der Entitie hinzu.
        myTargetEntitie.ApplyBuff(this);

        //Speicher die Transform der Entitie. Relevant f�r Positionierung von Particel-Effekten.
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
        Debug.Log($"[BuffInstance] Remove() wird aufgerufen für {buffName} auf {MyTargetEntitie.GetTransform().name}");
        MyTargetEntitie.RemoveBuff(this);
    }

    public override void Expired(IEntitie instanceTarget, IEntitie instanceOrigin)
    {
        Debug.Log($"[BuffInstance] Expired() wird aufgerufen für {buffName} - Delegiere zu originalBuff.Expired()");
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
        {
            Debug.Log($"[BuffInstance] {buffName} ist abgelaufen! MyDuration: {MyDuration:F2} - Rufe Remove() auf...");
            Remove();
        }

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

    /// <summary>
    /// Kopiert Buff-spezifische Logik in eine BuffInstance.
    /// Wird überschrieben von abgeleiteten Klassen (z.B. SlowDebuff, Poison, etc.)
    /// </summary>
    public virtual void CopyFrom(Buff source)
    {
        // Basis-Implementierung: Nichts zu tun
        // Wird von SlowDebuff, Poison, etc. überschrieben
    }


}
