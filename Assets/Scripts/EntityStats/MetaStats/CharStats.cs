using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class CharStats : MobStats
{
    //BaseValue ist im Prinzip der Value, welcher von MobStats geerbt wurde und in Unity eingestellt wurde.
    //Erklär das mal dem Programm.
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

    public CharStats(float baseValue)
    {
        BaseValue = baseValue;
        statModifiers = new List<StatModifier>();
        StatModifiers = statModifiers.AsReadOnly();
    }

    public void AddModifier(StatModifier mod)
    {
        isDirty = true;
        statModifiers.Add(mod);
        statModifiers.Sort(CompareModifierOrder);
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




    // Start is called before the first frame update
    void Start()
    {
        // Nach dem Tutorial von Kryzarel, hier die entsprechenden Randomrolls anwenden?
        // Oder: Hier die entsprechenden Randomrolls von Item als StatModifier kalkulieren. Ich hätte die Modifier lieber in der Itemklasse. 
        // Allerdings muss hier die Kalkulation losgetreten werden, denke ich.

        //Dem Beispiel folgend:

    }

    // Update is called once per frame
    void Update()
    {

    }
}

