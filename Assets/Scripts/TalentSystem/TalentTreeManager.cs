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
        Debug.Log("=== [TalentTreeManager.Awake] START ===");

        //FindObjectsByType<Talent_UI>(FindObjectsSortMode.None); // true, um auch inaktive Objekte einzuschließen

        allTalents.Clear();

        var foundTalents = FindObjectsByType<Talent_UI>(FindObjectsSortMode.None);
        Debug.Log($"[Awake] Gefundene Talent_UI Objekte: {foundTalents.Length}");

        foreach (Talent_UI talent in foundTalents)
        {
            Debug.Log($"[Awake] Füge Talent hinzu: {talent.name}");
            Debug.Log($"[Awake] - passive: {talent.passive}");
            Debug.Log($"[Awake] - myAbility: {(talent.myAbility != null ? talent.myAbility.name : "null")}");
            Debug.Log($"[Awake] - myNode: {(talent.myNode != null ? $"ID={talent.myNode.ID}" : "null")}");
            Debug.Log($"[Awake] - currentCount: {talent.currentCount}");
            
            allTalents.Add(talent);
            talent.SetTalentUIVariables();
        }

        Debug.Log($"[Awake] Finale Anzahl allTalents: {allTalents.Count}");

        if (!PlayerPrefs.HasKey("Load"))
        {
            Debug.Log("[Awake] Kein 'Load' PlayerPref gefunden - resetze Talente");
            ResetTalents();
            UpdateTalentPointText(); // Nur hier aufrufen
        }
        else
        {
            Debug.Log("[Awake] 'Load' PlayerPref gefunden - Talente werden später geladen");
            // UpdateTalentPointText() wird später in PlayerLoad.LoadTalents() aufgerufen
        }

        UpdateTalentPointText();
        Debug.Log("=== [TalentTreeManager.Awake] ENDE ===");
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
        Debug.Log("=== [TryUseTalent] START ===");
        Debug.Log($"[TryUseTalent] Talent: {clickedTalent.name}");
        Debug.Log($"[TryUseTalent] Aktuelle Skillpoints: {PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints()}");
        Debug.Log($"[TryUseTalent] Talent currentCount: {clickedTalent.currentCount}");
        Debug.Log($"[TryUseTalent] Talent unlocked: {clickedTalent.unlocked}");
        Debug.Log($"[TryUseTalent] Talent passive: {clickedTalent.passive}");
        Debug.Log($"[TryUseTalent] Talent myAbility: {(clickedTalent.myAbility != null ? clickedTalent.myAbility.name : "null")}");
        Debug.Log($"[TryUseTalent] Talent myNode: {(clickedTalent.myNode != null ? $"ID={clickedTalent.myNode.ID}" : "null")}");

        //Nur wenn der Spieler auch über Skillpunkte verfügt
        if (PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints() > 0 && clickedTalent.Click())
        {
            Debug.Log($"[TryUseTalent] ✓ Talent-Click erfolgreich! Neue currentCount: {clickedTalent.currentCount}");

            // Skillpoints reduzieren (für alle Talente)
            PlayerManager.instance.player.GetComponent<PlayerStats>().Decrease_SkillPoints(1);
            Debug.Log($"[TryUseTalent] Skillpoints reduziert. Neue Anzahl: {PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints()}");

            UpdateTalentPointText();

            if(clickedTalent.myNode != null)
            {
                Debug.Log($"[TryUseTalent] Node-basiertes Talent verarbeiten");
                clickedTalent.myNode.myCurrentCount++;
                Debug.Log($"[TryUseTalent] Node.myCurrentCount erhöht auf: {clickedTalent.myNode.myCurrentCount}");

                UpdateTalentTree(clickedTalent.myNode);

                //Füge die Eigenschaften des Talents dem Spieler hinzu
                ApplyPassivePointsAndEffects(clickedTalent);
            }

            //Zusätzliche Abfrage für die Root Ability (Sie kann durch ein bool in UseBase nur 1x verwendet werden)
            if (clickedTalent.myAbility != null)
            {
                Debug.Log($"[TryUseTalent] Ability-basiertes Talent verarbeiten");
                clickedTalent.myAbility.UseBase(PlayerStats.instance);
                Debug.Log($"[TryUseTalent] Ability.UseBase() aufgerufen");
            }

            //Füge das Talent der Liste von Talenten hinzu.
            if (!allTalents.Contains(clickedTalent))
            {
                allTalents.Add(clickedTalent);
                Debug.Log($"[TryUseTalent] Talent zur allTalents-Liste hinzugefügt");
            }
            else
            {
                Debug.Log($"[TryUseTalent] Talent bereits in allTalents-Liste vorhanden");
            }

            if(clickedTalent.myNode != null && clickedTalent.myNode.myCurrentCount <= 1)
            {
                Debug.Log($"[TryUseTalent] Prüfe Node-Expansion");
                if (!clickedTalent.myNode.IsExpanded())
                {
                    TalentTreeGenerator.instance.ExpandNode(clickedTalent.myNode);
                    Debug.Log($"[TryUseTalent] Node expandiert");
                }

                TalentTreeGenerator.instance.ExpandBranch(clickedTalent.myNode);
                Debug.Log($"[TryUseTalent] Branch expandiert");
            }

            Debug.Log($"[TryUseTalent] === ENDE === Talent erfolgreich verwendet");
        }
        else
        {
            Debug.LogWarning($"[TryUseTalent] ❌ Talent konnte nicht verwendet werden!");
            Debug.LogWarning($"[TryUseTalent] Grund: Skillpoints: {PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints()}, Talent unlocked: {clickedTalent.unlocked}");
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

    public void ApplyPassivePointsAndEffects(Talent_UI clickedTalent)
    {
        var playerStats = PlayerStats.instance;
        Debug.Log($"[ApplyPassivePointsAndEffects] Talent: {clickedTalent.name}");
        Debug.Log($"[ApplyPassivePointsAndEffects] Anzahl myTypes: {clickedTalent.myTypes.Count}");

        foreach (TalentType type in clickedTalent.myTypes)
        {
            Debug.Log($"[ApplyPassivePointsAndEffects] Verarbeite TalentType: {type}");
            
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
                float modifierValue = clickedTalent.value * 0.01f;
                Debug.Log($"[ApplyPassivePointsAndEffects] Füge Modifier hinzu: {stat} += {modifierValue} (PercentAdd)");
                Debug.Log($"[ApplyPassivePointsAndEffects] Wert vor Modifier: {playerStats.GetStat(stat).Value}");
                
                playerStats.GetStat(stat).AddModifier(new StatModifier(modifierValue, StatModType.PercentAdd));
                
                Debug.Log($"[ApplyPassivePointsAndEffects] Wert nach Modifier: {playerStats.GetStat(stat).Value}");
            }
            else
            {
                Debug.LogWarning($"[ApplyPassivePointsAndEffects] Ungültiger TalentType: {type}");
            }
        }
    }

    public void ResetTalents()
    {
        Debug.Log("[ResetTalents] Alle Talente werden zurückgesetzt.");

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
        // Null-Prüfung hinzufügen
        if(PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            talentPointText.text = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints().ToString();
        }
        else
        {
            Debug.Log("[UpdateTalentPointText] PlayerManager oder Player noch nicht verfügbar - überspringe Aktualisierung");
            // Optional: Setze einen Standardwert
            if (talentPointText != null)
                talentPointText.text = "0";
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
