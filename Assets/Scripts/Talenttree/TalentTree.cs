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
    private Talent[] talents;

    [SerializeField]
    private Talent[] defaultTalents;

    [SerializeField]
    private Text talentPointText;

    /*
    public int CharPoints
    {
        get
        {
            return points;
        }
        set
        {
            points = value;
            UpdateTalentPointText();
        }
    }
    */

    public void TryUseTalent(Talent talent)
    {
        #region "Tutorial"
        if (PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints() > 0 && talent.Click() && PlayerManager.instance.player.GetComponent<PlayerStats>().Get_level() == 2)
        {
            PlayerManager.instance.player.GetComponent<PlayerStats>().Set_SkillPoints(-1);

            Tutorial tutorialScript = GameObject.FindGameObjectWithTag("TutorialScript").GetComponent<Tutorial>();
            tutorialScript.ShowTutorial(7);

            UpdateTalentPointText();
        }
        #endregion


        else if (PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints() > 0 && talent.Click())
        {
            PlayerManager.instance.player.GetComponent<PlayerStats>().Set_SkillPoints(-1);
            UpdateTalentPointText();
        }


    }

    void Start()
    {
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

    private void UpdateTalentPointText()
    {
        talentPointText.text = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints().ToString();
    }
}
