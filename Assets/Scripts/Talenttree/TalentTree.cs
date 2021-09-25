using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public enum TalentType { None, Life, Void, Combat }
public class TalentTree : MonoBehaviour
{
    //private int points = 11;
    #region Singleton
    public static TalentTree instance;
    private void Awake()
    {
        instance = this;

        talents = FindObjectsOfType<Talent>();

        CalculateAllTalents();

        ResetTalents();

        DrawTalentTreeLines();

        UpdateTalentPointText();

    }
    #endregion

    public Transform talentLineParent;


    public Talent[] talents { get; private set; }

    [SerializeField]
    public Talent[] defaultTalents; 

    [HideInInspector]
    public Talent[] allTalents;

    [SerializeField]
    private Text talentPointText;

    //Für jedes Talent, welches gesetzt wurde, erhöhe die talentTypePoins
    [HideInInspector]
    public int voidPoints = 0;
    [HideInInspector]
    public int lifePoints = 0;
    [HideInInspector]
    public int combatPoints = 0;
    [HideInInspector]
    public List<Talent> voidTalents = new List<Talent>();
    [HideInInspector]
    public List<Talent> lifeTalents = new List<Talent>();
    [HideInInspector]
    public List<Talent> combatTalents = new List<Talent>();

    public void TryUseTalent(Talent talent)
    {

        if (PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints() > 0 && talent.Click())
        {
            talent.IncreaseTalentTypePoins(talent);
            #region "Tutorial"
            if (PlayerManager.instance.player.GetComponent<PlayerStats>().level == 2 && GameObject.FindGameObjectWithTag("TutorialScript") != null)
            {
                Tutorial tutorialScript = GameObject.FindGameObjectWithTag("TutorialScript").GetComponent<Tutorial>();
                tutorialScript.ShowTutorial(7);

            }
            #endregion

            
            if (talent is ISmallTalent smallTalent)
            {
                smallTalent.PassiveEffect();
            }
            
            PlayerManager.instance.player.GetComponent<PlayerStats>().Decrease_SkillPoints(1);
            UpdateTalentPointText();
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
        foreach (Talent talent in talents)
            talent.LockTalents();

        foreach (Talent talent in defaultTalents)
            talent.Unlock();

        UpdateTalentPointText();
    }

    public void UpdateTalentPointText()
    {
        
        if(PlayerManager.instance.player != null)
        talentPointText.text = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints().ToString();

    }
    
    void CalculateAllTalents()
    {
        List<Talent> allTalentsList = new List<Talent>();

        foreach (Talent talent in defaultTalents)
        {

            allTalentsList.Add(talent);

        }
        foreach (Talent talent in talents)
        {

            allTalentsList.Add(talent);

        }

        foreach(Talent talent in talents)
        {
            if (talent.talentType == TalentType.Void)
                voidTalents.Add(talent);

            if (talent.talentType == TalentType.Combat)
                combatTalents.Add(talent);

            if (talent.talentType == TalentType.Life)
                lifeTalents.Add(talent);
        }

        allTalents = allTalentsList.ToArray();

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
