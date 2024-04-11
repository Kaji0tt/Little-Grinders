using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Relike der Anfänge. Die EQSlots sind mit das erste komplexere, was je programmiert wurde in diesem Spiel, und sind deshalb der absolute Inbegriff des Spaghetti-Monsters.
public class EQSlotWeapon : MonoBehaviour
{

    public static ItemInstance weapon_Item;

    private GameObject weaponAnim;

    private Int_SlotBtn int_slotBtn;



    private void OnEnable()
    {
        GameEvents.instance.equipWeapon += equip;

        int_slotBtn = GetComponent<Int_SlotBtn>();

        weaponAnim = GameObject.Find("WeaponAnimParent");
    }

    private void OnDisable()
    {
        GameEvents.instance.equipWeapon -= equip;
    }

    private void Start()
    {


    }


    public void equip(ItemInstance item)
    {
        if (weapon_Item == null)
        {
            EquipItem(item);


            //Line für Tutorial-Text
            if (item.ItemID == "WP0001" && GameObject.FindGameObjectWithTag("TutorialScript") != null)
            {
                Tutorial tutorialScript = GameObject.FindGameObjectWithTag("TutorialScript").GetComponent<Tutorial>();

                tutorialScript.ShowTutorial(5);

            }


        }
        else
        {
            Dequip();
            EquipItem(item);

        }

    }


    public void Dequip()
    {


        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory.AddItem(weapon_Item);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().Dequip(weapon_Item);

        weaponAnim.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Blank_Icon");

        if (weapon_Item.RangedWeapon)
            PlayerManager.instance.player.GetComponent<IsometricPlayer>().rangedWeapon = false;

        weapon_Item = null;


        int_slotBtn.storedItem = null;
    }

    public void EquipItem(ItemInstance item)
    {
        weapon_Item = item;

        //ItemSave.equippedItems.Add(item);

        int_slotBtn.StoreItem(item);

        GetComponent<Image>().sprite = item.icon;

        weaponAnim.GetComponent<SpriteRenderer>().sprite = item.icon;

        //playerStats = player.GetComponent<PlayerStats>();

        PlayerManager.instance.player.GetComponent<PlayerStats>().Range = item.Range;


        if (item.RangedWeapon)
            PlayerManager.instance.player.GetComponent<IsometricPlayer>().rangedWeapon = true;
    }

    public void TaskOnClick()
    {
        if (weapon_Item != null)
            Dequip();
    }


    public void LoadItem(ItemInstance item)
    {
        weapon_Item = item;

        PlayerManager.instance.player.GetComponent<IsometricPlayer>().equippedItems.Add(item);

        item.Equip(PlayerManager.instance.player.GetComponent<PlayerStats>());

        GetComponent<Image>().sprite = item.icon;

        Int_SlotBtn int_slotBtn = gameObject.GetComponentInChildren<Int_SlotBtn>();
        int_slotBtn.storedItem = item;

        weaponAnim = GameObject.Find("WeaponAnimParent");

        weaponAnim.GetComponent<SpriteRenderer>().sprite = item.icon;

    }
}
