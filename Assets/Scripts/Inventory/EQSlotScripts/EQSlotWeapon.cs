using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotWeapon : MonoBehaviour
{

    public static ItemInstance weapon_Item;
    private Inventory inventory;
    private GameObject weaponAnim;

    public IsometricPlayer isometricPlayer;


    private Int_SlotBtn int_slotBtn;


    private void Awake()
    {

    }
    
    
    private void Start()
    {
        GameEvents.current.equipWeapon += equip;

        int_slotBtn = GetComponent<Int_SlotBtn>();

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


        inventory = PlayerManager.instance.player.GetComponent<IsometricPlayer>().Inventory;

        inventory.AddItem(weapon_Item);

        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");

        isometricPlayer.Dequip(weapon_Item);

        weaponAnim.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Blank_Icon");

        if (weapon_Item.RangedWeapon)
            isometricPlayer.rangedWeapon = false;

        ItemSave.equippedItems.Remove(weapon_Item);

        weapon_Item = null;


        int_slotBtn.storedItem = null;
    }

    public void EquipItem(ItemInstance item)
    {
        weapon_Item = item;

        ItemSave.equippedItems.Add(item);

        int_slotBtn.StoreItem(item);

        GetComponent<Image>().sprite = item.icon;

        weaponAnim = GameObject.Find("WeaponAnimParent");

        weaponAnim.GetComponent<SpriteRenderer>().sprite = item.icon;

        //playerStats = player.GetComponent<PlayerStats>();

        PlayerManager.instance.player.GetComponent<PlayerStats>().Range = item.Range;


        if (item.RangedWeapon)
            isometricPlayer.rangedWeapon = true;
    }

    public void TaskOnClick()
    {
        if (weapon_Item != null)
            Dequip();
    }


    public void LoadItem(ItemInstance item)
    {
        weapon_Item = item;

        ItemSave.equippedItems.Add(item);

        item.Equip(PlayerManager.instance.player.GetComponent<PlayerStats>());

        GetComponent<Image>().sprite = item.icon;

        Int_SlotBtn int_slotBtn = gameObject.GetComponentInChildren<Int_SlotBtn>();
        int_slotBtn.storedItem = item;

        weaponAnim = GameObject.Find("WeaponAnimParent");

        weaponAnim.GetComponent<SpriteRenderer>().sprite = item.icon;

    }
}
