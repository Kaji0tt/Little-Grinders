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
        Debug.Log("=== [PlayerLoad.LoadPlayer] START ===");
        
        if (data != null)
        {
            // WICHTIG: Seed ZUERST setzen, VOR allem anderen!
            if (TalentTreeGenerator.instance != null && data.talentTreeSeed > 0)
            {
                Debug.Log($"[PlayerLoad.LoadPlayer] Setze TalentTreeSeed: {data.talentTreeSeed}");
                TalentTreeGenerator.instance.SetTalentTreeSeed(data.talentTreeSeed);
            }
            
            Debug.Log("[PlayerLoad.LoadPlayer] LoadPlayerStats...");
            LoadPlayerStats(data);
            
            Debug.Log("[PlayerLoad.LoadPlayer] LoadInventory...");
            LoadInventory(data);
            
            Debug.Log("[PlayerLoad.LoadPlayer] LoadEquippedItems...");
            LoadEquippedItems(data);
            
            Debug.Log("[PlayerLoad.LoadPlayer] LoadSkillPoints...");
            LoadSkillPoints(data);
            
            // TALENTE WERDEN SPÄTER GELADEN - siehe unten!

            if (data.currentMap != null)
            {
                Debug.Log("[PlayerLoad.LoadPlayer] LoadGlobalMap...");
                LoadGlobalMap(data);
            }
        }
        
        Debug.Log("=== [PlayerLoad.LoadPlayer] ENDE ===");
    }

    // NEUE Methode: Talente zeitversetzt laden
    public void LoadTalentsDelayed(PlayerSave data)
    {
        Debug.Log("=== [PlayerLoad.LoadTalentsDelayed] START ===");
        
        // Warte bis TalentTreeGenerator fertig ist
        if (TalentTreeGenerator.instance != null && TalentTreeGenerator.instance.allNodes.Count > 0)
        {
            Debug.Log("[PlayerLoad.LoadTalentsDelayed] TalentTree ist bereit - lade Talente");
            LoadTalents(data);
        }
        else
        {
            Debug.Log("[PlayerLoad.LoadTalentsDelayed] TalentTree noch nicht bereit - starte Coroutine");
            StartCoroutine(WaitForTalentTreeAndLoad(data));
        }
        
        Debug.Log("=== [PlayerLoad.LoadTalentsDelayed] ENDE ===");
    }

    private IEnumerator WaitForTalentTreeAndLoad(PlayerSave data)
    {
        Debug.Log("[WaitForTalentTreeAndLoad] Warte auf TalentTree...");
        
        // Warte bis der TalentTree generiert ist
        while (TalentTreeGenerator.instance == null || 
               TalentTreeGenerator.instance.allNodes == null || 
               TalentTreeGenerator.instance.allNodes.Count == 0)
        {
            yield return new WaitForEndOfFrame();
        }
        
        Debug.Log("[WaitForTalentTreeAndLoad] TalentTree bereit - lade Talente");
        LoadTalents(data);
    }

    private void LoadPlayerStats(PlayerSave data)
    {
        Debug.Log("=== [LoadPlayerStats] START ===");
        
        if (PlayerManager.instance?.player != null)
        {
            PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                Debug.Log($"[LoadPlayerStats] Vor dem Laden: Level={playerStats.level}, XP={playerStats.xp}, HP={playerStats.Get_currentHp()}");

                playerStats.level = data.mySavedLevel;
                playerStats.xp = data.mySavedXp;
                playerStats.Load_currentHp(data.hp);
                playerStats.Set_SkillPoints(data.mySavedSkillpoints);

                Debug.Log($"[LoadPlayerStats] Nach dem Laden: Level={playerStats.level}, XP={playerStats.xp}, HP={playerStats.Get_currentHp()}, Skillpoints={playerStats.Get_SkillPoints()}");
            }
            else
            {
                Debug.LogError("[LoadPlayerStats] ❌ PlayerStats Komponente nicht gefunden!");
            }
        }
        else
        {
            Debug.LogError("[LoadPlayerStats] ❌ PlayerManager.instance oder player ist null!");
        }
        
        Debug.Log("=== [LoadPlayerStats] ENDE ===");
    }

    private void LoadSkillPoints(PlayerSave data)
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        Debug.Log($"[LoadSkillPoints] Lade Skillpunkte: {data.mySavedSkillpoints}");
        playerStats.Set_SkillPoints(data.mySavedSkillpoints);
    }

    private void LoadEquippedItems(PlayerSave data)
    {
        Debug.Log("=== [LoadEquippedItems] START ===");
        Debug.Log($"[LoadEquippedItems] Zu ladende Ausrüstung: {data.mySavedEquip.Count}");
        
        // Finde alle Equip-Slots
        var allSlots = UnityEngine.Object.FindObjectsByType<Int_SlotBtn>(FindObjectsSortMode.None);
        var equipSlots = allSlots.Where(slot => slot.slotType != ItemType.None).ToList();
        
        Debug.Log($"[LoadEquippedItems] Gefundene Equip-Slots: {equipSlots.Count}");
        
        // Entferne aktuelle Ausrüstung
        foreach (var equipSlot in equipSlots)
        {
            Debug.Log($"[LoadEquippedItems] Clearing slot {equipSlot.slotType}");
            equipSlot.ClearSlot();
        }

        // Setze gespeicherte Ausrüstung
        foreach (var kvp in data.mySavedEquip)
        {
            string slotKey = kvp.Key;
            SavedItem savedItem = kvp.Value;

            Debug.Log($"[LoadEquippedItems] Versuche zu laden: {slotKey} -> {savedItem.itemID}");

            var equipSlot = equipSlots.FirstOrDefault(slot => slot.slotType.ToString().ToUpper() == slotKey);
            
            if (equipSlot != null)
            {
                ItemInstance itemInstance = CreateItemInstanceFromSave(savedItem);
                
                if (itemInstance != null)
                {
                    Debug.Log($"[LoadEquippedItems] Equipping {itemInstance.GetName()} in slot {equipSlot.slotType}");
                    equipSlot.StoreItem(itemInstance);
                    
                    Debug.Log($"[LoadEquippedItems] Applying stats for {itemInstance.GetName()}");
                    itemInstance.Equip(PlayerManager.instance.player.playerStats);
                    UI_Manager.instance.UpdateAbilityForEquippedItem(itemInstance);
                }
                else
                {
                    Debug.LogError($"[LoadEquippedItems] ❌ Fehler beim Erstellen von ItemInstance für {savedItem.itemID}");
                }
            }
            else
            {
                Debug.LogWarning($"[LoadEquippedItems] ⚠️ Kein Equip-Slot gefunden für Typ {slotKey}");
            }
        }
        
        Debug.Log("=== [LoadEquippedItems] ENDE ===");
    }
    
    private void LoadInventory(PlayerSave data)
    {
        Debug.Log("=== [LoadInventory] START ===");
        Debug.Log($"[LoadInventory] Zu ladende Items: {data.mySavedInventory.Count}");
        
        if (UI_Inventory.instance?.inventory != null)
        {
            var inventory = UI_Inventory.instance.inventory;
            
            // Leere das Inventar vorher
            Debug.Log("[LoadInventory] Leere aktuelles Inventar...");
            for (int i = 0; i < UI_Inventory.instance.inventorySlots.Count; i++)
                inventory.RemoveItemAtIndex(i);

            foreach (var savedItem in data.mySavedInventory)
            {
                ItemInstance itemInstance = CreateItemInstanceFromSave(savedItem);
                if (itemInstance != null)
                {
                    inventory.AddItemAtIndex(itemInstance, savedItem.slotIndex);
                    Debug.Log($"[LoadInventory] Item geladen: Slot {savedItem.slotIndex} -> {itemInstance.GetName()}");
                }
                else
                {
                    Debug.LogError($"[LoadInventory] ❌ Fehler beim Laden von Item: {savedItem.itemID}");
                }
            }
            
            Debug.Log($"[LoadInventory] Inventar erfolgreich geladen");
        }
        else
        {
            Debug.LogError("[LoadInventory] ❌ UI_Inventory.instance oder inventory ist null!");
        }
        
        Debug.Log("=== [LoadInventory] ENDE ===");
    }

    private void LoadTalents(PlayerSave data)
    {
        Debug.Log("=== [LoadTalents] START ===");
        Debug.Log($"[LoadTalents] Anzahl gespeicherte Talente: {data.mySavedTalents.Count}");
        Debug.Log($"[LoadTalents] Anzahl allTalents: {TalentTreeManager.instance.allTalents.Count}");
        
        int loadedCount = 0;

        // Alle gespeicherten Talente durchgehen
        for (int i = 0; i < data.mySavedTalents.Count; i++)
        {
            TalentSave save = data.mySavedTalents[i];
            Debug.Log($"[LoadTalents] === TALENT {i+1}/{data.mySavedTalents.Count} ===");
            Debug.Log($"[LoadTalents] isAbilityTalent: {save.isAbilityTalent}");
            Debug.Log($"[LoadTalents] currentCount: {save.currentCount}");
            Debug.Log($"[LoadTalents] nodeID: {save.nodeID}");
            Debug.Log($"[LoadTalents] abilityName: '{save.abilityName}'");

            bool talentFound = false;

            if (save.isAbilityTalent)
            {
                Debug.Log($"[LoadTalents] Suche Ability-Talent mit Name: '{save.abilityName}'");
                
                // Lade Ability-basierte Talente
                foreach (Talent_UI talent in TalentTreeManager.instance.allTalents)
                {
                    Debug.Log($"[LoadTalents] - Prüfe Talent '{talent.name}': myAbility={(talent.myAbility != null ? talent.myAbility.name : "null")}, passive={talent.passive}");
                    
                    if (talent.myAbility != null && talent.myAbility.name == save.abilityName)
                    {
                        Debug.Log($"[LoadTalents] ✓ MATCH! Lade Ability-Talent: {talent.name}");
                        talent.Set_currentCount(save.currentCount);
                        talent.Unlock();
                        
                        //Hardcoded fix für Regeneration
                        if (PlayerStats.instance.Regeneration.Value < 1)
                        {
                            PlayerStats.instance.Regeneration.AddModifier(new StatModifier(1, StatModType.Flat));
                            Debug.Log($"[LoadTalents] Regeneration hinzugefügt, neuer Wert: {PlayerStats.instance.Regeneration.Value}");
                        }

                        // WICHTIG: UI aktualisieren!
                        talent.SetTalentUIVariables();
                        talent.UpdateTalent();
                        
                        // Falls es das defaultTalent ist, Root-Nodes freischalten
                        if (talent == TalentTreeManager.instance.defaultTalent)
                        {
                            Debug.Log($"[LoadTalents] DefaultTalent geladen - schalte Root-Nodes frei");
                            UnlockRootNodes();
                        }
                        
                        Debug.Log($"[LoadTalents] Ability-Talent geladen: {talent.name}, Count={save.currentCount}");
                        loadedCount++;
                        talentFound = true;
                        break;
                    }
                }
                
                if (!talentFound)
                {
                    Debug.LogError($"[LoadTalents] ❌ Ability-Talent nicht gefunden: '{save.abilityName}'");
                }
            }
            else
            {
                Debug.Log($"[LoadTalents] Suche Node-Talent mit ID: {save.nodeID}");
                
                if (TalentTreeGenerator.instance != null && TalentTreeGenerator.instance.allNodes != null)
                {
                    Debug.Log($"[LoadTalents] TalentTreeGenerator verfügbar mit {TalentTreeGenerator.instance.allNodes.Count} Nodes");
                    
                    // Suche zuerst in den generierten Nodes
                    TalentNode foundNode = TalentTreeGenerator.instance.allNodes.FirstOrDefault(n => n.ID == save.nodeID);
                    
                    if (foundNode != null)
                    {
                        Debug.Log($"[LoadTalents] ✓ Node gefunden in TalentTreeGenerator: ID={foundNode.ID}");
                        
                        // Setze die Werte in der Node
                        foundNode.myCurrentCount = save.currentCount;
                        foundNode.Unlock();
                        
                        // Finde das zugehörige UI-Element, falls vorhanden
                        if (foundNode.myTalentUI != null)
                        {
                            Debug.Log($"[LoadTalents] ✓ UI-Element gefunden: {foundNode.myTalentUI.name}");
                            foundNode.myTalentUI.Set_currentCount(save.currentCount);
                            foundNode.myTalentUI.Unlock();
                            foundNode.myTalentUI.SetTalentUIVariables(); // UI-Text aktualisieren
                            foundNode.myTalentUI.UpdateTalent();
                        }
                        else
                        {
                            Debug.LogWarning($"[LoadTalents] Node hat kein UI-Element: ID={foundNode.ID}");
                        }
                        
                        TalentTreeManager.instance.UpdateTalentTree(foundNode);
                        Debug.Log($"[LoadTalents] Node-Talent geladen: NodeID={save.nodeID}, Count={save.currentCount}");
                        loadedCount++;
                        talentFound = true;
                    }
                    else
                    {
                        Debug.LogError($"[LoadTalents] ❌ Node nicht gefunden in TalentTreeGenerator: ID={save.nodeID}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[LoadTalents] ⚠️ TalentTreeGenerator oder allNodes ist null");
                }
                
                // Fallback: Suche in allTalents (für den Fall, dass doch UI-Elemente vorhanden sind)
                if (!talentFound)
                {
                    Debug.Log($"[LoadTalents] Fallback: Suche in TalentTreeManager.allTalents");
                    foreach (Talent_UI talent in TalentTreeManager.instance.allTalents)
                    {
                        Debug.Log($"[LoadTalents] - Prüfe Talent '{talent.name}': myNode={(talent.myNode != null ? $"ID={talent.myNode.ID}" : "null")}, passive={talent.passive}");
                        
                        if (talent.myNode != null && talent.myNode.ID == save.nodeID)
                        {
                            Debug.Log($"[LoadTalents] ✓ FALLBACK MATCH! Lade Node-Talent: {talent.name}");
                            talent.Set_currentCount(save.currentCount);
                            talent.myNode.myCurrentCount = save.currentCount;
                            talent.Unlock();
                            talent.myNode.Unlock();
                            talent.SetTalentUIVariables(); // UI-Text aktualisieren
                            talent.UpdateTalent();
                            TalentTreeManager.instance.UpdateTalentTree(talent.myNode);
                            Debug.Log($"[LoadTalents] Fallback: Node-Talent geladen: NodeID={save.nodeID}, Count={save.currentCount}");
                            loadedCount++;
                            talentFound = true;
                            break;
                        }
                    }
                }
                
                if (!talentFound)
                {
                    Debug.LogError($"[LoadTalents] ❌ Node-Talent konnte nirgends gefunden werden: ID={save.nodeID}");
                }
            }
        }

        Debug.Log($"[LoadTalents] === ENDE === Insgesamt geladen: {loadedCount} Talente");
        
        // Debug: Zeige finale Zustände aller Talente
        Debug.Log("=== [LoadTalents] FINALE ZUSTÄNDE ===");
        foreach (Talent_UI talent in TalentTreeManager.instance.allTalents)
        {
            Debug.Log($"[LoadTalents] Final: '{talent.name}' - Count: {talent.currentCount}, Unlocked: {talent.unlocked}, Ability: {(talent.myAbility != null ? talent.myAbility.name : "null")}, Node: {(talent.myNode != null ? $"ID={talent.myNode.ID}" : "null")}");
        }
        
        // WICHTIG: Nach dem Laden alle UI-Elemente aktualisieren und Root-Nodes prüfen
        UpdateAllTalentUI();
    }

    private void UnlockRootNodes()
    {
        Debug.Log("[UnlockRootNodes] Prüfe und schalte Root-Nodes frei");
        
        if (TalentTreeGenerator.instance != null && TalentTreeGenerator.instance.allNodes != null)
        {
            foreach (TalentNode node in TalentTreeGenerator.instance.allNodes)
            {
                if (node.Depth == 0) // Root-Nodes
                {
                    Debug.Log($"[UnlockRootNodes] Root-Node gefunden: ID={node.ID}");
                    
                    // Prüfe, ob Regeneration >= 1 ist (Bedingung für Root-Node-Unlock)
                    if (PlayerStats.instance.Regeneration.Value >= 1)
                    {
                        node.Unlock();
                        
                        // Aktualisiere auch das UI-Element, falls vorhanden
                        if (node.myTalentUI != null)
                        {
                            node.myTalentUI.Unlock();
                            node.myTalentUI.SetTalentUIVariables();
                            node.myTalentUI.UpdateTalent();
                            Debug.Log($"[UnlockRootNodes] Root-Node freigeschaltet: ID={node.ID}");
                        }
                        else
                        {
                            Debug.LogWarning($"[UnlockRootNodes] Root-Node hat kein UI-Element: ID={node.ID}");
                        }
                    }
                    else
                    {
                        Debug.Log($"[UnlockRootNodes] Regeneration zu niedrig für Root-Node: {PlayerStats.instance.Regeneration.Value}");
                    }
                }
            }
        }
    }

    private void UpdateAllTalentUI()
    {
        Debug.Log("[UpdateAllTalentUI] Aktualisiere alle Talent-UIs");
        
        // Aktualisiere alle Talent_UI Elemente
        foreach (Talent_UI talent in TalentTreeManager.instance.allTalents)
        {
            talent.SetTalentUIVariables();
            talent.UpdateTalent();
            Debug.Log($"[UpdateAllTalentUI] UI aktualisiert für: {talent.name}");
        }
        
        // Aktualisiere auch die Skillpoint-Anzeige
        TalentTreeManager.instance.UpdateTalentPointText();
        Debug.Log("[UpdateAllTalentUI] Alle UIs aktualisiert");
    }

    private void LoadGlobalMap(PlayerSave data)
    {
        Debug.Log("=== [LoadGlobalMap] START ===");
        Debug.Log($"[LoadGlobalMap] ExploredMaps Count: {data.exploredMaps.Count}");
        Debug.Log($"[LoadGlobalMap] CurrentMap null: {data.currentMap == null}");
        Debug.Log($"[LoadGlobalMap] LastSpawnpoint: {data.lastSpawnpoint}");
        
        if (data.exploredMaps.Count != 0)
        {
            if (GlobalMap.instance != null)
            {
                GlobalMap.instance.exploredMaps = data.exploredMaps;
                GlobalMap.instance.Set_CurrentMap(data.currentMap);
                GlobalMap.instance.lastSpawnpoint = data.lastSpawnpoint;
                
                Debug.Log("[LoadGlobalMap] ✓ GlobalMap erfolgreich geladen");
                Debug.Log($"[LoadGlobalMap] Geladene exploredMaps: {GlobalMap.instance.exploredMaps.Count}");
            }
            else
            {
                Debug.LogError("[LoadGlobalMap] ❌ GlobalMap.instance ist null!");
            }
        }
        else
        {
            Debug.LogError("[LoadGlobalMap] ❌ Keine exploredMaps gefunden - etwas ist falsch!");
        }
        
        Debug.Log("=== [LoadGlobalMap] ENDE ===");
    }

    // Hilfsmethode zum Erzeugen einer ItemInstance aus SavedItem
    private ItemInstance CreateItemInstanceFromSave(SavedItem savedItem)
    {
        Debug.Log($"[CreateItemInstanceFromSave] Erstelle ItemInstance für: {savedItem.itemID}");
        
        ItemInstance instance = ItemInstance.FromSave(savedItem);
        
        if (instance == null)
        {
            Debug.LogError($"[CreateItemInstanceFromSave] ❌ Fehler beim Erstellen der ItemInstance für ID: {savedItem.itemID}");
            return null;
        }

        Debug.Log($"[CreateItemInstanceFromSave] ✓ ItemInstance erfolgreich erstellt: {instance.ItemName}");
        return instance;
    }
}

