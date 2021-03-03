using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerSave
{

    /// <summary>
    /// Player Save
    /// </summary>

    public int level;

    public float Hp, maxHp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;

    public float[] position;

    public int xp;


    /// <summary>
    /// Item Save
    /// </summary>

    //EQ SLOTS

    public string brust, hose, kopf, schmuck, schuhe, weapon;

    public string brust_r, hose_r, kopf_r, schmuck_r, schuhe_r, weapon_r;

    public List<ItemModsData> modsBrust = new List<ItemModsData>(), modsHose = new List<ItemModsData>(), modsKopf = new List<ItemModsData>(), modsSchmuck = new List<ItemModsData>(), modsSchuhe = new List<ItemModsData>(), modsWeapon = new List<ItemModsData>();

    //Inventory

    public List<ItemModsData>[] inventoryItemMods;

    public string[] inventoryItemRarity;

    public List<string> inventorySave = new List<string>();


    /// <summary>
    /// Talents Save
    /// </summary>

    public List<TalentSave> talentsToBeSaved = new List<TalentSave>();

    public int skillPoints;



    /// <summary>
    /// Action Buttons
    /// </summary>

    public string[] savedActionButtons { get; set; }

    //public bool IsItem { get; set; } Not yet Implemented

    

    public int[] savedActionButtonIndex  { get; set; }


    /// <summary>
    /// Scene Save
    /// </summary>

    public int loadIndex;

    public int MyScene;







    public PlayerSave()
    {
        ///Player Speichern
        level = PlayerManager.instance.player.GetComponent<PlayerStats>().level;

        Hp = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_currentHp();

        xp = PlayerManager.instance.player.GetComponent<PlayerStats>().xp;

        skillPoints = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints();


        /// Items Speichern
        #region Items
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
        #endregion;


        ///Skill-Points speichern.

        TalentTree talentTree = GameObject.Find("TalentTree").GetComponent<TalentTree>();

        foreach (Talent talent in talentTree.allTalents)
        {
            talentsToBeSaved.Add(new TalentSave(talent.name, talent.currentCount, talent.unlocked));
        }

        //Es muss noch herausgefunden werden, wie viele Talenttree-Skillpoints unverteilt sind und die abgespeichert werden!



        ///Inventar speichern.

        Debug.Log(PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.GetItemList());

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

        ///Szene Speichern.
        ///
        MyScene = SceneManager.GetActiveScene().buildIndex;

        // Wenn die richtige Szene bereits geladen ist, kann der Spielstand nicht wieder hergestellt werden, WorkAround derzeit: Haupt Menü -> Laden.

        ///Action Buttons.
        ///



        ActionButton[] actionButtons = Object.FindObjectsOfType<ActionButton>();

        savedActionButtons = new string[5]; // Ggf. hier anknüpfen, sobald man Items auch safen kann. Man könnte das MyUseable as ItemInstance saven

        savedActionButtonIndex = new int[5];

        Debug.Log("actionButtons" + actionButtons.Length);

        Debug.Log("saved ACtionButtons " + savedActionButtons.Length);

            foreach(ActionButton slot in actionButtons)
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

        
        loadIndex = 1;
    }

    private void SaveActionbarSlot(int i, ActionButton slot)
    {
        if (slot.MyUseable != null)
        {
            savedActionButtonIndex[i-1] = i;

            Debug.Log("Found Useable at" + i + " and Saving at Index " + savedActionButtonIndex[i]);

            savedActionButtons[i-1] = (slot.MyUseable as Spell).GetSpellName;


        }
    }
}
