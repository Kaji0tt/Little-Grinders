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

    public TalentType talentType;


    public AbilityTalent abilityTalent;

    //Description of the Spell
    [SerializeField]
    [TextArea]
    private string description;

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
    public void SetTalentVariables()
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



    public void LockTalents()
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

    public void IncreaseTalentTypePoins(Talent talent)
    {
        //Prüfe welchen Typ das Talent besitzt.
        switch (talent.talentType)
        {
            //Falls das Talent vom Typ Utility ist
            case TalentType.Utility:

                //Erhöhe den Utility-Counter der dazugehörigen BaseAbility um 1
                abilityTalent.baseAbility.utilityCounter += 1;

                //..und erhöhe den TotalCounter von Utility
                TalentTree.instance.totalUtilityPoints++;

                /* - Als die Klasse Spells verantwortlich war für die aktiven Fähigkeiten, wurde diese herangehensweise benutzt.
                 * - Außerdem ist der Approach verkehrt herum gewesen. (Das Talent sollte lieber in TalenTree prüfen,
                 * - ob dieser ausreichend Punkte in der entsprechenden Kategorie hat, bevor um es dann zu entsperren - nicht vice versa)
                foreach (Talent lT in TalentTree.instance.lifeTalents)
                {
                        lT.collectedTypePoints = TalentTree.instance.totalUtilityPoints;

                    if (lT.collectedTypePoints == lT.requiredTypePoints && lT is Spell)
                        lT.Unlock();
                }
                */

                break;

            case TalentType.Combat:

                abilityTalent.baseAbility.combatCounter += 1;

                TalentTree.instance.totalCombatPoints++;
                /*
                foreach (Talent cT in TalentTree.instance.combatTalents)
                {
                        cT.collectedTypePoints = TalentTree.instance.totalCombatPoints;

                    if (cT.collectedTypePoints == cT.requiredTypePoints && cT is Spell)
                        cT.Unlock();
                }
                */
                break;

            case TalentType.Void:

                abilityTalent.baseAbility.voidCounter += 1;

                TalentTree.instance.totalVoidPoints++;
                /*
                foreach(Talent vT in TalentTree.instance.voidTalents)
                {
                        vT.collectedTypePoints = TalentTree.instance.totalVoidPoints;

                    if (vT.collectedTypePoints == vT.requiredTypePoints && vT is Spell)
                        vT.Unlock();
                }
                */
                break;

        }

        foreach (AbilityTalent abilityTalent in TalentTree.instance.allAbilityTalents)
            abilityTalent.CheckForUnlock();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        TalentTree.instance.TryUseTalent(this);
        print(abilityTalent.baseAbility.voidCounter);
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
