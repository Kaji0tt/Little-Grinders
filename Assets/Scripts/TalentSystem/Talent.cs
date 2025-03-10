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

//values for passive talents
public enum TalentType { Health, Regenration, Armor, AttackDamage, AbilityPower, Movement };

public class Talent : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    //Is this talent a passive talent? If not, set the ability.
    public bool passive;

    //if its not a passive, set the talent via inspector
    //HINT: the "EditorTalentPassivToggle.cs" manages the display in unity editor
    public Ability myAbility;

    //name of the talent, needed for loading / saving
    public string talentName;

    //set the value of this passive
    public TalentType myType;

    //set the value of % increase, gained by 1 point in talent count
    public float value;

    //the commulated value of the value.
    public int currentCount { get; private set; }

    public void Set_currentCount(int newCount)
    {
        currentCount = newCount;
    }

    //the maximum count of this passive talent
    [SerializeField]
    private int maxCount;

    //the descirption of the talent will be generated automaticly
    private string description;

    private void Start()
    {
        if (passive)
        {
            SetDescription(); // Setzt die korrekte Beschreibung für passive Talente
        }
        else if (myAbility != null)
        {
            description = myAbility.description; // Setzt die Beschreibung für aktive Talente
            maxCount = 1;
        }
        else
        {
            description = "Talent has not been declared in Unity editor yet.";
        }


    }

    public void SetDescription()
    {
        description = "Increases the players <b>" + myType.ToString() + "</b> by <b>" + value.ToString() + "</b> per skillpoint invested. ";

    }



    private Image image;

    public Sprite icon;
    

    private Text textComponent;



    [HideInInspector]
    public bool unlocked;

    //[SerializeField]
    public Talent[] childTalent;



    //Die Weiterleitung zu anderen Talenten macht er in Abhängig von "Pfeilen", also wenn ein PfeilSprite auf eine andere Fähigkeit zeigt, bzw. sie einen Pfeil besitzt, dann wird das Folgetalent freigeschaltet.
    //Kp ob ich das so machen wollen würde, entsprechendes Video hier:
    // https://youtu.be/NEqaBBnAFfM?t=406

    //Drag and Drop Magie
    //https://youtu.be/ILaDr3CE7QY?t=615


    public string GetName()
    {
            return talentName;
    }


 
    private void RunThroughChildTalents(Talent talent)
    {
        //Und füge dieses der entsprechenden Spezialisierungsliste hinzu. (Spec1 / Spec2 / Spec3)
        //AddSpecializationTalents(talent);
        if (talent.childTalent.Length > 0)
        {
            foreach (Talent childTalent in talent.childTalent)
            {
                RunThroughChildTalents(childTalent);
            }


        }
    }


    //Void to be called in TalentTree.cs for structural purpose.
    //Setzt Image und Count Text für jedes einzelne Talent.
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

    
    //Setze die passiven Werte des Talentbaums ein.
    public void ApplyPassivePointsAndEffects(Talent talent)
    {
        Debug.Log("Ist es nicht fragwürdig, das über diese Methode Talente nichts können, außer langweilige FlatStats hinzuzufügen?");
        if(talent.passive)
        switch (myType)
        {
            case TalentType.Health:
                PlayerManager.instance.player.GetComponent<PlayerStats>().GetStat(EntitieStats.AttackPower).AddModifier( new StatModifier(1f, StatModType.Flat));
                break;

            case TalentType.Regenration:
                PlayerManager.instance.player.GetComponent<PlayerStats>().GetStat(EntitieStats.AbilityPower).AddModifier(new StatModifier(1f, StatModType.Flat));
                break;

            case TalentType.Armor:
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
        UI_Manager.instance.ShowTooltip(description);
    }


}
