using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum SpawnPoint
{
    SpawnRight,
    SpawnLeft, 
    SpawnTop,
    SpawnBot
}

[System.Serializable]
public class SavedItem
{
    public string itemID;
    public string rarity;
    public List<ItemModSave> mods = new List<ItemModSave>();
    public int slotIndex;
    
    // Neue Felder für die gewürfelten Stats
    public Dictionary<string, int> flatStats = new Dictionary<string, int>();
    public Dictionary<string, float> percentStats = new Dictionary<string, float>();
    public int requiredLevel = 1;
    public string itemName;
    public string itemDescription;
}

[System.Serializable]
public class ActionbarSlotSave
{
    public string slotType; // "Ability" oder "Item"
    public string id;       // AbilityName oder ItemID
}

[System.Serializable]
public class ItemModSave
{
    public string modName;
    public float savedValue;
    public string modRarity;
}

[System.Serializable]
public class TalentSave
{
    public int nodeID;
    public int currentCount;
    public string abilityName; // Für Ability-basierte Talente
    public bool isAbilityTalent; // Unterscheidet zwischen Node- und Ability-basierten Talenten
    // Optional: public List<TalentType> types;
}

[System.Serializable]
public class PlayerSave
{
    // Spieler-Stats
    public int mySavedLevel;
    public float hp, armor, attackPower, abilityPower, movementSpeed, attackSpeed, regeneration, criticalChance, criticalDamage;
    public int mySavedXp;
    public float[] mySavedPosition;

    // Ausrüstungsslots (z.B. "Brust", "Kopf", ...)
    public Dictionary<string, SavedItem> mySavedEquip = new Dictionary<string, SavedItem>();

    // Inventar
    public List<SavedItem> mySavedInventory = new List<SavedItem>();

    // Talente
    public int talentTreeSeed;
    public List<TalentSave> mySavedTalents = new List<TalentSave>();
    public int mySavedSkillpoints;

    // Szene/Map
    public int currentScene;
    public SpawnPoint lastSpawnpoint = SpawnPoint.SpawnRight; // Enum statt string
    public float globalMapX, globalMapY;
    public List<MapSave> exploredMaps = new List<MapSave>();
    public MapSave currentMap;




    // Konstruktor, Save-Methoden etc.
    public PlayerSave()
    {
        Debug.Log("=== [PlayerSave.Constructor] START ===");
        
        ///Player speichern.
        Debug.Log("[PlayerSave.Constructor] SaveThePlayer...");
        SaveThePlayer();

        /// Items Speichern
        Debug.Log("[PlayerSave.Constructor] SaveTheInventory...");
        SaveTheInventory();
        Debug.Log("[PlayerSave.Constructor] SaveTheEquipment...");
        SaveTheEquipment();

        ///Skill-Points speichern.
        Debug.Log("[PlayerSave.Constructor] SaveTheTalents...");
        SaveTheTalents();

        /// Aktuelle Position speichern.
        Debug.Log("[PlayerSave.Constructor] Initialisiere Position Array...");
        mySavedPosition = new float[3];

        ///Save GlobalMap Settings
        Debug.Log($"[PlayerSave.Constructor] Aktuelle Szene buildIndex: {SceneManager.GetActiveScene().buildIndex}");
        if (SceneManager.GetActiveScene().buildIndex != 1)
        {
            Debug.Log("[PlayerSave.Constructor] SaveTheGlobalMap...");
            SaveTheGlobalMap();
        }
        else
        {
            Debug.Log("[PlayerSave.Constructor] Überspringe GlobalMap Save (Tutorial-Szene)");
        }
        
        Debug.Log("=== [PlayerSave.Constructor] ENDE ===");
    }

    private void SaveTheGlobalMap()
    {
        Debug.Log("=== [SaveTheGlobalMap] START ===");
        
        //GlobalMap only gets saved as the player is in Scene 2.
        currentScene = SceneManager.GetActiveScene().buildIndex;
        Debug.Log($"[SaveTheGlobalMap] CurrentScene gesetzt auf: {currentScene}");

        if (GlobalMap.instance != null)
        {
            // Update current map with latest interactable states before saving
            if (GlobalMap.instance.currentMap != null)
            {
                Debug.Log("[SaveTheGlobalMap] Updating current map interactables...");
                GlobalMap.instance.currentMap.SaveInteractables();
            }
            
            exploredMaps = GlobalMap.instance.exploredMaps;
            currentMap = GlobalMap.instance.currentMap;
            lastSpawnpoint = GlobalMap.instance.lastSpawnpoint;
            globalMapX = GlobalMap.instance.currentPosition.x; 
            globalMapY = GlobalMap.instance.currentPosition.y;
            
            Debug.Log($"[SaveTheGlobalMap] ExploredMaps: {exploredMaps.Count}");
            Debug.Log($"[SaveTheGlobalMap] CurrentMap null: {currentMap == null}");
            Debug.Log($"[SaveTheGlobalMap] LastSpawnpoint: {lastSpawnpoint}");
            Debug.Log($"[SaveTheGlobalMap] GlobalMap Position: ({globalMapX}, {globalMapY})");
        }
        else
        {
            Debug.LogError("[SaveTheGlobalMap] ❌ GlobalMap.instance ist null!");
        }
        
        Debug.Log("=== [SaveTheGlobalMap] ENDE ===");
    }

