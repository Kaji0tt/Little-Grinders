using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLoad : MonoBehaviour
{

    public void LoadPlayer(PlayerSave data)
    {
        LoadPlayerStats(data);

        //Die Gegenstände werden initialisiert und neu angezogen.
        LoadEquippedItems(data);

        //Skillpunkte werden geladen.
        LoadSkillPoints(data);

        // Inventar wird geladen.
        LoadInventory(data);

        // ActionButtons werden geladen.
        LoadActionbar(data);

        //Map-Daten werden geladen. 
        if(data.currentMap != null)
        LoadGlobalMap(data);
        
    }
    /*
    public void LoadScenePlayer(PlayerSave data)
    {
        LoadPlayerStats(data);

        //Die Gegenstände werden initialisiert und neu angezogen.
        LoadEquippedItems(data);

        //Skillpunkte werden geladen.
        LoadSkillPoints(data);

        // Inventar wird geladen.
        LoadInventory(data);

        // ActionButtons werden geladen.
        LoadActionbar(data);

    }
    */

    private void LoadActionbar(PlayerSave data)
    {
        ActionButton[] actionButtons = FindObjectsOfType<ActionButton>();

        foreach (ActionButton slot in actionButtons)
        {
            if (slot.gameObject.name.StartsWith("ActionButton"))
            {
                // Extrahiere die Slot-Nummer dynamisch aus dem Namen (z.B. "ActionButton1" => 0)
                int slotIndex = int.Parse(slot.gameObject.name.Replace("ActionButton", "")) - 1;

                // Lade den Slot mit der entsprechenden gespeicherten Aktion
                LoadActionbarSlot(slotIndex, slot, data);
            }
        }
    }

    private void LoadInventory(PlayerSave data)
    {
        int currentItem = 0;

        foreach (string item in data.inventorySave)
        {

            PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.AddItem(ItemRolls.GetItemStats(item, data.inventoryItemMods[currentItem], data.inventoryItemRarity[currentItem]));

            currentItem += 1;

        }
    }

    private void LoadSkillPoints(PlayerSave data)
    {
        TalentTree talentTree = TalentTree.instance;

        talentTree.ResetTalents();

        foreach (TalentSave savedTalent in data.talentsToBeSaved)
        {
            //Debug.Log("loading Talent: " + savedTalent.talentName + " loading it for " + talentTree.gameObject.name);
            for (int i = 0; i < talentTree.allTalents.Length; i++)
            {
                if (talentTree.allTalents[i].name == savedTalent.talentName)
                {
                    talentTree.allTalents[i].Set_currentCount(savedTalent.talentPoints);

                    talentTree.allTalents[i].unlocked = savedTalent.unlocked;

                    talentTree.allTalents[i].UpdateTalent();
                }

            }


            talentTree.UpdateTalentPointText();

            talentTree.totalVoidSpecPoints = data.savedVP; 
            
            talentTree.totalUtilitySpecPoints = data.savedLP; 
            
            talentTree.totalCombatSpecPoints = data.savedCP;



        }


        foreach (Talent talent in talentTree.allTalents)
        {
            if (talent.unlocked)
            {
                //Falls es sich nicht um ein AbilityTalent handelt, füge die Effekte hinzu.
                if (talent.GetType() != typeof(AbilityTalent))
                {

                    for (int i = 1; i <= talent.currentCount; i++)
                        //talent.ApplyPassivePointsAndEffects(talent); Ehemaliger Ansatz, aber ApplyPassivePointsAndEffects sieht hinfällig im vgl. TryUseTalent aus
                        TalentTree.instance.TryUseTalent(talent);
                }
                else
                {
                    foreach(TalentSave savedTalent in data.talentsToBeSaved)
                    if(talent.talentName == savedTalent.talentName)
                    {
                            //print("JUHU; FOUND: " + savedTalent.talentName + ", got the Spec: " + (Ability.AbilitySpecialization)savedTalent.spec);
                            //print("Talent:" + talent.talentName + " got the Spec:" + talent.abilityTalent.baseAbility.abilitySpec);
                            talent.abilityTalent.baseAbility.abilitySpec = (Ability.AbilitySpecialization)savedTalent.spec;
                        }

                }

                talent.Unlock();
            }

            else
            {
                talent.LockTalent();
            }

        }
    }

    private void LoadEquippedItems(PlayerSave data)
    {
        if (data.brust != null)
            FindObjectOfType<EQSlotBrust>().LoadItem(ItemRolls.GetItemStats(data.brust, data.modsBrust, data.brust_r));

        if (data.hose != null)
            FindObjectOfType<EQSlotHose>().LoadItem(ItemRolls.GetItemStats(data.hose, data.modsHose, data.hose_r));

        if (data.kopf != null)
            FindObjectOfType<EQSlotKopf>().LoadItem(ItemRolls.GetItemStats(data.kopf, data.modsKopf, data.kopf_r));

        if (data.schuhe != null)
            FindObjectOfType<EQSlotSchuhe>().LoadItem(ItemRolls.GetItemStats(data.schuhe, data.modsSchuhe, data.schuhe_r));

        if (data.schmuck != null)
            FindObjectOfType<EQSlotSchmuck>().LoadItem(ItemRolls.GetItemStats(data.schmuck, data.modsSchmuck, data.schmuck_r));

        if (data.weapon != null)
            FindObjectOfType<EQSlotWeapon>().LoadItem(ItemRolls.GetItemStats(data.weapon, data.modsWeapon, data.weapon_r));
    }

    private void LoadPlayerStats(PlayerSave data)
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        playerStats.level = data.level;

        playerStats.xp = data.xp;

        playerStats.Load_currentHp(data.Hp);

        playerStats.Set_SkillPoints(data.skillPoints);
    }

    private void LoadGlobalMap(PlayerSave data)
    {
        if (data.exploredMaps.Count != 0)
        {
            print("GlobalMap should be loading: " + data.exploredMaps.Count + " maps.");
            GlobalMap.instance.exploredMaps = data.exploredMaps;

            GlobalMap.instance.Set_CurrentMap(data.currentMap);

            GlobalMap.instance.lastSpawnpoint = data.lastSpawnpoint;

        }
        else
            print("something is wrong with the playerlaod");


    }

    void LoadActionbarSlot(int i, ActionButton slot, PlayerSave data)
    {
        // Überprüfen, ob Daten für diesen Slot vorhanden sind
        if (data.savedActionButtons[i] != null)
        {
            bool itemLoaded = false;

            // Überprüfen, ob der gespeicherte Name einem Zauber entspricht
            foreach (Talent talent in TalentTree.instance.allTalents)
            {
                if (talent.abilityTalent.baseAbility.abilityName == data.savedActionButtons[i])
                {
                    slot.LoadAbilityUseable(talent.abilityTalent); // Zauber in den Slot laden
                    itemLoaded = true; // Markiere, dass ein Item geladen wurde
                    break; // Beende die Schleife, wenn der Zauber gefunden wurde
                }
            }

            // Überprüfen, ob der gespeicherte Name einem Item entspricht (wenn noch nichts geladen wurde)
            if (!itemLoaded)
            {
                // Hole das Item basierend auf der ID
                Item item = ItemDatabase.GetItemByID(data.savedActionButtons[i]);

                // Erstelle eine neue ItemInstance aus dem Item, wenn es gefunden wurde
                if (item != null)
                {
                    ItemInstance itemInstance = new ItemInstance(item); // Erstelle eine neue ItemInstance
                    slot.LoadItemUseable(itemInstance); // ItemInstance in den Slot laden
                    itemLoaded = true;
                }
            }

            // Füge eine Fehlerüberprüfung hinzu
            if (!itemLoaded)
            {
                Debug.LogWarning($"Kein Zauber oder Item gefunden für Actionbar-Slot {i}: {data.savedActionButtons[i]}");
            }
        }
        else
        {
            Debug.LogWarning($"Keine gespeicherten Daten für Actionbar-Slot {i}");
        }
    }


}

