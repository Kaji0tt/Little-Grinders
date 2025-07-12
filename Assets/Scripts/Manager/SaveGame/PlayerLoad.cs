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
        LoadEquippedItems(data);
        LoadSkillPoints(data);
        LoadInventory(data);
        LoadTalents(data);

        if (data.currentMap != null)
            LoadGlobalMap(data);
    }

    private void LoadPlayerStats(PlayerSave data)
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        playerStats.level = data.mySavedLevel;
        playerStats.xp = data.mySavedXp;
        playerStats.Load_currentHp(data.hp);
        playerStats.Set_SkillPoints(data.mySavedSkillpoints);
    }

    private void LoadSkillPoints(PlayerSave data)
    {
        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        playerStats.Set_SkillPoints(data.mySavedSkillpoints);
    }

    private void LoadEquippedItems(PlayerSave data)
    {
        // Entferne aktuelle Ausrüstung
        Int_SlotBtn[] slotButtons = GameObject.FindObjectsOfType<Int_SlotBtn>();
        foreach (var slotBtn in slotButtons)
        {
            slotBtn.ClearSlot(); // Falls vorhanden, Methode zum Leeren des Slots
        }

        // Setze gespeicherte Ausrüstung
        foreach (var kvp in data.mySavedEquip)
        {
            string slotKey = kvp.Key;
            SavedItem savedItem = kvp.Value;

            // Finde passenden SlotButton anhand des ItemType
            var slotBtn = slotButtons.FirstOrDefault(btn => btn.slotType.ToString().ToUpper() == slotKey);
            if (slotBtn != null)
            {
                // Hole das Inventar des Spielers
                var inventory = PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory;

                // Erzeuge ItemInstance aus SavedItem
                ItemInstance itemInstance = CreateItemInstanceFromSave(savedItem);
                slotBtn.SetItem(itemInstance, inventory); // Methode zum Setzen des Items im Slot
            }
        }
    }

    private void LoadInventory(PlayerSave data)
    {
        var inventory = PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory;
        //inventory.Clear(); // Methode zum Leeren des Inventars

        foreach (var savedItem in data.mySavedInventory)
        {
            ItemInstance itemInstance = CreateItemInstanceFromSave(savedItem);
            inventory.AddItem(itemInstance, savedItem.amount);
        }
    }

    private void LoadTalents(PlayerSave data)
    {
        // Die K.I. hat hier einen Clear veranlasst, der vorerst entfernt wurde.         

        // Lade geskillte Talente
        foreach (TalentSave save in data.mySavedTalents)
        {
            TalentNode node = TalentTreeGenerator.instance.allNodes.FirstOrDefault(n => n.ID == save.nodeID);
            if (node != null)
            {
                node.myCurrentCount = save.currentCount;
                node.Unlock();
                // Optional: weitere Felder setzen
            }
        }
    }

    private void LoadGlobalMap(PlayerSave data)
    {
        if (data.exploredMaps.Count != 0)
        {
            GlobalMap.instance.exploredMaps = data.exploredMaps;
            GlobalMap.instance.Set_CurrentMap(data.currentMap);
            GlobalMap.instance.lastSpawnpoint = data.lastSpawnpoint;
        }
        else
        {
            print("something is wrong with the playerload");
        }
    }

    // Hilfsmethode zum Erzeugen einer ItemInstance aus SavedItem
    private ItemInstance CreateItemInstanceFromSave(SavedItem savedItem)
    {
        Item baseItem = ItemDatabase.GetItemByID(savedItem.itemID);
        ItemInstance instance = new ItemInstance(baseItem);

        if (Enum.TryParse<Rarity>(savedItem.rarity, out var rarity))
            instance.itemRarity = rarity;

        instance.addedItemMods.Clear();
        foreach (var modSave in savedItem.mods)
        {
            var modDef = ItemDatabase.instance.GetModDefinitionByName(modSave.modName);
            var mod = new ItemMod();
            mod.definition = modDef;
            mod.rolledRarity = Enum.TryParse<Rarity>(modSave.modRarity, out var modRarity) ? modRarity : Rarity.Common;
            instance.addedItemMods.Add(mod);
        }

        return instance;
    }
}

