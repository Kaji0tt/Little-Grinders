using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Was muss die Ability-Klasse k�nnen?
//Mit der Ability-Klasse k�nnen wir ein SO erstellen, was f�r alle Abilities identisch ist.
// --> Namen, cooldownTime, activeTime, Sprite, ..
//Wir k�nnen jedoch nicht die Use-Methoden schreiben. Deshalb wird f�r diese Methoden eine Unterklasse erstellt. (AbilityTalent)

//Mehrere Stances!! - enum stance {void, utility, combat}
//Talent referiert zu einer Ability. 
//Follow Talent-Nodes erh�hen entsprechende stance-integers.
//Die Stance Integers sind die Base-Werte, mit denen in Activate multipliziert wird.


public class Ability : ScriptableObject
{
    //Standard Daten zu einer F�higkeit
    public string abilityID;
    public string abilityName;
    [TextArea]
    public string description;
    [Space]
    public float cooldownTime;
    public float activeTime;

    public Sprite image;
    public Sprite icon => image;

    //Spezialisierungs Enum
    public enum AbilityType { Void, Combat, Utility}; // Wird in Abh�ngigkeit vom Talent entschieden.

    public AbilityType abilityType;

    public enum AbilitySpecialization { VoidSpec, CombatSpec, UtilitySpec}

    public AbilitySpecialization abilitySpec;


    //Integers, welche sich erh�hen, sobald ein entsprechendes Talent erh�ht wird.
    [HideInInspector]
    public int voidCounter, combatCounter, utilityCounter;

    public int requiredTypePoints;

    //Wenn wir das hier
    public virtual void Use(GameObject entitie)
    {
        //Falls eine Spezialisierung der F�higkeit vorliegt, sollte die F�higkeit stets die entsprechenden Spezialisierungen.
        switch(abilitySpec)
        {
            case AbilitySpecialization.VoidSpec:
                VoidSpec();
                break;

            case AbilitySpecialization.CombatSpec:
                CombatSpec();
                break;

            case AbilitySpecialization.UtilitySpec:
                UtilitySpec();
                break;
        }
    }



    //Es folgen 
    public virtual void VoidSpec()
    {

    }

    public virtual void UtilitySpec()
    {

    }

    public virtual void CombatSpec()
    {

    }


    //Sobald der Spieler eine F�higkeit skill, gehen entsprechende Counter hoch.
    //Sobald diese Counter >= 0 sind, werden alle anderen Talente welche sich auf diese F�higkeit beziehen, gelocked.
    //Au�erdem sollte zus�tzlich daf�r gesorgt werden, das die entsprechenden Spec-Uses benutzt werden.
    public void SetSpec()
    {
        if (combatCounter >= 0)
            abilitySpec = AbilitySpecialization.CombatSpec;

        if (utilityCounter >= 0)
            abilitySpec = AbilitySpecialization.UtilitySpec;

        if (voidCounter >= 0)
            abilitySpec = AbilitySpecialization.VoidSpec;

        Debug.Log(abilitySpec);
    }
}
