using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalentTree : MonoBehaviour
{
    private int points = 10;

    // Relevant wird nochmal: Welche Fähigkeiten sind zwar freigeschaltet, aber ausgegraut, sobald keine Verteilbaren punkte mehr da sind..
    // Wobei.. dann wären sie Verwendbar solange wir die Skillpunkte noch nicht gesetzt wurden, es sei denn ich mach eine currentCoutn != 0 Abfrage..
    // Jedenfalls, hier das Video: https://youtu.be/NEqaBBnAFfM?t=624


    [SerializeField]
    private Talent[] talents;

    [SerializeField]
    private Talent[] defaultTalents;

    [SerializeField]
    private Text talentPointText;

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

    public void TryUseTalent(Talent talent)
    {
        if (CharPoints > 0 && talent.Click())
            CharPoints--;
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
        talentPointText.text = points.ToString();
    }
}
