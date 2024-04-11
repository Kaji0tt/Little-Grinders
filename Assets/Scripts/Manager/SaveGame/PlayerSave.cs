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

        savedActionButtons = new string[5]; // Ggf. hier anknüpfen, sobald man Items auch safen kann. Man könnte das MyUseable as ItemInstance saven

        savedActionButtonIndex = new int[5];

        //Debug.Log("actionButtons" + actionButtons.Length);

        //Debug.Log("saved ACtionButtons " + savedActionButtons.Length);

        foreach (ActionButton slot in actionButtons)
        {
            if (slot.gameObject.name == "ActionButton1")
                SaveActionbarSlot(1, slot);

            if (slot.gameObject.name == "ActionButton2")
                SaveActionbarSlot(2, slot);

            if (slot.gameObject.name == "ActionButton3")
                SaveActionbarSlot(3, slot);

            if (slot.gameObject.name == "ActionButton4")
                SaveActionbarSlot(4, slot);

            if (slot.gameObject.name == "ActionButton5")
                SaveActionbarSlot(5, slot);

        }
    }

    private void SaveTheInventory()
    {
        inventoryItemMods = new List<ItemModsData>[PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.GetItemList().Count];

        inventoryItemRarity = new string[PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.GetItemList().Count];

        int currentItem = 0;

        foreach (ItemInstance item in PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.GetItemList())
        {
            inventorySave.Add(item.ItemID);

            inventoryItemMods[currentItem] = item.addedItemMods;

            inventoryItemRarity[currentItem] = item.itemRarity;

            currentItem += 1;

        }
    }

    private void SaveTheTalents()
    {
        //TalentTree talentTree = GameObject.Find("TalentTree").GetComponent<TalentTree>();

        Talent[] allTalents = Object.FindObjectsOfType<Talent>();

        foreach (Talent talent in allTalents)
        {
            //Debug.Log(talent.talentName + "Spezialisierung: " + (int)talent.abilitySpecialization);
            talentsToBeSaved.Add(new TalentSave(talent.name, talent.currentCount, talent.unlocked, (int)talent.abilityTalent.baseAbility.abilitySpec));
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
            if((slot.MyUseable is Spell))
            {
                savedActionButtons[i - 1] = (slot.MyUseable as Spell).GetSpellName;
            }
            else if (slot.MyUseable is ItemInstance)
            {
                savedActionButtons[i - 1] = (slot.MyUseable as ItemInstance).ItemID;
            }


        }
    }
}
