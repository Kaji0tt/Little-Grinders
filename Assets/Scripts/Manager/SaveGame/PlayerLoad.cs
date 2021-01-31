using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLoad : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.HasKey("Load"))
        {
            PlayerSave data = SaveSystem.LoadPlayer();

            PlayerLoad playerLoad = FindObjectOfType<PlayerLoad>();

            playerLoad.LoadPlayer(data);

            PlayerPrefs.DeleteKey("Load");

        }
    }

    public void LoadPlayer(PlayerSave data)
    {
        //PlayerSave data = SaveSystem.LoadScenePlayer();

        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        playerStats.level = data.level;

        playerStats.xp = data.xp;

        playerStats.Load_currentHp(data.Hp);

        //Die Gegenstände werden initialisiert und neu angezogen.

        #region Load-Equipped Items
        if (data.brust != null)
            FindObjectOfType<EQSlotBrust>().LoadItem(ItemDatabase.GetItemID(data.brust));

        if (data.hose != null)
            FindObjectOfType<EQSlotHose>().LoadItem(ItemDatabase.GetItemID(data.hose));

        if (data.kopf != null)
            FindObjectOfType<EQSlotKopf>().LoadItem(ItemDatabase.GetItemID(data.kopf));

        if (data.schuhe != null)
            FindObjectOfType<EQSlotSchuhe>().LoadItem(ItemDatabase.GetItemID(data.schuhe));

        if (data.schmuck != null)
            FindObjectOfType<EQSlotSchmuck>().LoadItem(ItemDatabase.GetItemID(data.schmuck));

        if (data.weapon != null)
            FindObjectOfType<EQSlotWeapon>().LoadItem(ItemDatabase.GetItemID(data.weapon));
        #endregion

        //Skillpunkte werden geladen.

        #region Load-Skillpoints

        TalentTree talentTree = FindObjectOfType<TalentTree>();

        foreach (TalentSave savedTalent in data.talentsToBeSaved)
        {

            for (int i = 0; i < talentTree.defaultTalents.Length; i++)
            {
                if (talentTree.defaultTalents[i].name == savedTalent.talentName)
                {
                    talentTree.defaultTalents[i].Set_currentCount(savedTalent.talentPoints);

                    talentTree.defaultTalents[i].UpdateTalent();
                }
            }

            for (int i = 0; i < talentTree.talents.Length; i++)
            {
                if (talentTree.talents[i].name == savedTalent.talentName)
                {
                    talentTree.talents[i].Set_currentCount(savedTalent.talentPoints);

                    talentTree.talents[i].UpdateTalent();
                }
            }

            talentTree.UpdateTalentPointText();

        }
        #endregion

        // Inventar wird geladen.

        foreach(string ID in data.inventorySave)
        {

            PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.AddItem(ItemDatabase.GetItemID(ID));

        }

    }


}

