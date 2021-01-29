using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLoad : MonoBehaviour
{

    //Ich weiß zwar, dass das ganz schön Spaghetti ist und ich einfach eine EQSlot Klasse schreiben sollte - ich tu's jetzt aber trotzdem nicht.
    public EQSlotBrust brust;
    public EQSlotHose hose;
    public EQSlotKopf kopf;
    public EQSlotSchmuck schmuck;
    public EQSlotSchuhe schuhe;
    public EQSlotWeapon weapon;

    [SerializeField]
    private TalentTree talentTree;

    private void Awake()
    {

        LoadScenePlayer();
    }

    public void LoadScenePlayer()
    {
        PlayerSave data = SaveSystem.LoadScenePlayer();

        PlayerStats playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        playerStats.level = data.level;

        playerStats.xp = data.xp;

        playerStats.Load_currentHp(data.Hp);

        //Die Gegenstände werden initialisiert und neu angezogen.

        #region Load-Equipped Items
        if (data.brust != null)
            brust.LoadItem(ItemDatabase.GetItemID(data.brust));

        if (data.hose != null)
            hose.LoadItem(ItemDatabase.GetItemID(data.hose));

        if (data.kopf != null)
            kopf.LoadItem(ItemDatabase.GetItemID(data.kopf));

        if (data.schuhe != null)
            schuhe.LoadItem(ItemDatabase.GetItemID(data.schuhe));

        if (data.schmuck != null)
            schmuck.LoadItem(ItemDatabase.GetItemID(data.schmuck));

        if (data.weapon != null)
            weapon.LoadItem(ItemDatabase.GetItemID(data.weapon));
        #endregion

        //Skillpunkte werden geladen.

        #region Load-Skillpoints

        foreach (TalentSave savedTalent in data.talentsToBeSaved)
        {
            print(savedTalent.talentName + " " + savedTalent.talentPoints);

            print(talentTree.defaultTalents[0].name + " aus dem Stanni tree mit " + talentTree.defaultTalents[0].currentCount + " geladen.");

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

