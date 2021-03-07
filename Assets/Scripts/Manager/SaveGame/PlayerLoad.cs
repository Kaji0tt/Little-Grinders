using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        playerStats.Set_SkillPoints(data.skillPoints);

        //Die Gegenstände werden initialisiert und neu angezogen.

        #region Load-Equipped Items
        if (data.brust != null)
            FindObjectOfType<EQSlotBrust>().LoadItem(ItemRolls.GetItemStats(data.brust, data.modsBrust, data.brust_r));

        if (data.hose != null)
            FindObjectOfType<EQSlotHose>().LoadItem(ItemRolls.GetItemStats(data.hose, data.modsHose, data.hose_r));

        if (data.kopf != null)
            FindObjectOfType<EQSlotKopf>().LoadItem(ItemRolls.GetItemStats(data.kopf, data.modsKopf, data.kopf_r));

        if (data.schuhe != null)
            FindObjectOfType<EQSlotSchuhe>().LoadItem(ItemRolls.GetItemStats(data.schuhe, data.modsSchuhe, data.schuhe_r));

        if (data.schmuck != null)
            FindObjectOfType<EQSlotSchmuck>().LoadItem(ItemRolls.GetItemStats(data.schmuck, data.modsSchmuck, data.schmuck_r));

        if (data.weapon != null)
            FindObjectOfType<EQSlotWeapon>().LoadItem(ItemRolls.GetItemStats(data.weapon, data.modsWeapon, data.weapon_r));
        #endregion

        //Skillpunkte werden geladen.

        #region Load-Skillpoints

        TalentTree talentTree = FindObjectOfType<TalentTree>();

        foreach (TalentSave savedTalent in data.talentsToBeSaved)
        {

            for (int i = 0; i < talentTree.allTalents.Length; i++)
            {
                if (talentTree.allTalents[i].name == savedTalent.talentName)
                {
                    talentTree.allTalents[i].Set_currentCount(savedTalent.talentPoints);

                    talentTree.allTalents[i].unlocked = savedTalent.unlocked;

                    talentTree.allTalents[i].UpdateTalent();
                }
            }

            /*
            for (int i = 0; i < talentTree.talents.Length; i++)
            {
                if (talentTree.talents[i].name == savedTalent.talentName)
                {
                    talentTree.talents[i].Set_currentCount(savedTalent.talentPoints);

                    talentTree.talents[i].UpdateTalent();
                }
            }
            */
            talentTree.UpdateTalentPointText();

        }
        #endregion

        // Inventar wird geladen.

        int currentItem = 0;

        foreach(string item in data.inventorySave)
        {

            PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.AddItem(ItemRolls.GetItemStats(item, data.inventoryItemMods[currentItem], data.inventoryItemRarity[currentItem]));

            currentItem += 1;

        }

        /// ActionButtons werden geladen
        /// 
        
        ActionButton[] actionButtons = FindObjectsOfType<ActionButton>();
        //Talent[] spells = Object.FindObjectsOfType<Spell>();


        foreach (ActionButton slot in actionButtons)
        {
            if (slot.gameObject.name == "ActionButton1")
                LoadActionbarSlot(0, slot, data);

            if (slot.gameObject.name == "ActionButton2")
                LoadActionbarSlot(1, slot, data);

            if (slot.gameObject.name == "ActionButton3")
                LoadActionbarSlot(2, slot, data);

            if (slot.gameObject.name == "ActionButton4")
                LoadActionbarSlot(3, slot, data);

            if (slot.gameObject.name == "ActionButton5")
                LoadActionbarSlot(4, slot, data);
        }
            // Is it possible to refer to a class, inherting form another from its parent class?
            /*
            foreach(Spell spell in spells)
            {
                if (data.savedActionButtons[i] == spell.name)
                {
                    actionButtons[i].SetUseable(spell);
                }
            }
            */


        
        
    }

    void LoadActionbarSlot(int i, ActionButton slot, PlayerSave data)
    {
        if (data.savedActionButtons[i] != null)
        {

            foreach (Spell spell in TalentTree.instance.allTalents.OfType<Spell>())
            {
                if (spell.GetSpellName == data.savedActionButtons[i])
                {
                    slot.LoadUseable(spell, spell);
                }

            }

        }
    }


}

