using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TalentSave
{

    public string talentName { get; set; }

    public int talentPoints { get; set; }

    public int spec { get; set; }
    
    public TalentSave(string name, int points, bool unlocked, int spec)
    {
        this.talentName = name;

        this.talentPoints = points;

        this.unlocked = unlocked;

        this.spec = spec;

    }

    public bool unlocked { get; set; }

}
