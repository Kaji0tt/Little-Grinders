using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalentTree : MonoBehaviour
{
    //private int points = 11;


    // Relevant wird nochmal: Welche Fähigkeiten sind zwar freigeschaltet, aber ausgegraut, sobald keine Verteilbaren punkte mehr da sind..
    // Wobei.. dann wären sie Verwendbar solange wir die Skillpunkte noch nicht gesetzt wurden, es sei denn ich mach eine currentCoutn != 0 Abfrage..
    // Jedenfalls, hier das Video: https://youtu.be/NEqaBBnAFfM?t=624


    [SerializeField]
    public Talent[] talents;

    [SerializeField]
    public Talent[] defaultTalents; 

    [HideInInspector]
    public Talent[] allTalents;

    [SerializeField]
    private Text talentPointText;

    //public List<Talent> allTalents = new List<Talent>();


    public void TryUseTalent(Talent talent)
    {

        if (PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints() > 0 && talent.Click())
        {
            
            #region "Tutorial"
            if (PlayerManager.instance.player.GetComponent<PlayerStats>().level == 2)
            {
                Tutorial tutorialScript = GameObject.FindGameObjectWithTag("TutorialScript").GetComponent<Tutorial>();
                tutorialScript.ShowTutorial(7);

            }
            #endregion
            
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

    void Start()
    {
        CalculateAllTalents();
        ResetTalents();
    }


    private void ResetTalents()
    {
        UpdateTalentPointText();
        foreach (Talent talent in talents)
            talent.LockTalents();

        foreach (Talent talent in defaultTalents)
            talent.Unlock();
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

        allTalents = allTalentsList.ToArray();

    }
    
}
