using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class PlayerSave
{
    public int level;

    public float Hp, maxHp, Armor, AttackPower, AbilityPower, MovementSpeed, AttackSpeed;

    public float[] position;

    public int xp;

    public string brust, hose, kopf, schmuck, schuhe, weapon;
    public string brust_r, hose_r, kopf_r, schmuck_r, schuhe_r, weapon_r;

    public List<ItemModsData> modsBrust, modsHose, modsKopf, modsSchmuck, modsSchuhe, modsWeapon;

    public List<TalentSave> talentsToBeSaved = new List<TalentSave>();


    public List<ItemModsData>[] inventoryItemMods;

    public string[] inventoryItemRarity;

    public List<string> inventorySave = new List<string>();

    //public List<string> inventoryItemMods;

    public int MyScene;

    public int skillPoints;

    public string[] savedActionButtons;

    //public string[] itemMods;

    //Wird relevant, sobald wir mehrere SaveGames haben möchten. 
    public int loadIndex;





    public PlayerSave()
    {
        level = PlayerManager.instance.player.GetComponent<PlayerStats>().level;

        Hp = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_currentHp();

        xp = PlayerManager.instance.player.GetComponent<PlayerStats>().xp;

        skillPoints = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_SkillPoints();


        /// Items Speichern
        /// 
        #region Items
        ItemSave.SaveEquippedItems();

        //Die Brust scheint nicht richtig geladen zu werden, zumindest wird andauerd ein weißes Hemd geladen, obwohl andere Brust ausgestattet.

        if (ItemSave.brust != null)
        {
            brust = ItemSave.brust; //Gewollte Logik: Speichere String für ID, speichere List of ItemMods
                                    //Lade Item by ID, add Mods by List
            brust_r = ItemSave.brust_r;

            modsBrust = ItemSave.modsBrust;

        }

        if (ItemSave.hose != null)
        {
            hose = ItemSave.hose;

            hose_r = ItemSave.hose_r;

            modsHose = ItemSave.modsHose;

        }

        if (ItemSave.kopf != null)
        {
            kopf = ItemSave.kopf;

            kopf_r = ItemSave.kopf_r;

            modsKopf = ItemSave.modsKopf;

        }

        if (ItemSave.schmuck != null)
        {
            schmuck = ItemSave.schmuck;

            schmuck_r = ItemSave.schmuck_r;

            modsSchmuck = ItemSave.modsSchmuck;

        }

        if (ItemSave.schuhe != null)
        {
            schuhe = ItemSave.schuhe;

            schuhe_r = ItemSave.schuhe_r;

            modsSchuhe = ItemSave.modsSchuhe;

        }

        if (ItemSave.weapon != null)
        {
            weapon = ItemSave.weapon;

            weapon_r = ItemSave.weapon_r;

            modsWeapon = ItemSave.modsWeapon;

        }
        #endregion;


        ///Skill-Points speichern.
        ///

        TalentTree talentTree = GameObject.Find("TalentTree").GetComponent<TalentTree>();

        foreach (Talent talent in talentTree.allTalents)
        {
            talentsToBeSaved.Add(new TalentSave(talent.name, talent.currentCount, talent.unlocked));
        }

        //Es muss noch herausgefunden werden, wie viele Talenttree-Skillpoints unverteilt sind und die abgespeichert werden!



        ///Inventar speichern.
        ///
        //Inventory inventory = PlayerManager.instance.player.GetComponent<Inventory>();
        //PlayerManager.instance.player.GetComponent<Inventory>();

        Debug.Log(PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.GetItemList());


        //Logik-Gedanke: Erstelle Array von Listen(ItemModsData) in Größe des Inventares -> Foreach

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

        string[] inventorySaveArr = inventorySave.ToArray();

        //inventoryItemMods = new List<string>();

        /*foreach (ItemModsData modData in item.addedItemMods)
        {
            inventoryItemMods.Add(modData.id);
        }*/

        ///Szene Speichern.
        ///
        MyScene = SceneManager.GetActiveScene().buildIndex;

        // Wenn die richtige Szene bereits geladen ist, kann der Spielstand nicht wieder hergestellt werden, WorkAround derzeit: Haupt Menü -> Laden.

        ///Action Buttons.
        ///
        /*
        ActionButton[] actionButtons = Object.FindObjectsOfType<ActionButton>();

        for(int i = 0; i < actionButtons.Length; i++)
        {
            if (actionButtons[i].name != null) 
            savedActionButtons[i] = actionButtons[i].name; 
        }
        */
        loadIndex = 1;
    }

   
}
