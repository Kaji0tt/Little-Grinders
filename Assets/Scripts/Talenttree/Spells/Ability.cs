using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Was muss die Ability-Klasse können?
//Mit der Ability-Klasse können wir ein SO erstellen, was für alle Abilities identisch ist.
// --> Namen, cooldownTime, activeTime, Sprite, ..
//Wir können jedoch nicht die Use-Methoden schreiben. Deshalb wird für diese Methoden eine Unterklasse erstellt. (AbilityTalent)

//Mehrere Stances!! - enum stance {void, utility, combat}
//Talent referiert zu einer Ability. 
//Follow Talent-Nodes erhöhen entsprechende stance-integers.
//Die Stance Integers sind die Base-Werte, mit denen in Activate multipliziert wird.


public class Ability : ScriptableObject
{
    //Standard Daten zu einer Fähigkeit
    public string abilityID;
    public string abilityName;
    [TextArea]
    public string description;
    [Space]
    public float cooldownTime;
    public float activeTime;

    public Sprite image;
    public Sprite icon => image;

    /// <summary>
    /// Daten zum Typ der Fähigkeit.
    /// </summary>
    public enum AbilityType { Void, Combat, Utility}; // Wird in Abhängigkeit vom Talent entschieden.

    public AbilityType abilityType;

    public int requiredTypePoints;

    /// <summary>
    /// Daten zur Spezialisierung der Fähigkeit.
    /// </summary>
    public enum AbilitySpecialization { Undefined, Spec1, Spec2, Spec3}

    public AbilitySpecialization abilitySpec;




    //Wenn wir das hier
    public virtual void Use(GameObject entitie)
    {
        //Falls eine Spezialisierung der Fähigkeit vorliegt, sollte die Fähigkeit stets die entsprechenden Spezialisierungen ausführen.
        switch(abilitySpec)
        {
            case AbilitySpecialization.Spec1:
                VoidSpec();
                break;

            case AbilitySpecialization.Spec2:
                CombatSpec();
                break;

            case AbilitySpecialization.Spec3:
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


    //Sobald der Spieler eine Fähigkeit skillt, gehen entsprechende Counter hoch.
    //Sobald diese Counter >= 0 sind, werden alle anderen Talente welche sich auf diese Fähigkeit beziehen, gelocked.
    //Außerdem sollte zusätzlich dafür gesorgt werden, das die entsprechenden Spec-Uses benutzt werden.


    public void SetSpec(AbilitySpecialization specialization)
    {
        abilitySpec = specialization;



        Debug.Log(abilitySpec);
    }
}
