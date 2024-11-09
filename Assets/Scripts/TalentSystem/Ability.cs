using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Was muss die Ability-Klasse können?
//Mit der Ability-Klasse können wir ein SO erstellen, was für alle Abilities identisch ist.
// --> Namen, cooldownTime, activeTime, Sprite, ..
//Wir können jedoch nicht die Use-Methoden schreiben. Deshalb wird für diese Methoden eine Unterklasse erstellt. (AbilityTalent)

/// <summary>
/// Die Ability sollte stets um alle ihrer Child-Talente wissen, damit die Ability mit dessen Skillpunkten arbeiten kann.
/// </summary>
/// 

//Currently Troubling mit: 
//Die Ability Klasse weiß nicht von der Instanz AbilityTalent-Klasse, welche sie trägt. Was wäre die sinnigste Methode, ihr von den entsprechenden Chil-Talenten der 
//jeweiligen Spezialisierung zu berichten?


//Abilitty wäre immenoch besser als SO, da damit feindliche MOBs die Fähigkeiten verwenden können (Diese wären als SO leicht zuzuweisen.)
//Oder: Es gibt ein Script, welches per Singleton abrufbar ist, in welchem die Fähigkeiten liegen. Problem wäre: Eine einzelne Instanz der Klasse händelt alle FÄhigkeiten.
//                                                                                                 Oder ist das ein Problem?
//                                                                                                 In den Funktionen der Ability sollte demnach stets ein GO mitgegegeben werden, welches als Referenz für den Nutzer dient.
//
//GGf. könnte man die Ability Componente den Mobs / Spieler direkt geben, beim Call wird auf die Instanz der Klasse auf dem Spielobjekt zugegriffen.
public abstract class Ability : MonoBehaviour, IMoveable, IUseable, IDragHandler, IEndDragHandler
{
    //Standard Daten zu einer Fähigkeit
    //public string abilityID;
    public string abilityName;
    [TextArea]
    public string description;
    [Space]
    public float cooldownTime;
    private float runtimeCooldownTime;

    public float activeTime;
    private float runtimeActive;


    float tickerTimer;

    [HideInInspector]
    public float tickTimer = 1;

    public Sprite image;
    public Sprite icon => image;

    /// <summary>
    /// Daten zum Typ der Fähigkeit.
    /// </summary>
    public enum AbilityType { Void, Utility, Combat }; // Wird in Abhängigkeit vom Talent entschieden.

    public AbilityType abilityType;

    public int requiredTypePoints;



    /// <summary>
    /// Daten zur Spezialisierung der Fähigkeit.
    /// </summary>
    public enum AbilitySpecialization { Undefined, Spec1, Spec2, Spec3 }

    public AbilitySpecialization abilitySpec;


    //Versuch, die Instanzen der Talente in einer Liste zu speichern -> Woher weiß das Ability, welches Talent wozu gehört? -> Theoretisch durch die Reihenfolge.
    public List<Talent> spec1Talents = new List<Talent>();
    public List<Talent> spec2Talents = new List<Talent>();
    public List<Talent> spec3Talents = new List<Talent>();




    #region Abilitiy Stuff


    //Ggf. sollten States in einer anderen Klasse / einem Manager reguliert werden?
    private enum AbilityState { ready, active, cooldown };
    AbilityState state = AbilityState.ready;

    void Start()
    {
        runtimeActive = activeTime;
        runtimeCooldownTime = cooldownTime;
    }

    // Update is called once per frame
    void Update()
    {



        if (state == AbilityState.active)
        {

            if (runtimeActive > 0)
            {
                runtimeActive -= Time.deltaTime;


                tickerTimer -= Time.deltaTime;

                if (tickerTimer <= 0)
                {
                    CallAbilityFunctions("tick", PlayerManager.instance.player.GetComponent<PlayerStats>());
                    tickerTimer = tickTimer;
                }
            }


            else
            {
                OnCooldown(PlayerManager.instance.player.GetComponent<PlayerStats>());
                CallAbilityFunctions("cooldown", PlayerManager.instance.player.GetComponent<PlayerStats>());

                state = AbilityState.cooldown;
                runtimeActive = activeTime;
            }

        }

        //During Cooldown State, reduce the Cooldown Time
        if (state == AbilityState.cooldown)
        {
            //print("ability is on cooldown" + cooldownTime);
            if (cooldownTime > 0)
            {
                cooldownTime -= Time.deltaTime;

                //image.color = Color.grey;
            }

            else
            {
                state = AbilityState.ready;

                cooldownTime = cooldownTime;

                //image.color = Color.white;
            }

        }



    }



    public void Use()
    {
        //Setzen der Spezialisierungspunkte in der Ability.

        if (state == AbilityState.ready)
        {

            //tickerTimer = tickTimer;


            //activeTime = activeTime;


            state = AbilityState.active;


            UseBase(PlayerManager.instance.player.GetComponent<PlayerStats>());
        }

    }
    #endregion


    private void OnEnable()
    {
    }

