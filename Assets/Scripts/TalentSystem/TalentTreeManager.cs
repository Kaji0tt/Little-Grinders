using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;



public class TalentTreeManager : MonoBehaviour
{

    #region Singleton
    public static TalentTreeManager instance;
    private void Awake()
    {
        instance = this;

        //FindObjectsByType<Talent_UI>(FindObjectsSortMode.None); // true, um auch inaktive Objekte einzuschließen

        allTalents.Clear();

        foreach (Talent_UI talent in FindObjectsByType<Talent_UI>(FindObjectsSortMode.None))
        {
            allTalents.Add(talent);
            talent.SetTalentUIVariables();
        }

        //Sorge dafür, das alle Talente locked sind, welche zu Beginn nicht freigeschaltet sind.
        ResetTalents();

        //Ziehe die 
        //DrawTalentTreeLines();

        UpdateTalentPointText();



    }


    #endregion

    //Verweis auf ein Talent im Inspektor, das als Grundlage für die Erstellung aus der Datenbank des TalentTreeGenerators gilt.
    public Talent_UI talentUI;

    public Transform parentGameObject;

    public Ability startAbility; // Ursprung ist eine Ability

    public List<Talent_UI> allTalents = new List<Talent_UI>();  // Alle erzeugten Talente im UI

    //talentLineParent sollte jedes AbilityTalent sein.
    public Transform talentLineParent;




    [SerializeField]
    public Talent_UI defaultTalent; 

    [SerializeField]
    private Text talentPointText;


    public Sprite hpIcon, arIcon, asIcon, adIcon, apIcon, reIcon, defaultIcon;


    public void TryUseTalent(Talent_UI clickedTalent)
    {

        
        //Nur wenn der Spieler auch über Skillpunkte verfügt
        if (PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints() > 0 && clickedTalent.Click())
        {
            //Füge die EIgenschaften des Talents dem Spieler hinzu
            clickedTalent.ApplyPassivePointsAndEffects(clickedTalent);

            PlayerManager.instance.player.GetComponent<PlayerStats>().Decrease_SkillPoints(1);
            UpdateTalentPointText();
            if(clickedTalent.myNode != null)
            {
                clickedTalent.myNode.myCurrentCount++;
                UpdateTalentTree(clickedTalent.myNode);
            }


            //Zusätzliche Abfrage für die Root Ability (Sie kann durch ein bool in UseBase nur 1x verwendet werden)
            if (clickedTalent.myAbility != null)
                clickedTalent.myAbility.UseBase(PlayerStats.instance);

            //Füge das Talent der Liste von Talenten hinzu.
            allTalents.Add(clickedTalent);

            if(clickedTalent.myNode != null && clickedTalent.myNode.myCurrentCount <= 1)
            {
                if (!clickedTalent.myNode.IsExpanded())
                    TalentTreeGenerator.instance.ExpandNode(clickedTalent.myNode);

                TalentTreeGenerator.instance.ExpandBranch(clickedTalent.myNode);
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
        foreach (Talent_UI talent in allTalents)
        {
            if (talent == defaultTalent)
                talent.Unlock();
            else
                talent.Lock();
        }

        UpdateTalentPointText();
    }

    public void UpdateTalentPointText()
    {
       
        if(PlayerManager.instance.player != null)
        {
            talentPointText.text = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints().ToString();
        }

    }
    public void UpdateTalentTree(TalentNode node)
    {
        // Aktualisiere den Text auf dem UI-Element (z.B. 1/3)
        node.uiElement.GetComponentInChildren<Text>().text = node.myCurrentCount + "/" + node.myMaxCount;

        // Falls es sich um ein Wurzeltalent handelt (Tiefe == 0)
        if (node.Depth == 0)
        {
            // Wenn Spieler eine bestimmte Fähigkeit (Regeneration) besitzt
            if (PlayerStats.instance.Regeneration.Value >= 1)
                node.uiElement.GetComponent<Talent_UI>().Unlock();
            else
                node.uiElement.GetComponent<Talent_UI>().Lock();
        }


        // Schalte verbundene Talente frei, wenn dieser Knoten voll geskillt ist
        foreach (TalentNode neighborNode in node.myConnectedNodes)
        {
            if (neighborNode != null && node.myCurrentCount >= node.myMaxCount)
            {
                neighborNode.uiElement.GetComponent<Talent_UI>().Unlock();
            }

        }
        
    }

}
