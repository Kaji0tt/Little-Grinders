using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerSave
{

    #region PlayerSave

    public int level;

    public float Hp, maxHp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;

    public float[] position;

    public int xp;
    #endregion

    #region ItemSave

    //EQ SLOTS

    public string brust, hose, kopf, schmuck, schuhe, weapon;

    public string brust_r, hose_r, kopf_r, schmuck_r, schuhe_r, weapon_r;

    public List<ItemModsData> modsBrust = new List<ItemModsData>(), modsHose = new List<ItemModsData>(), modsKopf = new List<ItemModsData>(), modsSchmuck = new List<ItemModsData>(), modsSchuhe = new List<ItemModsData>(), modsWeapon = new List<ItemModsData>();

    //Inventory

    public List<ItemModsData>[] inventoryItemMods;

    public string[] inventoryItemRarity;

    public List<string> inventorySave = new List<string>();

    #endregion

    #region TalentsToBeSaved

    public List<TalentSave> talentsToBeSaved = new List<TalentSave>();

    public int skillPoints;

    public int savedLP;
    public int savedCP;
    public int savedVP;

    #endregion

    #region ActionsButtons

    public string[] savedActionButtons { get; set; }

    //public bool IsItem { get; set; } Not yet Implemented  

    public int[] savedActionButtonIndex  { get; set; }

    #endregion

    #region SceneSave

    public int loadIndex;

    public int MyScene = 1;

    public List<MapSave> exploredMaps;

    public MapSave currentMap;

    public string lastSpawnpoint;

    public float globalMapX; public float globalMapY;


    #endregion

    #region KeybindSave



    #endregion



    public PlayerSave()
    {
        ///Player Speichern
        SaveThePlayer();

        /// Items Speichern
        SaveTheItems();

        ///Skill-Points speichern.
        SaveTheTalents(); //Es muss noch herausgefunden werden, wie viele Talenttree-Skillpoints unverteilt sind und die abgespeichert werden!

        ///Inventar speichern.
        SaveTheInventory();

        ///Action Buttons.
        SaveTheActionButtons();

        ///Szene Speichern.
        SaveKeyBinds();
        
        loadIndex = 1;

        ///Save GlobalMap Settings
        if(SceneManager.GetActiveScene().buildIndex != 1)
        SaveTheGlobalMap();

    }

    private void SaveKeyBinds()
    {
    }

    private void SaveTheGlobalMap()
    {
        //GlobalMap only gets saved as the player is in Scene 2.
        MyScene = 2;

        exploredMaps = GlobalMap.instance.exploredMaps;

        currentMap = GlobalMap.instance.currentMap;

        lastSpawnpoint = GlobalMap.instance.lastSpawnpoint;

        globalMapX = GlobalMap.instance.currentPosition.x; globalMapY = GlobalMap.instance.currentPosition.y;

    }

    private void SaveTheActionButtons()
    {
        ActionButton[] actionButtons = Object.FindObjectsOfType<ActionButton>();

        // Setze die Größe der Arrays entsprechend der Anzahl der ActionButtons dynamisch
        savedActionButtons = new string[actionButtons.Length];
        savedActionButtonIndex = new int[actionButtons.Length];

        foreach (ActionButton slot in actionButtons)
        {
            // Extrahiere die Nummer aus dem ActionButton-Namen, z.B. "ActionButton1" => 1
            if (slot.gameObject.name.StartsWith("ActionButton"))
            {
                int slotIndex = int.Parse(slot.gameObject.name.Replace("ActionButton", "")) - 1;
                SaveActionbarSlot(slotIndex, slot); // Speichere den Slot dynamisch basierend auf der Slot-Nummer
            }
        }
    }
    private void SaveTheInventory()
    {
        // Hole das Item-Dictionary vom Inventar (Consumables) und die Liste der Non-Consumables
        var conDict = PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.GetConsumableDict();
        var itemList = PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.GetItemList();

        // Initialisiere die Arrays basierend auf der Gesamtanzahl der Items (Consumables + Non-Consumables)
        int totalItemCount = conDict.Count + itemList.Count;
        inventoryItemMods = new List<ItemModsData>[totalItemCount];
        inventoryItemRarity = new string[totalItemCount];

        int currentItem = 0;

        // Iteriere über das Dictionary der Consumables (Key: ItemID, Value: Anzahl)
        foreach (KeyValuePair<string, int> entry in conDict)
        {
            // Hole das ItemInstance basierend auf der ItemID
            ItemInstance item = new ItemInstance(ItemDatabase.GetItemByID(entry.Key));
            int amount = entry.Value;

            // Speichere die ItemID so oft, wie das Item vorhanden ist (Anzahl)
            for (int i = 0; i < amount; i++)
            {
                // Füge die ItemID zur Speicherliste hinzu
                inventorySave.Add(item.ItemID);

                // Speichere die Mods und die Rarity für das aktuelle Item
                inventoryItemMods[currentItem] = item.addedItemMods;
                inventoryItemRarity[currentItem] = item.itemRarity;

                currentItem++;
            }
        }

        // Iteriere über die Liste der Non-Consumables
        foreach (ItemInstance item in itemList)
        {
            // Speichere die ItemID des Non-Consumable-Items
            inventorySave.Add(item.ItemID);

            // Speichere die Mods und die Rarity für das aktuelle Item
            inventoryItemMods[currentItem] = item.addedItemMods;
            inventoryItemRarity[currentItem] = item.itemRarity;

            currentItem++;
        }
    }

    private void SaveTheTalents()
    {
        //TalentTree talentTree = GameObject.Find("TalentTree").GetComponent<TalentTree>();

        Talent[] allTalents = Object.FindObjectsOfType<Talent>();

        foreach (Talent talent in allTalents)
        {
            //Debug.Log(talent.talentName + "Spezialisierung: " + (int)talent.abilitySpecialization);
            talentsToBeSaved.Add(new TalentSave(talent.name, talent.currentCount, talent.unlocked, (int)talent.baseAbility.abilitySpec));
        }

        //Debug.Log(allTalents.Length);

        TalentTree talentTree = TalentTree.instance;

        savedLP = talentTree.totalUtilitySpecPoints;

        savedCP = talentTree.totalCombatSpecPoints;

        savedVP = talentTree.totalVoidSpecPoints;
    }

    private void SaveTheItems()
    {
        foreach (ItemInstance item in PlayerManager.instance.player.GetComponent<IsometricPlayer>().equippedItems)
        {
            switch (item.itemType)
            {
                case ItemType.Kopf:

                    kopf = item.ItemID;

                    kopf_r = item.itemRarity;

                    foreach (ItemModsData mod in item.addedItemMods)
                    {
                        modsKopf.Add(mod);
                    }


                    break;

                case ItemType.Brust:

                    brust = item.ItemID;

                    brust_r = item.itemRarity;

                    foreach (ItemModsData mod in item.addedItemMods)
                    {

                        modsBrust.Add(mod);
                    }

                    break;

                case ItemType.Beine:

                    hose = item.ItemID;

                    hose_r = item.itemRarity;


                    foreach (ItemModsData mod in item.addedItemMods)
                    {
                        modsHose.Add(mod);
                    }

                    break;

                case ItemType.Schuhe:

                    schuhe = item.ItemID;

                    schuhe_r = item.itemRarity;

                    foreach (ItemModsData mod in item.addedItemMods)
                    {
                        modsSchuhe.Add(mod);
                    }

                    break;

                case ItemType.Schmuck:

                    schmuck = item.ItemID;

                    schmuck_r = item.itemRarity;

                    foreach (ItemModsData mod in item.addedItemMods)
                    {
                        modsSchmuck.Add(mod);
                    }

                    break;

                case ItemType.Weapon:

                    weapon = item.ItemID;

                    weapon_r = item.itemRarity;

                    foreach (ItemModsData mod in item.addedItemMods)
                    {

                        modsWeapon.Add(mod);
                    }

                    break;
                case ItemType.Consumable:
                    //Placeholder. Call for ItemDelte or something. --> Irgendwie, wird das irgendwo anders schon gemacht.
                    break;

            }
        }
    }

    private void SaveThePlayer()
    {
        level = PlayerManager.instance.player.GetComponent<PlayerStats>().level;

        Hp = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_currentHp();

        xp = PlayerManager.instance.player.GetComponent<PlayerStats>().xp;

        skillPoints = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints();
    }

    private void SaveActionbarSlot(int i, ActionButton slot)
    {
        if (slot.MyUseable != null)
        {
            savedActionButtonIndex[i-1] = i;

            //Debug.Log("Found Useable at" + i + " and Saving at Index " + savedActionButtonIndex[i]);
            if(slot.MyUseable is Ability)
            {
                savedActionButtons[i - 1] = (slot.MyUseable as Ability).abilityName;
            }
            else if (slot.MyUseable is ItemInstance)
            {
                savedActionButtons[i - 1] = (slot.MyUseable as ItemInstance).ItemID;
            }


        }
    }
}
