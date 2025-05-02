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
//public enum TalentType { Health, Regenration, Armor, AttackDamage, AbilityPower, Movement };

public class Talent_UI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    //Is this talent a passive talent? If not, set the ability.
    public bool passive;

    //if its not a passive, set the talent via inspector
    //HINT: the "EditorTalentPassivToggle.cs" manages the display in unity editor
    public Ability myAbility;

    //name of the talent, needed for loading / saving
    public string talentName;

    //set the value of this passive
    public List<TalentType> myTypes;

    public TalentNode myNode { get; private set; }

    //set the value of % increase, gained by 1 point in talent count
    public float value;

    //the commulated amount of the value, according to its set skillpoints.
    public int currentCount { get; private set; }
    public RectTransform circle;

    public RectTransform myRectTransform { get; private set;} 

    public void Set_currentCount(int newCount)
    {
        currentCount = newCount;
    }

    //the maximum count of this passive talent
    [SerializeField]
    private int maxCount;

    //the descirption of the talent will be generated automaticly
    private string description;


    private void OwnStart()
    {
        myRectTransform = GetComponent<RectTransform>();

        if (myAbility != null)
        {
            description = myAbility.description; // Setzt die Beschreibung für aktive Talente
            maxCount = 1;
        }

        //Debug.Log("I got enabled!");
        if (myNode == null)
        {
            myNode = GetComponent<TalentNode>();
            myNode.IsExpanded();
        }



        image = GetComponent<Image>();

        StartCoroutine(RotateCircle());
        circle.gameObject.SetActive(false);

    }





    public void SetNode(TalentNode node)
    {



        node.uiElement = GetComponent<Transform>();
        myRectTransform = GetComponent<RectTransform>();
        myNode = node;
        image = GetComponent<Image>();
        textComponent = GetComponentInChildren<Text>();

        if (node != null)
        {
            SetNodeInfo();
            SetTalentUIVariables();
            Unlockable();
            Lock();
            UpdateTalent();
            node.SetGameObject(this);
        }
        OwnStart();
    }

    private void SetNodeInfo()
    {

        //Debug.Log(myNode.myTypes.Count);
        //Setze Werte

        myTypes = myNode.myTypes;

        //Setze Icon
        var manager = TalentTreeManager.instance;
        foreach (TalentType type in myTypes)
        {
            icon = type switch
            {
                TalentType.HP => image.sprite = manager.hpIcon,
                TalentType.AP => image.sprite = manager.apIcon,
                TalentType.AD => image.sprite = manager.adIcon,
                TalentType.AR => image.sprite = manager.arIcon,
                TalentType.AS => image.sprite = manager.asIcon,
                TalentType.RE => image.sprite = manager.reIcon,
                _ => image.sprite = manager.defaultIcon // Falls kein passendes TalentType existiert
            };
        }

        //Setze Count, bzw. modifiziere Sprite in Abhängigkeit der Werte
        if(myNode.myTypes.Count > 1)
        {
            circle.sizeDelta *= 1.5f;
            Outline hybrid = gameObject.GetComponent<Outline>();

            hybrid.enabled = true;
        }
        else
        {
            circle.sizeDelta *= 1.2f;
        }

        //Setze Werte
        currentCount = myNode.myCurrentCount;
        maxCount = myNode.myMaxCount;
        value = myNode.myValue;

        //Setze UI Paramenter
        gameObject.GetComponent<RectTransform>().anchoredPosition = myNode.myPosition;
        //gameObject.GetComponentInChildren<Text>().text = string.Join(", ", myTypes);

        SetDescription();

    }

    public void SetDescription()
    {
        if (myAbility == null )
            description = "<b>ID: " + myNode.ID + "</b>\n Increases the players <b>" + string.Join(", ", myTypes) + "</b> by <b>" + value.ToString() + "%</b> per skillpoint invested. ";
        else
            description = myAbility.description;

    }



    private Image image;

    public Sprite icon;
    

    private Text textComponent;



    [HideInInspector]
    public bool unlocked;

    //[SerializeField]
    public List<Talent_UI> childTalent = new List<Talent_UI>();



    //Die Weiterleitung zu anderen Talenten macht er in Abhängig von "Pfeilen", also wenn ein PfeilSprite auf eine andere Fähigkeit zeigt, bzw. sie einen Pfeil besitzt, dann wird das Folgetalent freigeschaltet.
    //Kp ob ich das so machen wollen würde, entsprechendes Video hier:
    // https://youtu.be/NEqaBBnAFfM?t=406

    //Drag and Drop Magie
    //https://youtu.be/ILaDr3CE7QY?t=615


    public string GetName()
    {
            return talentName;
    }


 
    private void RunThroughChildTalents(Talent_UI talent)
    {
        //Und füge dieses der entsprechenden Spezialisierungsliste hinzu. (Spec1 / Spec2 / Spec3)
        //AddSpecializationTalents(talent);
        if (talent.childTalent.Count > 0)
        {
            foreach (Talent_UI childTalent in talent.childTalent)
            {
                RunThroughChildTalents(childTalent);
            }


        }
    }


    //Void to be called in TalentTree.cs for structural purpose.
    //Setzt Image und Count Text für jedes einzelne Talent.
    public void SetTalentUIVariables()
    {
        //image = GetComponentInChildren<Image>().sprite;
        icon = GetComponentInChildren<Image>().sprite;

        // Hole alle direkten Kind-GameObjects
        foreach (Transform child in transform)
        {
            // Versuche, die Text-Komponente im Kind-GameObject zu finden
            textComponent = child.GetComponent<Text>();

            if (textComponent != null)
            {
                if(myNode != null)
                // Hier kannst du mit der gefundenen Text-Komponente arbeiten
                textComponent.text = $"ID: {myNode.ID}:{currentCount}/{maxCount}";

                else
                {
                    Debug.Log("In: SetTalentUIVariables() konnte kein Node gefunden werden.");
                    textComponent.text = $"{currentCount}/{maxCount}";
                }

            }
        }
    }

    public bool Click()
    {
        textComponent = transform.GetComponentInChildren<Text>();
        if (currentCount < maxCount && unlocked)
        {
            currentCount++;

            if(myNode != null)
            textComponent.text = $"ID: {myNode.ID}:{currentCount}/{maxCount}";

            else
            {
                Debug.Log("In: Click() konnte kein Node gefunden werden.");
                textComponent.text = $"{currentCount}/{maxCount}";
            }
               // textComponent.text = $"{currentCount}/{maxCount}";

            // Nur wenn es sich nicht um den Urpsrung des TalentTrees handelt!
            if (myAbility == null)
            // 💥 Neuer Code: Bei erster Investition expandieren
            if (currentCount == 1)   
            {
                // Nur einmalig expandieren!
                //TalentTreeGenerator.instance.ExpandNode(myNode);
            }

            if (currentCount == maxCount)
            {
                for (int i = 0; i < childTalent.Count; i++)
                {
                    if (childTalent[i] != null)
                        childTalent[i].Unlock();
                }
            }

            return true;
        }
        return false;
    }

    public void ChangeImageToExpanded()
    {
        image = GetComponent<Image>();
        image.color = Color.yellow;
    }


    public bool Unlockable()
    {
        if (myNode != null)
        {
            if (unlocked) return true;


            foreach (TalentNode neighbor in myNode.myConnectedNodes)
                if (neighbor != null && neighbor.myCurrentCount >= neighbor.myMaxCount)
                    Unlock();

            return false;
        }
        return false;

    }



    public void Lock()
    {
        //Debug.Log("Lets lock " + gameObject.name);
        image = GetComponent<Image>();
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
        image = GetComponent<Image>();
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

        //if(myNode != null)
        //textComponent.text = $"{myNode.myCurrentCount}/{myNode.myMaxCount}";

        if (unlocked)
        {
            Unlock();
        }



    }

    public void ApplyPassivePointsAndEffects(Talent_UI skilledTalent)
    {
        var playerStats = PlayerStats.instance;

        foreach (TalentType type in skilledTalent.myTypes)
        {
            EntitieStats stat = type switch
            {
                TalentType.HP => EntitieStats.Hp,
                TalentType.AP => EntitieStats.AbilityPower,
                TalentType.AD => EntitieStats.AttackPower,
                TalentType.AR => EntitieStats.Armor,
                TalentType.AS => EntitieStats.AttackSpeed,
                TalentType.RE => EntitieStats.Regeneration,
                _ => EntitieStats.None // Falls du eine ungültige Auswahl abfangen willst
            };

            if (stat != EntitieStats.None)
            {
                //Debug.Log("Cool, adding" + stat + "  with " +  skilledTalent.value + " as PercentMult to the Player.");
                playerStats.GetStat(stat).AddModifier(new StatModifier(skilledTalent.value * 0.01f, StatModType.PercentAdd));
            }
            else
            {
                Console.WriteLine("Entweder wurde einem passiven Talent kein Typ zugewiesen, oder es handelt sich um einen Sockel");
            }
        }

    }




    public void OnPointerClick(PointerEventData eventData)
    {
        TalentTreeManager.instance.TryUseTalent(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_Manager.instance.ShowTooltip(description);
        circle.gameObject.SetActive(true);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_Manager.instance.HideTooltip();
        circle.gameObject.SetActive(false);

    }



    private IEnumerator RotateCircle()
    {
        while (true)
        {
            circle.Rotate(0, 0, 20f * Time.deltaTime); // 50° pro Sekunde
            yield return null;
        }
    }

}
