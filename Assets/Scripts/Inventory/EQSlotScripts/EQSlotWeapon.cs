using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EQSlotWeapon : MonoBehaviour
{

    private Item storedItem;
    private Inventory inventory;
    private GameObject player, weaponAnim;

    public IsometricPlayer isometricPlayer;
    private PlayerStats playerStats;

    private Int_SlotBtn int_slotBtn;

    private void Start()
    {
        GameEvents.current.equipWeapon += equip;
        storedItem = null;
        int_slotBtn = GetComponent<Int_SlotBtn>();
        player = PlayerManager.instance.player.gameObject;
        isometricPlayer = player.GetComponent<IsometricPlayer>();
    }


    public void equip(Item item)
    {
        if (storedItem == null)
        {
            EquipItem(item);




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
        inventory.AddItem(storedItem);
        GetComponent<Image>().sprite = Resources.Load<Sprite>("Blank_Icon");
        isometricPlayer.Dequip(storedItem);
        weaponAnim.GetComponent<SpriteRenderer>().sprite = null;

        if (storedItem.RangedWeapon)
            isometricPlayer.rangedWeapon = false;

        this.storedItem = null;
        int_slotBtn.storedItem = null;
    }

    public void EquipItem(Item item)
    {
        storedItem = item;
        int_slotBtn.StoreItem(item);
        GetComponent<Image>().sprite = item.icon;
        weaponAnim = GameObject.Find("WeaponAnimParent");
        weaponAnim.GetComponent<SpriteRenderer>().sprite = item.icon;
        playerStats = player.GetComponent<PlayerStats>();
        playerStats.Range = item.Range;
        print("i got trigerred with " + item.ItemName);


        if (item.RangedWeapon)
            isometricPlayer.rangedWeapon = true;
    }

    public void TaskOnClick()
    {
        Dequip();
    }

}
