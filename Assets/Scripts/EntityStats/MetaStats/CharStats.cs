using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

[Serializable]
public class CharStats //: MobStats - Die Stats werden in der entsprechenden Klasse definiert. 
                       //Ein Mob könnte die Egenschaften CharStats Hp, Armor etc. erhalten, wie auch der Isometric Player.
{
    public float BaseValue;
    
    public float Value
    {
        get
        {
            if (isDirty || BaseValue !=lastBaseValue)
            {
                lastBaseValue = BaseValue;
                _value = CalculateFinalValue();
                isDirty = false;
            }
            return _value;
        }
    }

    private bool isDirty = true;
    private float _value;
    private float lastBaseValue = float.MinValue;

    private readonly List<StatModifier> statModifiers;
    public readonly ReadOnlyCollection<StatModifier> StatModifiers;

    public CharStats()
    {
        statModifiers = new List<StatModifier>();
        StatModifiers = statModifiers.AsReadOnly();
    }

    public CharStats(float baseValue) : this()
    {
        BaseValue = baseValue;

    }

    public void AddModifier(StatModifier mod)
    {
        isDirty = true;
        statModifiers.Add(mod);
        statModifiers.Sort(CompareModifierOrder);
        //print(mod);
    }

    private int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        if (a.Order < b.Order)
            return -1;
        else if (a.Order > b.Order)
            return 1;
        return 0;
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if(statModifiers.Remove(mod))
        {
            isDirty = true;
            return true;
        }
        return false;
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        bool didRemove = false;

        for (int i = statModifiers.Count - 1; i >= 0; i++)
        {
            if (statModifiers[i].Source == source)
            {
                isDirty = true;
                didRemove = true;
                statModifiers.RemoveAt(i);

            }
        }

        return didRemove;
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier mod = statModifiers[i];

            if (mod.Type == StatModType.Flat)
            {
                finalValue += mod.Value;
            }
            else if (mod.Type == StatModType.PercentAdd)
            {
                sumPercentAdd += mod.Value;

                if (i + 1 >=statModifiers.Count || statModifiers[i +1].Type != StatModType.PercentAdd)
                {
                    finalValue *= 1 + sumPercentAdd;
                    sumPercentAdd = 0;
                }
            }
            else if (mod.Type == StatModType.PercentMult)
            {
                finalValue *= 1 + mod.Value;
            }
            
        }

        return (float)Math.Round(finalValue, 4);
    }


    //Eigenes Tobabo, hier beginnt der Splash von eigenen Funktionen & Methoden, deren Sinn zunächst Fragwürdig sein könnte.
    

        /*
        private int xp;

        public int Xp
        {
            get { return xp; }
            private set
            {
                if (value < 0) xp = 0;
                else xp = value;
            }
        }
        */
        

}

