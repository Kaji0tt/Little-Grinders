using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SavedItem
{
    public string itemID;
    public string rarity;
    public List<ItemModSave> mods = new List<ItemModSave>();
    public int amount; // Für Consumables
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
    public string lastSpawnpoint;
    public float globalMapX, globalMapY;
    public List<MapSave> exploredMaps = new List<MapSave>();
    public MapSave currentMap;




    // Konstruktor, Save-Methoden etc.
    public PlayerSave()
    {
        ///Player speichern.
        /// Aktuell werden keine weiteren Stats gespeichert außer das aktuelle Leben, da
        /// sich diese über die Stat-Modifier dynamisch ändern.
        SaveThePlayer();

        /// Items Speichern
        SaveTheInventory();
        SaveTheEquipment();

        ///Skill-Points speichern.
        SaveTheTalents();

        /// Aktuelle Position speichern.
        mySavedPosition = new float[3];

        ///Save GlobalMap Settings
        if (SceneManager.GetActiveScene().buildIndex != 1)
            SaveTheGlobalMap();

    }

    private void SaveTheGlobalMap()
    {
        //GlobalMap only gets saved as the player is in Scene 2.
        //MyScene = 2;

        exploredMaps = GlobalMap.instance.exploredMaps;

        currentMap = GlobalMap.instance.currentMap;

        lastSpawnpoint = GlobalMap.instance.lastSpawnpoint;

        globalMapX = GlobalMap.instance.currentPosition.x; globalMapY = GlobalMap.instance.currentPosition.y;

    }

    /// <summary>
    /// Speichert die Spieler-Stats.   
    private void SaveThePlayer()
    {
        mySavedLevel = PlayerManager.instance.player.GetComponent<PlayerStats>().level;

        hp = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_currentHp();

        mySavedXp = PlayerManager.instance.player.GetComponent<PlayerStats>().xp;

        mySavedSkillpoints = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints();
    }

    /// <summary>
    /// Speichert das Inventar.
    /// </summary>
    private void SaveTheInventory()
    {
        var inventory = PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory;
        mySavedInventory.Clear();

        // Non-Consumables
        foreach (ItemInstance item in inventory.GetItemList())
        {
            mySavedInventory.Add(PlayerSave.SaveFromInstance(item));
        }

        // Consumables
        foreach (var kvp in inventory.GetConsumableDict())
        {
            ItemInstance item = new ItemInstance(ItemDatabase.GetItemByID(kvp.Key));
            mySavedInventory.Add(PlayerSave.SaveFromInstance(item, kvp.Value));
        }
    }

    /// <summary>
    /// Speichert die ausgerüsteten Gegenstände.
    /// </summary>
    private void SaveTheEquipment()
    {
        mySavedEquip.Clear();

        // Alle Int_SlotBtn-Instanzen in der Szene finden
        Int_SlotBtn[] slotButtons = GameObject.FindObjectsOfType<Int_SlotBtn>();

        foreach (var slotBtn in slotButtons)
        {
            // Nur speichern, wenn ein Item vorhanden ist
            if (slotBtn.storedItem != null)
            {
                // Nutze den ItemType als Key (z.B. "KOPF", "BRUST", ...)
                string slotKey = slotBtn.storedItem.itemType.ToString().ToUpper();

                mySavedEquip[slotKey] = PlayerSave.SaveFromInstance(slotBtn.storedItem);
            }
        }
    }

    public static SavedItem SaveFromInstance(ItemInstance itemInstance, int amount = 1)
    {
        var save = new SavedItem();
        save.itemID = itemInstance.ItemID;
        save.rarity = itemInstance.itemRarity.ToString();
        save.amount = amount;

        // Mods speichern
        foreach (var mod in itemInstance.addedItemMods)
        {
            var modSave = new ItemModSave();
            modSave.modName = mod.definition.modName;
            modSave.savedValue = mod.rolledValue;
            modSave.modRarity = mod.rolledRarity.ToString();
            save.mods.Add(modSave);
        }

        return save;
    }
    
    /// <summary>
    /// Speichert alle geskillten Talente.
    /// </summary>
    private void SaveTheTalents()
    {
        mySavedTalents.Clear();

        // Hole alle TalentNodes aus dem TalentTreeGenerator
        foreach (TalentNode node in TalentTreeGenerator.instance.allNodes)
        {
            // Nur geskillte Talente speichern
            if (node.myCurrentCount > 0)
            {
                TalentSave save = new TalentSave();
                save.nodeID = node.ID;
                save.currentCount = node.myCurrentCount;
                // Optional: weitere Felder wie Typen, Werte etc. hinzufügen

                mySavedTalents.Add(save);
            }
        }
    }
}




