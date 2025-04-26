using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


//Ability Talent ist das Script, welches sich im Editor innerhalb des Canvas auf dem GameObject befinden sollte, damit der Spieler das Talent "bewegen" kann.

/// <summary>
/// Das obere Zitat: Lässt sich leichter handhaben. Die besagten Infos kann die bestimmte Ability leichter nutzen.
///  -------------------------------------------------------------------------------------------------------------------------
/// |   Zitat AI:                                                                                                             |
/// |   Entferne die Cooldown-Logik aus AbilityTalent und integriere sie direkt in die Pace-Klasse. Dort kann die Auflade-    |
/// |   und Cooldown-Mechanik besser gesteuert werden, sodass die Fähigkeit bei fehlender Aufladung nicht im Cooldown-Zustand | 
/// |   bleibt, sondern weiterhin intern die chargeCount auflädt.                                                             |
///  -------------------------------------------------------------------------------------------------------------------------
///  
/// Was genau verwaltet AbilityTalent derzeit?
/// Den Zustand der Ability für den TalentTree - Gut! Aber auch im Talent machbar: If baseAbility != null...        Talent!
/// Spezialisierungspunkte - Nicht gut. Warum macht das Talent das nicht selbst?                                    Talent!
/// Cooldown und Namen - States - Ab in                                                                             Ability!
/// Cooldown                                                                                                        CooldownManager!
/// RunThroughChildTalents - Talentbaum berechnung! Ab in TalentTree oder Talent!                                   TalentTree / Talent!
/// AbilityState                                                                                                    Ability!
/// Interfacekram... eigentlich reicht im Talent ein Haken: Useable? Und wenn es ja ist, dann eine Referenz zu      Ability!
/// Frage bzgl. RequiredPoints... welche Points tragen wozu bei? könnte aber auch über TalentTree ausgelesen [...] TalentTree!
/// 
/// </summary>

//Obsolete.
public class AbilityTalent : Talent_UI,  IPointerEnterHandler, IPointerExitHandler //IUseable,
{


 
    /*
    #region EventSystem Stuff
    public bool IsOnCooldown()
    {
        if (state == AbilityState.cooldown)
            return true;
        else
            return false;
    }

    public float GetCooldown()
    {
        return cooldownTime;
    }

    public float CooldownTimer()
    {
        return cooldownTime;
    }
    */



    /*
    public bool IsActive()
    {

        if (state == AbilityState.active)
            return true;
        else
            return false;
    }

    #endregion
    */
}