    /// <summary>
    /// Speichert die Spieler-Stats.   
    private void SaveThePlayer()
    {
        Debug.Log("=== [SaveThePlayer] START ===");
        
        if (PlayerManager.instance?.player != null)
        {
            var playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                mySavedLevel = playerStats.level;
                hp = playerStats.Get_currentHp();
                mySavedXp = playerStats.xp;
                mySavedSkillpoints = playerStats.Get_SkillPoints();
                
                Debug.Log($"[SaveThePlayer] Level: {mySavedLevel}");
                Debug.Log($"[SaveThePlayer] HP: {hp}");
                Debug.Log($"[SaveThePlayer] XP: {mySavedXp}");
                Debug.Log($"[SaveThePlayer] Skillpoints: {mySavedSkillpoints}");
            }
            else
            {
                Debug.LogError("[SaveThePlayer] ❌ PlayerStats Komponente nicht gefunden!");
            }
        }
        else
        {
            Debug.LogError("[SaveThePlayer] ❌ PlayerManager.instance oder player ist null!");
        }
        
        Debug.Log("=== [SaveThePlayer] ENDE ===");
    }

    /// <summary>
    /// Speichert das Inventar.
    /// </summary>
    private void SaveTheInventory()
    {
        Debug.Log("=== [SaveTheInventory] START ===");
        
        if (UI_Inventory.instance?.inventory != null)
        {
            var inventory = UI_Inventory.instance.inventory;
            mySavedInventory.Clear();

            Debug.Log($"[SaveTheInventory] Inventar ItemDict Count: {inventory.itemDict.Count}");
            
            foreach (var kvp in inventory.itemDict.OrderBy(kvp => kvp.Key))
            {
                var item = kvp.Value;
                if (item != null)
                {
                    var saved = PlayerSave.SaveFromInstance(item);
                    saved.slotIndex = kvp.Key;
                    mySavedInventory.Add(saved);
                    Debug.Log($"[SaveTheInventory] Item gespeichert: Slot {kvp.Key} -> {item.GetName()}");
                }
            }
            
            Debug.Log($"[SaveTheInventory] Insgesamt gespeicherte Items: {mySavedInventory.Count}");
        }
        else
        {
            Debug.LogError("[SaveTheInventory] ❌ UI_Inventory.instance oder inventory ist null!");
        }
        
        Debug.Log("=== [SaveTheInventory] ENDE ===");
    }

    /// <summary>
    /// Speichert die ausgerüsteten Gegenstände.
    /// </summary>
    private void SaveTheEquipment()
    {
        Debug.Log("=== [SaveTheEquipment] START ===");
        
        mySavedEquip.Clear();

        // Suche alle Equip-Slots über FindObjectsByType
        var allSlots = UnityEngine.Object.FindObjectsByType<Int_SlotBtn>(FindObjectsSortMode.None);
        var equipSlots = allSlots.Where(slot => slot.slotType != ItemType.None).ToList();

        Debug.Log($"[SaveTheEquipment] Gefundene Equip-Slots: {equipSlots.Count}");

        foreach (var equipSlot in equipSlots)
        {
            var item = equipSlot.GetEquippedItem();
            
            if (item != null)
            {
                string slotKey = equipSlot.slotType.ToString().ToUpper();
                var savedItem = PlayerSave.SaveFromInstance(item);
                savedItem.slotIndex = equipSlot.slotIndex;
                mySavedEquip[slotKey] = savedItem;
                
                Debug.Log($"[SaveTheEquipment] Ausrüstung gespeichert: {slotKey} -> {item.GetName()}");
            }
            else
            {
                Debug.Log($"[SaveTheEquipment] Leerer Slot: {equipSlot.slotType}");
            }
        }

        Debug.Log($"[SaveTheEquipment] Insgesamt gespeicherte Ausrüstung: {mySavedEquip.Count}");
        Debug.Log("=== [SaveTheEquipment] ENDE ===");
    }

    public static SavedItem SaveFromInstance(ItemInstance itemInstance)
    {
        var save = new SavedItem();
        save.itemID = itemInstance.ItemID;
        save.rarity = itemInstance.itemRarity.ToString();
        save.requiredLevel = itemInstance.requiredLevel;
        save.itemName = itemInstance.ItemName;
        save.itemDescription = itemInstance.ItemDescription;

        // Speichere die gewürfelten Flat Stats
        foreach (var kvp in itemInstance.flatStats)
        {
            save.flatStats[kvp.Key.ToString()] = kvp.Value;
        }

        // Speichere die gewürfelten Percent Stats  
        foreach (var kvp in itemInstance.percentStats)
        {
            save.percentStats[kvp.Key.ToString()] = kvp.Value;
        }

        // Mods speichern
        foreach (var mod in itemInstance.addedItemMods)
        {
            var modSave = new ItemModSave();
            modSave.modName = mod.definition.modName;
            modSave.savedValue = mod.rolledValue;
            modSave.modRarity = mod.rolledRarity.ToString();
            save.mods.Add(modSave);
        }

        Debug.Log($"[SaveFromInstance] Item gespeichert: {itemInstance.ItemName} mit {save.flatStats.Count} Flat-Stats und {save.percentStats.Count} Percent-Stats");

        return save;
    }
    
    /// <summary>
    /// Speichert alle geskillten Talente.
    /// </summary>
    private void SaveTheTalents()
    {
        mySavedTalents.Clear();
        Debug.Log("=== [SaveTheTalents] START ===");
        Debug.Log($"[SaveTheTalents] Anzahl allTalents: {TalentTreeManager.instance.allTalents.Count}");
        Debug.Log($"[SaveTheTalents] Anzahl allNodes: {TalentTreeGenerator.instance.allNodes.Count}");

        int savedCount = 0;
        
        // Seed vom TalentTreeGenerator holen, nicht selbst erstellen!
        if (TalentTreeGenerator.instance != null)
        {
            talentTreeSeed = TalentTreeGenerator.instance.GetTalentTreeSeed();
            Debug.Log($"[SaveTheTalents] TalentTreeSeed von Generator geholt: {talentTreeSeed}");
        }
        else
        {
            Debug.LogWarning("[SaveTheTalents] TalentTreeGenerator.instance ist null - behalte aktuellen Seed");
        }


        // 1. Speichere Ability-basierte Talente (aus allTalents)
        foreach (Talent_UI talent in TalentTreeManager.instance.allTalents)
        {
            Debug.Log($"[SaveTheTalents] Prüfe Talent_UI: {talent.name}");
            Debug.Log($"[SaveTheTalents] - currentCount: {talent.currentCount}");
            Debug.Log($"[SaveTheTalents] - passive: {talent.passive}");
            Debug.Log($"[SaveTheTalents] - myAbility: {(talent.myAbility != null ? talent.myAbility.name : "null")}");
            Debug.Log($"[SaveTheTalents] - myNode: {(talent.myNode != null ? $"ID={talent.myNode.ID}" : "null")}");

            // Nur Ability-basierte Talente mit currentCount > 0
            if (talent.myAbility != null && talent.currentCount > 0)
            {
                Debug.Log($"[SaveTheTalents] Ability-Talent hat currentCount > 0, wird gespeichert");

                TalentSave save = new TalentSave();
                save.abilityName = talent.myAbility.name;
                save.currentCount = talent.currentCount;
                save.isAbilityTalent = true;

                mySavedTalents.Add(save);
                savedCount++;
                Debug.Log($"[SaveTheTalents] ✓ Ability-Talent gespeichert: AbilityName={save.abilityName}, Count={save.currentCount}");
            }
            else if (talent.myAbility != null)
            {
                Debug.Log($"[SaveTheTalents] Ability-Talent hat currentCount = 0, wird übersprungen");
            }
            Debug.Log("---");
        }
        
        // 2. Speichere Node-basierte Talente (aus allNodes)
        Debug.Log($"[SaveTheTalents] Prüfe TalentNodes...");
        
        if (TalentTreeGenerator.instance != null && TalentTreeGenerator.instance.allNodes != null)
        {
            Debug.Log($"[SaveTheTalents] TalentTreeGenerator verfügbar mit {TalentTreeGenerator.instance.allNodes.Count} Nodes");
            
            foreach (TalentNode node in TalentTreeGenerator.instance.allNodes)
            {
                Debug.Log($"[SaveTheTalents] Prüfe TalentNode: ID={node.ID}");
                Debug.Log($"[SaveTheTalents] - myCurrentCount: {node.myCurrentCount}");
                Debug.Log($"[SaveTheTalents] - Depth: {node.Depth}");
                Debug.Log($"[SaveTheTalents] - myTalentUI: {(node.myTalentUI != null ? node.myTalentUI.name : "null")}");

                // Nur geskillte Nodes speichern
                if (node.myCurrentCount > 0)
                {
                    Debug.Log($"[SaveTheTalents] Node hat myCurrentCount > 0, wird gespeichert");
                    
                    TalentSave save = new TalentSave();
                    save.nodeID = node.ID;
                    save.currentCount = node.myCurrentCount;
                    save.isAbilityTalent = false;
                    
                    mySavedTalents.Add(save);
                    savedCount++;
                    Debug.Log($"[SaveTheTalents] ✓ Node-Talent gespeichert: NodeID={save.nodeID}, Count={save.currentCount}");
                }
                else
                {
                    Debug.Log($"[SaveTheTalents] Node hat myCurrentCount = 0, wird übersprungen");
                }
                Debug.Log("---");
            }
        }
        else
        {
            Debug.LogWarning($"[SaveTheTalents] ⚠️ TalentTreeGenerator oder allNodes ist null - keine Node-Talente gespeichert");
        }
        
        Debug.Log($"[SaveTheTalents] === ENDE === Insgesamt gespeichert: {savedCount} Talente");
    }
}