    public virtual void UseBase(IEntitie entitie)
    {
        //Falls eine Spezialisierung der Fähigkeit vorliegt, sollte die Fähigkeit stets zusätzlich die entsprechenden Spezialisierungen ausführen.
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


    public virtual void CallAbilityFunctions(string action, IEntitie entitie)
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
    public abstract void OnUseSpec1(IEntitie entitie);


    public abstract void OnUseSpec2(IEntitie entitie);


    public abstract void OnUseSpec3(IEntitie entitie);
 


    //Tick Functions - Should rather be Managed mit dem Buff-System.
    public abstract void OnTickSpec1(IEntitie entitie);

    public abstract void OnTickSpec2(IEntitie entitie);

    public abstract void OnTickSpec3(IEntitie entitie);



    public virtual void OnCooldown(IEntitie entitie)
    {

    }

    //On Cooldown Start Functions
    public abstract void OnCooldownSpec1(IEntitie entitie);

    // In case you'd want the entitie to be faster for the active time, you may add a StatModifier OnUse and remove it OnCooldown.
    // (This might be done with Buffs aswell though)

    public abstract void OnCooldownSpec2(IEntitie entitie);

    public abstract void OnCooldownSpec3(IEntitie entitie);



    //Sobald der Spieler eine Fähigkeit skillt, gehen entsprechende Counter hoch.
    //Sobald diese Counter >= 0 sind, werden alle anderen Talente welche sich auf diese Fähigkeit beziehen, gelocked.
    //Außerdem sollte zusätzlich dafür gesorgt werden, das die entsprechenden Spec-Uses benutzt werden.

    //Wird gecalled, sobald erfolgreich ein Talent, welches keine Fähigkeit ist, geskilled wird.
    public void SetSpec(AbilitySpecialization specialization)
    {
        //Setze die Spezialisierung der Fähigkeit auf jene des geklickten Talents, falls diese noch nicht gesetzt wurde.
        if(abilitySpec == AbilitySpecialization.Undefined)
        abilitySpec = specialization;


        //Debug.Log(spec1Counter + spec2Counter + spec3Counter);
    }

    //Dieso Funktionen werden gecalled, sobald ein Talent, welches nicht passiv ist, angeklickt und erfolgreich geskilled wurde.
    //Somit lassen sich die Boni (e.g. Range + 1) vor der Anwendung der Fähigkeit bereits setzen.
    public abstract void ApplySpec1Bonus(Talent t);
    public abstract void ApplySpec2Bonus(Talent t);
    public abstract void ApplySpec3Bonus(Talent t);

    public void AddSpec1TalentsToList(Talent talent)
    {
        spec1Talents.Add(talent);
    }

    public void AddSpec2TalentsToList(Talent talent)
    {
        spec2Talents.Add(talent);
    }

    public void AddSpec3TalentsToList(Talent talent)
    {
        spec3Talents.Add(talent);
    }

    public string GetName()
    {
        return abilityName;
    }

    public bool IsOnCooldown()
    {
        if (state == AbilityState.cooldown)
            return true;
        else
            return false;
    }

    public float GetCooldown()
    {
        return cooldownTime;
    }

    public float CooldownTimer()
    {
        return cooldownTime;
    }

    public bool IsActive()
    {
        if (state == AbilityState.active)
            return true;
        else
            return false;
    }

    public void OnDrag(PointerEventData eventData)
    {
            HandScript.instance.TakeMoveable(this);
    }


    // Diese Methode wird ausgeführt, wenn das Ziehen des Objekts endet
    public void OnEndDrag(PointerEventData eventData)
    {

        // Liste für die Raycast-Ergebnisse
        List<RaycastResult> results = new List<RaycastResult>();

        // Raycast durchführen und Ergebnisse sammeln
        EventSystem.current.RaycastAll(eventData, results);

        // Ergebnisse überprüfen
        foreach (RaycastResult result in results)
        {
            ActionButton actionButton = result.gameObject.GetComponent<ActionButton>();
            if (actionButton != null)
            {
                // SetUseable aufrufen, wenn ActionButton gefunden wird
                actionButton.SetUseable(this);
                Debug.Log("Item dropped onto ActionButton, SetUseable called.");
                return;
            }
        }


    }

}














public class AbilityTemplate : Ability
{
    /// <summary> --> AI SUMMARY <---
    /// This Summary serves as Information for AI:
    /// Player Data:
    /// PlayerManager.instance.player as Reference for the Player GameObject for e.g. Transform.
    /// PlayerManager.instance.player.GetComponent<PlayerStats>(); is reference for the different Stats of the player.
    /// These include floats as of: 
    /// AbilityPower, Armor, AttackPower, AttackSpeed, Hp (Health), MovementSpeed
    /// theire Reference is fetched via: float aSpeed = pStats.AttackSpeed.Value;
    /// </summary>
    /// <param name="entitie"></param>

    #region UseRegion
    public override void UseBase(IEntitie entitie)
    {

        base.UseBase(entitie);


    }


    public override void ApplySpec1Bonus(Talent t)
    {


    }


    public override void OnUseSpec1(IEntitie entitie)
    {


    }

    public override void ApplySpec2Bonus(Talent t)
    {

    }
    public override void OnUseSpec2(IEntitie entitie)
    {

    }


    public override void ApplySpec3Bonus(Talent t)
    {

    }
    public override void OnUseSpec3(IEntitie entitie)
    {

    }

    #endregion


    #region TickRegion

    //Ein TickTimer, welcher alle x Sekunden während der Aktiven Zeit ausgeführt wird. Standard tickTimer ist auf 1 - also alle 1 Sekunden.
    public override void OnTickSpec1(IEntitie entitie)
    {
 

    }

    public override void OnTickSpec2(IEntitie entitie)
    {
 

    }

    public override void OnTickSpec3(IEntitie entitie)
    {
        //throw new NotImplementedException();
    }

    #endregion


    #region CooldownRegion
    public override void OnCooldown(IEntitie entitie)
    {

    }

    public override void OnCooldownSpec1(IEntitie entitie)
    {

    }

    public override void OnCooldownSpec2(IEntitie entitie)
    {

    }

    public override void OnCooldownSpec3(IEntitie entitie)
    {

    }
    #endregion
}


