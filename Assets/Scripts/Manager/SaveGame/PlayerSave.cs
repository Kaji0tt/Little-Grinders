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

    public int scene;
    void Awake()
    {


    }



    public PlayerSave()
    {
        level = PlayerManager.instance.player.GetComponent<PlayerStats>().level;

        Hp = PlayerManager.instance.player.GetComponent<PlayerStats>().Get_currentHp();

        xp = PlayerManager.instance.player.GetComponent<PlayerStats>().xp;


        /// Items Speichern
        /// 
        ItemSave.SaveEquippedItems();

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
            talentsToBeSaved.Add(new TalentSave(talent.name, talent.currentCount));
        }


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
        scene = SceneManager.GetActiveScene().buildIndex;

    }

   
}
