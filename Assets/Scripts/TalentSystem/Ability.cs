using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Was muss die Ability-Klasse k�nnen?
//Mit der Ability-Klasse k�nnen wir ein SO erstellen, was f�r alle Abilities identisch ist.
// --> Namen, cooldownTime, activeTime, Sprite, ..
//Wir k�nnen jedoch nicht die Use-Methoden schreiben. Deshalb wird f�r diese Methoden eine Unterklasse erstellt. (AbilityTalent)

/// <summary>
/// Die Ability sollte stets um alle ihrer Child-Talente wissen, damit die Ability mit dessen Skillpunkten arbeiten kann.
/// </summary>
/// 

//Currently Troubling mit: 
//Die Ability Klasse wei� nicht von der Instanz AbilityTalent-Klasse, welche sie tr�gt. Was w�re die sinnigste Methode, ihr von den entsprechenden Chil-Talenten der 
//jeweiligen Spezialisierung zu berichten?


//Abilitty w�re immenoch besser als SO, da damit feindliche MOBs die F�higkeiten verwenden k�nnen (Diese w�ren als SO leicht zuzuweisen.)
//Oder: Es gibt ein Script, welches per Singleton abrufbar ist, in welchem die F�higkeiten liegen. Problem w�re: Eine einzelne Instanz der Klasse h�ndelt alle F�higkeiten.
//                                                                                                 Oder ist das ein Problem?
//                                                                                                 In den Funktionen der Ability sollte demnach stets ein GO mitgegegeben werden, welches als Referenz f�r den Nutzer dient.
//
//GGf. k�nnte man die Ability Componente den Mobs / Spieler direkt geben, beim Call wird auf die Instanz der Klasse auf dem Spielobjekt zugegriffen.
public class Ability : MonoBehaviour
{
    //Standard Daten zu einer F�higkeit
    //public string abilityID;
    public string abilityName;
    [TextArea]
    public string description;
    [Space]
    public float cooldownTime;
    public float activeTime;

    [HideInInspector]
    public float tickTimer = 1;

    public Sprite image;
    public Sprite icon => image;

    /// <summary>
    /// Daten zum Typ der F�higkeit.
    /// </summary>
    public enum AbilityType { Void, Combat, Utility }; // Wird in Abh�ngigkeit vom Talent entschieden.

    public AbilityType abilityType;

    public int requiredTypePoints;

    /// <summary>
    /// Daten zur Spezialisierung der F�higkeit.
    /// </summary>
    public enum AbilitySpecialization { Undefined, Spec1, Spec2, Spec3 }

    public AbilitySpecialization abilitySpec;


    //Versuch, die Instanzen der Talente in einer Liste zu speichern -> Woher wei� das Ability, welches Talent wozu geh�rt? -> Theoretisch durch die Reihenfolge.
    public List<Talent> spec1Talents { get; private set; }
    public List<Talent> spec2Talents { get; private set; }
    public List<Talent> spec3Talents { get; private set; }


    private void Awake()
    {
        spec1Talents = new List<Talent>();
        spec2Talents = new List<Talent>();
        spec3Talents = new List<Talent>();
    }

    public virtual void UseBase(GameObject entitie)
    {
        //Falls eine Spezialisierung der F�higkeit vorliegt, sollte die F�higkeit stets zus�tzlich die entsprechenden Spezialisierungen ausf�hren.
        switch(abilitySpec)
        {
            case AbilitySpecialization.Spec1:
                OnUseSpec1(entitie);
                break;

            case AbilitySpecialization.Spec2:
                OnUseSpec2(entitie);
                break;

            case AbilitySpecialization.Spec3:
                OnUseSpec3(entitie);
                break;
        }
    }


    public virtual void CallAbilityFunctions(string action, GameObject entitie)
    {
        switch (abilitySpec)
        {
            case AbilitySpecialization.Spec1:
                if(action == "use")
                    OnUseSpec1(entitie);

                if(action == "tick")
                    OnTickSpec1(entitie);

                if (action == "cooldown")
                    OnCooldownSpec1(entitie);

                break;

            case AbilitySpecialization.Spec2:
                if (action == "use")
                    OnUseSpec2(entitie);

                if (action == "tick")
                    OnTickSpec2(entitie);

                if (action == "cooldown")
                    OnCooldownSpec2(entitie);
                break;

            case AbilitySpecialization.Spec3:
                if (action == "use")
                    OnUseSpec3(entitie);

                if (action == "tick")
                    OnTickSpec3(entitie);

                if (action == "cooldown")
                    OnCooldownSpec3(entitie);
                break;
        }
    }

    //Use Functions.
    public virtual void OnUseSpec1(GameObject entitie)
    {

    }

    public virtual void OnUseSpec2(GameObject entitie)
    {

    }

    public virtual void OnUseSpec3(GameObject entitie)
    {

    }


    //Tick Functions
    public virtual void OnTickSpec1(GameObject entitie)
    {

    }

    public virtual void OnTickSpec2(GameObject entitie)
    {

    }

    public virtual void OnTickSpec3(GameObject entitie)
    {

    }



    public virtual void OnCooldown(GameObject entitie)
    {

    }
    //On Cooldown Start Functions
    public virtual void OnCooldownSpec1(GameObject entitie)
    {
        // In case you'd want the entitie to be faster for the active time, you may add a StatModifier OnUse and remove it OnCooldown.
        
    }

    public virtual void OnCooldownSpec2(GameObject entitie)
    {

    }

    public virtual void OnCooldownSpec3(GameObject entitie)
    {

    }


    //Sobald der Spieler eine F�higkeit skillt, gehen entsprechende Counter hoch.
    //Sobald diese Counter >= 0 sind, werden alle anderen Talente welche sich auf diese F�higkeit beziehen, gelocked.
    //Au�erdem sollte zus�tzlich daf�r gesorgt werden, das die entsprechenden Spec-Uses benutzt werden.

    //Wird gecalled, sobald erfolgreich ein Talent, welches keine F�higkeit ist, geskilled wird.
    public void SetSpec(AbilitySpecialization specialization)
    {
        //Setze die Spezialisierung der F�higkeit auf jene des geklickten Talents, falls diese noch nicht gesetzt wurde.
        if(abilitySpec == AbilitySpecialization.Undefined)
        abilitySpec = specialization;

        //Debug.Log(spec1Counter + spec2Counter + spec3Counter);
    }



    public void SetSpec1Talent(Talent talent)
    {
        spec1Talents.Add(talent);
    }

    public void SetSpec2Talent(Talent talent)
    {
        spec2Talents.Add(talent);
    }

    public void SetSpec3Talent(Talent talent)
    {
        spec3Talents.Add(talent);
    }
}
