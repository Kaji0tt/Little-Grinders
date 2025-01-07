using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//Jedes Talent sollte sich auf eine Ability beziehen. 
//In der Ability, werden in Abhängigkeit von den gewählten Talente Werte geändert, welche die Ability und ihre Spezialisierung beeinflussen.
//Alle Informationen über mögliche Ausrichtungen von Abilities, liegen somit in jeweils einzelnen Ability-Scriptable Objects.

//Die Base-Ability (derzeit Heilung) sollte kein SO sein, sondern von Monobehaviour geerbt werden. (Derzeit stellt Spell diese Erbschaft dar.)
//Das wäre vergleichbar zum Ability-Holder im Video: https://www.youtube.com/watch?v=ry4I6QyPw4E

//Damit wäre ein Talent der entsprechende Ability Holder - wenn ein Talent geskilled wird, werden in Abhängigkeit von TalentType die AbilityType Integers erhöht.
public class Talent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    //bool zum überprüfen, welche Spezialisierungspunkte dem TalenTree hinzugefügt werden sollen.
    //public bool voidTalent, combatTalent, utilityTalent;

    //Versuch den Typ in einem Enum zu verpacken wegen übersichtlich.
    public Ability.AbilityType abilityType;

    //Enum Referenz zum Abgleich, ob die Spezialisierung des Talents jener der Base-Ability entspricht.
    public Ability.AbilitySpecialization abilitySpecialization;


    public Ability baseAbility;

    //public AbilityTalent abilityTalent;

    //Description of the Spell
    [SerializeField]
    [TextArea]
    private string description;

    //Der talentName wird zum abgleich der Talente beim Laden des spiels verwendet und dient der Anzeige im interface.
    public string talentName;

    public bool passive;

    public string GetDescription
    {
        get
        {
            return description;
        }
    }
    
    public virtual void SetDescription(string newDes)
    {
        this.description = newDes;
    }
    
    private Image image;
    
    public Sprite icon
    {
        get
        {
            return baseAbility.icon;
        }
    }
    

    private Text textComponent;

    [SerializeField]
    private int maxCount;
    public int currentCount { get; private set; }

    public void Set_currentCount(int newCount)
    {
        currentCount = newCount;
    }

    public bool unlocked;

    //[SerializeField]
    public Talent[] childTalent;

    //Wie viele Punkte in TalentType sind nötig, für dieses Talent?
    public int requiredTypePoints;

    private int collectedTypePoints;

    //Die Weiterleitung zu anderen Talenten macht er in Abhängig von "Pfeilen", also wenn ein PfeilSprite auf eine andere Fähigkeit zeigt, bzw. sie einen Pfeil besitzt, dann wird das Folgetalent freigeschaltet.
    //Kp ob ich das so machen wollen würde, entsprechendes Video hier:
    // https://youtu.be/NEqaBBnAFfM?t=406

    //Drag and Drop Magie
    //https://youtu.be/ILaDr3CE7QY?t=615


    public string GetName()
    {
            return talentName;
    }

    //CheckForUnlock soll überprüfen, ob im Talenbaum ausreichend Talentpunkte gesammelt wurden, damit das entsprechende Talent freigeschaltet werden kann.
    public void CheckForUnlock()
    {
        //In Abhängigkeit vom Typ der Fähigkeit, wird der Talentbaum auf eine entsprchend ausreichende Anzahl von Spezialisierungspunkten überprüft.
        switch (baseAbility.abilityType)
        {
            case Ability.AbilityType.Combat:
                if (TalentTree.instance.totalCombatSpecPoints >= baseAbility.requiredTypePoints)
                    Unlock();
                break;

            case Ability.AbilityType.Void:
                if (TalentTree.instance.totalVoidSpecPoints >= baseAbility.requiredTypePoints)
                    Unlock();
                break;

            case Ability.AbilityType.Utility:
                if (TalentTree.instance.totalUtilitySpecPoints >= baseAbility.requiredTypePoints)
                    Unlock();
                break;

        }
    }
    //AbilityTalent Merge- hinzugefügt 28.10
    private void RunThroughChildTalents(Talent talent)
    {
        //Und füge dieses der entsprechenden Spezialisierungsliste hinzu. (Spec1 / Spec2 / Spec3)
        AddSpecializationTalents(talent);
        if (talent.childTalent.Length > 0)
        {
            foreach (Talent childTalent in talent.childTalent)
            {
                RunThroughChildTalents(childTalent);
            }


        }
    }

    //AbilityTalent Merge- hinzugefügt 28.10
    private void AddSpecializationTalents(Talent childTalent)
    {
        if (!childTalent.passive)
            switch (childTalent.abilitySpecialization)
            {
                case Ability.AbilitySpecialization.Spec1:
                    baseAbility.AddSpec1TalentsToList(childTalent);
                    break;

                case Ability.AbilitySpecialization.Spec2:
                    baseAbility.AddSpec2TalentsToList(childTalent);
                    break;

                case Ability.AbilitySpecialization.Spec3:
                    baseAbility.AddSpec3TalentsToList(childTalent);
                    break;

            }
    }

    internal void ApplySpecialization(Talent talent)
    {
        if (talent.baseAbility != null)
        {
            switch (talent.abilitySpecialization)
            {
                case Ability.AbilitySpecialization.Spec1:
                    //Debug.Log(talent.talentName + " soll auf spec1 geskilled werden.");
                    baseAbility.ApplySpec1Bonus(talent);
                    break;

                case Ability.AbilitySpecialization.Spec2:
                    baseAbility.ApplySpec2Bonus(talent);
                    break;

                case Ability.AbilitySpecialization.Spec3:
                    baseAbility.ApplySpec3Bonus(talent);
                    break;

            }
        }
        else
            Debug.Log("Das geklickte Talent hat keine Basisfähigkeit hinterlegt, oder diese ist null");



    }

    //Void to be called in TalentTree.cs for structural purpose.
    public void SetTalentUIVariables()
    {
        image = GetComponent<Image>();

        // Hole alle direkten Kind-GameObjects
        foreach (Transform child in transform)
        {
            // Versuche, die Text-Komponente im Kind-GameObject zu finden
            textComponent = child.GetComponent<Text>();

            if (textComponent != null)
            {
                // Hier kannst du mit der gefundenen Text-Komponente arbeiten
                textComponent.text = $"{currentCount}/{maxCount}";
            }
        }
    }

    private void Awake()
    {
        //DoubleCheck if an AbilityTalent has been choosen, to make sure there are no problems within the editor.
        if (!baseAbility)
            print("Das Talent " + gameObject.name + " hat keine direkte Fähigkeit auf die es sich bezieht.");


        if (description == null)
            SetDescription("Keine Beschreibung angegeben, Basisfähigkeitsbeschreibng: \n" + baseAbility.description);

        if (talentName == null)
        {
            if (baseAbility)
                talentName = new string("Specialization of " + baseAbility.GetName());
            else
                talentName = new string("Talentname not set.");
        }




    }

    public bool Click()
    {


        textComponent = transform.GetComponentInChildren<Text>();
        if (currentCount < maxCount && unlocked)
        {
            currentCount++;

            textComponent.text = $"{currentCount}/{maxCount}";

            if (currentCount == maxCount)
            {
                for (int i = 0; i < childTalent.Length; i++)
                {
                    if (childTalent[i] != null)
                        childTalent[i].Unlock();

                }
            }
            return true;
        }
        return false;
    }



    public void LockTalent()
    {
        image.color = Color.grey;

        foreach (Transform child in transform)
        {
            // Versuche, die Text-Komponente im Kind-GameObject zu finden
            textComponent = child.GetComponent<Text>();

            if (textComponent != null)
            {
                // Hier kannst du mit der gefundenen Text-Komponente arbeiten
                textComponent.color = Color.grey;
            }
        }


        unlocked = false;
    }

    public void Unlock()
    {
        image.color = Color.white;

        foreach (Transform child in transform)
        {
            // Versuche, die Text-Komponente im Kind-GameObject zu finden
            textComponent = child.GetComponent<Text>();

            if (textComponent != null)
            {
                // Hier kannst du mit der gefundenen Text-Komponente arbeiten
                textComponent.color = Color.white;
            }
        }


        unlocked = true;
    }

    public void UpdateTalent()
    {
        image = GetComponent<Image>();

        textComponent.text = $"{currentCount}/{maxCount}";

        if (unlocked)
        {
            Unlock();
        }



    }


    //Methode, um die entsprechenden Spezialisierungspunkte im TalenTree zu erhöhen.
    //Parameter ggf. unnötig.
    
    public void IncreaseTalentTreeSpecPoints(Talent talent)
    {
        switch (talent.abilityType)
        {
            case Ability.AbilityType.Combat:
                TalentTree.instance.totalCombatSpecPoints++;
                break;

            case Ability.AbilityType.Void:
                TalentTree.instance.totalVoidSpecPoints++;
                break;

            case Ability.AbilityType.Utility:
                TalentTree.instance.totalUtilitySpecPoints++;
                break;

        }

        //Überprüfe, ob entsprechende Talente im TalentTree durch das erreichen von totalSpecPoints freigeschaltet wurde.
        foreach (AbilityTalent abilityTalent in TalentTree.instance.allAbilityTalents)
            abilityTalent.CheckForUnlock();
    }

    //Setze die passiven Werte des Talentbaums ein.
    public void ApplyPassivePointsAndEffects(Talent talent)
    {
        Debug.Log("Ist es nicht fragwürdig, das über diese Methode Talente nichts können, außer langweilige FlatStats hinzuzufügen?");
        if(talent.passive)
        switch (talent.abilityType)
        {
            case Ability.AbilityType.Combat:
                PlayerManager.instance.player.GetComponent<PlayerStats>().GetStat(EntitieStats.AttackPower).AddModifier( new StatModifier(1f, StatModType.Flat));
                break;

            case Ability.AbilityType.Void:
                PlayerManager.instance.player.GetComponent<PlayerStats>().GetStat(EntitieStats.AbilityPower).AddModifier(new StatModifier(1f, StatModType.Flat));
                break;

            case Ability.AbilityType.Utility:
                PlayerManager.instance.player.GetComponent<PlayerStats>().GetStat(EntitieStats.Hp).AddModifier( new StatModifier(2, StatModType.Flat));
                break;

            default:
                print("Entweder wurde einem passiven Talent kein Typ zugewiesen, oder es handelt sich um einen Sockel");
                    break;

        }


    }



    public void OnPointerClick(PointerEventData eventData)
    {
        TalentTree.instance.TryUseTalent(this);
        //print(abilityTalent.baseAbility.spec1Counter);
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(description + '\n' + GetRequirementsOfTalent(this));
    }

    public string GetRequirementsOfTalent(Talent talent)
    {
        string info = null;
        switch (talent.baseAbility.abilityType)
        {
            case Ability.AbilityType.Combat:
                if (TalentTree.instance.totalCombatSpecPoints <= talent.baseAbility.requiredTypePoints)
                    info = "<color=#b27d90>Benötigte Combat-Punkte:<b> " + talent.baseAbility.requiredTypePoints + "</b></color>";
                return info;

            case Ability.AbilityType.Void:
                if (TalentTree.instance.totalVoidSpecPoints <= talent.requiredTypePoints)
                    info = "<color=#b27d90>Benötigte Void-Punkte:<b> " + talent.baseAbility.requiredTypePoints + "</b></color>";
                return info;


            case Ability.AbilityType.Utility:
                if (TalentTree.instance.totalUtilitySpecPoints <= talent.requiredTypePoints)
                    info = "<color=#b27d90>Benötigte Utility-Punkte:<b> " + talent.baseAbility.requiredTypePoints + "</b></color>";
                return info;


            default: return info;

        }
    }

}
