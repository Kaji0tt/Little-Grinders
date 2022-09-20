using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;


//
public enum TalentType { None, Utility, Void, Combat }
public class TalentTree : MonoBehaviour
{
    //private int points = 11;
    #region Singleton
    public static TalentTree instance;
    private void Awake()
    {
        instance = this;

        allTalents = FindObjectsOfType<Talent>();

        foreach (Talent talent in allTalents)
            talent.SetTalentUIVariables();

        //Untersucht alle Talente auf ihre Typ hin und fügt sie entsprechenden Listen hinzu.
        //CalculateAllTalents();

        //Füge alle Abilitie's welche auch Talente sind einer entsprechenden Liste hinzu.
        CalculatAllAbilityTalents();

        //Sorge dafür, das alle Talente locked sind, welche zu Beginn nicht freigeschaltet sind.
        ResetTalents();

        //Ziehe die 
        DrawTalentTreeLines();

        UpdateTalentPointText();



    }


    #endregion

    public Transform talentLineParent;


    public Talent[] allTalents { get; private set; }

    [SerializeField]
    public Talent defaultTalent; 

    //Alle Aktiven Fähigkeiten des Talentbaums werden hier einer Liste hinzugefügt.
    public List<Talent> allAbilityTalents = new List<Talent>();

    //Durchsuche die Szene nach sämtliche Elementen, welche sowohl eine Fähigkeit, als auch ein Talent sind.
    private void CalculatAllAbilityTalents()
    {
        AbilityTalent[] allAbilityTalentsArray = FindObjectsOfType<AbilityTalent>();

        foreach (AbilityTalent abilityTalent in allAbilityTalentsArray)
            allAbilityTalents.Add(abilityTalent);
    }

    [SerializeField]
    private Text talentPointText;

    //Für jedes Talent, welches gesetzt wurde, erhöhe die talentTypePoins
    [HideInInspector]
    public int totalVoidSpecPoints = 0;
    [HideInInspector]
    public int totalUtilitySpecPoints = 0;
    [HideInInspector]
    public int totalCombatSpecPoints = 0;

    /*
    [HideInInspector]
    public List<Talent> voidTalents = new List<Talent>();
    [HideInInspector]
    public List<Talent> lifeTalents = new List<Talent>();
    [HideInInspector]
    public List<Talent> combatTalents = new List<Talent>();
    */

    public void TryUseTalent(Talent clickedTalent)
    {



        if (PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints() > 0 && clickedTalent.Click())
        {
            //if(clickedTalent.abilitySpecialization == Ability.AbilitySpecialization.Spec1)
            //Erhöhe die Speziliaiserungs-Counter des TalentTree's
            clickedTalent.IncreaseTalentTreeSpecPoints(clickedTalent);
            #region "Tutorial"
            if (PlayerManager.instance.player.GetComponent<PlayerStats>().level == 2 && GameObject.FindGameObjectWithTag("TutorialScript") != null)
            {
                Tutorial tutorialScript = GameObject.FindGameObjectWithTag("TutorialScript").GetComponent<Tutorial>();
                tutorialScript.ShowTutorial(7);

            }
            #endregion

            //Falls das geskillte Talent keine Fähigkeit ist, setze die entsprechende Spezialisierung für die Grundfähigkeit.
            if(clickedTalent.GetType() != typeof(AbilityTalent))
            SetSpecializationOfAbility(clickedTalent);

            //Falls es sich um ein passives Talent handelt, erhöhe die passiven Werte.
            if (clickedTalent.passive)
                clickedTalent.ApplyPassiveTalentPoints(clickedTalent);


            PlayerManager.instance.player.GetComponent<PlayerStats>().Decrease_SkillPoints(1);
            UpdateTalentPointText();
        }
        

    }

    private void SetSpecializationOfAbility(Talent clickedTalent)
    {
        //Setze die Spezialisierung der Grundfähigkeit auf jene, des geklickten Talents,
        clickedTalent.abilityTalent.baseAbility.SetSpec(clickedTalent.abilitySpecialization);

        //Gehe durch die Liste aller möglichen Child-Talents der Grundfähigkeit,
        foreach (Talent childTalent in clickedTalent.abilityTalent.childTalent)
        {
            //Überprüfe, ob die Spezialisierung der entsprechend Child-Talente unterschiedlich ist, von der nun gesetzten,
            if (childTalent.abilitySpecialization != clickedTalent.abilityTalent.baseAbility.abilitySpec)
            {
                //Und sperre diese.
                childTalent.LockTalent();

            }
        }
    }

    void OnEnable()
    {
        PlayerStats.eventLevelUp += UpdateTalentPointText;


    }

    void OnDisable()
    {
        PlayerStats.eventLevelUp -= UpdateTalentPointText;
    }


    
    public void ResetTalents()
    {
        foreach (Talent talent in allTalents)
        {
            if (talent == defaultTalent)
                talent.Unlock();

            else
                talent.LockTalent();
        }

        UpdateTalentPointText();
    }

    public void UpdateTalentPointText()
    {
        
        if(PlayerManager.instance.player != null)
        talentPointText.text = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints().ToString();

    }
    


    //Funktion um die Verbindungen zwischen den Talenten herzustellen
    //UILineRenderer currently creates Null-Reference on Start, however the interface is drawn accordingly.
    private void DrawTalentTreeLines()
    {
        foreach (Talent talent in allTalents)
        {
            //Erschaffe ein neues GO für die Talent-Tree Line
            GameObject gameObject = new GameObject("_TalentLine", typeof(UILineRenderer));

            //Setze Parent des GO für vernünftiges UI Layering
            gameObject.transform.SetParent(talentLineParent, false);

            //Füge UILineRendere der Unity.UI.Extensions hinzu
            UILineRenderer lineRend = gameObject.GetComponent<UILineRenderer>();

            //Setze Farbe der Talent-Linie
            lineRend.color = new Color(.3f, .3f, .3f, .5f);

            //Setze den Ankerpunkt der Talent-Linie nach Rechts-Unten
            RectTransform rectTrans = gameObject.GetComponent<RectTransform>();

            rectTrans.anchorMin = new Vector2(0, 0);
            rectTrans.anchorMax = new Vector2(0, 0);

            //Erstelle eine Liste der Positionen der Talente
            List<Vector2> childTalPos = new List<Vector2>();

            //Füge den Ursprung hinzu (Das Talent, welches Verbindungen zu den Kinder-Talenten aufbauen soll) - .5 um mittig positioniert zu werden.
            childTalPos.Add(talent.GetComponent<RectTransform>().anchoredPosition + talent.GetComponent<RectTransform>().sizeDelta * .5f);

            //Für jedes Kinder-Talent soll der Liste neue Positionen hinzugefügt werden, aus welchen später die Talent-Linien erstellt werden.
            foreach (Talent childTalent in talent.childTalent)
            {

                //Es müssten stets zwei Punkte hinzugefügt werden, die Mitte das Kinder-Talents und die des Ursprung-Talents (childTalent + talent)
                childTalPos.Add(childTalent.GetComponent<RectTransform>().anchoredPosition + childTalent.GetComponent<RectTransform>().sizeDelta * .5f);
                childTalPos.Add(talent.GetComponent<RectTransform>().anchoredPosition + childTalent.GetComponent<RectTransform>().sizeDelta * .5f);

            }

            //Füge die Liste dem erschaffenen GO mit UILineRenderer Komponente hinzu
            lineRend.Points = childTalPos.ToArray();

        }
    }

}
