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


    public AbilityTalent abilityTalent;

    //Description of the Spell
    [SerializeField]
    [TextArea]
    private string description;

    public string talentID;
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
            return abilityTalent.baseAbility.icon;
        }
    }
    

    private Text countText;

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



    //Void to be called in TalentTree.cs for structural purpose.
    public void SetTalentUIVariables()
    {
        image = GetComponent<Image>();
        countText = transform.GetComponentInChildren<Text>();
        countText.text = $"{currentCount}/{maxCount}";
    }

    private void Awake()
    {
        //DoubleCheck if an AbilityTalent has been choosen, to make sure there are no problems within the editor.
        if (abilityTalent == null)
            print("Es wurde vergessen, eine aktive Referenzfähigkeit im Talent: " + gameObject.name + " zu setzen.");
        

        if (description == null)
                description = abilityTalent.baseAbility.description;
    }

    public bool Click()
    {


        countText = transform.GetComponentInChildren<Text>();
        if (currentCount < maxCount && unlocked)
        {
            currentCount++;

            countText.text = $"{currentCount}/{maxCount}";

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

        countText.color = Color.grey;

        unlocked = false;
    }

    public void Unlock()
    {
        image.color = Color.white;

        countText.color = Color.white;

        unlocked = true;
    }

    public void UpdateTalent()
    {
        image = GetComponent<Image>();

        countText.text = $"{currentCount}/{maxCount}";

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
        if(talent.passive)
        switch (talent.abilityType)
        {
            case Ability.AbilityType.Combat:
                PlayerManager.instance.player.GetComponent<PlayerStats>().AddNewStatModifier(EntitieStats.AttackPower, new StatModifier(0.5f, StatModType.Flat));
                break;

            case Ability.AbilityType.Void:
                PlayerManager.instance.player.GetComponent<PlayerStats>().AddNewStatModifier(EntitieStats.AbilityPower, new StatModifier(0.5f, StatModType.Flat));
                break;

            case Ability.AbilityType.Utility:
                PlayerManager.instance.player.GetComponent<PlayerStats>().AddNewStatModifier(EntitieStats.Hp, new StatModifier(1, StatModType.Flat));
                break;

            default:
                print("Entweder wurde einem passiven Talent kein Typ zugewiesen, oder es handelt sich um einen Sockel");
                    break;

        }

        else
        {
            talent.abilityTalent.ApplySpecialization(talent);
        }

    }



    public void OnPointerClick(PointerEventData eventData)
    {
        TalentTree.instance.TryUseTalent(this);
        //print(abilityTalent.baseAbility.spec1Counter);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
    }
}
