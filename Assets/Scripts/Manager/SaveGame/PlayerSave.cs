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

    public List<TalentSave> talentsToBeSaved = new List<TalentSave>();

    public List<string> inventorySave = new List<string>();

    public int MyScene;

    public int skillPoints;

    public string[] savedActionButtons;

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
        ItemSave.SaveEquippedItems();

        //Die Brust scheint nicht richtig geladen zu werden, zumindest wird andauerd ein weißes Hemd geladen, obwohl andere Brust ausgestattet.

        if (ItemSave.brust != null) brust = ItemSave.brust;

        if (ItemSave.hose != null) hose = ItemSave.hose;

        if (ItemSave.kopf != null) kopf = ItemSave.kopf;

        if (ItemSave.schmuck != null) schmuck = ItemSave.schmuck;

        if (ItemSave.schuhe != null) schuhe = ItemSave.schuhe;

        if (ItemSave.weapon != null) weapon = ItemSave.weapon;



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
        PlayerManager.instance.player.GetComponent<Inventory>();

        Debug.Log(PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.GetItemList());

        foreach (Item item in PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.GetItemList())
        {
            inventorySave.Add(item.ItemID);
        }

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
