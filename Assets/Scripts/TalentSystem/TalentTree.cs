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

        allTalents = FindObjectsByType<Talent>(FindObjectsSortMode.None); // true, um auch inaktive Objekte einzuschließen
        
        foreach (Talent talent in allTalents)
        {
            talent.SetTalentUIVariables();
        }

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

    //talentLineParent sollte jedes AbilityTalent sein.
    public Transform talentLineParent;


    public Talent[] allTalents { get; private set; }

    [SerializeField]
    public Talent defaultTalent; 

    //Alle Aktiven Fähigkeiten des Talentbaums werden hier einer Liste hinzugefügt.
    public List<Talent> allAbilityTalents = new List<Talent>();

    //Durchsuche die Szene nach sämtliche Elementen, welche sowohl eine Fähigkeit, als auch ein Talent sind.
    private void CalculatAllAbilityTalents()
    {
        AbilityTalent[] allAbilityTalentsArray = FindObjectsByType<AbilityTalent>(FindObjectsSortMode.InstanceID);

        foreach (AbilityTalent abilityTalent in allAbilityTalentsArray)
            allAbilityTalents.Add(abilityTalent);
    }

    [SerializeField]
    private Text talentPointText;
    [SerializeField]
    private Text totalVoidPointsText;
    [SerializeField]
    private Text totalUtilityPointsText;
    [SerializeField]
    private Text totalCombatPointsText;

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

            totalVoidPointsText.text = totalVoidSpecPoints.ToString();
            totalUtilityPointsText.text = totalUtilitySpecPoints.ToString();
            totalCombatPointsText.text = totalCombatSpecPoints.ToString();

            //Falls das geskillte Talent keine Fähigkeit ist, setze die entsprechende Spezialisierung für die Grundfähigkeit.
            if (clickedTalent.baseAbility != null)
            {
                //Setze die Spezialisierung.
                SetSpecializationOfAbility(clickedTalent);

                //Und füge die Boni auf das Base Talent hinzu.
                if(!clickedTalent.passive)
                ApplySpecializationEffects(clickedTalent);

            }


            //Falls es sich um ein passives Talent handelt, erhöhe die passiven Werte.
            if (clickedTalent.passive)
                clickedTalent.ApplyPassivePointsAndEffects(clickedTalent);

            PlayerManager.instance.player.GetComponent<PlayerStats>().Decrease_SkillPoints(1);
            UpdateTalentPointText();
        }
        

    }

    private void SetSpecializationOfAbility(Talent clickedTalent)
    {
        //Falls sich das geklickte Talent auf eine Fähigkeit richtet, setze die Spezialisierung der Grundfähigkeit auf jene, des geklickten Talents
        if(clickedTalent.baseAbility != null)
        clickedTalent.baseAbility.SetSpec(clickedTalent.abilitySpecialization);

        //Überprüfe allTalents ob die baseAbility identisch ist mit der von clickedTalent,
        //falls ja, überprüfe ob das entsprechende Talent eine andere Spezialisierung hat, als die der baseAbility.
        //wenn dies so ist, dann LockTalent(); das entsprechende Talent.
        foreach (Talent aTalent in allTalents)
        {

            if (aTalent.abilitySpecialization != clickedTalent.baseAbility.abilitySpec)
            {

                aTalent.LockTalent();

            }
        }
    }

    private void ApplySpecializationEffects(Talent clickedTalent)
    {
        clickedTalent.ApplySpecialization(clickedTalent);
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
        {
            talentPointText.text = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints().ToString();
            totalVoidPointsText.text = totalVoidSpecPoints.ToString();
            totalUtilityPointsText.text = totalUtilitySpecPoints.ToString();
            totalCombatPointsText.text = totalCombatSpecPoints.ToString();
        }



    }

    // Funktion um die Verbindungen zwischen den Talenten herzustellen
    /*
    private void DrawTalentTreeLines()
    {
        foreach (Talent talent in allTalents)
        {
            // Erschaffe ein neues GameObject für die Talent-Tree Line
            GameObject lineGO = new GameObject("_TalentLine", typeof(UILineRenderer));

            // Setze Parent des GO für vernünftiges UI Layering
            lineGO.transform.SetParent(talentLineParent, false);

            // Füge UILineRenderer der Unity.UI.Extensions hinzu
            UILineRenderer lineRend = lineGO.GetComponent<UILineRenderer>();

            // Setze Farbe der Talent-Linie
            lineRend.color = new Color(.7f, .7f, .7f, 1f);

            // Setze den Ankerpunkt der Talent-Linie nach Rechts-Unten
            RectTransform rectTrans = lineGO.GetComponent<RectTransform>();
            rectTrans.anchorMin = new Vector2(0, 0);
            rectTrans.anchorMax = new Vector2(0, 0);

            // Erstelle eine Liste der Positionen der Talente
            List<Vector2> talentLinePositions = new List<Vector2>();

            // Füge die Position des Ursprungstalents (Parent-Talent) hinzu
            RectTransform parentRect = talent.GetComponent<RectTransform>();
            Vector2 parentPosition = parentRect.anchoredPosition + parentRect.sizeDelta * 0.5f;
            talentLinePositions.Add(parentPosition);

            // Überprüfe alle Child-Talente
            foreach (Talent childTalent in talent.childTalent)
            {
                if (childTalent != null)
                {
                    // Füge die Position des Child-Talents hinzu
                    RectTransform childRect = childTalent.GetComponent<RectTransform>();
                    Vector2 childPosition = childRect.anchoredPosition + childRect.sizeDelta * 0.5f;

                    // Füge die Linie von Parent zu Child hinzu
                    talentLinePositions.Add(childPosition);
                }
                else
                {
                    Debug.LogWarning("Child talent is null for parent: " + talent.name);
                }
            }

            // Übergebe die Positionen an den UILineRenderer
            lineRend.Points = talentLinePositions.ToArray();
        }
    }
    */
    /*
        //Funktion um die Verbindungen zwischen den Talenten herzustellen
        //UILineRenderer currently creates Null-Reference on Start, however the interface is drawn accordingly.
        private void DrawTalentTreeLines()
        {
            foreach(AbilityTalent aTalent in allAbilityTalents)
            {                


                foreach (Talent talent in allTalents)
                {
                    //Erschaffe ein neues GO für die Talent-Tree Line
                    GameObject gameObject = new GameObject("_TalentLine", typeof(UILineRenderer));

                    //Setze Parent des GO für vernünftiges UI Layering
                    gameObject.transform.SetParent(aTalent.gameObject.transform, false);

                    //Füge UILineRendere der Unity.UI.Extensions hinzu
                    UILineRenderer lineRend = gameObject.GetComponent<UILineRenderer>();

                    //Setze Farbe der Talent-Linie
                    lineRend.color = new Color(.7f, .7f, .7f, 1f);

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
    */


    private void DrawTalentTreeLines()
    {
        Transform contentTransform = GetComponentInParent<ScrollRect>().content;

        foreach(Talent talent in allTalents)
        {
            DrawLinesRecursively(talent, talent.childTalent, contentTransform);
        }
    }

    // Rekursive Methode, um Linien zwischen Parent- und Child-Talenten zu zeichnen
    private void DrawLinesRecursively(Talent parentTalent, Talent[] childTalents, Transform contentTransform)
    {
        RectTransform parentRect = parentTalent.GetComponent<RectTransform>();
        Vector2 parentCenter = GetRectTransformCenterInLocalSpace(parentRect, contentTransform);

        foreach (Talent childTalent in childTalents)
        {
            if (childTalent == null) continue;

            GameObject lineGO = new GameObject("_TalentLine", typeof(UILineRenderer));
            lineGO.transform.SetParent(talentLineParent, false);

            UILineRenderer lineRend = lineGO.GetComponent<UILineRenderer>();
            lineRend.color = new Color(75f / 255f, 75f / 255f, 75f / 255f, 1f);

            lineRend.LineThickness = 4f;

            RectTransform childRect = childTalent.GetComponent<RectTransform>();
            Vector2 childCenter = GetRectTransformCenterInLocalSpace(childRect, contentTransform);

            List<Vector2> linePoints = new List<Vector2> { parentCenter, childCenter };
            lineRend.Points = linePoints.ToArray();

            DrawLinesRecursively(childTalent, childTalent.childTalent, contentTransform);
        }
    }

    // Hilfsfunktion zur Berechnung der Position der Mitte eines RectTransforms im lokalen Raum des übergeordneten Canvas
    private Vector2 GetRectTransformCenterInLocalSpace(RectTransform rect, Transform referenceTransform)
    {
        Vector2 worldCenter = rect.TransformPoint(rect.rect.center);
        Vector2 localCenter;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            referenceTransform as RectTransform,
            worldCenter,
            null,
            out localCenter
        );
        return localCenter;
    }

}
